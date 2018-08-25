//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGenerator.Models.Parser
{
	/// <summary>
	/// Class parsing a field of C# and Java.
	/// </summary>
	public class FieldParser : ComponentParser<FieldInfo>
	{
		// Groups : [1] Modifier, [2] Return type, [3] Field name
		private static readonly Regex FieldRegex = new Regex(
			$"^\\s*{AttributePattern}{AnnotationPattern}((?:{ModifierPattern}\\s+)*)({TypePattern})\\s+({NamePattern})\\s*"
			+ $"(?:\\[\\s*({ArgumentPattern}?(?:\\s*,\\s*(?:{ArgumentPattern}))*)\\s*\\])?");

		private readonly ClassInfo classInfo;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="classInfo"><see cref="ClassInfo"/> containing fields</param>
		public FieldParser(ClassInfo classInfo)
		{
			this.classInfo = classInfo;
		}

		public override bool TryParse(SourceCodeReader reader, out FieldInfo info)
		{
			if(!this.TryParseDefinitionLine(reader, out info, out var depth))
				return false;

			this.ParseImplementationLines(reader, info, depth);
			return true;
		}

		/// <summary>
		/// Tries to parse field definition line.
		/// </summary>
		/// <param name="reader"><see cref="SourceCodeReader"/></param>
		/// <param name="info">[out] Parsed <see cref="FieldInfo"/> (only succeeded in parsing)</param>
		/// <param name="depth">[out] Depth of field definition line (only succeeded in parsing)</param>
		/// <returns>Whether succeeded in parsing or not</returns>
		private bool TryParseDefinitionLine(SourceCodeReader reader, out FieldInfo info, out int depth)
		{
			if(!reader.TryRead(out var text))
			{
				info = null;
				depth = 0;
				return false;
			}

			depth = text.Depth;
			var match = FieldRegex.Match(text.Text);

			if(!match.Success)
			{
				reader.Position--;
				info = null;
				return false;
			}

			var mod = this.ParseModifiers(match.Groups[1].Value);
			var type = string.IsNullOrWhiteSpace(match.Groups[2].Value) ? null : ParseType(match.Groups[2].Value);
			var name = match.Groups[3].Value;
			var args = ParseArguments(match.Groups[4].Value);

			// Parsing is failed when parsed type name matches modifier
			// Field regex pattern matches invalid pattern below
			// ex) "public int" -> Type : public, FiledName : int
			if(Modifiers.Contains(type.Name))
			{
				reader.Position--;
				info = null;
				return false;
			}

			info = new FieldInfo(mod, name, type, args);

			return true;
		}

		/// <summary>
		/// Parse implementation lines.
		/// </summary>
		/// <param name="reader"><see cref="SourceCodeReader"/></param>
		/// <param name="info"><see cref="FieldInfo"/> to hold implementation contents</param>
		/// <param name="definitionDepth">Depth of field definition line</param>
		private void ParseImplementationLines(SourceCodeReader reader, FieldInfo info, int definitionDepth)
		{
			var subLines = GetMoreDeepLineCount(reader, definitionDepth);

			for(var i = 0; i < subLines; i++)
			{
				// Skip implementation lines
				reader.TryRead(out var sub);
			}
		}

		protected override Modifier ParseModifiers(string modifierText)
		{
			var mod = base.ParseModifiers(modifierText);
			return mod;
		}
	}
}
