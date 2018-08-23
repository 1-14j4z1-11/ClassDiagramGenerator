﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestSupport.MSTest;

using ClassDiagramGenerator.Models.Diagram;
using ClassDiagramGenerator.Models.Structure;
using static ClassDiagramGenerator.Models.Diagram.RelationType;
using static ClassDiagramGeneratorTest.Models.TestSupport;

namespace ClassDiagramGeneratorTest.Models.Diagram
{
	[TestClass]
	public class RelationFactoryTest
	{
		[TestMethod]
		public void TestInheritances()
		{
			var relations = RelationFactory.CreateFromClasses(new[]
			{
				Class(Type("ClassA"), List(Type("Base1"), Type("IF1"), Type("IF2"), Type("IF3"))),
				Class(Type("Base1")),
				IF(Type("IF1")),
				IF(Type("IF2")),
			});

			// Note that relation of 'IF3' is not contained
			relations.IsCollectionUnorderly(
				Relation("ClassA", "Base1", Generalization),
				Relation("ClassA", "IF1", Realization),
				Relation("ClassA", "IF2", Realization));
		}

		[TestMethod]
		public void TestNestedClasses()
		{
			var root = Class(Type("Root"));
			var inner1 = Class(Type("In1"));
			var inner11 = Class(Type("In1-1"));

			inner11.InnerClasses.Add(Class(Type("In1-1-1")));
			inner1.InnerClasses.Add(inner11);
			inner1.InnerClasses.Add(Class(Type("In1-2")));
			inner1.InnerClasses.Add(Class(Type("In1-3")));
			root.InnerClasses.Add(inner1);
			root.InnerClasses.Add(Class(Type("In2")));

			var relations = RelationFactory.CreateFromClasses(new[] { root });

			// All Nested relations are included regardless of whether the classes are included in RelationFactory's argument or not.
			relations.IsCollectionUnorderly(
				Relation("In1", "Root", Nested),
				Relation("In2", "Root", Nested),
				Relation("In1-1", "In1", Nested),
				Relation("In1-2", "In1", Nested),
				Relation("In1-3", "In1", Nested),
				Relation("In1-1-1", "In1-1", Nested));
		}

		[TestMethod]
		public void TestFieldRelations()
		{
			var cls = Class(Type("MainClass"));
			cls.Fields.AddRange(List(
				new FieldInfo(Modifier.Public, "field1", Type("List", Type("FieldType1"))),
				new FieldInfo(Modifier.Protected | Modifier.Readonly, "field2", Type("List", Type("Dictionary", Type("string"), Type("FieldType2")))),
				new FieldInfo(Modifier.Private | Modifier.Const, "field3", Type("FieldType3"))
				));

			var relations = RelationFactory.CreateFromClasses(new[]
			{
				cls,
				Class(Type("FieldType1")),
				Class(Type("FieldType2")),
			});
			
			relations.IsCollectionUnorderly(
				Relation("MainClass", "FieldType1", Association),
				Relation("MainClass", "FieldType2", Association));
		}

		[TestMethod]
		public void TestMethodRelations()
		{
			var cls = Class(Type("MainClass"));
			cls.Methods.AddRange(List(
				new MethodInfo(Modifier.Public, "Method1", Type("void"), null),
				new MethodInfo(Modifier.Protected | Modifier.Abstract, "Method2", Type("ReturnType2"), List(Arg(Type("List", Type("Dictionary", Type("string"), Type("ArgType2"))), "arg"))),
				new MethodInfo(Modifier.Private | Modifier.Static, "Method3", Type("ReturnType3"), List(Arg(Type("ArgType3A"), "argA"), Arg(Type("ArgType3B"), "argB")))
				));

			var relations = RelationFactory.CreateFromClasses(new[]
			{
				cls,
				Class(Type("ReturnType2")),
				Class(Type("ArgType2")),
				Class(Type("ReturnType3")),
				Class(Type("ArgType3A")),
			});

			relations.IsCollectionUnorderly(
				Relation("MainClass", "ReturnType2", Dependency),
				Relation("MainClass", "ArgType2", Dependency),
				Relation("MainClass", "ReturnType3", Dependency),
				Relation("MainClass", "ArgType3A", Dependency));
		}

		/// <summary>
		/// Creates <see cref="ClassInfo"/> whose category is <see cref="ClassCategory.Class"/>.
		/// </summary>
		/// <param name="type">Class type</param>
		/// <param name="inheritances">Inherited classes</param>
		/// <returns><see cref="ClassInfo"/></returns>
		private static ClassInfo Class(TypeInfo type, IEnumerable<TypeInfo> inheritances = null)
		{
			return new ClassInfo(Modifier.Public, ClassCategory.Class, "TestNameSpace", type, inheritances);
		}

		/// <summary>
		/// Creates <see cref="ClassInfo"/> whose category is <see cref="ClassCategory.Interface"/>.
		/// </summary>
		/// <param name="type">Interface type</param>
		/// <param name="inheritances">Inherited classes</param>
		/// <returns><see cref="ClassInfo"/></returns>
		private static ClassInfo IF(TypeInfo type, IEnumerable<TypeInfo> inheritances = null)
		{
			return new ClassInfo(Modifier.Public, ClassCategory.Interface, "TestNameSpace", type, inheritances);
		}
	}
}