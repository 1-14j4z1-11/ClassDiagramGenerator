//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDiagramGenerator.Models.Structure
{
	/// <summary>
	/// Specifies the property types.
	/// </summary>
	[Flags]
	public enum PropertyType
	{
		None = 0,

		Get = 1 << 0,

		Set = 1 << 1,

		Indexer = 1 << 2,
	}
}
