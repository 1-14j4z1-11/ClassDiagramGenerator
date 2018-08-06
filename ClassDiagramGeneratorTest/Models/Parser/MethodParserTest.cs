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
	public class MethodParserTest : ComponentParserTestBase
	{
		[TestMethod]
		public void TestParseMethodDefinition()
		{
			TestcaseParseMethodDefinition("public void Func()", true,
				Modifier.Public, Type("void"), "Func", null);
			TestcaseParseMethodDefinition("public abstract void Func (  ) ", true,
				Modifier.Public | Modifier.Abstract, Type("void"), "Func", null);
			TestcaseParseMethodDefinition("internal string Arg1 ( int i )", true,
				Modifier.Internal, Type("string"), "Arg1", List(Arg(Type("int"), "i")));
			TestcaseParseMethodDefinition("IEnumerable<int> arg2 ( IEnumerable < string > _ , Dictionary < string , int > map ) ", true,
				Modifier.None, Type("IEnumerable", Type("int")), "arg2", List(Arg(Type("IEnumerable", Type("string")), "_"), Arg(Type("Dictionary", Type("string"), Type("int")), "map")));
			TestcaseParseMethodDefinition("private protected bool Out\t(object x, out string y)", true,
				Modifier.Private | Modifier.Protected, Type("bool"), "Out", List(Arg(Type("object"), "x"), Arg(Type("string"), "y")));
			TestcaseParseMethodDefinition("private bool Ref(ref in x, ref out y)", true,
				Modifier.Private, Type("bool"), "Ref", List(Arg(Type("in"), "x"), Arg(Type("out"), "y")));
			TestcaseParseMethodDefinition("static bool InOut(in object x, out string y)", true,
				Modifier.Static, Type("bool"), "InOut", List(Arg(Type("object"), "x"), Arg(Type("string"), "y")));
			TestcaseParseMethodDefinition("internal static bool Where < T > ( T value ) where T : IDisposable", true,
				Modifier.Internal | Modifier.Static, Type("bool"), "Where", List(Arg(Type("T"), "value")));

			// Constructor format
			TestcaseParseMethodDefinition("Func()", true,
				Modifier.None, null, "Func", null);
			TestcaseParseMethodDefinition("public Func()", true,
				Modifier.Public, null, "Func", null);
			TestcaseParseMethodDefinition("private Func(int x)", true,
				Modifier.Private, null, "Func", List(Arg(Type("int"), "x")));
			TestcaseParseMethodDefinition("internal static Func ( int  x ) ", true,
				Modifier.Internal | Modifier.Static, null, "Func", List(Arg(Type("int"), "x")));

			TestcaseParseMethodDefinition("public void", false);
			TestcaseParseMethodDefinition("public void ", false);
			TestcaseParseMethodDefinition("public void Func", false);
		}

		[TestMethod]
		public void TestParseMethodWithCode1()
		{
			TestcaseParseMethodAll(SampleCode1, 3, true, 5, Modifier.Public | Modifier.Static, Type("void"), "Main", List(Arg(Type("string"), "args")));
			TestcaseParseMethodAll(SampleCode1, 10, true, 3, Modifier.Private | Modifier.Static, Type("int"), "Output", List(Arg(Type("int"), "x")));

			foreach(var i in Enumerable.Range(0, 13).Except(new[] { 3, 10 }))
			{
				TestcaseParseMethodAll(SampleCode1, i, false);
			}
		}

		private static void TestcaseParseMethodAll(string code,
			int startPos,
			bool isSuccess,
			int? expectedReadLines = null,
			Modifier? mod = null,
			TypeInfo returnType = null,
			string methodName = null,
			IEnumerable<ArgumentInfo> argTypes = null)
		{
			var reader = ReaderFromCode(code);
			Enumerable.Range(0, startPos).ToList().ForEach(_ => reader.TryRead(out var _));

			new MethodParser().TryParse(reader, out var info).Is(isSuccess);

			if(isSuccess)
			{
				info.Modifier.Is(mod.Value);
				info.Name.Is(methodName);
				info.ReturnType.Is(returnType);
				info.Arguments.IsCollection(argTypes ?? Enumerable.Empty<ArgumentInfo>());

				reader.Position.Is(startPos + expectedReadLines.Value);
			}
			else
			{
				reader.Position.Is(startPos);
			}
		}

		private static void TestcaseParseMethodDefinition(string code,
			bool isSuccess,
			Modifier? mod = null,
			TypeInfo returnType = null,
			string methodName = null,
			IEnumerable<ArgumentInfo> argTypes = null)
		{
			TestcaseParseMethodAll(code, 0, isSuccess, 1, mod, returnType, methodName, argTypes);
		}
	}
}
