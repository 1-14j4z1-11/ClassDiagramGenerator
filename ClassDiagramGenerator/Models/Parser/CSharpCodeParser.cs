﻿//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGenerator.Models.Parser
{
	/// <summary>
	/// Class to parse a C# source code.
	/// </summary>
	public class CSharpCodeParser : ISourceCodeParser
	{
		private static readonly Regex NameSpaceRegex = new Regex("^\\s*namespace\\s+([^\\s,:\\[\\]\\(\\)<>=]+)\\s*");

		public IEnumerable<ClassInfo> Parse(string code)
		{
			if(code == null)
				throw new ArgumentNullException();

			var reader = new SourceCodeReader(code);
			var classList = new List<ClassInfo>();
			var classParser = new ClassParser(string.Empty, Modifier.Internal);
			var prevNS = new Dictionary<int, string>();

			while(!reader.IsEndOfLines)
			{
				if(TryParseNameSpace(reader, out var nameSpace, out var depth))
				{
					// Already existed parent namespace, Add it before a nameSpace
					if(prevNS.ContainsKey(depth - 1))
					{
						nameSpace = prevNS[depth - 1] + "." + nameSpace;
					}

					prevNS[depth] = nameSpace;
					classParser = new ClassParser(nameSpace, Modifier.Internal);
					continue;
				}

				if(classParser.TryParse(reader, out var classInfo))
				{
					classList.Add(classInfo);
					continue;
				}

				reader.TryRead(out var _);
			}

			return classList;
		}

		/// <summary>
		/// Tries to parse a namespace.
		/// <para>If failed to parse, the position of <paramref name="reader"/> after this processing is the same as that of before this processing.</para>
		/// </summary>
		/// <param name="reader"><see cref="SourceCodeReader"/></param>
		/// <param name="nameSpace">[out] Namespace (only succeeded in parsing)</param>
		/// <param name="depth">[out] Depth of namespace (only succeeded in parsing)</param>
		/// <returns>Whether succeeded in parsing or not</returns>
		private static bool TryParseNameSpace(SourceCodeReader reader, out string nameSpace, out int depth)
		{
			if(!reader.TryRead(out var text))
			{
				nameSpace = null;
				depth = 0;
				return false;
			}

			var match = NameSpaceRegex.Match(text.Text);

			if(!match.Success)
			{
				reader.Position--;
				nameSpace = null;
				depth = 0;
				return false;
			}

			nameSpace = match.Groups[1].Value;
			depth = text.Depth;
			return true;
		}
	}
}
