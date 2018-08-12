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
		/// </summary>
		/// <param name="classes">Collection of classes</param>
		/// <returns>Collection of relations</returns>
		public static IEnumerable<Relation> CreateFromClasses(IEnumerable<ClassInfo> classes)
		{
			var relations = new HashSet<Relation>();
			var classNames = classes.Select(c => c.Name).ToList();

			foreach(var cls in classes)
			{
				// Inheritance
				foreach(var inheritedName in cls.InheritedClasses.Select(c => c.Name).Where(classNames.Contains))
				{
					var inheritedClass = classes.First(c => c.Name == inheritedName);
					var type = (inheritedClass.Category == ClassCategory.Interface) ? RelationType.Realization : RelationType.Generalization;
					relations.Add(new Relation(cls.Name, inheritedName, type));
				}

				// Inner classes
				foreach(var innerClass in cls.InnerClasses.Select(c => c.Name).Where(classNames.Contains))
				{
					// TODO Add Nested relations 'recursively'
					relations.Add(new Relation(cls.Name, innerClass, RelationType.Nested));
				}

				// Type used in fields -> Association
				foreach(var type in cls.Fields.Select(f => f.GetRelatedTypeNames()).SelectMany(t => t).Where(classNames.Contains))
				{
					relations.Add(new Relation(cls.Name, type, RelationType.Association));
				}

				// Type used in methods -> Dependency
				foreach(var type in cls.Methods.Select(m => m.GetRelatedTypeNames()).SelectMany(t => t).Where(classNames.Contains))
				{
					relations.Add(new Relation(cls.Name, type, RelationType.Dependency));
				}
			}

			RemoveRedundantRelations(relations);

			return relations;
		}

		/// <summary>
		/// Removes Redundant ralations.
		/// </summary>
		/// <param name="relations">List of <see cref="Relation"/></param>
		private static void RemoveRedundantRelations(HashSet<Relation> relations)
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
