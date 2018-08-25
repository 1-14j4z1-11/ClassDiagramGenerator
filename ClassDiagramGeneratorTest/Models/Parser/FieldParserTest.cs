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
				Modifier.Public, Type("int"), "X");
			TestcaseParseFieldDefinition("public static string Y", true,
				Modifier.Public | Modifier.Static, Type("string"), "Y");
			TestcaseParseFieldDefinition("internal string Str", true,
				Modifier.Internal, Type("string"), "Str");
			TestcaseParseFieldDefinition("string this[int i]", true,
				Modifier.None, Type("string"), "this", List(Arg(Type("int"), "i")));
			TestcaseParseFieldDefinition("private protected abstract IEnumerable<string> this [ int i , string j ] ", true,
				Modifier.Private | Modifier.Protected | Modifier.Abstract, Type("IEnumerable", Type("string")), "this", List(Arg(Type("int"), "i"), Arg(Type("string"), "j")));
			TestcaseParseFieldDefinition("protected internal event Action EventHandler", true,
				Modifier.Protected | Modifier.Internal | Modifier.Event, Type("Action"), "EventHandler");
			TestcaseParseFieldDefinition("[Attribute] public int X", true,
				Modifier.Public, Type("int"), "X");
			TestcaseParseFieldDefinition("[Attribute1][Attribute2] [Attribute3] public int X", true,
				Modifier.Public, Type("int"), "X");
			TestcaseParseFieldDefinition("@Annotation public int X", true,
				Modifier.Public, Type("int"), "X");
			TestcaseParseFieldDefinition("@Annotation @Annotation(0) @Annotation( 1, 2 ) public int X", true,
				Modifier.Public, Type("int"), "X");

			TestcaseParseFieldDefinition("public int", false);
			TestcaseParseFieldDefinition("public static int", false);
		}

		[TestMethod]
		public void TestParseWithCSCode1()
		{
			TestcaseParseFieldAll(CSharpCode1, 3, true, 1, Modifier.Private | Modifier.Static | Modifier.Readonly, Type("string"), "logText");
			TestcaseParseFieldAll(CSharpCode1, 9, true, 2, Modifier.Public | Modifier.Static, Type("string"), "LogText");
			
			foreach(var i in new[] { 8, 12 })
			{
				TestcaseParseFieldAll(CSharpCode1, i, false);
			}
		}

		[TestMethod]
		public void TestParseFailure()
		{
			var reader = ReaderFromCode(string.Empty);
			var parser = new FieldParser(null);

			parser.TryParse(reader, out var _).IsFalse();
		}

		private static void TestcaseParseFieldAll(string code,
			int startPos,
			bool isSuccess,
			int? expectedReadLines = null,
			Modifier? mod = null,
			TypeInfo type = null,
			string propertyName = null,
			IEnumerable<ArgumentInfo> indexerArgTypes = null)
		{
			var reader = ReaderFromCode(code);
			Enumerable.Range(0, startPos).ToList().ForEach(_ => reader.TryRead(out var _));
			new FieldParser(null).TryParse(reader, out var info).Is(isSuccess);

			if(isSuccess)
			{
				info.Modifier.Is(mod.Value);
				info.Name.Is(propertyName);
				info.Type.Is(type);
				info.IndexerArguments.IsCollection(indexerArgTypes ?? Enumerable.Empty<ArgumentInfo>());

				reader.Position.Is(startPos + expectedReadLines.Value);
			}
			else
			{
				reader.Position.Is(startPos);
			}
		}

		private static void TestcaseParseFieldDefinition(string code,
			bool isSuccess,
			Modifier? mod = null,
			TypeInfo type = null,
			string propertyName = null,
			IEnumerable<ArgumentInfo> indexerArgTypes = null)
		{
			TestcaseParseFieldAll(code, 0, isSuccess, 1, mod, type, propertyName, indexerArgTypes);
		}
	}
}
