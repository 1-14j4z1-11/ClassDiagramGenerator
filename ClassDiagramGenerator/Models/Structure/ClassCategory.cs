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

	public static class ClassCategories
	{
		private static readonly Dictionary<ClassCategory, string> StringMap = new Dictionary<ClassCategory, string>()
		{
			[ClassCategory.Class] = "class",
			[ClassCategory.Interface] = "interface",
			[ClassCategory.Enum] = "enum",
			[ClassCategory.Struct] = "struct",
		};

		/// <summary>
		/// Parse <see cref="ClassCategory"/> from string.
		/// <para>If argument does not match any of the <see cref="ClassCategory"/>, returns null.</para>
		/// </summary>
		/// <param name="str">String to be parsed</param>
		/// <returns><see cref="ClassCategory"/> parsed from string, or null</returns>
		/// <exception cref="ArgumentNullException">If argument is null.</exception>
		public static ClassCategory? Parse(string str)
		{
			if(str == null)
				throw new ArgumentNullException();

			foreach(var value in StringMap.Keys)
			{
				if(StringMap[value] == str)
				{
					return value;
				}
			}

			return null;
		}

		/// <summary>
		/// Convert <see cref="ClassCategory"/> into string.
		/// </summary>
		/// <param name="category"></param>
		/// <returns>String of <see cref="ClassCategory"/></returns>
		public static string ToCategoryString(this ClassCategory category)
		{
			return StringMap.TryGetValue(category, out var value)
				? value
				: throw new NotImplementedException();
		}
	}
}
