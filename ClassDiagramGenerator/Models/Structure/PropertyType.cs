//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;

namespace ClassDiagramGenerator.Models.Structure
{
	/// <summary>
	/// Enum of property types.
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
