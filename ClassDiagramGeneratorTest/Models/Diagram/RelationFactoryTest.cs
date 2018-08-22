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
		public void TestInheritance()
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

		private static ClassInfo Class(TypeInfo type, IEnumerable<TypeInfo> inheritances = null)
		{
			return new ClassInfo(Modifier.Public, ClassCategory.Class, "TestNameSpace", type, inheritances);
		}

		private static ClassInfo IF(TypeInfo type, IEnumerable<TypeInfo> inheritances = null)
		{
			return new ClassInfo(Modifier.Public, ClassCategory.Interface, "TestNameSpace", type, inheritances);
		}
	}
}
