//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;

namespace ClassDiagramGenerator.Models.Structure
{
	/// <summary>
	/// Enum of argument modifier.
	/// </summary>
	public enum ArgumentModifier
	{
		/// <summary>No modifier (All arguments always have this in Java)</summary>
		None,

		/// <summary>this (only in C#)</summary>
		This,

		/// <summary>in (only in C#)</summary>
		In,

		/// <summary>out (only in C#)</summary>
		Out,

		/// <summary>ref (only in C#)</summary>
		Ref,
	}


	/// <summary>
	/// Support class of <see cref="ArgumentModifier"/>.
	/// </summary>
	public static class ArgumentModifiers
	{
		private static readonly Dictionary<ArgumentModifier, string> StringMap = new Dictionary<ArgumentModifier, string>()
		{
			[ArgumentModifier.None] = string.Empty,
			[ArgumentModifier.This] = "this",
			[ArgumentModifier.In] = "in",
			[ArgumentModifier.Out] = "out",
			[ArgumentModifier.Ref] = "ref",
		};

		/// <summary>
		/// Parses <see cref="ArgumentModifier"/> from string.
		/// <para>If an argument string does not match any of <see cref="ArgumentModifier"/> except for <see cref="ArgumentModifier.None"/>,
		/// returns <see cref="ArgumentModifier.None"/>.</para>
		/// </summary>
		/// <param name="str">String to be parsed</param>
		/// <returns><see cref="ArgumentModifier"/> parsed from string</returns>
		/// <exception cref="ArgumentNullException">If argument is null.</exception>
		public static ArgumentModifier Parse(string str)
		{
			if(str == null)
				throw new ArgumentNullException();

			foreach(var enumValue in StringMap.Keys)
			{
				if(StringMap[enumValue].ToLower() == str.ToLower())
				{
					return enumValue;
				}
			}

			return ArgumentModifier.None;
		}


		/// <summary>
		/// Converts <see cref="ArgumentModifier"/> into string.
		/// </summary>
		/// <param name="modifier"></param>
		/// <returns>String of <see cref="ArgumentModifier"/></returns>
		public static string ToModifierString(this ArgumentModifier modifier)
		{
			return StringMap[modifier];
		}
	}
}
