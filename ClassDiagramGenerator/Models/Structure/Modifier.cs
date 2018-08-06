using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDiagramGenerator.Models.Structure
{
	/// <summary>
	/// Specifies the modifiers.
	/// </summary>
	[Flags]
	public enum Modifier
	{
		None = 0,

		Public = 1 << 0,

		Protected = 1 << 1,

		Internal = 1 << 2,

		Private = 1 << 3,

		Static = 1 << 4,

		Abstract = 1 << 5,

		Virtual = 1 << 6,

		Override = 1 << 7,

		New = 1 << 8,

		Readonly = 1 << 9,

		Const = 1 << 10,

		Event = 1 << 11,

		Async = 1 << 12,
	}

	/// <summary>
	/// Support class of <see cref="Modifier"/>.
	/// </summary>
	public static class Modifiers
	{
		private static readonly OrderedDictionary StringMap = new OrderedDictionary()
		{
			[Modifier.None] = string.Empty,
			[Modifier.Public] = "public",
			[Modifier.Protected] = "protected",
			[Modifier.Internal] = "internal",
			[Modifier.Private] = "private",
			[Modifier.Static] = "static",
			[Modifier.Abstract] = "abstract",
			[Modifier.Virtual] = "virtual",
			[Modifier.Override] = "override",
			[Modifier.New] = "new",
			[Modifier.Readonly] = "readonly",
			[Modifier.Const] = "const",
			[Modifier.Event] = "event",
			[Modifier.Async] = "async",
		};

		/// <summary>
		/// Parse modifiers from string.
		/// <para>If argument string contains no modifiers, returns <see cref="Modifier.None"/>.</para>
		/// </summary>
		/// <param name="str">文字列</param>
		/// <returns><see cref="Modifier"/> parsed from string</returns>
		/// <exception cref="ArgumentNullException">If argument is null.</exception>
		public static Modifier Parse(string str)
		{
			if(str == null)
				throw new ArgumentNullException();

			foreach(var enumValue in StringMap.Keys.Cast<Modifier>())
			{
				if((string)StringMap[enumValue] == str)
				{
					return enumValue;
				}
			}

			return Modifier.None;
		}

		/// <summary>
		/// Convert <see cref="Modifier"/> into string.
		/// </summary>
		/// <param name="modifier"></param>
		/// <returns>string of <see cref="Modifier"/></returns>
		public static string ToModifierString(this Modifier modifier)
		{
			var containedValues = new List<Modifier>();

			foreach(var enumValue in Enum.GetValues(typeof(Modifier)).Cast<Modifier>())
			{
				if(!modifier.HasFlag(enumValue) || string.IsNullOrEmpty((string)StringMap[enumValue]))
					continue;

				containedValues.Add(enumValue);
			}

			return string.Join(" ", containedValues.Select(m => StringMap[m]));
		}
	}
}
