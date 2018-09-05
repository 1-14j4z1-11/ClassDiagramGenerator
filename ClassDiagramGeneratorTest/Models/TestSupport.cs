using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ClassDiagramGenerator.Models.Diagram;
using ClassDiagramGenerator.Models.Parser;
using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGeneratorTest.Models
{
	/// <summary>
	/// The test support class.
	/// </summary>
	public static class TestSupport
	{
		/// <summary>
		/// Generates a <see cref="SourceCodeReader"/> from a source code text.
		/// </summary>
		/// <param name="code">Source code text</param>
		/// <returns><see cref="SourceCodeReader"/></returns>
		public static SourceCodeReader ReaderFromCode(string code)
		{
			return new SourceCodeReader(code);
		}

		/// <summary>
		/// Gets a total line number of code.
		/// </summary>
		/// <param name="code">Code to be gotten number of lines</param>
		/// <returns>Total line number of code</returns>
		public static int TotalLineCount(string code)
		{
			return new SourceCodeReader(code).Lines.Count;
		}

		/// <summary>
		/// Creates a <see cref="ClassInfo"/> whose category is <see cref="ClassCategory.Class"/>.
		/// </summary>
		/// <param name="type">Class type</param>
		/// <param name="inheritances">Inherited classes</param>
		/// <returns><see cref="ClassInfo"/></returns>
		public static ClassInfo Class(TypeInfo type, IEnumerable<TypeInfo> inheritances = null)
		{
			return new ClassInfo(Modifier.Public, ClassCategory.Class, "TestNameSpace", type, inheritances);
		}

		/// <summary>
		/// Creates a <see cref="ClassInfo"/> whose category is <see cref="ClassCategory.Class"/>.
		/// </summary>
		/// <param name="nameSpace">Namespace of class</param>
		/// <param name="type">Class type</param>
		/// <param name="inheritances">Inherited classes</param>
		/// <returns><see cref="ClassInfo"/></returns>
		public static ClassInfo ClassFully(string nameSpace, TypeInfo type, IEnumerable<TypeInfo> inheritances = null)
		{
			return new ClassInfo(Modifier.Public, ClassCategory.Class, nameSpace, type, inheritances);
		}

		/// <summary>
		/// Creates a <see cref="ClassInfo"/> whose category is <see cref="ClassCategory.Interface"/>.
		/// </summary>
		/// <param name="type">Interface type</param>
		/// <param name="inheritances">Inherited classes</param>
		/// <returns><see cref="ClassInfo"/></returns>
		public static ClassInfo Interface(TypeInfo type, IEnumerable<TypeInfo> inheritances = null)
		{
			return new ClassInfo(Modifier.Public, ClassCategory.Interface, "TestNameSpace", type, inheritances);
		}

		/// <summary>
		/// Creates a <see cref="ClassInfo"/> whose category is <see cref="ClassCategory.Interface"/>.
		/// </summary>
		/// <param name="nameSpace">Namespace of interface</param>
		/// <param name="type">Interface type</param>
		/// <param name="inheritances">Inherited classes</param>
		/// <returns><see cref="ClassInfo"/></returns>
		public static ClassInfo InterfaceFully(string nameSpace, TypeInfo type, IEnumerable<TypeInfo> inheritances = null)
		{
			return new ClassInfo(Modifier.Public, ClassCategory.Interface, nameSpace, type, inheritances);
		}

		/// <summary>
		/// Creates a <see cref="TypeInfo"/>.
		/// </summary>
		/// <param name="type">Type name</param>
		/// <param name="typeArgs">Type arguments</param>
		/// <returns><see cref="TypeInfo"/></returns>
		public static TypeInfo Type(string type, params TypeInfo[] typeArgs)
		{
			return new TypeInfo(type, typeArgs);
		}

		/// <summary>
		/// Creates a Array <see cref="TypeInfo"/>.
		/// </summary>
		/// <param name="type">Type name</param>
		/// <param name="typeArgs">Type arguments</param>
		/// <returns><see cref="TypeInfo"/></returns>
		public static TypeInfo TypeArray(string type, params TypeInfo[] typeArgs)
		{
			return new TypeInfo(true, type, typeArgs);
		}

		/// <summary>
		/// Creates an <see cref="ArgumentInfo"/>.
		/// </summary>
		/// <param name="type"><see cref="TypeInfo"/></param>
		/// <param name="name">Argument name</param>
		/// <returns><see cref="ArgumentInfo"/></returns>
		public static ArgumentInfo Arg(TypeInfo type, string name)
		{
			return new ArgumentInfo(type, name);
		}

		/// <summary>
		/// Creates a <see cref="PlainRelation"/>.
		/// </summary>
		/// <param name="class1">Class name 1</param>
		/// <param name="class2">Class name 2</param>
		/// <param name="type">Relation type</param>
		/// <returns><see cref="PlainRelation"/></returns>
		public static PlainRelation Relation(string class1, string class2, RelationType type)
		{
			return new PlainRelation(class1, class2, type);
		}

		/// <summary>
		/// Creates a <see cref="List{T}"/>.
		/// </summary>
		/// <typeparam name="T">Contents type in List</typeparam>
		/// <param name="values">Contents in List</param>
		/// <returns><see cref="List{T}"/></returns>
		public static List<T> List<T>(params T[] values)
		{
			return values.ToList();
		}

		/// <summary>
		/// Creates a <see cref="DepthText"/>.
		/// </summary>
		/// <param name="depth">Depth</param>
		/// <param name="text">Text</param>
		/// <returns><see cref="DepthText"/></returns>
		public static DepthText Text(int depth, string text)
		{
			return new DepthText(text, depth);
		}
	}
}
