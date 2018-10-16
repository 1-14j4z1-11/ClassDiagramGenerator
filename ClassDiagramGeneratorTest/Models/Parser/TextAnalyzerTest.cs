//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ClassDiagramGenerator.Models.Parser;
using ClassDiagramGeneratorTest.Support;
using static ClassDiagramGeneratorTest.Support.TestSupport;

namespace ClassDiagramGeneratorTest.Models.Parser
{
	[TestClass]
	public class TextAnalyzerTest
	{
		[TestMethod]
		public void TestSplitAndMarge()
		{
			TestcaseSplitAndMarge(new[] { Text(0, "A"), Text(1, "B"), Text(2, "C"), Text(3, "D"), Text(2, "E"), Text(1, "F"), Text(2, "G"), Text(1, "H"), Text(0, "I") },
				"A<B<C<D>E>F<G>H>I", "<", ">");
			TestcaseSplitAndMarge(new[] { Text(0, "Class"), Text(1, "T"), Text(0, "") },
				"Class<T>", "<", ">");
			TestcaseSplitAndMarge(new[] { Text(0, ""), Text(1, "Dictionary"), Text(2, "List"), Text(3, "int"), Text(2, ",string"), Text(1, ""), Text(0, "") },
				"<Dictionary<List<int>,string>>", "<", ">");
			TestcaseSplitAndMarge(new[] { Text(0, "int Sum(int n)"), Text(1, "if(i == 0)"), Text(2, "return 0;"), Text(1, "else"), Text(2, "return Sum(n-1)+n;"), Text(1, ""), Text(0, "") },
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
			target.Split(separator, nest, unnest, filter).IsCollection(splittedWords);
		}

		private static void TestcaseSplitAndMarge(IEnumerable<DepthText> splittedTexts, string margedText, string nest, string unnest)
		{
			TextAnalyzer.SplitWithDepth(margedText, nest, unnest).IsCollection(splittedTexts);
			splittedTexts.Marge(nest, unnest).Is(margedText);
		}
	}
}
