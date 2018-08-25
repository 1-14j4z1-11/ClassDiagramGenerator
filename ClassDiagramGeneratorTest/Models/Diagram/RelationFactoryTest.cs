using System;
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
				Class(Type("ClassA"), List(Type("Base1"), Type("IF1"), Type("IF2", Type("TypeArg")), Type("IF3"))),
				Class(Type("Base1")),
				IF(Type("IF1")),
				IF(Type("IF2")),
				IF(Type("TypeArg")),
			});

			// Note that relation of 'IF3' and 'TypeArg' are not contained
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
				new FieldInfo(Modifier.Public, "field1", Type("List", Type("T1"))),
				new FieldInfo(Modifier.Protected | Modifier.Readonly, "field2", Type("List", Type("Dictionary", Type("string"), Type("T2")))),
				new FieldInfo(Modifier.Private | Modifier.Const, "field3", Type("T3")),
				new FieldInfo(Modifier.Internal | Modifier.Static, "field4", TypeArray("T4")),
				new FieldInfo(Modifier.Protected | Modifier.Internal, "field5", Type("Dictionary", Type("List", Type("T5a")), Type("T5b")))
				));

			var relations = RelationFactory.CreateFromClasses(new[]
			{
				cls,
				Class(Type("T1")),
				Class(Type("T2")),
				Class(Type("T4")),
				Class(Type("T5a")),
				Class(Type("T5b")),
			});
			
			relations.IsCollectionUnorderly(
				Relation("MainClass", "T1", Association),
				Relation("MainClass", "T2", Association),
				Relation("MainClass", "T4", Association),
				Relation("MainClass", "T5a", Association),
				Relation("MainClass", "T5b", Association));
		}

		[TestMethod]
		public void TestMethodRelations()
		{
			var cls = Class(Type("MainClass"));
			cls.Methods.AddRange(List(
				new MethodInfo(Modifier.Public, "Method1", Type("void"), null),
				new MethodInfo(Modifier.Protected | Modifier.Abstract, "Method2", Type("R2"), List(Arg(Type("List", Type("Dictionary", Type("List", Type("A2a")), Type("A2b"))), "arg"))),
				new MethodInfo(Modifier.Private | Modifier.Static, "Method3", Type("R3"), List(Arg(Type("A3a"), "argA"), Arg(Type("A3b"), "argB"))),
				new MethodInfo(Modifier.Internal | Modifier.Virtual, "Method4", Type("R4"), List(Arg(TypeArray("A4a"), "argA"), Arg(TypeArray("List", Type("A4b")), "argB")))
				));

			var relations = RelationFactory.CreateFromClasses(new[]
			{
				cls,
				Class(Type("R2")),
				Class(Type("A2a")),
				Class(Type("A2b")),
				Class(Type("R3")),
				Class(Type("A3a")),
			});

			relations.IsCollectionUnorderly(
				Relation("MainClass", "R2", Dependency),
				Relation("MainClass", "A2a", Dependency),
				Relation("MainClass", "A2b", Dependency),
				Relation("MainClass", "R3", Dependency),
				Relation("MainClass", "A3a", Dependency));
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
