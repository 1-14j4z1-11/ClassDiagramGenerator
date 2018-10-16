//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ClassDiagramGenerator.Models.Structure
{
	/// <summary>
	/// Class possessing class information.
	/// </summary>
	public class ClassInfo
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="modifier">Modifier</param>
		/// <param name="category">Class category</param>
		/// <param name="package">Package name or namespace</param>
		/// <param name="type">Class type</param>
		/// <param name="inheritedClasses">Inherited classes and interfaces</param>
		/// <exception cref="ArgumentNullException">If <paramref name="type"/> is null.</exception>
		public ClassInfo(Modifier modifier, ClassCategory category, string package, TypeInfo type, IEnumerable<TypeInfo> inheritedClasses)
		{
			this.Modifier = modifier;
			this.Category = category;
			this.Package = package;
			this.Type = type ?? throw new ArgumentNullException();
			this.InheritedClasses = new ReadOnlyCollection<TypeInfo>(
				new List<TypeInfo>(inheritedClasses ?? Enumerable.Empty<TypeInfo>()));
			this.InnerClasses = new List<ClassInfo>();
			this.Methods = new List<MethodInfo>();
			this.Fields = new List<FieldInfo>();
		}

		/// <summary>
		/// Gets a modifier of class.
		/// </summary>
		public Modifier Modifier { get; }

		/// <summary>
		/// Gets a class category.
		/// </summary>
		public ClassCategory Category { get; }

		/// <summary>
		/// Gets a package (or namespace) of class.
		/// </summary>
		public string Package { get; }

		/// <summary>
		/// Gets a class type. (Always not null)
		/// </summary>
		public TypeInfo Type { get; }

		/// <summary>
		/// Gets a class name.
		/// </summary>
		public string Name { get => this.Type?.Name; }
		
		/// <summary>
		/// Gets inherited classes and interfaces.
		/// <para>If this inherits no classes and interfaces, return empty list.</para>
		/// </summary>
		public IReadOnlyList<TypeInfo> InheritedClasses { get; }

		/// <summary>
		/// Gets a mutable list of inner classes.
		/// <para>If this have no inner classes, return empty list.</para>
		/// </summary>
		public List<ClassInfo> InnerClasses { get; }

		/// <summary>
		/// Gets a mutable list of methods.
		/// <para>If this have no methods, return empty list.</para>
		/// </summary>
		public List<MethodInfo> Methods { get; }

		/// <summary>
		/// Gets a mutable list of fields.
		/// <para>If this have no fields, return empty list.</para>
		/// </summary>
		public List<FieldInfo> Fields { get; }

		/// <summary>
		/// Gets a collection of classes contained in this class (including this class itself).
		/// </summary>
		/// <returns>A collection of classes</returns>
		public List<ClassInfo> GetAllClasses()
		{
			var classes = new List<ClassInfo>() { this };

			foreach(var inner in this.InnerClasses)
			{
				classes.AddRange(inner.GetAllClasses());
			}

			return classes;
		}
	}
}
