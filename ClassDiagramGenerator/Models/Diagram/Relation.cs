//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static ClassDiagramGenerator.Models.Diagram.RelationType;

namespace ClassDiagramGenerator.Models.Diagram
{
	/// <summary>
	/// Specifies the relation type.
	/// </summary>
	public enum RelationType
	{
		Dependency,
		
		Association,
		
		Aggregation,
		
		Composition,
		
		Generalization,
		
		Realization,
		
		Nested,
	}

	/// <summary>
	/// Class that indicates relation between two classes.
	/// </summary>
	public class Relation
	{
		// Dictionary containing weaker RelationType than key.
		private static readonly Dictionary<RelationType, List<RelationType>> RedundantMap = new Dictionary<RelationType, List<RelationType>>()
		{
			[Composition] = new List<RelationType>() { Aggregation, Association, Dependency },
			[Aggregation] = new List<RelationType>() { Association, Dependency },
			[Association] = new List<RelationType>() { Dependency },
			[Dependency] = new List<RelationType>(),
			[Generalization] = new List<RelationType>(),
			[Realization] = new List<RelationType>(),
			[Nested] = new List<RelationType>(),
		};

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="class1">Class name 1</param>
		/// <param name="class2">Class name 2</param>
		/// <param name="type"><see cref="RelationType"/> from <see cref="class1"/> to <see cref="class2"/></param>
		public Relation(string class1, string class2, RelationType type)
		{
			this.Class1 = class1 ?? throw new ArgumentNullException();
			this.Class2 = class2 ?? throw new ArgumentNullException();
			this.Type = type;
		}

		/// <summary>
		/// Gets a Class name 1.
		/// </summary>
		public string Class1 { get; }

		/// <summary>
		/// Gets a Class name 2.
		/// </summary>
		public string Class2 { get; }

		/// <summary>
		/// Gets a <see cref="RelationType"/> from <see cref="class1"/> to <see cref="class2"/>.
		/// </summary>
		public RelationType Type { get; }

		/// <summary>
		/// Gets a collection of <see cref="Relation"/>[s]
		/// that are redundant (weaker than this relation) from <paramref name="relations"/>.
		/// </summary>
		/// <param name="relations">Collection of <see cref="Relation"/></param>
		/// <returns>Collection of redundant <see cref="Relation"/></returns>
		/// <exception cref="ArgumentNullException">If argument is null.</exception>
		public IEnumerable<Relation> GetRedundantsFrom(IEnumerable<Relation> relations)
		{
			if(relations == null)
				throw new ArgumentNullException();
			
			return relations.Where(r => (r.Class1 == this.Class1) && (r.Class2 == this.Class2))
				.Where(r => RedundantMap[this.Type].Contains(r.Type));
		}

		public override bool Equals(object obj)
		{
			if(!(obj is Relation other))
				return false;

			return (this.Class1 == other.Class1)
				&& (this.Class2 == other.Class2)
				&& (this.Type == other.Type);
		}

		public override int GetHashCode()
		{
			return this.Class1.GetHashCode()
				^ this.Class2.GetHashCode()
				^ this.Type.GetHashCode();
		}
	}
}
