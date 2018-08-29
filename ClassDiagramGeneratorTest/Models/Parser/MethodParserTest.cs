using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestSupport.MSTest;

using ClassDiagramGenerator.Models.Parser;
using ClassDiagramGenerator.Models.Structure;
using static ClassDiagramGeneratorTest.Models.TestSupport;

namespace ClassDiagramGeneratorTest.Models.Parser
{
	[TestClass]
	public class MethodParserTest
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

			// Lambda format
			TestcaseParseMethodDefinition("public int Max(int x, int y) => (x > y) ? x : y", true,
				Modifier.Public, Type("int"), "Max", List(Arg(Type("int"), "x"), Arg(Type("int"), "y")));

			// C# Generic
			TestcaseParseMethodDefinition("public bool Generic<T>(T value)", true,
				Modifier.Public, Type("bool"), "Generic", List(Arg(Type("T"), "value")));
			TestcaseParseMethodDefinition("internal static int Where < T > ( T value ) where T : IDisposable", true,
				Modifier.Internal | Modifier.Static, Type("int"), "Where", List(Arg(Type("T"), "value")));
			
			// Java Generic
			TestcaseParseMethodDefinition("protected <T> bool Generic(T value)", true,
				Modifier.Protected, Type("bool"), "Generic", List(Arg(Type("T"), "value")));
			TestcaseParseMethodDefinition("static < T extends String > String Extends ( T value )", true,
				Modifier.Static, Type("String"), "Extends", List(Arg(Type("T"), "value")));
			TestcaseParseMethodDefinition("static <T> String Extends(List<? extends T> values)", true,
				Modifier.Static, Type("String"), "Extends", List(Arg(Type("List", Type("? extends T")), "values")));
			TestcaseParseMethodDefinition("static <T> String Super(List < ? super T > values)", true,
				Modifier.Static, Type("String"), "Super", List(Arg(Type("List", Type("? super T")), "values")));

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

			// Not method
			TestcaseParseMethodDefinition("public void", false);
			TestcaseParseMethodDefinition("public void Func", false);
			TestcaseParseMethodDefinition("public object obj = new object()", false);
			TestcaseParseMethodDefinition("private List<int> list = new List<int>()", false);
			TestcaseParseMethodDefinition("private List<Srting> list = new ArrayList<>()", false);
			TestcaseParseMethodDefinition("private List<Srting> list = new ArrayList()", false);
		}

		[TestMethod]
		public void TestParseMethodAll()
		{
			TestcaseParseMethodAll(@"public static void Main(string[] args) { Console.WriteLine(""Hello world.""); }", true, 2,
				Modifier.Public | Modifier.Static, Type("void"), "Main", List(Arg(TypeArray("string"), "args")));

			TestcaseParseMethodAll(@"
protected virtual T SetValue<T>(string x, T value)
{
	for(var i = 0; i < keys.Length; i++)
	{
		if(keys[i] == x)
		{
			values[i] = value;
			return value;
		}
	}

	throw new ArgumentException($""Invalid Key: {x}"");
}
", 
				true, 8, Modifier.Protected | Modifier.Virtual, Type("T"), "SetValue", List(Arg(Type("string"), "x"), Arg(Type("T"), "value")));
		}
		
		[TestMethod]
		public void TestParseFailure()
		{
			var reader = ReaderFromCode(string.Empty);
			var parser = new MethodParser(null);

			parser.TryParse(reader, out var _).IsFalse();
		}

		[TestMethod]
		public void TestParseMethodInInterface()
		{
			var ifClass = Interface(Type("Interface"));

			TestcaseParseMethodDefinition("void Func()", true,
				Modifier.Public | Modifier.Abstract, Type("void"), "Func", null, ifClass);
			TestcaseParseMethodDefinition("R Func<T, R>(T x)", true,
				Modifier.Public | Modifier.Abstract, Type("R"), "Func", List(Arg(Type("T"), "x")), ifClass);
		}

		private static void TestcaseParseMethodAll(string code,
			bool isSuccess,
			int? expectedReadLines = null,
			Modifier? mod = null,
			TypeInfo returnType = null,
			string methodName = null,
			IEnumerable<ArgumentInfo> argTypes = null,
			ClassInfo parserClassInfo = null)
		{
			var reader = ReaderFromCode(code);

			new MethodParser(parserClassInfo).TryParse(reader, out var info).Is(isSuccess);

			if(isSuccess)
			{
				info.Modifier.Is(mod.Value);
				info.Name.Is(methodName);
				info.ReturnType.Is(returnType);
				info.Arguments.IsCollection(argTypes ?? Enumerable.Empty<ArgumentInfo>());

				reader.Position.Is(expectedReadLines.Value);
			}
			else
			{
				reader.Position.Is(0);
			}
		}

		private static void TestcaseParseMethodDefinition(string code,
			bool isSuccess,
			Modifier? mod = null,
			TypeInfo returnType = null,
			string methodName = null,
			IEnumerable<ArgumentInfo> argTypes = null,
			ClassInfo parserClassInfo = null)
		{
			TestcaseParseMethodAll(code, isSuccess, 1, mod, returnType, methodName, argTypes, parserClassInfo);
		}
	}
}
