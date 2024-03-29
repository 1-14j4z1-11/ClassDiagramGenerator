﻿//
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
			
			TestcaseParseFieldDefinition("internal Outer.Inner value", true,
				Modifier.Internal, Type("Outer.Inner"), "value", PropertyType.None);
			TestcaseParseFieldDefinition("internal Outer<int>.Inner value", true,
				Modifier.Internal, Type("Outer.Inner"), "value", PropertyType.None);
			TestcaseParseFieldDefinition("internal Outer<int>.Inner<int> value", true,
				Modifier.Internal, Type("Outer.Inner", Type("int")), "value", PropertyType.None);
		}

		[TestMethod]
		public void TestParseFieldDefinitionContainingAttributesOrAnnotations()
		{
			TestcaseParseFieldDefinition("[Attribute] public int X", true,
				Modifier.Public, Type("int"), "X", PropertyType.None);
			TestcaseParseFieldDefinition("[Attribute1][Attribute2] [Attribute3] public int this[int x]", true,
				Modifier.Public, Type("int"), "this", PropertyType.Indexer, List(Arg(Type("int"), "x")));
			TestcaseParseFieldDefinition("@Annotation public int X", true,
				Modifier.Public, Type("int"), "X", PropertyType.None);
			TestcaseParseFieldDefinition("@Annotation @Annotation(0) @Annotation( 1, 2 ) public int X", true,
				Modifier.Public, Type("int"), "X", PropertyType.None);
		}

		[TestMethod]
		public void TestParseGenericFieldDefinition()
		{
			TestcaseParseFieldDefinition("private List<int> list = new List<int>()", true,
				Modifier.Private, Type("List", Type("int")), "list");
			TestcaseParseFieldDefinition("private List<String> list = new ArrayList<>()", true,
				Modifier.Private, Type("List", Type("String")), "list");
			TestcaseParseFieldDefinition("private List<String> list = new ArrayList()", true,
				Modifier.Private, Type("List", Type("String")), "list");

			TestcaseParseFieldDefinition("private List<String[]> list = null", true,
				Modifier.Private, Type("List", TypeArray("String", 1)), "list");
		}

		[TestMethod]
		public void TestParseArrayFieldDefinition()
		{
			TestcaseParseFieldDefinition("private string[] array = new [] ", true,
				Modifier.Private, TypeArray("string", 1), "array");
			TestcaseParseFieldDefinition("private String[] array = new String[]", true,
				Modifier.Private, TypeArray("String", 1), "array");
			TestcaseParseFieldDefinition("private String[][] array = new String[5][7]", true,
				Modifier.Private, TypeArray("String", 2), "array");

			TestcaseParseFieldDefinition("private List<string>[] array = null", true,
				Modifier.Private, TypeArray("List", 1, Type("string")), "array");
			TestcaseParseFieldDefinition("private List<string>[][][] array = null", true,
				Modifier.Private, TypeArray("List", 3, Type("string")), "array");

			TestcaseParseFieldDefinition("private string[,] array = new string[5, 5]", true,
				Modifier.Private, TypeArray("string", 2), "array");
		}
		
		[TestMethod]
		public void TestParseNotField()
		{
			TestcaseParseFieldDefinition("public int", false);
			TestcaseParseFieldDefinition("public static int", false);
		}

		[TestMethod]
		public void TestParseFieldAll()
		{
			TestcaseParseFieldAll("int field;", true, 1,
				Modifier.None, Type("int"), "field", PropertyType.None);
			TestcaseParseFieldAll("int field = 0;", true, 1,
				Modifier.None, Type("int"), "field", PropertyType.None);
			TestcaseParseFieldAll("int[] field = new [] { 1, 2 };", true, 2,
				Modifier.None, TypeArray("int", 1), "field", PropertyType.None);
		}

		[TestMethod]
		public void TestParseAutoPropertyAll()
		{
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

			TestcaseParseFieldAll("int Property => 0", true, 1,
				Modifier.None, Type("int"), "Property", PropertyType.Get);
		}

		[TestMethod]
		public void TestParseImplementedPropertyAll()
		{
			TestcaseParseFieldAll("int Property{get => 0;}", true, 2,
				Modifier.None, Type("int"), "Property", PropertyType.Get);
			TestcaseParseFieldAll("int Property { get => x; public set => x = value; }", true, 3,
				Modifier.None, Type("int"), "Property", PropertyType.Get | PropertyType.Set);
			TestcaseParseFieldAll("int Property { get { return 0; } }", true, 3,
				Modifier.None, Type("int"), "Property", PropertyType.Get);
			TestcaseParseFieldAll("int Property { get { return x; } internal set { x = value; } }", true, 5,
				Modifier.None, Type("int"), "Property", PropertyType.Get | PropertyType.Set);
		}

		[TestMethod]
		public void TestParseIndexerAll()
		{
			TestcaseParseFieldAll("int this[string x]{get => values[x];}", true, 2,
				Modifier.None, Type("int"), "this", PropertyType.Get | PropertyType.Indexer, List(Arg(Type("string"), "x")));
			TestcaseParseFieldAll("int this [ string x ] { get => values[x]; public set => values[x] = value; }", true, 3,
				Modifier.None, Type("int"), "this", PropertyType.Get | PropertyType.Set | PropertyType.Indexer, List(Arg(Type("string"), "x")));
			TestcaseParseFieldAll("string this[int x,int y]{get{return values[x][y];}}", true, 3,
				Modifier.None, Type("string"), "this", PropertyType.Get | PropertyType.Indexer, List(Arg(Type("int"), "x"), Arg(Type("int"), "y")));
			TestcaseParseFieldAll("string this [ int x, int y ] { get { return values[x][y]; } internal set { values[x][y] = value; } }", true, 5,
				Modifier.None, Type("string"), "this", PropertyType.Get | PropertyType.Set | PropertyType.Indexer, List(Arg(Type("int"), "x"), Arg(Type("int"), "y")));

			TestcaseParseFieldAll("int this[string x] => values[x]", true, 1,
				Modifier.None, Type("int"), "this", PropertyType.Get | PropertyType.Indexer, List(Arg(Type("string"), "x")));
		}

		[TestMethod]
		public void TestParseComplicatedCase()
		{
			// 'get =>' text is contained, but it is a field, not a property
			TestcaseParseFieldAll("Func<int, string>[] field = new Func<int, string>[] { get => null };", true, 2,
				Modifier.None, TypeArray("Func", 1, Type("int"), Type("string")), "field", PropertyType.None);

			// This is a property
			TestcaseParseFieldAll("Func<int, string>[] Property { get => null; }", true, 2,
				Modifier.None, TypeArray("Func", 1, Type("int"), Type("string")), "Property", PropertyType.Get);
		}

		[TestMethod]
		public void TestParseFailure()
		{
			var reader = ReaderFromCode(string.Empty);
			var parser = new FieldParser(null, Modifier.None);

			parser.TryParse(reader, out var _).IsFalse();
		}

		private static void TestcaseParseFieldAll(string code,
			bool isSuccess,
			int? expectedReadLines = null,
			Modifier? mod = null,
			TypeInfo type = null,
			string fieldName = null,
			PropertyType propertyType = default(PropertyType),
			IEnumerable<ArgumentInfo> indexerArgTypes = null,
			Modifier defaultAL = Modifier.None)
		{
			var reader = ReaderFromCode(code);
			new FieldParser(null, defaultAL).TryParse(reader, out var info).Is(isSuccess);

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
