﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGenerator.Models.Diagram
{
	public static class DiagramGenerator
	{
		private const string NewLine = "\r\n";
		private const Modifier AllAccessModifier = Modifier.Public | Modifier.Protected | Modifier.Internal | Modifier.Private;
		private static Dictionary<RelationType, string> Arrows = new Dictionary<RelationType, string>()
		{
			[RelationType.Dependency] = ".down.>",
			[RelationType.Association] = "-down->",
			[RelationType.Aggregation] = "o-down->",
			[RelationType.Composition] = "*-down->",
			[RelationType.Generalization] = "-up-|>",
			[RelationType.Realization] = ".up.|>",
			[RelationType.Nested] = "--+",
		};

		public static string Generate(string title, IEnumerable<ClassInfo> classInfoList, IEnumerable<Relation> relations, Modifier accessFilter = AllAccessModifier)
		{
			var builder = new StringBuilder($"@startuml {{{title}}}").Append(NewLine).Append(NewLine);

			foreach(var classInfo in classInfoList ?? Enumerable.Empty<ClassInfo>())
			{
				WriteDiagramOfClass(builder, classInfo, accessFilter);
			}

			foreach(var relation in relations ?? Enumerable.Empty<Relation>())
			{
				// Does not draw self relation
				if(relation.Class1 == relation.Class2)
					continue;

				WriteDiagramOfRelation(builder, relation);
			}

			return builder.Append(NewLine).Append("@enduml").ToString();
		}

		/// <summary>
		/// Writes a class which structs class diagram to <paramref name="builder"/>.
		/// <para>If <paramref name="accessFilter"/> is specified,
		/// contents that do not correspond to the filter are not described.</para>
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> possessing class diagram description</param>
		/// <param name="classInfo"><see cref="ClassInfo"/> to be written</param>
		/// <param name="accessFilter">Access level filter</param>
		private static void WriteDiagramOfClass(StringBuilder builder, ClassInfo classInfo, Modifier accessFilter = AllAccessModifier)
		{
			if(classInfo == null)
				throw new ArgumentNullException();

			builder.Append($"package {classInfo.NameSpace} {{").Append(NewLine);

			var classModifier = (classInfo.Modifier.HasFlag(Modifier.Abstract)) ? "abstract " : string.Empty;
			builder.Append($"\t{classModifier}{ClassCategoryText(classInfo.Category)} {classInfo.Type} {{").Append(NewLine);

			foreach(var field in classInfo.Fields ?? Enumerable.Empty<FieldInfo>())
			{
				if(!IsTargetAccessModifier(field.Modifier, accessFilter))
					continue;

				builder.Append($"\t\t{AccessSymbol(field.Modifier)} {ModifierText(field.Modifier, true)}{field.Type} {field.Name}")
					.Append(NewLine);
			}

			foreach(var method in classInfo.Methods ?? Enumerable.Empty<MethodInfo>())
			{
				if(!IsTargetAccessModifier(method.Modifier, accessFilter))
					continue;

				builder.Append($"\t\t{AccessSymbol(method.Modifier)} ")
					.Append($"{ModifierText(method.Modifier, true)}")
					.Append($"{method.ReturnType}{((method.ReturnType != null) ? " " : "")}")
					.Append($"{method.Name}(");
				
				var args = method.Arguments.Select(a => $"{a.Type} {a.Name}") ?? Enumerable.Empty<string>();
				builder.Append(string.Join(", ", args));

				builder.Append(")").Append(NewLine);
			}

			builder.Append("\t}").Append(NewLine)
				.Append("}").Append(NewLine).Append(NewLine);
		}

		/// <summary>
		/// Writes a relation which structs class diagram to <paramref name="builder"/>.
		/// </summary>
		/// <param name="builder"><see cref="StringBuilder"/> possessing class diagram description</param>
		/// <param name="relation"><see cref="Relation"/> to be written</param>
		private static void WriteDiagramOfRelation(StringBuilder builder, Relation relation)
		{
			if(relation == null)
				throw new ArgumentNullException();
			
			var arrow = Arrows[relation.Type];

			builder.Append(relation.Class1)
				.Append(" ")
				.Append(arrow)
				.Append(" ")
				.Append(relation.Class2)
				.Append(NewLine);
		}

		/// <summary>
		/// Check whether access level of <paramref name="target"/> is included in <paramref name="filter"/>.
		/// <para>Modifiers except for acccess level are ignored.</para>
		/// </summary>
		/// <param name="target">Target <see cref="Modifier"/></param>
		/// <param name="filter">Valid access level <see cref="Modifier"/></param>
		/// <returns>whether access level of <paramref name="target"/> is included in <paramref name="filter"/></returns>
		private static bool IsTargetAccessModifier(Modifier target, Modifier filter)
		{
			return (target & filter & AllAccessModifier) != Modifier.None;
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
			else if(modifier.HasFlag(Modifier.Internal))
				return "~";
			else if(modifier.HasFlag(Modifier.Private))
				return "-";
			else
				return "~";	// Default access level is 'package'
		}

		/// <summary>
		/// Gets a text describing class category from <see cref="ClassCategory"/>.
		/// </summary>
		/// <param name="category">Class category</param>
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
					return "struct";
				default:
					throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets a text describing modifier except for access level from <see cref="ClassCategory"/>.
		/// </summary>
		/// <param name="modifier"><see cref="Modifier"/></param>
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
	}
}