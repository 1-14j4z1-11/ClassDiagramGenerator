using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGenerator.Models.Parser
{
	public class MethodParser : ComponentParser<MethodInfo>
	{
		// Groups : [1] Modifier, [2] Return type, [3] Method name, [4] Arguments
		private static readonly Regex MethodRegex = new Regex(
			$"^\\s*{AttributePattern}?((?:{ModifierPattern}\\s+)*)(?:({TypePattern})\\s+)?({NamePattern})\\s*(?:<{TypeArgPattern}>\\s*)?"
			+ $"\\(\\s*({ArgumentPattern}?(?:\\s*,\\s*(?:{ArgumentPattern}))*)\\s*\\)");

		private static readonly Regex CreateObjectRegex = new Regex($"\\s*new\\s+({NamePattern})\\s*");
		
		public override bool TryParse(SourceCodeReader reader, out MethodInfo info)
		{
			if(!this.TryParseDefinitionLine(reader, out info, out var depth))
				return false;

			this.ParseImplementationLines(reader, info, depth);
			return true;
		}

		/// <summary>
		/// Try to parse method definition line.
		/// </summary>
		/// <param name="reader"><see cref="SourceCodeReader"/></param>
		/// <param name="info">[out] Parsed <see cref="MethodInfo"/> (only succeeded in parsing)</param>
		/// <param name="depth">[out] Depth of method definition line (only succeeded in parsing)</param>
		/// <returns>Whether succeeded in parsing or not</returns>
		private bool TryParseDefinitionLine(SourceCodeReader reader, out MethodInfo info, out int depth)
		{
			if(!reader.TryRead(out var text))
			{
				info = null;
				depth = 0;
				return false;
			}

			depth = text.Depth;
			var match = MethodRegex.Match(text.Text);

			if(!match.Success)
			{
				reader.Position--;
				info = null;
				depth = 0;
				return false;
			}

			var mod = ParseModifiers(match.Groups[1].Value);
			var returnType = string.IsNullOrEmpty(match.Groups[2].Value) ? null : ParseType(match.Groups[2].Value);
			var name = match.Groups[3].Value;
			var args = ParseArguments(match.Groups[4].Value);
			info = new MethodInfo(mod, name, returnType, args);

			return true;
		}

		/// <summary>
		/// Parse implementation lines.
		/// </summary>
		/// <param name="reader"><see cref="SourceCodeReader"/></param>
		/// <param name="info"><see cref="MethodInfo"/> to hold implementation contents</param>
		/// <param name="definitionDepth">Depth of method definition line</param>
		private void ParseImplementationLines(SourceCodeReader reader, MethodInfo info, int definitionDepth)
		{
			var subLines = GetMoreDeepLineCount(reader, definitionDepth);

			for(var i = 0; i < subLines; i++)
			{
				reader.TryRead(out var sub);

				// Skip implementation lines
				// TODO Gets using type in implementation lines
			}
		}
	}
}
