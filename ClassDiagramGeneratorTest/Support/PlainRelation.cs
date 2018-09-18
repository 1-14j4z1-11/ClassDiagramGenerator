//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using ClassDiagramGenerator.Models.Diagram;

namespace ClassDiagramGeneratorTest.Support
{
	/// <summary>
	/// A simple relation class
	/// </summary>
	public class PlainRelation
	{
		public PlainRelation(string class1, string class2, RelationType type)
		{
			this.Class1 = class1;
			this.Class2 = class2;
			this.Type = type;
		}

		public PlainRelation(Relation relation)
			: this(relation.Class1.Type.ExactName, relation.Class2.Type.ExactName, relation.Type)
		{ }

		public string Class1 { get; }

		public string Class2 { get; }

		public RelationType Type { get; }
		
		public override bool Equals(object obj)
		{
			if(!(obj is PlainRelation other))
				return false;

			return this.Class1.Equals(other.Class1)
				&& this.Class2.Equals(other.Class2)
				&& (this.Type == other.Type);
		}

		public override int GetHashCode()
		{
			return this.Class1.GetHashCode()
				^ this.Class2.GetHashCode()
				^ this.Type.GetHashCode();
		}

		public override string ToString()
		{
			return $"({this.Class1} >> {this.Class2} : {this.Type})";
		}
	}
}
