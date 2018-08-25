using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestSupport.MSTest;

using ClassDiagramGenerator.Models.Parser;

namespace ClassDiagramGeneratorTest.Models.Parser
{
	[TestClass]
	public class TextAnalyzerTest
	{
		[TestMethod]
		public void TestSplitAndMarge()
		{
			TestcaseSplitAndMarge(new[] { DP("A", 0), DP("B", 1), DP("C", 2), DP("D", 3), DP("E", 2), DP("F", 1), DP("G", 2), DP("H", 1), DP("I", 0) },
				"A<B<C<D>E>F<G>H>I", "<", ">");
			TestcaseSplitAndMarge(new[] { DP("Class", 0), DP("T", 1), DP("", 0) },
				"Class<T>", "<", ">");
			TestcaseSplitAndMarge(new[] { DP("", 0), DP("Dictionary", 1), DP("List", 2), DP("int", 3), DP(",string", 2), DP("", 1), DP("", 0) },
				"<Dictionary<List<int>,string>>", "<", ">");
			TestcaseSplitAndMarge(new[] { DP("int Sum(int n)", 0), DP("if(i == 0)", 1), DP("return 0;", 2), DP("else", 1), DP("return Sum(n-1)+n;", 2), DP("", 1), DP("", 0) },
				"int Sum(int n){if(i == 0){return 0;}else{return Sum(n-1)+n;}}", "{", "}");
		}

		[TestMethod]
		public void TestSplitWithDepthFilter()
		{
			TestcaseSplitWithDepthFilter("int x", ",", "<", ">", null, new[] { "int x" });
			TestcaseSplitWithDepthFilter(" int x ", ",", "<", ">", null, new[] { " int x " });
			TestcaseSplitWithDepthFilter("string<X,Y> xy, List<Z> z", ",", "<", ">", null, new[] { "string<X", "Y> xy", " List<Z> z" });
			TestcaseSplitWithDepthFilter("string<X,Y> xy, List<Z> z", ",", "<", ">", d => d == 0, new[] { "string<X,Y> xy", " List<Z> z" });
		}

		private static void TestcaseSplitWithDepthFilter(string target, string separator, string nest, string unnest, Func<int, bool> filter, IEnumerable<string> splittedWords)
		{
			target.SplitEach(separator, nest, unnest, filter).IsCollection(splittedWords);
		}

		private static void TestcaseSplitAndMarge(IEnumerable<DepthText> splittedTexts, string margedText, string nest, string unnest)
		{
			TextAnalyzer.SplitWithDepth(margedText, nest, unnest).IsCollection(splittedTexts);
			splittedTexts.Marge(nest, unnest).Is(margedText);
		}
		
		private static DepthText DP(string text, int depth)
		{
			return new DepthText(text, depth);
		}
	}
}
