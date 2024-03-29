﻿//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ClassDiagramGenerator.Cui;
using ClassDiagramGeneratorTest.Support;

namespace ClassDiagramGeneratorTest.Cui
{
	[TestClass]
	public class CmdParserTest
	{
		[TestMethod]
		public void TestParse()
		{
			var emptyWords = Enumerable.Empty<string>();

			var flag0 = new CmdFlag(true, 0, "-0");
			var flag1 = new CmdFlag(true, 1, "-1");
			var flag2 = new CmdFlag(true, 2, "-2", "-22");
			var flag3 = new CmdFlag(false, 1, "-3");
			var parser = new CmdParser()
				.AddFlag(flag0, "0", emptyWords)
				.AddFlag(flag1, "1", emptyWords)
				.AddFlag(flag2, "2", emptyWords)
				.AddFlag(flag3, "3", emptyWords);

			{
				var expected = new Dictionary<CmdFlag, IEnumerable<string>>()
				{
					[flag0] = new string[0],
					[flag1] = new[] { "A" },
					[flag2] = new[] { "B", "C" },
				};

				parser.TryParse("-0 -1 A -2 B C".Split(' '), out var map1).IsTrue();
				IsExpectedArgMap(map1, expected);

				parser.TryParse("-2 B C -1 A -0".Split(' '), out var map2).IsTrue();
				IsExpectedArgMap(map2, expected);

				// It's OK for the same flag to appear twice, returned map contains value appearing later.
				parser.TryParse("-0 -1 B -1 A -2 B C".Split(' '), out var map3).IsTrue();
				IsExpectedArgMap(map3, expected);
			}
			{
				parser.TryParse("-0 -1 A -22 B C -3 D".Split(' '), out var map).IsTrue();
				IsExpectedArgMap(map, new Dictionary<CmdFlag, IEnumerable<string>>()
				{
					[flag0] = new string[0],
					[flag1] = new[] { "A" },
					[flag2] = new[] { "B", "C" },
					[flag3] = new[] { "D" },
				});
			}
			{
				parser.TryParse("-0 -1 A -2 A B -3 A -4 A".Split(' '), out var _).IsFalse();
				parser.TryParse("-0 -1 A -2 A B C -3 A".Split(' '), out var _).IsFalse();
				parser.TryParse("-1 A -2 A B -3 A".Split(' '), out var _).IsFalse();
			}
		}
		
		private static void IsExpectedArgMap(Dictionary<CmdFlag, List<string>> actualMap, Dictionary<CmdFlag, IEnumerable<string>> expectedMap)
		{
			// Since nested collection cannot be confirmed, value strings is flatted to a string.
			var flatActual = actualMap
				.Select(kv => new KeyValuePair<CmdFlag, string>(kv.Key, string.Join(",", kv.Value)))
				.ToDictionary(kv => kv.Key, kv => kv.Value);
			var flatExpected = expectedMap
				.Select(kv => new KeyValuePair<CmdFlag, string>(kv.Key, string.Join(",", kv.Value)))
				.ToDictionary(kv => kv.Key, kv => kv.Value);

			flatActual.IsCollectionUnorderly(flatExpected);
		}
	}
}
