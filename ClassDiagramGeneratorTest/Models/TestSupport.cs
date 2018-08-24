using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ClassDiagramGenerator.Models.Diagram;
using ClassDiagramGenerator.Models.Parser;
using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGeneratorTest.Models
{
	/// <summary>
	/// The test support class.
	/// </summary>
	public static class TestSupport
	{
		#region Sample codes

		/// <summary>
		/// Gets a C# sample source code 1.
		/// </summary>
		public static string CSharpCode1 { get; } = @"
using System;

namespace Sample1
{
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
}
";
		
		#endregion

		/// <summary>
		/// Generates <see cref="SourceCodeReader"/> from a source code text.
		/// </summary>
		/// <param name="code">Source code text</param>
		/// <returns><see cref="SourceCodeReader"/></returns>
		public static SourceCodeReader ReaderFromCode(string code)
		{
			return new SourceCodeReader(code);
		}

		/// <summary>
		/// Gets a total line number of code.
		/// </summary>
		/// <param name="code">Code to be gotten number of lines</param>
		/// <returns>Total line number of code</returns>
		public static int TotalLineCount(string code)
		{
			return new SourceCodeReader(code).Lines.Count;
		}

		/// <summary>
		/// Creates a <see cref="TypeInfo"/>.
		/// </summary>
		/// <param name="type">Type name</param>
		/// <param name="typeArgs">Type arguments</param>
		/// <returns><see cref="TypeInfo"/></returns>
		public static TypeInfo Type(string type, params TypeInfo[] typeArgs)
		{
			return new TypeInfo(type, typeArgs);
		}

		/// <summary>
		/// Creates a Array <see cref="TypeInfo"/>.
		/// </summary>
		/// <param name="type">Type name</param>
		/// <param name="typeArgs">Type arguments</param>
		/// <returns><see cref="TypeInfo"/></returns>
		public static TypeInfo TypeArray(string type, params TypeInfo[] typeArgs)
		{
			return new TypeInfo(true, type, typeArgs);
		}

		/// <summary>
		/// Creates a <see cref="ArgumentInfo"/>.
		/// </summary>
		/// <param name="type"><see cref="TypeInfo"/></param>
		/// <param name="name">Argument name</param>
		/// <returns><see cref="ArgumentInfo"/></returns>
		public static ArgumentInfo Arg(TypeInfo type, string name)
		{
			return new ArgumentInfo(type, name);
		}

		/// <summary>
		/// Creates a <see cref="ClassDiagramGenerator.Models.Diagram.Relation"/>.
		/// </summary>
		/// <param name="class1">Class name 1</param>
		/// <param name="class2">Class name 2</param>
		/// <param name="type">Relation type</param>
		/// <returns><see cref="ClassDiagramGenerator.Models.Diagram.Relation"/></returns>
		public static Relation Relation(string class1, string class2, RelationType type)
		{
			return new ClassDiagramGenerator.Models.Diagram.Relation(class1, class2, type);
		}

		/// <summary>
		/// Creates a <see cref="List{T}"/>.
		/// </summary>
		/// <typeparam name="T">Contents type in List</typeparam>
		/// <param name="values">Contents in List</param>
		/// <returns><see cref="List{T}"/></returns>
		public static List<T> List<T>(params T[] values)
		{
			return values.ToList();
		}
	}
}
