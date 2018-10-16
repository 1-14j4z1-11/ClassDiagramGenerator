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
		/// <summary>None (A filed is not a property)</summary>
		None = 0,

		/// <summary>Getter property</summary>
		Get = 1 << 0,

		/// <summary>Setter property</summary>
		Set = 1 << 1,

		/// <summary>Indexer</summary>
		Indexer = 1 << 2,
	}
}
