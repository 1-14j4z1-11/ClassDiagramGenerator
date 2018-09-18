//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Linq;

using ClassDiagramGenerator.Models.Structure;
using static ClassDiagramGenerator.Models.Diagram.RelationType;

namespace ClassDiagramGenerator.Models.Diagram
{
	/// <summary>
	/// Enum of relation type.
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
		/// <param name="class1"><see cref="ClassInfo"/> 1</param>
		/// <param name="class2"><see cref="ClassInfo"/> 2</param>
		/// <param name="type"><see cref="RelationType"/> from <see cref="class1"/> to <see cref="class2"/></param>
		public Relation(ClassInfo class1, ClassInfo class2, RelationType type)
		{
			this.Class1 = class1 ?? throw new ArgumentNullException();
			this.Class2 = class2 ?? throw new ArgumentNullException();
			this.Type = type;
		}

		/// <summary>
		/// Gets a <see cref="ClassInfo"/>1, which is a relation source.
		/// </summary>
		public ClassInfo Class1 { get; }

		/// <summary>
		/// Gets a <see cref="ClassInfo"/>2, which is a relation destination.
		/// </summary>
		public ClassInfo Class2 { get; }

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
			
			return relations.Where(r => r.Class1.Equals(this.Class1) && r.Class2.Equals(this.Class2))
				.Where(r => RedundantMap[this.Type].Contains(r.Type));
		}

		public override bool Equals(object obj)
		{
			if(!(obj is Relation other))
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
	}
}
