﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestSupport.MSTest;

using ClassDiagramGenerator.Models.Parser;
using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGeneratorTest.Models.Parser
{
	[TestClass]
	public class FieldParserTest : ComponentParserTestBase
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

			TestcaseParseFieldDefinition("public int", false);
			TestcaseParseFieldDefinition("public static int", false);
		}

		[TestMethod]
		public void TestParseFieldWithCode1()
		{
			TestcaseParseFieldAll(SampleCode1, 2, true, 1, Modifier.Private | Modifier.Static | Modifier.Readonly, Type("string"), "logText");
			TestcaseParseFieldAll(SampleCode1, 8, true, 2, Modifier.Public | Modifier.Static, Type("string"), "LogText");
			
			foreach(var i in new[] { 7, 11 })
			{
				TestcaseParseFieldAll(SampleCode1, i, false);
			}
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
			new FieldParser().TryParse(reader, out var info).Is(isSuccess);

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
