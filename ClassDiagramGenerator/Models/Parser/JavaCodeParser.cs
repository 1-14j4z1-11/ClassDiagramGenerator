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
	public class JavaCodeParser : ISourceCodeParser
	{
		private static readonly Regex PackageRegex = new Regex("^\\s*package\\s+([^\\s,:\\[\\]\\(\\)<>=]+)\\s*");

		public IEnumerable<ClassInfo> Parse(string code)
		{
			if(code == null)
				throw new ArgumentNullException();

			var reader = new SourceCodeReader(code);
			var classList = new List<ClassInfo>();
			var classParser = new ClassParser(string.Empty, Modifier.Package);

			while(!reader.IsEndOfLines)
			{
				if(TryParsePackage(reader, out var package))
				{
					classParser = new ClassParser(package, Modifier.Package);
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
		/// Tries to parse a package.
		/// <para>If failed to parse, the position of <paramref name="reader"/> after this processing is the same as that of before this processing.</para>
		/// </summary>
		/// <param name="reader"><see cref="SourceCodeReader"/></param>
		/// <param name="package">[out] Package name (only succeeded in parsing)</param>
		/// <returns>Whether succeeded in parsing or not</returns>
		private static bool TryParsePackage(SourceCodeReader reader, out string package)
		{
			if(!reader.TryRead(out var text))
			{
				package = null;
				return false;
			}

			var match = PackageRegex.Match(text.Text);

			if(!match.Success)
			{
				reader.Position--;
				package = null;
				return false;
			}

			package = match.Groups[1].Value;
			return true;
		}
	}
}
