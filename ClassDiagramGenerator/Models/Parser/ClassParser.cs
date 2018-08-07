using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGenerator.Models.Parser
{
	public class ClassParser : ComponentParser<ClassInfo>
	{
		private static readonly IEnumerable<string> Categories = Enum.GetValues(typeof(ClassCategory)).Cast<ClassCategory>().Select(c => c.ToCategoryString());
		private static readonly string ClassCategoryPattern = "(?:" + string.Join("|", Categories) + ")";

		// Groups : [1] Modifier, [2] Class category, [3] Class name, [4] Inherited classes
		private static readonly Regex ClassRegex = new Regex(
			$"^\\s*{AttributePattern}?((?:{ModifierPattern}\\s+)*)({ClassCategoryPattern})\\s+({TypePattern})\\s*"
			+ $"(?::\\s*({TypePattern}(?:\\s*,\\s*(?:{TypePattern}))*))?");

		private readonly MethodParser methodParser = new MethodParser();
		private readonly FieldParser fieldParser = new FieldParser();
		private readonly string nameSpace;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="nameSpace">namespace of classes to be parsed</param>
		public ClassParser(string nameSpace)
		{
			this.nameSpace = nameSpace;
		}

		public override bool TryParse(SourceCodeReader reader, out ClassInfo info)
		{
			if(!this.TryParseDefinitionLine(reader, out info, out var depth))
				return false;

			this.ParseImplementationLines(reader, info, depth);
			return true;
		}

		/// <summary>
		/// Tries to parse class definition line.
		/// </summary>
		/// <param name="reader"><see cref="SourceCodeReader"/></param>
		/// <param name="classInfo">[out] Parsed <see cref="ClassInfo"/> (only succeeded in parsing)</param>
		/// <param name="depth">[out] Depth of class definition line (only succeeded in parsing)</param>
		/// <returns>Whether succeeded in parsing or not</returns>
		private bool TryParseDefinitionLine(SourceCodeReader reader, out ClassInfo classInfo, out int depth)
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

			var mod = ParseModifiers(match.Groups[1].Value);
			var category = ParseClassCategory(match.Groups[2].Value);
			var type = ParseType(match.Groups[3].Value);
			var inheriteds = match.Groups[4].Value.Split(",", "<", ">", d => d == 0)
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select(s => ParseType(s));
			classInfo = new ClassInfo(mod, category, this.nameSpace, type, inheriteds);

			return true;
		}

		/// <summary>
		/// Parse implementation lines.
		/// </summary>
		/// <param name="reader"><see cref="SourceCodeReader"/></param>
		/// <param name="classInfo"><see cref="ClassInfo"/> to hold implementation contents</param>
		/// <param name="definitionDepth">Depth of class definition line</param>
		private void ParseImplementationLines(SourceCodeReader reader, ClassInfo classInfo, int definitionDepth)
		{
			var endOfClass = reader.Position + GetMoreDeepLineCount(reader, definitionDepth);
			var enumParser = new EnumValuesParser(classInfo);

			while(reader.Position < endOfClass)
			{
				if(!IsNextLineDepth(reader, definitionDepth + 1))
				{
					reader.TryRead(out var _);
					continue;
				}

				if((classInfo.Category == ClassCategory.Enum) && enumParser.TryParse(reader, out var values))
				{
					classInfo.Fields.AddRange(values);
				}
				else if(this.TryParse(reader, out var innerInfo))
				{
					classInfo.InnerClasses.Add(innerInfo);
				}
				else if(this.methodParser.TryParse(reader, out var methodInfo))
				{
					classInfo.Methods.Add(methodInfo);
				}
				else if(this.fieldParser.TryParse(reader, out var fieldInfo))
				{
					// Parsing filed is executed after parsing method because field pattern also matches method
					classInfo.Fields.Add(fieldInfo);
				}
				else
				{
					// Skip a line if it did not match any pattern
					reader.TryRead(out var _);
				}
			}
		}
		
		/// <summary>
		/// Check whether the depth of next line of <see cref="SourceCodeReader"/> is <paramref name="depth"/> or not.
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
		/// Parse <see cref="ClassCategory"/>.
		/// </summary>
		/// <param name="text">string of class category</param>
		/// <returns><see cref="ClassCategory"/></returns>
		private static ClassCategory ParseClassCategory(string text)
		{
			return ClassCategories.Parse(text) ?? throw new NotImplementedException($"Unknown class category : {text}");
		}
	}
}
