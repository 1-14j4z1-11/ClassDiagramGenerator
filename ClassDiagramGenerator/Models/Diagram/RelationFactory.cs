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
			var allClassNames = classes.SelectMany(c => c.GetAllClasses().Select(c0 => new NameInfo(c0))).ToList();

			foreach(var cls in classes)
			{
				// Inheritance
				foreach(var inheritedName in cls.InheritedClasses.Select(c => c.Name))
				{
					var inheritedInfo = SearchClassName(inheritedName, allClassNames);

					if(inheritedInfo == null)
						continue;

					var type = (inheritedInfo.ClassInfo.Category == ClassCategory.Interface) ? RelationType.Realization : RelationType.Generalization;
					relations.Add(new Relation(cls.Name, inheritedInfo.ClassName, type));
				}

				// Inner classes
				GetNestedRelationsRecursively(cls, relations);
				
				// Types used in fields -> Association
				foreach(var typeName in cls.Fields.Select(f => f.GetRelatedTypeNames()).SelectMany(t => t))
				{
					var typeInfo = SearchClassName(typeName, allClassNames);

					if(typeInfo == null)
						continue;

					relations.Add(new Relation(cls.Name, typeInfo.ClassName, RelationType.Association));
				}

				// Types used in methods -> Dependency
				foreach(var typeName in cls.Methods.Select(m => m.GetRelatedTypeNames()).SelectMany(t => t))
				{
					var typeInfo = SearchClassName(typeName, allClassNames);

					if(typeInfo == null)
						continue;

					// Uses a corresponding type name in the ClassInfo collection, instead of using a type name described in the function definition.
					relations.Add(new Relation(cls.Name, typeInfo.ClassName, RelationType.Dependency));
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
		/// Returns a <see cref="NameInfo"/> that matches <paramref name="targetName"/> included in <paramref name="names"/>.
		/// <para>If no class name matching the argument '<paramref name="targetName"/>' found, returns null.</para>
		/// <para>If <paramref name = "targetName" /> partially matches one of the <paramref name="names"/>, returns it.</para>
		/// </summary>
		/// <param name="targetName">Class name confirmed to be included in <paramref name="names"/></param>
		/// <param name="names">Collection of class names</param>
		/// <returns>A value of whether <paramref name="targetName"/> is contained in <paramref name="names"/> or not</returns>
		private static NameInfo SearchClassName(string targetName, IEnumerable<NameInfo> names)
		{
			return names.FirstOrDefault(n => n.IsMatch(targetName));
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

		/// <summary>
		/// The class possessing name information (and class information itself).
		/// </summary>
		private class NameInfo
		{
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="classInfo"></param>
			public NameInfo(ClassInfo classInfo)
			{
				this.ClassInfo = classInfo;
			}

			/// <summary>
			/// Gets a class name.
			/// </summary>
			public string ClassName
			{
				get => this.ClassInfo.Name ?? string.Empty;
			}

			/// <summary>
			/// Gets a package name or namespace.
			/// </summary>
			public string PackageName
			{
				get => this.ClassInfo.Package ?? string.Empty;
			}

			/// <summary>
			/// Gets a full name of this class name.
			/// </summary>
			public string FullName
			{
				get
				{
					var package = (this.PackageName != null) ? this.PackageName + "." : string.Empty;
					return package + (this.ClassName ?? string.Empty);
				}
			}

			/// <summary>
			/// Gets a <see cref="ClassInfo"/>.
			/// </summary>
			public ClassInfo ClassInfo { get; }

			/// <summary>
			/// Returns a value of whether this class name matches a argument <paramref name="name"/> or not.
			/// </summary>
			/// <param name="name">A class name confirmed to match this class name</param>
			/// <returns>Whether this class name matches a argument <paramref name="name"/> or not</returns>
			public bool IsMatch(string name)
			{
				if(name == null)
					return false;

				var thisSegs = this.FullName.Split('.').Reverse().ToList();
				var argSegs = name.Split('.').Reverse().ToList();
				var length = Math.Min(thisSegs.Count, argSegs.Count);
				
				// Confirms all segments of class name (included in shorter one) matches other's with reverse order.
				for(var i = 0; i < length; i++)
				{
					if(thisSegs[i] != argSegs[i])
						return false;
				}

				return true;
			}
		}
	}
}
