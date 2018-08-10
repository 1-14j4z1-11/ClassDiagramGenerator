using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ClassDiagramGenerator.Models.Parser;
using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGeneratorTest.Models.Parser
{
	/// <summary>
	/// The support class to test a class inheriting <see cref="ComponentParser{T}"/>.
	/// </summary>
	public class ParserTestBase
	{
		/// <summary>
		/// Gets a sample source code 1.
		/// </summary>
		protected static string SampleCode1 { get; } = @"
using System;

public static class MainClass
{
	private static readonly string logText = ""Output"";
	
	public static void Main(string args)
	{
		for(var i = 0; i < 10; i++)
		{
			Output(i);
		}
	}
	
	public static string LogText { get => logText; } 
	
	private static int Output ( int x )
	{
		Console.WriteLine(LogText + x);
		return x;
	}
}
";

		/// <summary>
		/// Generates <see cref="SourceCodeReader"/> from a source code text.
		/// </summary>
		/// <param name="code">Source code text</param>
		/// <returns><see cref="SourceCodeReader"/></returns>
		protected static SourceCodeReader ReaderFromCode(string code)
		{
			return new SourceCodeReader(code);
		}

		/// <summary>
		/// Creates <see cref="TypeInfo"/>.
		/// </summary>
		/// <param name="type">Type name</param>
		/// <param name="typeArgs">Type arguments</param>
		/// <returns><see cref="TypeInfo"/></returns>
		protected static TypeInfo Type(string type, params TypeInfo[] typeArgs)
		{
			return new TypeInfo(type, typeArgs);
		}

		/// <summary>
		/// Creates <see cref="ArgumentInfo"/>.
		/// </summary>
		/// <param name="type"><see cref="TypeInfo"/></param>
		/// <param name="name">Argument name</param>
		/// <returns><see cref="ArgumentInfo"/></returns>
		protected static ArgumentInfo Arg(TypeInfo type, string name)
		{
			return new ArgumentInfo(type, name);
		}

		/// <summary>
		/// Creates <see cref="List{T}"/>.
		/// </summary>
		/// <typeparam name="T">Contents type in List</typeparam>
		/// <param name="values">Contents in List</param>
		/// <returns><see cref="List{T}"/></returns>
		protected static List<T> List<T>(params T[] values)
		{
			return values.ToList();
		}
	}
}
