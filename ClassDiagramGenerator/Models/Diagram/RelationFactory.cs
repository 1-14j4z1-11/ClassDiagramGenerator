//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGenerator.Models.Diagram
{
	public static class RelationFactory
	{
		/// <summary>
		/// Creates a collection of relations from a collection of classes.
		/// <para>A returned collection contains only relations between classes included in argument <paramref name="classes"/></para>
		/// </summary>
		/// <param name="classes">Collection of classes</param>
		/// <returns>Collection of relations</returns>
		public static IEnumerable<Relation> CreateFromClasses(IEnumerable<ClassInfo> classes)
		{
			var relations = new HashSet<Relation>();
			var allClassNames = classes.Select(c => c.Name).ToList();	// contains only top level classes (not contains inner classes)

			foreach(var cls in classes)
			{
				// Inheritance
				foreach(var inheritedName in cls.InheritedClasses.Select(c => c.Name).Where(allClassNames.Contains))
				{
					var inheritedClass = classes.First(c => c.Name == inheritedName);
					var type = (inheritedClass.Category == ClassCategory.Interface) ? RelationType.Realization : RelationType.Generalization;
					relations.Add(new Relation(cls.Name, inheritedName, type));
				}

				// Inner classes
				GetNestedRelationsRecursively(cls, relations);
				
				// Type used in fields -> Association
				foreach(var type in cls.Fields.Select(f => f.GetRelatedTypeNames()).SelectMany(t => t).Where(allClassNames.Contains))
				{
					relations.Add(new Relation(cls.Name, type, RelationType.Association));
				}

				// Type used in methods -> Dependency
				foreach(var type in cls.Methods.Select(m => m.GetRelatedTypeNames()).SelectMany(t => t).Where(allClassNames.Contains))
				{
					relations.Add(new Relation(cls.Name, type, RelationType.Dependency));
				}
			}

			RemoveRedundantRelations(relations);

			return relations;
		}

		/// <summary>
		/// Gets nested relations recursively.
		/// </summary>
		/// <param name="parentClass">Parent class</param>
		/// <param name="results">A collection to put result relations in</param>
		private static void GetNestedRelationsRecursively(ClassInfo parentClass, ICollection<Relation> results)
		{
			if(!parentClass.InnerClasses.Any())
				return;

			foreach(var innerClass in parentClass.InnerClasses)
			{
				results.Add(new Relation(innerClass.Name, parentClass.Name, RelationType.Nested));
				GetNestedRelationsRecursively(innerClass, results);
			}
		}

		/// <summary>
		/// Removes Redundant ralations.
		/// </summary>
		/// <param name="relations">List of <see cref="Relation"/></param>
		private static void RemoveRedundantRelations(ICollection<Relation> relations)
		{
			var redundants = new HashSet<Relation>();

			foreach(var relation in relations)
			{
				foreach(var redundant in relation.GetRedundantsFrom(relations))
				{
					redundants.Add(redundant);
				}
			}

			foreach(var redundant in redundants)
			{
				relations.Remove(redundant);
			}
		}
	}
}
