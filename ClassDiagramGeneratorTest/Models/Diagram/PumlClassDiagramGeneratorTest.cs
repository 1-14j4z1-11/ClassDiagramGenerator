//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ClassDiagramGenerator.Models.Diagram;
using ClassDiagramGenerator.Models.Parser;
using ClassDiagramGenerator.Models.Structure;
using ClassDiagramGeneratorTest.Support;
using static ClassDiagramGeneratorTest.Support.TestSupport;

namespace ClassDiagramGeneratorTest.Models.Diagram
{
	[DeploymentItem(@"TestFiles\")]
	[TestClass]
	public class PumlClassDiagramGeneratorTest
	{
		private static readonly string CommentSymbol = "'";
		private static readonly ISourceCodeParser CSParser = new CSharpCodeParser();
		private static readonly ISourceCodeParser JavaParser = new JavaCodeParser();

		[TestMethod]
		public void TestGenerateFromCSharpCode1()
		{
			var expectedBase = List(
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
				"~ Method3(x : int, y : int) : object",

				"enum EnumValues",
				"+ {static} A : int",
				"+ {static} B : int",
				"+ {static} C : int",
				"+ {static} D : int",

				"interface IInterface",
				"+ {abstract} Method2() : string",

				"Derived --|> Base",
				"Derived ..|> IInterface");

			var expectedXClass = List(
				"class X",
				"+ <<get,set>> Value : EnumValues",
				"Derived ..> X",
				"X --> EnumValues");

			var expectedAll = expectedBase.Concat(expectedXClass);

			TestcaseGenerate(CSParser, LoadCode("Base.cs"), expectedAll);
			TestcaseGenerate(CSParser, LoadCode("Base.cs"), expectedBase, expectedXClass, Modifiers.AllAccessLevels, List("X"));
			TestcaseGenerate(CSParser, LoadCode("Base.cs"),
				expectedAll.Where(s => !s.StartsWith("-")),
				expectedAll.Where(s => s.StartsWith("-")),
				Modifier.Public | Modifier.Protected | Modifier.Internal,
				null);
			TestcaseGenerate(CSParser, LoadCode("Base.cs"),
				 expectedBase.Where(s => !s.StartsWith("+")),
				 expectedBase.Where(s => s.StartsWith("+")).Concat(expectedXClass),
				 Modifier.Protected | Modifier.Internal | Modifier.Private,
				 List("X"));
		}

		[TestMethod]
		public void TestGenerateFromJavaCode1()
		{
			var expectedBase = List(
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
				"~ Method3(x : int, y : int) : Object",

				"interface IInterface",
				"+ {abstract} Method2() : String",

				"enum EnumValues",
				"+ {static} A : int",
				"+ {static} B : int",
				"+ {static} C : int",
				"+ {static} D : int",
				"+ value : int",
				"- EnumValues(value : int)",

				"Derived --|> Base",
				"Derived ..|> IInterface");

			var expectedXClass = List(
				"class X",
				"- value : EnumValues",
				"+ getValue() : EnumValues",
				"+ setValue(value : EnumValues) : void",
				"Derived ..> X",
				"X --> EnumValues");

			var expectedAll = expectedBase.Concat(expectedXClass);

			TestcaseGenerate(JavaParser, LoadCode("Base.java"), expectedBase.Concat(expectedXClass));
			TestcaseGenerate(JavaParser, LoadCode("Base.java"), expectedBase, expectedXClass, Modifiers.AllAccessLevels, List("X"));
			TestcaseGenerate(JavaParser, LoadCode("Base.java"),
				expectedAll.Where(s => !s.StartsWith("~") && !s.StartsWith("#")),
				expectedAll.Where(s => s.StartsWith("~") || s.StartsWith("#")),
				Modifier.Public | Modifier.Private,
				null);
			TestcaseGenerate(JavaParser, LoadCode("Base.java"),
				 expectedBase.Where(s => !s.StartsWith("#")),
				 expectedBase.Where(s => s.StartsWith("#")).Concat(expectedXClass),
				 Modifier.Public | Modifier.Package | Modifier.Private,
				 List("X"));
		}

		[TestMethod]
		public void TestGenerateFromCSharpCode2()
		{
			var expectedBase = List(
				"package CSharp.Testcase2",

				"class Inner <<struct>>",
				"~ value : SampleClass.Inner");

			var expectedSampleClass = List(
				"abstract class SampleClass<T>",
				"- intArray2 : int[][]",
				"# strArray3 : string[][][]",
				"~ {static} Value : T",
				"# SampleClass(x : List<int[][]>, y : List<string[][][]>)",
				"# <<get,set>> X : int[][]",
				"~ <<get>> Y : string[][][]",
				"- <<get,set>> this[x : int, y : string] : object",
				"+ Func1(x : int, objects : List<object>[]) : List<Dictionary<string[][][], int[][]>>",

				"class SampleClass.Inner",
				"- x : double",
				"- y : double",
				"+ Inner(x : double, y : double)",
				"+ <<get>> X : double",
				"+ <<get,set>> Y : double",

				"SampleClass.Inner --+ SampleClass",
				"Inner --> SampleClass.Inner");

			var expectedAll = expectedBase.Concat(expectedSampleClass);

			TestcaseGenerate(CSParser, LoadCode("SampleClass.cs"), expectedAll);
			TestcaseGenerate(CSParser, LoadCode("SampleClass.cs"), expectedBase, expectedSampleClass, Modifiers.AllAccessLevels, List("SampleClass"));
			TestcaseGenerate(CSParser, LoadCode("SampleClass.cs"),
				expectedBase.Where(s => !s.StartsWith("-") && !s.StartsWith("~")),
				expectedBase.Where(s => s.StartsWith("-") || s.StartsWith("~")).Concat(expectedSampleClass)
				, Modifier.Public | Modifier.Protected, List("SampleClass"));
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
		/// <param name="expectedIgnoreLines">Text lines which is expected to be included as commented line in a generated class diagram</param>
		/// <param name="accessFilter">Access level filter</param>
		/// <param name="excludedClasses">Class names to be excluded in a diagram (actually they are included as commented lines in a diagram)</param>
		private static void TestcaseGenerate(ISourceCodeParser parser,
			string inputCode,
			IEnumerable<string> expectedLines,
			IEnumerable<string> expectedIgnoreLines = null,
			Modifier accessFilter = Modifiers.AllAccessLevels,
			IEnumerable<string> excludedClasses = null)
		{
			var title = "test_title";

			var classes = parser.Parse(inputCode);
			var relations = RelationFactory.CreateFromClasses(classes);

			var diag = PumlClassDiagramGenerator.Generate(title, classes, relations, accessFilter, excludedClasses);
			diag = Regex.Replace(diag, "\\s+", string.Empty);

			var emptyDiag = PumlClassDiagramGenerator.Generate(title, null, null);
			var emptyDiagLines = emptyDiag.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => Regex.Replace(s, "\\s+", string.Empty));

			var allExpLines = expectedLines
				.Concat(emptyDiagLines)		// To check common lines
				.Concat(expectedIgnoreLines?.Select(s => CommentSymbol + s) ?? Enumerable.Empty<string>())
				.Select(s => Regex.Replace(s, "\\s+", string.Empty));

			foreach(var line in allExpLines)
			{
				// It is OK, if one of patterns generated from a expected line matches a part of input code.
				var patterns = CreatePatterns(line);
				var matchedPattern = patterns.Where(p => diag.IndexOf(p) >= 0).FirstOrDefault();

				if(matchedPattern == null)
				{
					Console.WriteLine(diag);
					AssertEx.Fail($"Expected '{line}' is contained in a generated class diagram"
						+ $", but actual diagram does not contain it.");
				}

				diag = diag.Remove(diag.IndexOf(matchedPattern), matchedPattern.Length);
			}
			
			// Remove comment symbol attached to bracket.
			diag = diag.Replace($"{CommentSymbol}{{", "{").Replace($"{CommentSymbol}}}", "}");

			// If all texts are checked, diag is expected to contain only symbols.
			Regex.IsMatch(diag, "^[\\s{}]*$").IsTrue($"Unexpected lines are contained {diag}");
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

		/// <summary>
		/// Splits a sequence into a sequence matched with <paramref name="predicate"/> and a sequence unmatched with it.
		/// </summary>
		/// <typeparam name="T">Type of sequence</typeparam>
		/// <param name="seq">A sequence to be splitted</param>
		/// <param name="predicate">A function that determines whether a item in sequence matches a conditioin or not</param>
		/// <param name="matchedSeq">[out] A sequence that matches a condition</param>
		/// <param name="unmatchedSeq">[out] A sequence that does not match a condition</param>
		private static void SplitSequence<T>(IEnumerable<T> seq, Func<T, bool> predicate, out IEnumerable<T> matchedSeq, out IEnumerable<T> unmatchedSeq)
		{
			matchedSeq = seq.Where(predicate);
			unmatchedSeq = seq.Where(item => !predicate(item));
		}
	}
}
