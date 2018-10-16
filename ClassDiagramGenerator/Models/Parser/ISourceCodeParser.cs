//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System.Collections.Generic;

using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGenerator.Models.Parser
{
	/// <summary>
	/// Interface to parse a source code.
	/// </summary>
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
