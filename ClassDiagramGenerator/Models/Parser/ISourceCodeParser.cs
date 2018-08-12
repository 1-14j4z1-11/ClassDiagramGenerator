//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGenerator.Models.Parser
{
	public interface ISourceCodeParser
	{
		/// <summary>
		/// Parses a source code, and returns a collection of parsed classes.
		/// </summary>
		/// <param name="code">Source code to be parsed</param>
		/// <returns>Collection of classes parsed from source code</returns>
		IEnumerable<ClassInfo> Parse(string code);
	}
}
