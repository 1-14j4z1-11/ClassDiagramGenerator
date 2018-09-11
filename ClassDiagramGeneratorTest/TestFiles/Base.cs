using System;

namespace CSharp.Testcase1
{
	public class Base<T> where T : class
	{
		private readonly T value;

		public Base(T value)
		{
			this.value = value;
		}

		public T Value
		{
			get => this.value;
		}

		protected static int Y { get; set; }

		public string Method1(T x) { return null; }
	}

	interface IInterface
	{
		string Method2();
	}

	class Derived : Base<X>, IInterface
	{
		public Derived(X value) : base(value)
		{ }

		public string Method2() { return null; }

		internal virtual object Method3(int x, int y) { return null; }
	}

	internal class X
	{
		public EnumValues Value { get; set; }
	}

	[Flags]
	enum EnumValues
	{
		A = 1,
		B,
		C = 4,
		D = 8,
	}
}
