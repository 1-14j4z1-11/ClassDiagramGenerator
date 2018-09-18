//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
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
	public class ComponentParserTest : ComponentParser<object>
	{
		public ComponentParserTest()
			: base(Modifier.None)
		{ }

		[TestMethod]
		public void TestParseGenericTypes()
		{
			TestcaseParseTypes("string", Type("string"));
			TestcaseParseTypes("List<string>", Type("List", Type("string")));
			TestcaseParseTypes("List < List < int > >",
				Type("List", Type("List", Type("int"))));
			TestcaseParseTypes("Dictionary<string,int>",
				Type("Dictionary", Type("string"), Type("int")));
			TestcaseParseTypes("Dictionary<List<Dictionary<List<string>, int>>, Dictionary<int, List<int>>>",
				Type("Dictionary", Type("List", Type("Dictionary", Type("List", Type("string")), Type("int"))), Type("Dictionary", Type("int"), Type("List", Type("int")))));
		}

		[TestMethod]
		public void TestParseArrayTypes()
		{
			TestcaseParseTypes("string[]", TypeArray("string", 1));
			TestcaseParseTypes("List<string>[]", TypeArray("List", 1, Type("string")));
			TestcaseParseTypes("List < List < int [ ] > > [ ]",
				TypeArray("List", 1, Type("List", TypeArray("int", 1))));
			TestcaseParseTypes("Dictionary<string[],int[]>[]",
				TypeArray("Dictionary", 1, TypeArray("string", 1), TypeArray("int", 1)));
			TestcaseParseTypes("Dictionary < List<Dictionary<List<string>, int>> [] , Dictionary<int[], List<int>[]> > [ ]",
				TypeArray("Dictionary", 1, TypeArray("List", 1, Type("Dictionary", Type("List", Type("string")), Type("int"))), Type("Dictionary", TypeArray("int", 1), TypeArray("List", 1, Type("int")))));

			TestcaseParseTypes("string[][]", TypeArray("string", 2));
			TestcaseParseTypes("string[][][]", TypeArray("string", 3));
			TestcaseParseTypes("string...", TypeArray("string", 1));
			TestcaseParseTypes("string[] ...", TypeArray("string", 2));
			TestcaseParseTypes("List<string> ...", TypeArray("List", 1, Type("string")));
			TestcaseParseTypes("List<string>[] ...", TypeArray("List", 2, Type("string")));
			TestcaseParseTypes("List<string>[] ...", TypeArray("List", 2, Type("string")));
		}

		private void TestcaseParseTypes(string targetText, TypeInfo expextedType)
		{
			Action<IEnumerable<TypeInfo>, IEnumerable<TypeInfo>> recursiveCheck = null;

			recursiveCheck = (exp, act) =>
			{
				var expList = exp.ToList();
				var actList = act.ToList();

				actList.Count.Is(expList.Count);

				for(var i = 0; i < expList.Count; i++)
				{
					var subExp = expList[i];
					var subAct = actList[i];

					subAct.Name.Is(subExp.Name);
					recursiveCheck(subExp.TypeArgs, subAct.TypeArgs);
				}
			};

			var actualType = ParseType(targetText);
			actualType.Name.Is(expextedType.Name);
			recursiveCheck(expextedType.TypeArgs, actualType.TypeArgs);
		}
		
		public override bool TryParse(SourceCodeReader reader, out object obj)
		{
			// This method is not be implemented, because purpose of this class is testing static methods
			throw new NotImplementedException();
		}
	}
}
