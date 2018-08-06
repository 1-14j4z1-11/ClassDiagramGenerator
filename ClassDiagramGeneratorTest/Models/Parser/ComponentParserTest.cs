using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestSupport.MSTest;
using ClassDiagramGenerator.Models.Parser;
using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGeneratorTest.Models.Parser
{
	[TestClass]
	public class ComponentParserTest : ComponentParser<object>
	{
		[TestMethod]
		public void TestParseType()
		{
			TestcaseParseTypes("string", Type("string"));
			TestcaseParseTypes("List<string>", Type("List", Type("string")));
			TestcaseParseTypes("List < List < int > >",
				Type("List", Type("List", Type("int"))));
			TestcaseParseTypes("Dictionary<string,int>",
				Type("Dictionary", Type("string"), Type("int")));
			TestcaseParseTypes("Dictionary<string, Dictionary<int, int>>",
				Type("Dictionary", Type("string"), Type("Dictionary", Type("int"), Type("int"))));
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

		private static TypeInfo Type(string type, params TypeInfo[] typeArgs)
		{
			return new TypeInfo(type, typeArgs);
		}
		
		public override bool TryParse(SourceCodeReader reader, out object obj)
		{
			// This method is not be implemented, because purpose of this class is testing static methods
			throw new NotImplementedException();
		}
	}
}
