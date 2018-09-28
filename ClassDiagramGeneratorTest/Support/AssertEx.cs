//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassDiagramGeneratorTest.Support
{
	public static class AssertEx
	{
		/// <summary>
		/// A test is treated as failure.
		/// </summary>
		/// <param name="message">A message to be output</param>
		public static void Fail(string message = "")
		{
			Assert.Fail(message);
		}
		
		/// <summary>
		/// Checks whether a value is true or not.
		/// </summary>
		/// <param name="value">A value to be checked</param>
		/// <param name="message">A message to be output if <paramref name="value"/> is not a expected value</param>
		public static void IsTrue(this bool value, string message = "")
		{
			Assert.IsTrue(value, message);
		}

		/// <summary>
		/// Checks whether a value is false or not.
		/// </summary>
		/// <param name="value">A value to be checked</param>
		/// <param name="message">A message to be output if <paramref name="value"/> is not a expected value</param>
		public static void IsFalse(this bool value, string message = "")
		{
			Assert.IsFalse(value, message);
		}

		/// <summary>
		/// Checks whether a value is an expected value or not.
		/// </summary>
		/// <typeparam name="T">Type of a checked value</typeparam>
		/// <param name="actual">A value to be checked</param>
		/// <param name="expected">A expected value</param>
		/// <param name="message">A message to be output if <paramref name="actual"/> is not a expected value</param>
		public static void Is<T>(this T actual, T expected, string message = "")
		{
			Assert.AreEqual(expected, actual, message);
		}

		/// <summary>
		/// Checks whether a collection contains expected values or not.
		/// </summary>
		/// <typeparam name="T">Type of items in a collection</typeparam>
		/// <param name="actual">Values to be checked</param>
		/// <param name="expected">Expected values</param>
		/// <param name="message">A message to be output if <paramref name="actual"/> does not contain expected values</param>
		public static void IsCollection<T>(this IEnumerable<T> actual, IEnumerable<T> expected, string message = "")
		{
			CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray(), message);
		}

		/// <summary>
		/// Checks whether a collection contains expected values or not.
		/// <para>This check is treated as success if a collection contains all expected values regardless of order of its items.</para>
		/// </summary>
		/// <typeparam name="T">Type of items in a collection</typeparam>
		/// <param name="actual">Values to be checked</param>
		/// <param name="expected">Expected values</param>
		/// <param name="message">A message to be output if <paramref name="actual"/> does not contain expected values</param>
		public static void IsCollectionUnorderly<T>(this IEnumerable<T> actual, IEnumerable<T> expected, string message = "")
		{
			if((actual == null) || (expected == null))
				throw new ArgumentNullException();

			var restActualItems = actual.ToList();

			foreach(var expItem in expected)
			{
				if(!restActualItems.Contains(expItem))
				{
					Fail($"Expected that {expItem} is included in a collection, but it was not included. "
						+ message);
				}

				restActualItems.Remove(expItem);
			}

			if(restActualItems.Any())
			{
				Fail($"All expected items are included in a collection, but unexpected item(s) is/are included. {restActualItems.ToCollectionLog()}"
					+ message);
			}
		}

		/// <summary>
		/// Creates a collection log text.
		/// </summary>
		/// <typeparam name="T">Type of items in collection</typeparam>
		/// <param name="e"></param>
		/// <returns>A collection log text</returns>
		private static string ToCollectionLog<T>(this IEnumerable<T> e)
		{
			if(!e.Any())
				return "[]";

			var builder = new StringBuilder("[");

			foreach(var v in e)
			{
				if(v is IEnumerable<object> inner)
					builder.Append(inner.ToCollectionLog());
				else
					builder.Append(v);

				builder.Append(",");
			}

			builder[builder.Length - 1] = ']';
			return builder.ToString();
		}
	}
}
