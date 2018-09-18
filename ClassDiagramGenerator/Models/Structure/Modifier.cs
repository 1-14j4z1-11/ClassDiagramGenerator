//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ClassDiagramGenerator.Models.Structure
{
	/// <summary>
	/// Enum of modifier.
	/// </summary>
	[Flags]
	public enum Modifier
	{
		None = 0,

		/// <summary>public</summary>
		Public = 1 << 0,

		/// <summary>protected</summary>
		Protected = 1 << 1,

		/// <summary>internal (C# only)</summary>
		Internal = 1 << 2,

		/// <summary>package (Java only, not described in codes)</summary>
		Package = 1 << 3,

		/// <summary>private</summary>
		Private = 1 << 4,

		/// <summary>static</summary>
		Static = 1 << 5,

		/// <summary>abstract</summary>
		Abstract = 1 << 6,

		/// <summary>sealed (C# only)</summary>
		Sealed = 1 << 7,

		/// <summary>final</summary>
		Final = 1 << 8,

		/// <summary>virtual (C# only)</summary>
		Virtual = 1 << 9,

		/// <summary>new (C# only)</summary>
		New = 1 << 10,

		/// <summary>override (C# only)</summary>
		Override = 1 << 11,

		/// <summary>readonly (C# only)</summary>
		Readonly = 1 << 12,

		/// <summary>const (C# only)</summary>
		Const = 1 << 13,

		/// <summary>volatile</summary>
		Volatile = 1 << 14,

		/// <summary>event (C# only)</summary>
		Event = 1 << 15,

		/// <summary>async (C# only)</summary>
		Async = 1 << 16,

		/// <summary>extern (C# only)</summary>
		Extern = 1 << 17,

		/// <summary>partial (C# only)</summary>
		Partial = 1 << 18,

		/// <summary>unsafe (C# only)</summary>
		Unsafe = 1 << 19,

		/// <summary>strictfp (Java only)</summary>
		Strictfp = 1 << 20,

		/// <summary>transient (Java only)</summary>
		Transient = 1 << 21,

		/// <summary>native (Java only)</summary>
		Native = 1 << 22,

		/// <summary>synchronized (Java only)</summary>
		Synchronized = 1 << 23,
	}

	/// <summary>
	/// Support class of <see cref="Modifier"/>.
	/// </summary>
	public static class Modifiers
	{
		/// <summary>All access levels</summary>
		public const Modifier AllAccessLevels = Modifier.Public | Modifier.Protected | Modifier.Internal | Modifier.Package | Modifier.Private;

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
			[Modifier.Virtual] = "virtual",
			[Modifier.New] = "new",
			[Modifier.Override] = "override",
			[Modifier.Abstract] = "abstract",
			[Modifier.Readonly] = "readonly",
			[Modifier.Const] = "const",
			[Modifier.Volatile] = "volatile",
			[Modifier.Event] = "event",
			[Modifier.Async] = "async",
			[Modifier.Extern] = "extern",
			[Modifier.Partial] = "partial",
			[Modifier.Unsafe] = "unsafe",
			[Modifier.Strictfp] = "strictfp",
			[Modifier.Transient] = "transient",
			[Modifier.Native] = "native",
			[Modifier.Synchronized] = "synchronized",
		};

		/// <summary>
		/// Gets all <see cref="Modifier"/> values.
		/// </summary>
		public static IEnumerable<Modifier> AllModifiers { get; } = Enum.GetValues(typeof(Modifier)).Cast<Modifier>();

		/// <summary>
		/// Parses <see cref="Modifier"/> from string.
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
		/// Converts <see cref="Modifier"/> into string.
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
