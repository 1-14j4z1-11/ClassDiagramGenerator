﻿using System;
using System.Collections.Generic;

namespace CSharp.Testcase2
{
	[Serializable]
	abstract class SampleClass<T> where T : new()
	{
		[NonSerialized] private int[,] intArray2 = new int[5, 5];
		protected string[][][] strArray3 = new string[5][][];
		internal static readonly T Value = new T();

		protected SampleClass(List<int[,]> x, List<string[][][]> y) { }

		protected internal int[][] X { get; set; }
		
		internal string[,,] Y => null;

		private object this[int x, string y]
		{
			get { return null; }
			set { }
		}

		public List<Dictionary<string[][][], int[,]>> Func1(out int x, params List<object>[] objects)
		{
			x = 0;
			return null;
		}

		public class Inner
		{
			private readonly double x;
			private double y;

			public Inner(double x, double y)
			{
				this.x = x;
				this.y = y;
			}

			public double X { get => this.x; }

			public double Y
			{
				get => this.y;
				set => this.y = value;
			}

			string Func<U, V>(U i, out V o) where V : new()
			{
				o = new V();
				return null;
			}
		}
	}

	public struct Inner
	{
		internal SampleClass<object>.Inner value;

		internal Inner(SampleClass<object>.Inner value)
		{
			this.value = value;
		}
	}

	public static class Extension
	{
		public static string This(this Inner obj) => null;

		internal static void Out(out SampleClass<int>.Inner obj) => obj = null;

		public static void Ref(ref object obj) { }
	}
}
