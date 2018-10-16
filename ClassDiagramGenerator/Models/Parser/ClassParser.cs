//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGenerator.Models.Parser
{
	/// <summary>
	/// Class to parse classes of C# and Java.
	/// </summary>
	public class ClassParser : ComponentParser<ClassInfo>
	{
		private static readonly IEnumerable<string> Categories = Enum.GetValues(typeof(ClassCategory)).Cast<ClassCategory>().Select(c => c.ToCategoryString());
		private static readonly string ClassCategoryPattern = "(?:" + string.Join("|", Categories) + ")";

		// Pattern string that matches class type (Note that it differs from TypePattern, no grouping)</summary>
		private static readonly string ClassTypePattern = $"{NamePattern}(?:\\s*<{TypeParamPattern}>\\s*)?";

		// Groups : [1] Modifier, [2] Class category, [3] Class name, [4] Inherited classes
		private static readonly Regex ClassRegex = new Regex(
			$"^\\s*{AttributePattern}{AnnotationPattern}((?:{ModifierPattern}\\s+)*)({ClassCategoryPattern})\\s+({ClassTypePattern})\\s*"
			+ $"((?:\\s*(?::|extends|implements)\\s*(?:{TypePattern}(?:\\s*,\\s*(?:{TypePattern}))*))*)");

		private readonly string package;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="package">Package or namespace of classes to be parsed</param>
		/// <param name="defaultAccessLevel">Default access level attached to classes without access level modifier
		/// (Modifiers not indicating access level are ignored)</param>
		public ClassParser(string package, Modifier defaultAccessLevel)
			: base(defaultAccessLevel)
		{
			this.package = package;
		}

		public override bool TryParse(SourceCodeReader reader, out ClassInfo info)
		{
			return this.TryParseInternal(reader, string.Empty, out info);
		}

		/// <summary>
		/// Tries to parsing a class.
		/// </summary>
		/// <param name="reader"><see cref="SourceCodeReader"/></param>
		/// <param name="parentClassName">Parent class name added to a parsed class</param>
		/// <param name="info">[out] Parsed class (only succeeded in parsing)</param>
		/// <returns>Whether succeeded in parsing or not</returns>
		public bool TryParseInternal(SourceCodeReader reader, string parentClassName, out ClassInfo info)
		{
			if(!this.TryParseDefinitionLine(reader, parentClassName, out info, out var depth))
				return false;

			this.ParseImplementationLines(reader, info, info.Name, depth);
			return true;
		}

		/// <summary>
		/// Tries to parse class definition line.
		/// </summary>
		/// <param name="reader"><see cref="SourceCodeReader"/></param>
		/// <param name="parentClassName">Parent class name added to a parsed class</param>
		/// <param name="classInfo">[out] Parsed <see cref="ClassInfo"/> (only succeeded in parsing)</param>
		/// <param name="depth">[out] Depth of class definition line (only succeeded in parsing)</param>
		/// <returns>Whether succeeded in parsing or not</returns>
		private bool TryParseDefinitionLine(SourceCodeReader reader, string parentClassName, out ClassInfo classInfo, out int depth)
		{
			if(!reader.TryRead(out var text))
			{
				classInfo = null;
				depth = 0;
				return false;
			}

			depth = text.Depth;
			var match = ClassRegex.Match(text.Text);

			if(!match.Success)
			{
				reader.Position--;
				classInfo = null;
				depth = 0;
				return false;
			}

			var parentName = string.IsNullOrEmpty(parentClassName) ? string.Empty : parentClassName + ".";
			var mod = this.ParseModifiers(match.Groups[1].Value);
			var category = ParseClassCategory(match.Groups[2].Value);
			var type = ParseType(parentName + match.Groups[3].Value);
			var inheriteds = ParseInheritance(match.Groups[4].Value);
			classInfo = new ClassInfo(mod, category, this.package, type, inheriteds);

			return true;
		}

		/// <summary>
		/// Parses implementation lines.
		/// </summary>
		/// <param name="reader"><see cref="SourceCodeReader"/></param>
		/// <param name="classInfo"><see cref="ClassInfo"/> to hold implementation contents</param>
		/// <param name="parentClassName">Parent class name added to a parsed class</param>
		/// <param name="definitionDepth">Depth of class definition line</param>
		private void ParseImplementationLines(SourceCodeReader reader, ClassInfo classInfo, string parentClassName, int definitionDepth)
		{
			var endOfClass = reader.Position + GetMoreDeepLineCount(reader, definitionDepth);
			var methodParser = new MethodParser(classInfo, this.DefaultAccessLevel);
			var fieldParser = new FieldParser(classInfo, this.DefaultAccessLevel);
			var enumParser = new EnumValuesParser(classInfo, definitionDepth);
			var isFirstLine = true;

			while(reader.Position < endOfClass)
			{
				if(!IsNextLineDepth(reader, definitionDepth + 1))
				{
					reader.TryRead(out var _);
					continue;
				}

				if(isFirstLine && (classInfo.Category == ClassCategory.Enum) && enumParser.TryParse(reader, out var valuesFL))
				{
					// Parsing enum values of first line is only executed at first
					classInfo.Fields.AddRange(valuesFL);
				}
				else if(this.TryParseInternal(reader, parentClassName, out var innerInfo))
				{
					classInfo.InnerClasses.Add(innerInfo);
				}
				else if(methodParser.TryParse(reader, out var methodInfo))
				{
					classInfo.Methods.Add(methodInfo);
				}
				else if(fieldParser.TryParse(reader, out var fieldInfo))
				{
					// Parsing filed is executed after trying to parse method because field pattern also matches method
					classInfo.Fields.Add(fieldInfo);
				}
				else if(!isFirstLine && (classInfo.Category == ClassCategory.Enum) && enumParser.TryParse(reader, out var values))
				{
					// Parsing enum values is executed after trying to parse method and field just as with field parsing (except for first line)
					classInfo.Fields.AddRange(values);
				}
				else
				{
					// Skip a line if it did not match any pattern
					reader.TryRead(out var _);
				}

				isFirstLine = false;
			}
		}

		/// <summary>
		/// Parses inheritance classes text into a collection of <see cref="TypeInfo"/>.
		/// </summary>
		/// <param name="text">Inheritance classes text</param>
		/// <returns>A collection of <see cref="TypeInfo"/> parsed from <paramref name="text"/></returns>
		private static IEnumerable<TypeInfo> ParseInheritance(string text)
		{
			// Inheritance text extracted by class definition regex contains inheritance keyword
			text = text.Replace(":", ",").Replace("extends", ",").Replace("implements", ",");

			return text.Split(",", "<", ">", d => d == 0)
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select(s => ParseType(s));
		}

		/// <summary>
		/// Checks whether the depth of next line of <see cref="SourceCodeReader"/> is <paramref name="depth"/> or not.
		/// <para>If failed to read, returns false.</para>
		/// <para>This processing does not change position of <see cref="SourceCodeReader"/>.</para>
		/// </summary>
		/// <param name="reader"><see cref="SourceCodeReader"/> to be checked.</param>
		/// <param name="depth">Expected depth</param>
		/// <returns>Whether the depth of next line is <paramref name="depth"/> or not</returns>
		private static bool IsNextLineDepth(SourceCodeReader reader, int depth)
		{
			if(!reader.TryRead(out var text))
				return false;

			reader.Position--;

			return (text.Depth == depth);
		}

		/// <summary>
		/// Parses <see cref="ClassCategory"/>.
		/// </summary>
		/// <param name="text">string of class category</param>
		/// <returns><see cref="ClassCategory"/></returns>
		private static ClassCategory ParseClassCategory(string text)
		{
			return ClassCategories.Parse(text) ?? throw new NotImplementedException($"Unknown class category : {text}");
		}
	}
}
