using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDiagramGenerator.Models.Structure
{
	/// <summary>
	/// Specifies the class category.
	/// </summary>
	public enum ClassCategory
	{
		/// <summary>Class</summary>
		Class,

		/// <summary>Interface</summary>
		Interface,

		/// <summary>Enum</summary>
		Enum,

		/// <summary>Struct</summary>
		Struct,
	}
}
