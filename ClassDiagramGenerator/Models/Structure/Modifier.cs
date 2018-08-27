//
// Copyright (c) 2018 Yasuhiro Hayashi
//

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

		Package = 1 << 3,

		Private = 1 << 4,

		Static = 1 << 5,

		Sealed = 1 << 6,

		Final = 1 << 7,

		Abstract = 1 << 8,

		Virtual = 1 << 9,

		Override = 1 << 10,

		New = 1 << 11,

		Readonly = 1 << 12,

		Const = 1 << 13,

		Event = 1 << 14,

		Async = 1 << 15,

		AllAccessLevels = Public | Protected | Internal | Package | Private,
	}

	/// <summary>
	/// Support class of <see cref="Modifier"/>.
	/// </summary>
	public static class Modifiers
	{
		private static readonly OrderedDictionary StringMap = new OrderedDictionary()
		{
			[Modifier.Public] = "public",
			[Modifier.Protected] = "protected",
			[Modifier.Internal] = "internal",
			[Modifier.Package] = "package",
			[Modifier.Private] = "private",
			[Modifier.Static] = "static",
			[Modifier.Sealed] = "sealed",
			[Modifier.Final] = "final",
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
		/// Parse <see cref="Modifier"/> from string.
		/// <para>If argument string contains no modifiers or contains only texts that are not modifier, returns <see cref="Modifier.None"/>.</para>
		/// </summary>
		/// <param name="str">String to be parsed</param>
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
		/// <returns>String of <see cref="Modifier"/></returns>
		public static string ToModifierString(this Modifier modifier)
		{
			if(modifier == Modifier.None)
				return string.Empty;

			var containedValues = new List<Modifier>();

			foreach(var enumValue in StringMap.Keys.Cast<Modifier>())
			{
				if(!modifier.HasFlag(enumValue))
					continue;

				containedValues.Add(enumValue);
			}

			return string.Join(" ", containedValues.Select(v => StringMap[v]));
		}
	}
}
