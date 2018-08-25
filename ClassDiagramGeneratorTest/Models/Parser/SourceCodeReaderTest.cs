using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestSupport.MSTest;

using ClassDiagramGenerator.Models.Parser;

namespace ClassDiagramGeneratorTest.Models.Parser
{
	[TestClass]
	public class SourceCodeReaderTest
	{
		[TestMethod]
		public void TestReader1()
		{
			// [Note]
			// - Comment is removed.
			// - String enclosed in '"' is removed.
			TestcaseReader(@"
// Main Funtion.
public static void main(string[] arg)
{
	Console.WriteLine(""Hello world."");
}",
				Text(0, "public static void main(string[] arg)"),
				Text(1, "Console.WriteLine()"));
		}

		[TestMethod]
		public void TestReader2()
		{
			// [Note]
			// - Doc comments are removed.
			// - All Single character is removed.
			// - String enclosed in '"' is removed.
			TestcaseReader(@"
using System.Collections.Generic;

/// SampleClass
public class SampleClass
{
	private const string Text1 = ""Test"";
	private const string Text2 = ""Test \"" Test"";
	private const char Char1 = '""';
	private const char Char2 = 'A';
	
	public int Max(int x, int y) => (x > y) ? x : y;
	
	public int Min(int x, int y)
	{
		return (x < y) ? x : y;
	}
	
	public static int Sum(IEnumerable<int> values)
	{
		// [1]
		var sum = 0;
		
		foreach(var v in values)
		{
			// [2]
			sum += v;
		}

		// [3]
		return sum;
	}
}",
				Text(0, "using System.Collections.Generic"),
				Text(0, "public class SampleClass"),
				Text(1, "private const string Text1 ="),
				Text(1, "private const string Text2 ="),
				Text(1, "private const char Char1 ="),
				Text(1, "private const char Char2 ="),
				Text(1, "public int Max(int x, int y) => (x > y) ? x : y"),
				Text(1, "public int Min(int x, int y)"),
				Text(2, "return (x < y) ? x : y"),
				Text(1, "public static int Sum(IEnumerable<int> values)"),
				Text(2, "var sum = 0"),
				Text(2, "foreach(var v in values)"),
				Text(3, "sum += v"),
				Text(2, "return sum"));
		}

		private static void TestcaseReader(string rawCode, params DepthText[] expectedTexts)
		{
			var reader = new SourceCodeReader(rawCode);

			foreach(var expText in expectedTexts)
			{
				reader.TryRead(out var actLine).IsTrue($"Expected line is {expText}, but Faield to read.");
				actLine.Text.Is(expText.Text, Message.OfValueName("Read Text"));
				actLine.Depth.Is(expText.Depth, Message.OfValueName("Read Text"));
			}

			reader.TryRead(out var text).IsFalse($"Expected reader reached end of code, but read a text '{text}'");
		}

		/// <summary>
		/// Creates a <see cref="DepthText"/>.
		/// </summary>
		/// <param name="depth">Depth</param>
		/// <param name="text">Text</param>
		/// <returns><see cref="DepthText"/></returns>
		private static DepthText Text(int depth, string text)
		{
			return new DepthText(text, depth);
		}
	}
}
