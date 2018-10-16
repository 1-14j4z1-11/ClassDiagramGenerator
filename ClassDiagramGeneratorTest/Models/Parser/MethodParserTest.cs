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
				Modifier.Private | Modifier.Protected, Type("bool"), "Out", List(Arg(Type("object"), "x"), Arg(ArgumentModifier.Out, Type("string"), "y")));
			TestcaseParseMethodDefinition("private bool Ref(ref in x, ref out y)", true,
				Modifier.Private, Type("bool"), "Ref", List(Arg(ArgumentModifier.Ref, Type("in"), "x"), Arg(ArgumentModifier.Ref, Type("out"), "y")));
			TestcaseParseMethodDefinition("static bool InOut(in object x, out string y)", true,
				Modifier.Static, Type("bool"), "InOut", List(Arg(ArgumentModifier.In, Type("object"), "x"), Arg(ArgumentModifier.Out, Type("string"), "y")));
			TestcaseParseMethodDefinition("static string This(this object x, out string y)", true,
				Modifier.Static, Type("string"), "This", List(Arg(ArgumentModifier.This, Type("object"), "x"), Arg(ArgumentModifier.Out, Type("string"), "y")));

			TestcaseParseMethodDefinition("public Outer.Inner Func(Outer.Inner x)", true,
				Modifier.Public, Type("Outer.Inner"), "Func", List(Arg(Type("Outer.Inner"), "x")));
			TestcaseParseMethodDefinition("public Outer<int>.Inner Func(Outer<double>.Inner x)", true,
				Modifier.Public, Type("Outer.Inner"), "Func", List(Arg(Type("Outer.Inner"), "x")));
			TestcaseParseMethodDefinition("public Outer<string>.Inner<int> Func(Outer<string>.Inner<double> x)", true,
				Modifier.Public, Type("Outer.Inner", Type("int")), "Func", List(Arg(Type("Outer.Inner", Type("double")), "x")));

			// Lambda format
			TestcaseParseMethodDefinition("public int Max(int x, int y) => (x > y) ? x : y", true,
				Modifier.Public, Type("int"), "Max", List(Arg(Type("int"), "x"), Arg(Type("int"), "y")));
		}

		[TestMethod]
		public void TestParseGenericMethodDefinition()
		{
			// C#
			TestcaseParseMethodDefinition("public bool Generic<T>(T value)", true,
				Modifier.Public, Type("bool"), "Generic", List(Arg(Type("T"), "value")));
			TestcaseParseMethodDefinition("internal static int Where < T > ( T value ) where T : IDisposable", true,
				Modifier.Internal | Modifier.Static, Type("int"), "Where", List(Arg(Type("T"), "value")));
			TestcaseParseMethodDefinition("public bool Generic<T>(T[] value)", true,
				Modifier.Public, Type("bool"), "Generic", List(Arg(TypeArray("T", 1), "value")));
			TestcaseParseMethodDefinition("public List<T> Generic<T>(T[] value)", true,
				Modifier.Public, Type("List", Type("T")), "Generic", List(Arg(TypeArray("T", 1), "value")));
			TestcaseParseMethodDefinition("public List<T> Generic<T, U>(T[] value, out Dictionary<T, U> map)", true,
				Modifier.Public, Type("List", Type("T")), "Generic", List(Arg(TypeArray("T", 1), "value"), Arg(ArgumentModifier.Out, Type("Dictionary", Type("T"), Type("U")), "map")));

			TestcaseParseMethodDefinition("internal static bool Where < T > ( T value ) where T : IDisposable", true,
				Modifier.Internal | Modifier.Static, Type("bool"), "Where", List(Arg(Type("T"), "value")));

			// Java
			TestcaseParseMethodDefinition("protected <T> bool Generic(T value)", true,
				Modifier.Protected, Type("bool"), "Generic", List(Arg(Type("T"), "value")));
			TestcaseParseMethodDefinition("static < T extends String > String Extends ( T value )", true,
				Modifier.Static, Type("String"), "Extends", List(Arg(Type("T"), "value")));
			TestcaseParseMethodDefinition("static <T> String Super(List < ? super T > values)", true,
				Modifier.Static, Type("String"), "Super", List(Arg(Type("List", Type("? super T")), "values")));
			TestcaseParseMethodDefinition("static <T> List<T> Extends(List<? extends T> values)", true,
				Modifier.Static, Type("List", Type("T")), "Extends", List(Arg(Type("List", Type("? extends T")), "values")));
			TestcaseParseMethodDefinition("static < T > List<T> Generic(T[][] values)", true,
				Modifier.Static, Type("List", Type("T")), "Generic", List(Arg(TypeArray("T", 2), "values")));
			TestcaseParseMethodDefinition("public <T, U> List<T> Generic(T[] value, Dictionary<T, U> map)", true,
				Modifier.Public, Type("List", Type("T")), "Generic", List(Arg(TypeArray("T", 1), "value"), Arg(Type("Dictionary", Type("T"), Type("U")), "map")));
		}

		[TestMethod]
		public void TestParseMethodDefinitionUsingGenericAndArray()
		{
			TestcaseParseMethodDefinition("public List<string> Func()", true,
				Modifier.Public, Type("List", Type("string")), "Func", null);
			TestcaseParseMethodDefinition("public List<List<string>> Func(Dictionary<string, List<int>> values)", true,
				Modifier.Public, Type("List", Type("List", Type("string"))), "Func", List(Arg(Type("Dictionary", Type("string"), Type("List", Type("int"))), "values")));
			TestcaseParseMethodDefinition("public string[] Func(int[] values)", true,
				Modifier.Public, TypeArray("string", 1), "Func", List(Arg(TypeArray("int", 1), "values")));
			TestcaseParseMethodDefinition("public string[][] Func(int[] ... values)", true,
				Modifier.Public, TypeArray("string", 2), "Func", List(Arg(TypeArray("int", 2), "values")));
			TestcaseParseMethodDefinition("public string[][] Func(params int[] values)", true,
				Modifier.Public, TypeArray("string", 2), "Func", List(Arg(TypeArray("int", 1), "values")));

			TestcaseParseMethodDefinition("public List<string[]> Func(List<int[]> values)", true,
				Modifier.Public, Type("List", TypeArray("string", 1)), "Func", List(Arg(Type("List", TypeArray("int", 1)), "values")));
			TestcaseParseMethodDefinition("public List<string>[] Func(List<int>[] values)", true,
				Modifier.Public, TypeArray("List", 1, Type("string")), "Func", List(Arg(TypeArray("List", 1, Type("int")), "values")));
			TestcaseParseMethodDefinition("public List<string>[][] Func(List<int>[][] ... values)", true,
				Modifier.Public, TypeArray("List", 2, Type("string")), "Func", List(Arg(TypeArray("List", 3, Type("int")), "values")));
			TestcaseParseMethodDefinition("public List<string>[][] Func(params List<int>[][] values)", true,
				Modifier.Public, TypeArray("List", 2, Type("string")), "Func", List(Arg(TypeArray("List", 2, Type("int")), "values")));
			
			TestcaseParseMethodDefinition("public string[,] Func(int[,] values)", true,
				Modifier.Public, TypeArray("string", 2), "Func", List(Arg(TypeArray("int", 2), "values")));
			TestcaseParseMethodDefinition("public string[][,,][] Func(int[,,,][][,] values)", true,
				Modifier.Public, TypeArray("string", 5), "Func", List(Arg(TypeArray("int", 7), "values")));
		}

		[TestMethod]
		public void TestParseConstructorDefinition()
		{
			TestcaseParseMethodDefinition("Func()", true,
				Modifier.None, null, "Func", null);
			TestcaseParseMethodDefinition("public Func()", true,
				Modifier.Public, null, "Func", null);
			TestcaseParseMethodDefinition("private Func(int x)", true,
				Modifier.Private, null, "Func", List(Arg(Type("int"), "x")));
			TestcaseParseMethodDefinition("internal static Func ( int  x ) ", true,
				Modifier.Internal | Modifier.Static, null, "Func", List(Arg(Type("int"), "x")));
		}

		[TestMethod]
		public void TestParseMethodDefinitionContainingAttributesOrAnnotations()
		{
			// Attribute
			TestcaseParseMethodDefinition("private void Func([Attribute1] int x)", true,
				Modifier.Private, Type("void"), "Func", List(Arg(Type("int"), "x")));
			TestcaseParseMethodDefinition("[Attribute0(\"X\")] private void Func([Attribute1][Attribute2]int x)", true,
				Modifier.Private, Type("void"), "Func", List(Arg(Type("int"), "x")));
			TestcaseParseMethodDefinition("private void Func( [Attribute1(X)] [Attribute2] [Attribute3(\"Y\")] int x, [Attribute4]int y)", true,
				Modifier.Private, Type("void"), "Func", List(Arg(Type("int"), "x"), Arg(Type("int"), "y")));

			TestcaseParseMethodDefinition("[Attribute0]public Func( [Attribute1(X)] [Attribute2] [Attribute3(\"Y\")] int x, [Attribute4]int y)", true,
				Modifier.Public, null, "Func", List(Arg(Type("int"), "x"), Arg(Type("int"), "y")));

			// Annotation
			TestcaseParseMethodDefinition("private void Func(@Annotation1 int x)", true,
				Modifier.Private, Type("void"), "Func", List(Arg(Type("int"), "x")));
			TestcaseParseMethodDefinition("@Annotation0(\"X\") private void Func(@Annotation1 @Annotation2 int x)", true,
				Modifier.Private, Type("void"), "Func", List(Arg(Type("int"), "x")));
			TestcaseParseMethodDefinition("private void Func(@Annotation1(X) @Annotation2 @Annotation3(\"Y\") int x, @Annotation4 int y)", true,
				Modifier.Private, Type("void"), "Func", List(Arg(Type("int"), "x"), Arg(Type("int"), "y")));

			TestcaseParseMethodDefinition("@Annotation0 public Func(@Annotation1(X) @Annotation2 @Annotation3(\"Y\") int x, @Annotation4 int y)", true,
				Modifier.Public, null, "Func", List(Arg(Type("int"), "x"), Arg(Type("int"), "y")));
		}

		[TestMethod]
		public void TestParseMethodDefinitionWithExplicitDefaultAccessLevel()
		{
			TestcaseParseMethodDefinition("void Func()", true,
				Modifier.Internal, Type("void"), "Func", null, null, Modifier.Internal);
			TestcaseParseMethodDefinition("void Func()", true,
				Modifier.Package, Type("void"), "Func", null, null, Modifier.Package);
			TestcaseParseMethodDefinition("void Func()", true,
				Modifier.Private | Modifier.Protected, Type("void"), "Func", null, null, Modifier.Private | Modifier.Protected);

			TestcaseParseMethodDefinition("void Func()", true,
				Modifier.Public, Type("void"), "Func", null, null, Modifier.Public | Modifier.Abstract);
			TestcaseParseMethodDefinition("void Func()", true,
				Modifier.None, Type("void"), "Func", null, null, Modifier.Abstract | Modifier.Async);
		}

		[TestMethod]
		public void TestParseNotMethod()
		{
			TestcaseParseMethodDefinition("public void", false);
			TestcaseParseMethodDefinition("public void Func", false);
			TestcaseParseMethodDefinition("public object obj = new object()", false);

			TestcaseParseMethodDefinition("private List<int> list = new List<int>()", false);
			TestcaseParseMethodDefinition("private List<String> list = new ArrayList<>()", false);
			TestcaseParseMethodDefinition("private List<String> list = new ArrayList()", false);
		}

		[TestMethod]
		public void TestParseMethodAll()
		{
			TestcaseParseMethodAll(@"public static void Main(string[] args) { Console.WriteLine(""Hello world.""); }", true, 2,
				Modifier.Public | Modifier.Static, Type("void"), "Main", List(Arg(TypeArray("string", 1), "args")));

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
			var parser = new MethodParser(null, Modifier.None);

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
			ClassInfo parserClassInfo = null,
			Modifier defaultAL = Modifier.None)
		{
			var reader = ReaderFromCode(code);

			new MethodParser(parserClassInfo, defaultAL).TryParse(reader, out var info).Is(isSuccess);

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
			ClassInfo parserClassInfo = null,
			Modifier defaultAL = Modifier.None)
		{
			TestcaseParseMethodAll(code, isSuccess, 1, mod, returnType, methodName, argTypes, parserClassInfo, defaultAL);
		}
	}
}
