//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGenerator.Models.Diagram
{
	/// <summary>
	/// Class to generate a class diagram written in PlantUML.
	/// </summary>
	public static class PumlClassDiagramGenerator
	{
		private const string NewLine = "\r\n";
		private const string CommentSymbol = "'";
		private static Dictionary<RelationType, string> Arrows = new Dictionary<RelationType, string>()
		{
			[RelationType.Dependency] = ".down.>",
			[RelationType.Association] = "-down->",
			[RelationType.Aggregation] = "o-down->",
			[RelationType.Composition] = "*-down->",
			[RelationType.Generalization] = "-up-|>",
			[RelationType.Realization] = ".up.|>",
			[RelationType.Nested] = "-down-+",		// not 'up', to locate inner class under parent class
		};

		/// <summary>
		/// Generates a class diagram.
		/// <para>If <paramref name="accessFilter"/> is specified,
		/// contents that do not correspond to the filter are not described.</para>
		/// </summary>
		/// <param name="title">Title of class diagram</param>
		/// <param name="classInfoList">Classes to be included in a class diagram</param>
		/// <param name="relations">Relations to be included in a class diagram</param>
		/// <param name="accessFilter">Access level filter</param>
		/// <param name="excludedClasses">A collection of class names not to be written to a class diagram</param>
		/// <returns>Class diagram text written in PlantUML</returns>
		public static string Generate(string title, IEnumerable<ClassInfo> classInfoList, IEnumerable<Relation> relations, Modifier accessFilter = Modifiers.AllAccessLevels, IEnumerable<string> excludedClasses = null)
		{
			var writer = new CodeWriter(NewLine);
			WriteHeader(writer, title);

			var groups = classInfoList?.GroupBy(cls => cls.Package)
				?? Enumerable.Empty<IGrouping<string, ClassInfo>>();

			foreach(var group in groups.OrderBy(g => g.Key))
			{
				writer.Write($"package {group.Key} {{").NewLine().NewLine();
				writer.IncreaseIndent();

				foreach(var cls in group.OrderBy(c => c.Name))
				{
					WriteClass(writer, cls, accessFilter, excludedClasses);
				}

				writer.DecreaseIndent();
				writer.Write("}").NewLine().NewLine();
			}

			foreach(var relation in relations ?? Enumerable.Empty<Relation>())
			{
				// Does not draw self relation
				if(relation.Class1 == relation.Class2)
					continue;

				WriteRelation(writer, relation, excludedClasses);
			}

			WriteFooter(writer);
			return writer.ToString();
		}

		/// <summary>
		/// Writes header texts into <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer"><see cref="CodeWriter"/> into which a class diagram is written</param>
		/// <param name="title">Title of class diagram</param>
		private static void WriteHeader(CodeWriter writer, string title)
		{
			writer.Write($"@startuml {title}").NewLine()
				.NewLine()
				.Write("skinparam classAttributeIconSize 0").NewLine()
				.NewLine();
		}

		/// <summary>
		/// Writes footer texts into <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer"><see cref="CodeWriter"/> into which a class diagram is written</param>
		private static void WriteFooter(CodeWriter writer)
		{
			writer.NewLine().Write("@enduml").NewLine();
		}

		/// <summary>
		/// Writes a class which structs class diagram into <paramref name="writer"/>.
		/// <para>If <paramref name="accessFilter"/> is specified,
		/// contents that do not correspond to the filter are not described.</para>
		/// </summary>
		/// <param name="writer"><see cref="CodeWriter"/> into which a class diagram is written</param>
		/// <param name="classInfo"><see cref="ClassInfo"/> to be written</param>
		/// <param name="accessFilter">Access level filter</param>
		/// <param name="excludedClasses">A collection of class names not to be written to a class diagram</param>
		private static void WriteClass(CodeWriter writer, ClassInfo classInfo, Modifier accessFilter, IEnumerable<string> excludedClasses)
		{
			if(classInfo == null)
				throw new ArgumentNullException();

			var isExcludedClass = excludedClasses?.Contains(classInfo.Name) ?? false;

			if(isExcludedClass)
			{
				StartCommentMode(writer);
			}

			var stereoType = (classInfo.Category == ClassCategory.Struct) ? "<<struct>> " : string.Empty;
			var modifier = (classInfo.Modifier.HasFlag(Modifier.Abstract)) ? "abstract " : string.Empty;
			writer.Write($"{modifier}{ClassCategoryText(classInfo.Category)} {classInfo.Type} {stereoType}{{").NewLine();
			writer.IncreaseIndent();

			foreach(var field in classInfo.Fields ?? Enumerable.Empty<FieldInfo>())
			{
				if(!IsTargetAccessModifier(field.Modifier, accessFilter))
				{
					StartCommentMode(writer);
				}

				WriteField(writer, field);

				// Ends comment mode only if this class is NOT excluded target
				if(!isExcludedClass)
				{
					EndCommentMode(writer);
				}
			}

			foreach(var method in classInfo.Methods ?? Enumerable.Empty<MethodInfo>())
			{
				if(!IsTargetAccessModifier(method.Modifier, accessFilter))
				{
					StartCommentMode(writer);
				}

				WriteMethod(writer, method);

				// Ends comment mode only if this class is NOT excluded target
				if(!isExcludedClass)
				{
					EndCommentMode(writer);
				}
			}

			writer.DecreaseIndent();
			writer.Write("}").NewLine().NewLine();

			// Writes inner classes in this class
			foreach(var innerClass in classInfo.InnerClasses)
			{
				WriteClass(writer, innerClass, accessFilter, excludedClasses);
			}

			EndCommentMode(writer);
		}

		/// <summary>
		/// Writes a field which structs class diagram into <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer"><see cref="CodeWriter"/> into which a class diagram is written</param>
		/// <param name="field"><see cref="FieldInfo"/> to be written</param>
		private static void WriteField(CodeWriter writer, FieldInfo field)
		{
			var stereoTypes = new List<string>();
			Action<bool, string> addStereoTypeIf = (condition, stereoTypeString) =>
			{
				if(condition)
					stereoTypes.Add(stereoTypeString);
			};

			addStereoTypeIf(field.Modifier.HasFlag(Modifier.Event), "event");
			addStereoTypeIf(field.PropertyType.HasFlag(PropertyType.Get), "get");
			addStereoTypeIf(field.PropertyType.HasFlag(PropertyType.Set), "set");
			var stereoTypeText = stereoTypes.Any() ? "<<" + string.Join(",", stereoTypes) + ">> " : string.Empty;

			var indexerArgs = field.IndexerArguments.Any() ? "[" + ArgumentsText(field.IndexerArguments) + "]" : string.Empty;

			writer.Write($"{AccessSymbol(field.Modifier)} ")
				.Write($"{stereoTypeText}")
				.Write($"{ModifierText(field.Modifier, true)}")
				.Write($"{field.Name}{indexerArgs}")
				.Write($" : {field.Type}")
				.NewLine();
		}

		/// <summary>
		/// Writes a method which structs class diagram into <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer"><see cref="CodeWriter"/> into which a class diagram is written</param>
		/// <param name="method"><see cref="MethodInfo"/> to be written</param>
		private static void WriteMethod(CodeWriter writer, MethodInfo method)
		{
			writer.Write($"{AccessSymbol(method.Modifier)} ")
				.Write($"{ModifierText(method.Modifier, true)}")
				.Write($"{method.Name}(")
				.Write(ArgumentsText(method.Arguments))
				.Write(")")
				.Write($"{((method.ReturnType != null) ? " : " : "")}{method.ReturnType}")
				.NewLine();
		}

		/// <summary>
		/// Writes a relation which structs class diagram into <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer"><see cref="CodeWriter"/> into which a class diagram is written</param>
		/// <param name="relation"><see cref="Relation"/> to be written</param>
		/// <param name="excludedClasses">A collection of class names not to be written to a class diagram</param>
		private static void WriteRelation(CodeWriter writer, Relation relation, IEnumerable<string> excludedClasses)
		{
			if(relation == null)
				throw new ArgumentNullException();

			if(excludedClasses?.Any(c => (c == relation.Class1.Name) || (c == relation.Class2.Name)) ?? false)
			{
				StartCommentMode(writer);
			}
			
			writer.Write(relation.Class1.Name)
				.Write(" ")
				.Write(Arrows[relation.Type])
				.Write(" ")
				.Write(relation.Class2.Name)
				.NewLine();

			EndCommentMode(writer);
		}

		/// <summary>
		/// Checks whether access level of <paramref name="target"/> is included in <paramref name="filter"/>.
		/// <para>Modifiers except for acccess level are ignored.</para>
		/// </summary>
		/// <param name="target">Target <see cref="Modifier"/></param>
		/// <param name="filter">Valid access level <see cref="Modifier"/></param>
		/// <returns>whether access level of <paramref name="target"/> is included in <paramref name="filter"/></returns>
		private static bool IsTargetAccessModifier(Modifier target, Modifier filter)
		{
			return (target & filter & Modifiers.AllAccessLevels) != Modifier.None;
		}

		/// <summary>
		/// Gets a text describing arguments.
		/// </summary>
		/// <param name="args">Arguments to be converted into a text</param>
		/// <returns>Text describing arguments</returns>
		private static string ArgumentsText(IEnumerable<ArgumentInfo> args)
		{
			var argsText = args?.Select(a => $"{a.Name} : {a.Type}") ?? Enumerable.Empty<string>();
			return string.Join(", ", argsText);
		}
		
		/// <summary>
		/// Gets a symbol describing access level from <see cref="Modifier"/>.
		/// </summary>
		/// <param name="modifier"><see cref="Modifier"/></param>
		/// <returns>Symbol describing access level</returns>
		private static string AccessSymbol(Modifier modifier)
		{
			if(modifier.HasFlag(Modifier.Public))
				return "+";
			else if(modifier.HasFlag(Modifier.Protected))
				return "#";
			else if(modifier.HasFlag(Modifier.Internal) || modifier.HasFlag(Modifier.Package))
				return "~";
			else if(modifier.HasFlag(Modifier.Private))
				return "-";
			else
				return "~";	// Default access level is 'package'
		}

		/// <summary>
		/// Gets a text describing class category from <see cref="ClassCategory"/>.
		/// </summary>
		/// <param name="category">Class category to be converted into a text</param>
		/// <returns>Text describing class category</returns>
		private static string ClassCategoryText(ClassCategory category)
		{
			switch(category)
			{
				case ClassCategory.Class:
					return "class";
				case ClassCategory.Interface:
					return "interface";
				case ClassCategory.Enum:
					return "enum";
				case ClassCategory.Struct:
					return "class";	// "struct" is not supported in UML.
				default:
					throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets a text describing modifier except for access level from <see cref="ClassCategory"/>.
		/// </summary>
		/// <param name="modifier"><see cref="Modifier"/> to be converted into a text</param>
		/// <param name="enclose">Whether enclose text with { }</param>
		/// <returns>Text describing modifier</returns>
		private static string ModifierText(Modifier modifier, bool enclose)
		{
			var builder = new StringBuilder();
			Action<string> append = word =>
			{
				builder.Append(enclose ? "{" : string.Empty)
					.Append(word)
					.Append(enclose ? "} " : string.Empty);
			};

			if(modifier.HasFlag(Modifier.Abstract))
				append("abstract");
			if(modifier.HasFlag(Modifier.Static) || modifier.HasFlag(Modifier.Const))
				append("static");

			return builder.ToString();
		}

		/// <summary>
		/// Starts comment mode.
		/// </summary>
		/// <param name="writer">A <see cref="CodeWriter"/> to be changed into comment mode</param>
		private static void StartCommentMode(CodeWriter writer)
		{
			writer.HeaderSymbol = CommentSymbol;
		}

		/// <summary>
		/// Ends comment mode.
		/// </summary>
		/// <param name="writer">A <see cref="CodeWriter"/> to be changed into normal mode</param>
		private static void EndCommentMode(CodeWriter writer)
		{
			writer.HeaderSymbol = null;
		}
	}
}
