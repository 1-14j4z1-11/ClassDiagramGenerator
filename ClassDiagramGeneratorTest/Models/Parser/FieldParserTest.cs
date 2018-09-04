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
	public class FieldParserTest
	{
		[TestMethod]
		public void TestParseFieldDefinition()
		{
			TestcaseParseFieldDefinition("public int X", true,
				Modifier.Public, Type("int"), "X", PropertyType.None);
			TestcaseParseFieldDefinition("public static string Y", true,
				Modifier.Public | Modifier.Static, Type("string"), "Y", PropertyType.None);
			TestcaseParseFieldDefinition("internal string Str", true,
				Modifier.Internal, Type("string"), "Str", PropertyType.None);
			TestcaseParseFieldDefinition("string this[int i]", true,
				Modifier.None, Type("string"), "this", PropertyType.Indexer, List(Arg(Type("int"), "i")));
			TestcaseParseFieldDefinition("private protected abstract IEnumerable<string> this [ int i , string j ] ", true,
				Modifier.Private | Modifier.Protected | Modifier.Abstract, Type("IEnumerable", Type("string")), "this", PropertyType.Indexer, List(Arg(Type("int"), "i"), Arg(Type("string"), "j")));
			TestcaseParseFieldDefinition("protected internal event Action EventHandler", true,
				Modifier.Protected | Modifier.Internal | Modifier.Event, Type("Action"), "EventHandler", PropertyType.None);
			TestcaseParseFieldDefinition("[Attribute] public int X", true,
				Modifier.Public, Type("int"), "X", PropertyType.None);
			TestcaseParseFieldDefinition("[Attribute1][Attribute2] [Attribute3] public int this[int x]", true,
				Modifier.Public, Type("int"), "this", PropertyType.Indexer, List(Arg(Type("int"), "x")));
			TestcaseParseFieldDefinition("@Annotation public int X", true,
				Modifier.Public, Type("int"), "X", PropertyType.None);
			TestcaseParseFieldDefinition("@Annotation @Annotation(0) @Annotation( 1, 2 ) public int X", true,
				Modifier.Public, Type("int"), "X", PropertyType.None);
			TestcaseParseFieldDefinition("private List<int> list = new List<int>()", true,
				Modifier.Private, Type("List", Type("int")), "list");
			TestcaseParseFieldDefinition("private List<String> list = new ArrayList<>()", true,
				Modifier.Private, Type("List", Type("String")), "list");
			TestcaseParseFieldDefinition("private List<String> list = new ArrayList()", true,
				Modifier.Private, Type("List", Type("String")), "list");
			TestcaseParseFieldDefinition("private string[] array = new [] ", true,
				Modifier.Private, TypeArray("string"), "array");
			TestcaseParseFieldDefinition("private String[] array = new String[] ", true,
				Modifier.Private, TypeArray("String"), "array");

			TestcaseParseFieldDefinition("public int", false);
			TestcaseParseFieldDefinition("public static int", false);
		}

		[TestMethod]
		public void TestParseFieldAll()
		{
			// Field
			TestcaseParseFieldAll("int field;", true, 1,
				Modifier.None, Type("int"), "field", PropertyType.None);
			TestcaseParseFieldAll("int field = 0;", true, 1,
				Modifier.None, Type("int"), "field", PropertyType.None);
			TestcaseParseFieldAll("int[] field = new [] { 1, 2 };", true, 2,
				Modifier.None, TypeArray("int"), "field", PropertyType.None);

			// Auto property
			TestcaseParseFieldAll("int Property{get;}", true, 2,
				Modifier.None, Type("int"), "Property", PropertyType.Get);
			TestcaseParseFieldAll("int Property { set; }", true, 2,
				Modifier.None, Type("int"), "Property", PropertyType.Set);
			TestcaseParseFieldAll("int Property { get; set; }", true, 3,
				Modifier.None, Type("int"), "Property", PropertyType.Get | PropertyType.Set);
			TestcaseParseFieldAll("int Property{set;private get;}", true, 3,
				Modifier.None, Type("int"), "Property", PropertyType.Get | PropertyType.Set);
			TestcaseParseFieldAll("int Property { get; protected internal set; }", true, 3,
				Modifier.None, Type("int"), "Property", PropertyType.Get | PropertyType.Set);

			// Implemented property
			TestcaseParseFieldAll("int Property{get => 0;}", true, 2,
				Modifier.None, Type("int"), "Property", PropertyType.Get);
			TestcaseParseFieldAll("int Property { get => x; public set => x = value; }", true, 3,
				Modifier.None, Type("int"), "Property", PropertyType.Get | PropertyType.Set);
			TestcaseParseFieldAll("int Property { get { return 0; } }", true, 3,
				Modifier.None, Type("int"), "Property", PropertyType.Get);
			TestcaseParseFieldAll("int Property { get { return x; } internal set { x = value; } }", true, 5,
				Modifier.None, Type("int"), "Property", PropertyType.Get | PropertyType.Set);

			// Indexer
			TestcaseParseFieldAll("int this[string x]{get => values[x];}", true, 2,
				Modifier.None, Type("int"), "this", PropertyType.Get | PropertyType.Indexer, List(Arg(Type("string"), "x")));
			TestcaseParseFieldAll("int this [ string x ] { get => values[x]; public set => values[x] = value; }", true, 3,
				Modifier.None, Type("int"), "this", PropertyType.Get | PropertyType.Set | PropertyType.Indexer, List(Arg(Type("string"), "x")));
			TestcaseParseFieldAll("string this[int x,int y]{get{return values[x][y];}}", true, 3,
				Modifier.None, Type("string"), "this", PropertyType.Get | PropertyType.Indexer, List(Arg(Type("int"), "x"), Arg(Type("int"), "y")));
			TestcaseParseFieldAll("string this [ int x, int y ] { get { return values[x][y]; } internal set { values[x][y] = value; } }", true, 5,
				Modifier.None, Type("string"), "this", PropertyType.Get | PropertyType.Set | PropertyType.Indexer, List(Arg(Type("int"), "x"), Arg(Type("int"), "y")));

			// 'get =>' text is contained, but it is a field, not a property
			TestcaseParseFieldAll("Func<int, string>[] field = new Func<int, string>[] { get => null };", true, 2,
				Modifier.None, TypeArray("Func", Type("int"), Type("string")), "field", PropertyType.None);
			TestcaseParseFieldAll("Func<int, string>[] Property { get => null; }", true, 2,
				Modifier.None, TypeArray("Func", Type("int"), Type("string")), "Property", PropertyType.Get);
		}

		[TestMethod]
		public void TestParseFailure()
		{
			var reader = ReaderFromCode(string.Empty);
			var parser = new FieldParser(null);

			parser.TryParse(reader, out var _).IsFalse();
		}

		private static void TestcaseParseFieldAll(string code,
			bool isSuccess,
			int? expectedReadLines = null,
			Modifier? mod = null,
			TypeInfo type = null,
			string fieldName = null,
			PropertyType propertyType = default(PropertyType),
			IEnumerable<ArgumentInfo> indexerArgTypes = null)
		{
			var reader = ReaderFromCode(code);
			new FieldParser(null).TryParse(reader, out var info).Is(isSuccess);

			if(isSuccess)
			{
				info.Modifier.Is(mod.Value);
				info.Name.Is(fieldName);
				info.Type.Is(type);
				info.PropertyType.Is(propertyType);
				info.IndexerArguments.IsCollection(indexerArgTypes ?? Enumerable.Empty<ArgumentInfo>());

				reader.Position.Is(expectedReadLines.Value);
			}
			else
			{
				reader.Position.Is(0);
			}
		}

		private static void TestcaseParseFieldDefinition(string code,
			bool isSuccess,
			Modifier? mod = null,
			TypeInfo type = null,
			string fieldName = null,
			PropertyType propertyType = default(PropertyType),
			IEnumerable<ArgumentInfo> indexerArgTypes = null)
		{
			TestcaseParseFieldAll(code, isSuccess, 1, mod, type, fieldName, propertyType, indexerArgTypes);
		}
	}
}
