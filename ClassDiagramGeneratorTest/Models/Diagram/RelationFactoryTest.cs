using System;
using System.Collections.Generic;
using System.Linq;
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
				Interface(Type("IF1")),
				Interface(Type("IF2")),
			});

			// Note that relation of 'IF3' is not contained
			AreEqualRelations(relations,
				Relation("ClassA", "Base1", Generalization),
				Relation("ClassA", "IF1", Realization),
				Relation("ClassA", "IF2", Realization));
		}

		[TestMethod]
		public void TestInheritances2()
		{
			var relations = RelationFactory.CreateFromClasses(new[]
			{
				Class(Type("Outer.Inner.ClassA"), List(Type("Base1"), Type("IF1"), Type("Inner2.IF2"), Type("IF3"))),
				ClassFully("External", Type("Base1")),
				Interface(Type("Outer.Inner.IF1")),
				Interface(Type("Outer.Inner2.IF2")),
				InterfaceFully("Internal", Type("Outer.Inner.IF3"))
			});

			AreEqualRelations(relations,
				Relation("Outer.Inner.ClassA", "Base1", Generalization),
				Relation("Outer.Inner.ClassA", "Outer.Inner.IF1", Realization),
				Relation("Outer.Inner.ClassA", "Outer.Inner2.IF2", Realization),
				Relation("Outer.Inner.ClassA", "Outer.Inner.IF3", Realization));
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

			AreEqualRelations(relations,
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
				new FieldInfo(Modifier.Public, "field1", Type("List", Type("F1"))),
				new FieldInfo(Modifier.Protected | Modifier.Readonly, "field2", Type("List", Type("Dictionary", Type("string"), Type("F2")))),
				new FieldInfo(Modifier.Private | Modifier.Const, "field3", Type("F3")),
				new FieldInfo(Modifier.Internal, "field4", Type("F4"))
				));

			var relations = RelationFactory.CreateFromClasses(new[]
			{
				cls,
				Class(Type("F1")),
				Class(Type("F2")),
				Class(Type("F3"))
			});

			AreEqualRelations(relations,
				Relation("MainClass", "F1", Association),
				Relation("MainClass", "F2", Association),
				Relation("MainClass", "F3", Association));
		}

		[TestMethod]
		public void TestFieldRelations2()
		{
			var cls = Class(Type("Outer.Inner.MainClass"));
			cls.Fields.AddRange(List(
				new FieldInfo(Modifier.Public, "field1", Type("List", Type("External.F1"))),
				new FieldInfo(Modifier.Protected | Modifier.Readonly, "field2", Type("List", Type("Dictionary", Type("string"), Type("Inner2.F2")))),
				new FieldInfo(Modifier.Private | Modifier.Const, "field3", Type("External.Outer.Inner.F3")),
				new FieldInfo(Modifier.Internal, "field4", Type("F4"))
				));

			var relations = RelationFactory.CreateFromClasses(new[]
			{
				cls,
				ClassFully("External", Type("F1")),
				Class(Type("Inner2.F2")),
				ClassFully("External", Type("Outer.Inner.F3")),
				ClassFully("Internal", Type("Outer.Inner.F4"))
			});

			AreEqualRelations(relations,
				Relation("Outer.Inner.MainClass", "F1", Association),
				Relation("Outer.Inner.MainClass", "Inner2.F2", Association),
				Relation("Outer.Inner.MainClass", "Outer.Inner.F3", Association),
				Relation("Outer.Inner.MainClass", "Outer.Inner.F4", Association));
		}

		[TestMethod]
		public void TestMethodRelations()
		{
			var cls = Class(Type("MainClass"));
			cls.Methods.AddRange(List(
				new MethodInfo(Modifier.Public, "Method1", Type("void"), null),
				new MethodInfo(Modifier.Protected | Modifier.Abstract, "Method2", Type("R2"), List(Arg(Type("List", Type("Dictionary", Type("string"), Type("A2"))), "arg"))),
				new MethodInfo(Modifier.Private | Modifier.Static, "Method3", Type("R3"), List(Arg(Type("A3a"), "argA"), Arg(Type("A3b"), "argB")))
				));

			var relations = RelationFactory.CreateFromClasses(new[]
			{
				cls,
				Class(Type("R2")),
				Class(Type("A2")),
				Class(Type("R3")),
				Class(Type("A3a")),
			});

			AreEqualRelations(relations,
				Relation("MainClass", "R2", Dependency),
				Relation("MainClass", "A2", Dependency),
				Relation("MainClass", "R3", Dependency),
				Relation("MainClass", "A3a", Dependency));
		}

		[TestMethod]
		public void TestMethodRelations2()
		{
			var cls = Class(Type("Outer.Inner.MainClass"));
			cls.Methods.AddRange(List(
				new MethodInfo(Modifier.Public, "Method1", Type("External.R1"), null),
				new MethodInfo(Modifier.Protected | Modifier.Abstract, "Method2", Type("Inner2.R2"), List(Arg(Type("List", Type("Dictionary", Type("string"), Type("Outer2.Inner.A2"))), "arg"))),
				new MethodInfo(Modifier.Private | Modifier.Static, "Method3", Type("R3"), List(Arg(Type("External.Outer.Inner.A3a"), "argA"), Arg(Type("External2.Outer.Inner.A3b"), "argB")))
				));

			var relations = RelationFactory.CreateFromClasses(new[]
			{
				cls,
				ClassFully("External", Type("R1")),
				Class(Type("Inner2.R2")),
				Class(Type("Outer2.Inner.A2")),
				ClassFully("Internal", Type("Outer.Inner.R3")),
				ClassFully("External", Type("Outer.Inner.A3a")),
				ClassFully("External", Type("Outer.Inner.A3b")),
			});

			// Note that relation of A3b does not contain
			AreEqualRelations(relations,
				Relation("Outer.Inner.MainClass", "R1", Dependency),
				Relation("Outer.Inner.MainClass", "Inner2.R2", Dependency),
				Relation("Outer.Inner.MainClass", "Outer2.Inner.A2", Dependency),
				Relation("Outer.Inner.MainClass", "Outer.Inner.R3", Dependency),
				Relation("Outer.Inner.MainClass", "Outer.Inner.A3a", Dependency));
		}

		[TestMethod]
		public void TestGenericRelations()
		{
			var cls = Class(Type("MainClass", Type("T")), List(Type("BaseClass", Type("T")), Type("IF", Type("T"))));
			cls.Methods.Add(new MethodInfo(Modifier.Public, "Method", Type("R", Type("T"), Type("T")), List(Arg(Type("A1", Type("T")), "arg1"), Arg(Type("A2", Type("T")), "arg2"))));
			cls.Fields.Add(new FieldInfo(Modifier.Private, "field", Type("F1", Type("T"), Type("T"))));

			var relations = RelationFactory.CreateFromClasses(new[]
			{
				cls,
				Class(Type("BaseClass", Type("X"))),
				Interface(Type("IF", Type("X"), Type("Y"))),
				Class(Type("R", Type("X"), Type("Y"))),
				Class(Type("A1", Type("X"))),
				Class(Type("A2", Type("X"), Type("Y"))),
				Class(Type("F1", Type("X"), Type("Y"))),
			});

			AreEqualRelations(relations,
				Relation("MainClass`1", "BaseClass`1", Generalization),
				Relation("MainClass`1", "R`2", Dependency),
				Relation("MainClass`1", "A1`1", Dependency),
				Relation("MainClass`1", "F1`2", Association));
		}

		/// <summary>
		/// Confirms <paramref name="actualRelations"/> are the same as <paramref name="expectedRelations"/> regardless of their order.
		/// <para>If they are different, this test is treated as failure.</para>
		/// </summary>
		/// <param name="actualRelations">Actual relations</param>
		/// <param name="expectedRelations">Expected relations</param>
		private static void AreEqualRelations(IEnumerable<Relation> actualRelations, params PlainRelation[] expectedRelations)
		{
			actualRelations.Select(r => new PlainRelation(r)).IsCollectionUnorderly(expectedRelations);
		}
	}
}
