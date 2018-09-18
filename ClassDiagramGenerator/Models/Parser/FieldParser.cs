//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System.Linq;
using System.Text.RegularExpressions;

using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGenerator.Models.Parser
{
	/// <summary>
	/// Class to parse a field of C# and Java.
	/// </summary>
	public class FieldParser : ComponentParser<FieldInfo>
	{
		// Groups : [1] Modifier, [2] Return type, [3] Field name,
		//          [4] Indexer's arguments includeing "[ ]", [5] Lambda arrow, [6] Default value assign
		private static readonly Regex FieldRegex = new Regex(
			$"^\\s*{AttributePattern}{AnnotationPattern}((?:{ModifierPattern}\\s+)*)({TypePattern})\\s+({NamePattern})\\s*"
			+ $"(\\[\\s*(?:{ArgumentPattern}?(?:\\s*,\\s*(?:{ArgumentPattern}))*)\\s*\\])?[^=]*(=)?[^=]*(=>)?");

		private static readonly Regex GetterRegex = new Regex($"^\\s*(?:{ModifierPattern}\\s+)*\\s*(get|get\\s*=>)");
		private static readonly Regex SetterRegex = new Regex($"^\\s*(?:{ModifierPattern}\\s+)*\\s*(set|set\\s*=>)");
		private readonly ClassInfo classInfo;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="classInfo"><see cref="ClassInfo"/> containing fields</param>
		/// <param name="defaultAccessLevel">Default access level attached to fields without access level modifier
		/// (Modifiers not indicating access level are ignored)</param>
		public FieldParser(ClassInfo classInfo, Modifier defaultAccessLevel)
			: base(defaultAccessLevel)
		{
			this.classInfo = classInfo;
		}

		public override bool TryParse(SourceCodeReader reader, out FieldInfo info)
		{
			if(!reader.TryRead(out var text))
			{
				info = null;
				return false;
			}

			var depth = text.Depth;
			var match = FieldRegex.Match(text.Text);

			if(!match.Success)
			{
				reader.Position--;
				info = null;
				return false;
			}

			var idxrArgs = match.Groups[4].Value;
			var mod = this.ParseModifiers(match.Groups[1].Value);
			var type = string.IsNullOrWhiteSpace(match.Groups[2].Value) ? null : ParseType(match.Groups[2].Value);
			var name = match.Groups[3].Value;
			var args = ParseArguments(Regex.Replace(idxrArgs, "(\\s*\\[\\s*|\\s*\\]\\s*)", string.Empty));

			// Parsing must be treated as failure if parsed type name matches modifier,
			// because field regex pattern matches invalid pattern below.
			// ex) "public int" -> Type : public, FiledName : int
			if(Modifiers.Contains(type.Name))
			{
				reader.Position--;
				info = null;
				return false;
			}

			this.ParseImplementationLines(reader, depth, out var propTypeFromImpl);
			var propType = PropertyType.None;
			var isIndexer = !string.IsNullOrEmpty(idxrArgs);
			var hasDefault = !string.IsNullOrEmpty(match.Groups[5].Value);
			var hasGetter = !string.IsNullOrEmpty(match.Groups[6].Value);

			if(isIndexer)
			{
				propType |= PropertyType.Indexer;
			}
			if(hasGetter)
			{
				propType |= PropertyType.Get;
			}
			if(!hasDefault)
			{
				// If definition line contains a default value assignment expression, this field is not a property.
				// If this field is a property, adds a flag parsed from implementation lines.
				propType |= propTypeFromImpl;
			}

			info = new FieldInfo(mod, name, type, propType, args);
			return true;
		}
		
		/// <summary>
		/// Parses implementation lines.
		/// <para>If declarations of getter or setter appears, adds <see cref="PropertyType"/> to <paramref name="propType"/>.</para>
		/// </summary>
		/// <param name="reader"><see cref="SourceCodeReader"/></param>
		/// <param name="definitionDepth">Depth of field definition line</param>
		/// <param name="propType"><see cref="PropertyType"/> to be added getter or setter flags</param>
		private void ParseImplementationLines(SourceCodeReader reader, int definitionDepth, out PropertyType propType)
		{
			var subLines = GetMoreDeepLineCount(reader, definitionDepth);
			propType = PropertyType.None;

			// Skips all implementation lines, and checks getter or setter declarations.
			for(var i = 0; i < subLines; i++)
			{
				if(!reader.TryRead(out var sub) || sub.Depth != definitionDepth + 1)
					continue;
				
				if(GetterRegex.IsMatch(sub.Text))
				{
					propType |= PropertyType.Get;
				}
				if(SetterRegex.IsMatch(sub.Text))
				{
					propType |= PropertyType.Set;
				}
			}
		}
	}
}
