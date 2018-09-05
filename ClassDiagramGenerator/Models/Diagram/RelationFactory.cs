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
			var classSearcher = new ClassSearcher(classes.SelectMany(c => c.GetAllClasses()));

			foreach(var cls in classes)
			{
				// Inheritance
				foreach(var inheritedType in cls.InheritedClasses)
				{
					var inheritedClass = classSearcher.Search(inheritedType);

					if(inheritedClass == null)
						continue;

					var type = (inheritedClass.Category == ClassCategory.Interface) ? RelationType.Realization : RelationType.Generalization;
					relations.Add(new Relation(cls, inheritedClass, type));
				}

				// Inner classes
				GetNestedRelationsRecursively(cls, relations);
				
				// Types used in fields -> Association
				foreach(var type in cls.Fields.Select(f => f.GetRelatedTypes()).SelectMany(t => t))
				{
					var relatedClass = classSearcher.Search(type);

					if(relatedClass == null)
						continue;

					relations.Add(new Relation(cls, relatedClass, RelationType.Association));
				}

				// Types used in methods -> Dependency
				foreach(var type in cls.Methods.Select(m => m.GetRelatedTypes()).SelectMany(t => t))
				{
					var relatedClass = classSearcher.Search(type);

					if(relatedClass == null)
						continue;

					// Uses a corresponding type name in the ClassInfo collection, instead of using a type name described in the function definition.
					relations.Add(new Relation(cls, relatedClass, RelationType.Dependency));
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
				results.Add(new Relation(innerClass, parentClass, RelationType.Nested));
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

		private class ClassSearcher
		{
			private readonly IEnumerable<ClassInfo> classes;

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="classes">A collection of <see cref="ClassInfo"/></param>
			public ClassSearcher(IEnumerable<ClassInfo> classes)
			{
				this.classes = classes ?? throw new ArgumentNullException();
			}

			/// <summary>
			/// Searches a <see cref="ClassInfo"/> matched with an argument from this instance.
			/// <para>If no matched <see cref="ClassInfo"/> is found, returns null.</para>
			/// </summary>
			/// <param name="type">A target <see cref="TypeInfo"/> to be searched</param>
			/// <returns>A <see cref="ClassInfo"/> matched with an argument</returns>
			public ClassInfo Search(TypeInfo type)
			{
				if(type == null)
					return null;

				foreach(var cls in this.classes)
				{
					if(IsNameMached(type, cls))
						return cls;
				}

				return null;
			}

			/// <summary>
			/// Returns a value whether <paramref name="typeInfo"/> name and <paramref name="classInfo"/> name are the same or not.
			/// </summary>
			/// <param name="typeInfo"><see cref="TypeInfo"/></param>
			/// <param name="classInfo"><see cref="ClassInfo"/></param>
			/// <returns>Whether <paramref name="typeInfo"/> name and <paramref name="classInfo"/> name are the same or not</returns>
			private static bool IsNameMached(TypeInfo typeInfo, ClassInfo classInfo)
			{
				var package = (classInfo.Package != null) ? classInfo.Package + "." : string.Empty;
				var exactName = package + (classInfo.Type?.ExactName ?? string.Empty);
				var clsSegs = exactName.Split('.').Reverse().ToList();
				var argSegs = typeInfo.ExactName.Split('.').Reverse().ToList();
				var length = Math.Min(clsSegs.Count, argSegs.Count);

				// Confirms all segments of class name (included in shorter one) matches other's with reverse order.
				for(var i = 0; i < length; i++)
				{
					if(clsSegs[i] != argSegs[i])
						return false;
				}

				return true;
			}
		}
	}
}
