package java.testcase1;

public class Base<T extends Object>
{
	private static int Y;
	private final T value;

	public Base(T value)
	{
		this.value = value;
	}

	public T getValue()
	{
		return this.value;
	}

	protected static int getY()
	{
		return Y;
	}

	protected static void setY(int y)
	{
		Y = y;
	}

	public String Method1(T x) { return null; }
}

interface IInterface
{
	String Method2();
}

class Derived extends Base<X> implements IInterface
{
	public Derived(X value)
	{
		super(value);
	}

	public String Method2() { return null; }
}

class X
{
	private EnumValues value;
	
	public EnumValues getValue() { return this.value; }
	public void setValue(EnumValues value) { this.value = value; }
}

enum EnumValues
{
	A(1),
	B(2),
	C(4),
	D(8);
	
	public final int value;
	
	private EnumValues(int value)
	{
		this.value = value;
	}
}