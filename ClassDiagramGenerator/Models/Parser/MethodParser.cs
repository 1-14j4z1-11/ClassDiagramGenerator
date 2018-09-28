//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System.Text.RegularExpressions;

using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGenerator.Models.Parser
{
	/// <summary>
	/// Class to parse a method of C# and Java.
	/// </summary>
	public class MethodParser : ComponentParser<MethodInfo>
	{
		// In order to support both Java and C# methods, Type argument declarations are included twice.
		// Groups : [1] Modifier, [2] Return type, [3] Method name, [4] Arguments
		private static readonly Regex MethodRegex = new Regex(
			$"^\\s*{AttributePattern}{AnnotationPattern}((?:{ModifierPattern}\\s+)*)(?:<{TypeParamPattern}>\\s*)?"	// Attributes + Modifier + TypeArgDeclaration
			+ $"(?:({TypePattern})\\s+)?({NamePattern})\\s*(?:<{TypeParamPattern}>\\s*)?"         // ReturnType + Name + TypeArgDeclaration
			+ $"\\(\\s*({ArgumentPattern}?(?:\\s*,\\s*(?:{ArgumentPattern}))*)\\s*\\)");        // Arguments

		private readonly ClassInfo classInfo;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="classInfo"><see cref="ClassInfo"/> containing methods</param>
		/// <param name="defaultAccessLevel">Default access level attached to methods without access level modifier
		/// (Modifiers not indicating access level are ignored)</param>
		public MethodParser(ClassInfo classInfo, Modifier defaultAccessLevel)
			: base(defaultAccessLevel)
		{
			this.classInfo = classInfo;
		}

		public override bool TryParse(SourceCodeReader reader, out MethodInfo info)
		{
			if(!this.TryParseDefinitionLine(reader, out info, out var depth))
				return false;

			this.ParseImplementationLines(reader, info, depth);
			return true;
		}

		/// <summary>
		/// Tries to parse method definition line.
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

			var mod = this.ParseModifiers(match.Groups[1].Value);
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
				// TODO Gets used types in implementation lines
			}
		}

		protected override Modifier ParseModifiers(string modifierText)
		{
			var mod = base.ParseModifiers(modifierText);

			if((this.classInfo == null) || (this.classInfo.Category != ClassCategory.Interface))
				return mod;

			// Method in interface is always Public and Abstract
			mod &= ~Modifiers.AllAccessLevels;
			return Modifier.Public | Modifier.Abstract | mod;
		}
	}
}
