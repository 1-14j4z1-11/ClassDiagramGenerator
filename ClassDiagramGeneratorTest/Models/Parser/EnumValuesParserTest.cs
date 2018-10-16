//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ClassDiagramGenerator.Models.Parser;
using ClassDiagramGenerator.Models.Structure;
using ClassDiagramGeneratorTest.Support;
using static ClassDiagramGeneratorTest.Support.TestSupport;

namespace ClassDiagramGeneratorTest.Models.Parser
{
	[TestClass]
	public class EnumValuesParserTest
	{
		[TestMethod]
		public void TestParseEnumValues()
		{
			TestcaseParse("A,B,C,", "A", "B", "C");
			TestcaseParse("A = 1,B = 2,C = 4", "A", "B", "C");
			TestcaseParse("A(1),B(2),C(4)", "A", "B", "C");
			TestcaseParse("A(1, \"a\"),B(2, \"b\"),C(4, \"c\")", "A", "B", "C");

			TestcaseParse("A{ String func() { return null; } },B{ String func() { return null; } },C{ String func() { return null; } }",
				"A", "B", "C");
			TestcaseParse("A(1, \"a\"){ String func() { return null; } },B(1, \"b\"){ String func() { return null; } },C(1, \"c\"){ String func() { return null; } }",
				"A", "B", "C");
		}

		private static void TestcaseParse(string code, params string[] expectedEnumValues)
		{
			var reader = ReaderFromCode(code);
			var parser = new EnumValuesParser(null, -1);
			var actualValues = new List<FieldInfo>();

			while(!reader.IsEndOfLines)
			{
				if(parser.TryParse(reader, out var values))
				{
					actualValues.AddRange(values);
				}
				else
				{
					reader.TryRead(out var _);
				}
			}

			actualValues.Select(v => v.Name).IsCollection(expectedEnumValues);
		}
	}
}
