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
		private static readonly string ClassCategoryPattern = "(?:class|interface|enum)";

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
		/// Try to parse class definition line.
		/// </summary>
		/// <param name="reader"><see cref="SourceCodeReader"/></param>
		/// <param name="info">[out] Parsed <see cref="ClassInfo"/> (only succeeded in parsing)</param>
		/// <param name="depth">[out] Depth of class definition line (only succeeded in parsing)</param>
		/// <returns>Whether succeeded in parsing or not</returns>
		private bool TryParseDefinitionLine(SourceCodeReader reader, out ClassInfo info, out int depth)
		{
			if(!reader.TryRead(out var text))
			{
				info = null;
				depth = 0;
				return false;
			}

			depth = text.Depth;
			var match = ClassRegex.Match(text.Text);

			if(!match.Success)
			{
				reader.Position--;
				info = null;
				depth = 0;
				return false;
			}

			var mod = ParseModifiers(match.Groups[1].Value);
			var category = ParseClassCategory(match.Groups[2].Value);
			var type = ParseType(match.Groups[3].Value);
			var inheriteds = TextAnalyzer.Split(match.Groups[4].Value, ",", "<", ">", d => d == 0)
				.Where(s => !string.IsNullOrWhiteSpace(s))
				.Select(s => ParseType(s));
			info = new ClassInfo(mod, category, this.nameSpace, type, inheriteds);

			return true;
		}

		/// <summary>
		/// Parse implementation lines.
		/// </summary>
		/// <param name="reader"><see cref="SourceCodeReader"/></param>
		/// <param name="info"><see cref="ClassInfo"/> to hold implementation contents</param>
		/// <param name="definitionDepth">Depth of class definition line</param>
		private void ParseImplementationLines(SourceCodeReader reader, ClassInfo info, int definitionDepth)
		{
			var endOfClass = reader.Position + GetMoreDeepLineCount(reader, definitionDepth);

			while(reader.Position < endOfClass)
			{
				if(!IsNextLineDepth(reader, definitionDepth + 1))
				{
					reader.TryRead(out var _);
					continue;
				}

				if(this.TryParse(reader, out var innerInfo))
				{
					info.InnerClasses.Add(innerInfo);
				}
				else if(this.methodParser.TryParse(reader, out var methodInfo))
				{
					info.Methods.Add(methodInfo);
				}
				else if(this.fieldParser.TryParse(reader, out var fieldInfo))
				{
					// Parsing filed is executed after parsing method because field pattern also matches method
					info.Fields.Add(fieldInfo);
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
		/// <returns>whether the depth of next line is <paramref name="depth"/> or not</returns>
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
			switch(text)
			{
				case "class":
					return ClassCategory.Class;
				case "interface":
					return ClassCategory.Interface;
				case "enum":
					return ClassCategory.Enum;
				case "struct":
					return ClassCategory.Struct;
				default:
					throw new NotImplementedException();
			}
		}
	}
}
