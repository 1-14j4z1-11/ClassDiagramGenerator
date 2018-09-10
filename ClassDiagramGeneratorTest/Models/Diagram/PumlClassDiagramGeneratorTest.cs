using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestSupport.MSTest;
using ClassDiagramGenerator.Models.Diagram;
using ClassDiagramGenerator.Models.Parser;

namespace ClassDiagramGeneratorTest.Models.Diagram
{
	[DeploymentItem(@"TestFiles\")]
	[TestClass]
	public class PumlClassDiagramGeneratorTest
	{
		private static readonly ISourceCodeParser CSParser = new CSharpCodeParser();
		private static readonly ISourceCodeParser JavaParser = new JavaCodeParser();

		[TestMethod]
		public void TestGenerateFromCSharpCode1()
		{
			TestcaseGenerate(CSParser, LoadCode("Base.cs"),
				"package CSharp.Testcase1",

				"class Base<T>",
				"- value : T",
				"+ <<get>> Value : T",
				"# <<get,set>> {static} Y : int",
				"+ Base(value : T)",
				"+ Method1(x : T) : string",

				"class Derived",
				"+ Derived(value : X)",
				"+ Method2() : string",

				"enum EnumValues",
				"+ {static} A : int",
				"+ {static} B : int",
				"+ {static} C : int",
				"+ {static} D : int",

				"interface IInterface",
				"+ {abstract} Method2() : string",

				"class X",
				"+ <<get,set>> Value : EnumValues",

				"Derived --|> Base",
				"Derived ..|> IInterface",
				"Derived ..> X",
				"X --> EnumValues");
		}

		[TestMethod]
		public void TestGenerateFromJavaCode1()
		{
			TestcaseGenerate(JavaParser, LoadCode("Base.java"),
				"package java.testcase1",

				"class Base<T extends Object>",
				"- {static} Y : int",
				"- value : T",
				"+ Base(value : T)",
				"+ getValue() : T",
				"# {static} getY() : int",
				"# {static} setY(y : int) : void",
				"+ Method1(x : T) : String",

				"class Derived",
				"+ Derived(value : X)",
				"+ Method2() : String",

				"interface IInterface",
				"+ {abstract} Method2() : String",

				"enum EnumValues",
				"+ {static} A : int",
				"+ {static} B : int",
				"+ {static} C : int",
				"+ {static} D : int",
				//"+ value : int",
				//"- EnumValues(value : int)",

				"class X",
				"- value : EnumValues",
				"+ getValue() : EnumValues",
				"+ setValue(value : EnumValues) : void",

				"Derived --|> Base",
				"Derived ..|> IInterface",
				"Derived ..> X",
				"X --> EnumValues");
		}

		/// <summary>
		/// Tests whether a class diagram generated from <paramref name="inputCode"/> contains all <paramref name="expectedLines"/>.
		/// (It's order does not matter)
		/// <para>This test method does not confirm that generated diagram constains unexpected lines.</para>
		/// <para>It searches expected texts with ignoring space characters.</para>
		/// </summary>
		/// <param name="parser">Parser to parse <paramref name="inputCode"/></param>
		/// <param name="inputCode">Input source code text to generate a class diagram</param>
		/// <param name="expectedLines">Text lines which is expected to be included in a generated class diagram</param>
		private static void TestcaseGenerate(ISourceCodeParser parser, string inputCode, params string[] expectedLines)
		{
			var title = "test_title";

			var classes = parser.Parse(inputCode);
			var relations = RelationFactory.CreateFromClasses(classes);

			var diag = PumlClassDiagramGenerator.Generate(title, classes, relations);
			diag = Regex.Replace(diag, "\\s+", string.Empty);

			var allExpLines = expectedLines.Concat(new[] { $"@startuml {title}", "@enduml" })
				.Select(s => Regex.Replace(s, "\\s+", string.Empty));

			foreach(var line in allExpLines)
			{
				// It is OK, if one of patterns generated from a expected line matches a part of input code.
				var patterns = CreatePatterns(line);
				var matchedPattern = patterns.Where(p => diag.IndexOf(p) >= 0).FirstOrDefault();

				if(matchedPattern == null)
				{
					TestResults.Fail($"Expected '{line}' is contained in a generated class diagram"
						+ $", but actual diagram does not contain it.");
				}

				diag = diag.Remove(diag.IndexOf(matchedPattern), matchedPattern.Length);
			}

			Console.WriteLine($"[Unchecked code]\n{diag}\n");
		}

		/// <summary>
		/// Creates a collection of pattern strings that can be treated as matching with argument string.
		/// </summary>
		/// <param name="s">A base string to creates patterns</param>
		/// <returns>Collection of pattern strings</returns>
		private static IEnumerable<string> CreatePatterns(string s)
		{
			var patterns = new List<string>() { s };
			var dirs = new[] { "up", "down", "left", "right", "u", "d", "l", "r" };

			if(Regex.IsMatch(s, "--"))
			{
				patterns.AddRange(dirs.Select(d => s.Replace("--", $"-{d}-")));
			}
			else if(Regex.IsMatch(s, "\\.\\."))
			{
				patterns.AddRange(dirs.Select(d => s.Replace("..", $".{d}.")));
			}
			else
			{
				// do nothing
			}

			return patterns;
		}

		/// <summary>
		/// Loads a source code from a file specified by <paramref name="fileName"/>.
		/// </summary>
		/// <param name="fileName">A file name to be loaded</param>
		/// <returns>A source code loaded from a file specified by <paramref name="fileName"/></returns>
		private static string LoadCode(string fileName)
		{
			return File.ReadAllText(fileName);
		}
	}
}
