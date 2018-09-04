﻿using System;
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
	public class ClassParserTest
	{
		[TestMethod]
		public void TestParseClassDefinition()
		{
			TestcaseParseClassDefinition("public class TestClass", true,
				Modifier.Public, ClassCategory.Class, Type("TestClass"));
			TestcaseParseClassDefinition("internal class Generic1<T>", true,
				Modifier.Internal, ClassCategory.Class, Type("Generic1", Type("T")));
			TestcaseParseClassDefinition("class Generic2<List<T1>,T2>", true,
				Modifier.None, ClassCategory.Class, Type("Generic2", Type("List", Type("T1")), Type("T2")));
			TestcaseParseClassDefinition("protected interface Generic3< T1, T2, T3 >", true,
				Modifier.Protected, ClassCategory.Interface, Type("Generic3", Type("T1"), Type("T2"), Type("T3")));
			TestcaseParseClassDefinition("private struct Derived1 : IDisposable", true,
				Modifier.Private, ClassCategory.Struct, Type("Derived1"), List(Type("IDisposable")));
			TestcaseParseClassDefinition("protected internal interface Derived2 : IDisposable, IComparable<Derived2>", true,
				Modifier.Protected | Modifier.Internal, ClassCategory.Interface, Type("Derived2"), List(Type("IDisposable"), Type("IComparable", Type("Derived2"))));
			TestcaseParseClassDefinition("public class Derived3:Dictionary<string,int>,IDisposable,IComparable<Derived3>", true,
				Modifier.Public, ClassCategory.Class, Type("Derived3"), List(Type("Dictionary", Type("string"), Type("int")), Type("IDisposable"), Type("IComparable", Type("Derived3"))));
			TestcaseParseClassDefinition("public static class StaticClass", true,
				Modifier.Public | Modifier.Static, ClassCategory.Class, Type("StaticClass"));
			TestcaseParseClassDefinition("private enum EnumValues", true,
				Modifier.Private, ClassCategory.Enum, Type("EnumValues"));
			TestcaseParseClassDefinition("private protected abstract class Where < T > : IDisposable where T : IDisposable", true,
				Modifier.Private | Modifier.Protected | Modifier.Abstract, ClassCategory.Class, Type("Where", Type("T")), List(Type("IDisposable")));
			TestcaseParseClassDefinition("class class", true,
				Modifier.None, ClassCategory.Class, Type("class"));     // OK, does not reject class name of reservation word.
			TestcaseParseClassDefinition("interface IF : ", true,
				Modifier.None, ClassCategory.Interface, Type("IF"));    // OK, extra text after class definition is allowed.

			// Inheritance of Java
			TestcaseParseClassDefinition("private struct Derived1 implements IDisposable", true,
				Modifier.Private, ClassCategory.Struct, Type("Derived1"), List(Type("IDisposable")));
			TestcaseParseClassDefinition("protected internal interface Derived2 extends IDisposable, IComparable<Derived2>", true,
				Modifier.Protected | Modifier.Internal, ClassCategory.Interface, Type("Derived2"), List(Type("IDisposable"), Type("IComparable", Type("Derived2"))));
			TestcaseParseClassDefinition("public class Derived3 extends Dictionary<string,int> implements IDisposable,IComparable<Derived3>", true,
				Modifier.Public, ClassCategory.Class, Type("Derived3"), List(Type("Dictionary", Type("string"), Type("int")), Type("IDisposable"), Type("IComparable", Type("Derived3"))));
			TestcaseParseClassDefinition("class Derived4 implements Closable extends HashMap < String , Integer >", true,
				Modifier.None, ClassCategory.Class, Type("Derived4"), List(Type("Closable"), Type("HashMap", Type("String"), Type("Integer"))));
			TestcaseParseClassDefinition("class Derived5 extends HashMap< String , Integer> implements Closable", true,
				Modifier.None, ClassCategory.Class, Type("Derived5"), List(Type("HashMap", Type("String"), Type("Integer")), Type("Closable")));

			TestcaseParseClassDefinition("public static TestClass", false);
			TestcaseParseClassDefinition("public TestClass", false);
			TestcaseParseClassDefinition("public static class ", false);
			TestcaseParseClassDefinition("public class : IDisposable", false);
			TestcaseParseClassDefinition("interface : IDisposable", false);
		}

		[TestMethod]
		public void TestParseWithCSCode1()
		{
			var code = @"
using System;

namespace Sample1
{
	public static class MainClass
	{
		private static readonly string logText = ""Output"";
	
		public static void Main(string args)
		{
			for(var i = 0; i < 10; i++)
			{
				Output(i);
			}
		}
	
		public static string LogText { get => logText; } 
	
		private static int Output ( int x )
		{
			Console.WriteLine(LogText + x);
			return x;
		}
	}
}
";

			TestcaseParseClassAll(code, 2, true, 12, Modifier.Public | Modifier.Static, ClassCategory.Class, Type("MainClass"),
				Enumerable.Empty<TypeInfo>(),
				List(new FieldInfo(Modifier.Private | Modifier.Static | Modifier.Readonly, "logText", Type("string")),
					new FieldInfo(Modifier.Public | Modifier.Static, "LogText", Type("string"))),
				List(new MethodInfo(Modifier.Public | Modifier.Static, "Main", Type("void"), List(Arg(Type("string"), "args"))),
					new MethodInfo(Modifier.Private | Modifier.Static, "Output", Type("int"), List(Arg(Type("int"), "x")))
				));
		}
		
		private static void TestcaseParseClassAll(string code,
			int startPos,
			bool isSuccess,
			int? expectedReadLines = null,
			Modifier? mod = null,
			ClassCategory category = default(ClassCategory),
			TypeInfo type = null,
			IEnumerable<TypeInfo> inheriteds = null,
			IEnumerable<FieldInfo> fields = null,
			IEnumerable<MethodInfo> methods = null,
			IEnumerable<string> innerClassNames = null)
		{
			var reader = ReaderFromCode(code);
			Enumerable.Range(0, startPos).ToList().ForEach(_ => reader.TryRead(out var _));

			new ClassParser("test-namespace").TryParse(reader, out var info).Is(isSuccess);

			if(isSuccess)
			{
				info.Modifier.Is(mod.Value);
				info.Category.Is(category);
				info.Type.Is(type);
				info.InheritedClasses.IsCollection(inheriteds ?? Enumerable.Empty<TypeInfo>());
				info.Fields.IsCollection(fields ?? Enumerable.Empty<FieldInfo>());
				info.Methods.IsCollection(methods ?? Enumerable.Empty<MethodInfo>());
				info.InnerClasses.Select(ic => ic.Name).IsCollection(innerClassNames ?? Enumerable.Empty<string>());
			}
			else
			{
				reader.Position.Is(startPos);
			}
		}

		private static void TestcaseParseClassDefinition(string code,
			bool isSuccess,
			Modifier? mod = null,
			ClassCategory category = default(ClassCategory),
			TypeInfo type = null,
			IEnumerable<TypeInfo> inheriteds = null)
		{
			TestcaseParseClassAll(code, 0, isSuccess, 1, mod, category, type, inheriteds);
		}
	}
}
