﻿//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGenerator.Models.Parser
{
	/// <summary>
	/// Base class of parser to parse source code component.
	/// </summary>
	/// <typeparam name="T">A type of parsed component</typeparam>
	public abstract class ComponentParser<T>
	{
		/// <summary>A list containing strings to be used as modifier</summary>
		protected static readonly IReadOnlyList<string> Modifiers
			= new ReadOnlyCollection<string>(new List<string>(
				Enum.GetValues(typeof(Modifier)).Cast<Modifier>().Select(m => m.ToModifierString()).Where(m => !string.IsNullOrEmpty(m))));

		/// <summary>Pattern string that matches modifiers (no grouping)</summary>
		protected static readonly string ModifierPattern = "(?:" + string.Join("|", Modifiers) + ")";

		/// <summary>Pattern string that matches name (no grouping)</summary>
		protected static readonly string NamePattern = "[^\\s,:\\[\\]\\(\\)<>=]+";

		/// <summary>Pattern string that matches type argument enclosed in &lt;&gt; (no grouping)</summary>
		protected static readonly string TypeArgPattern = "[^:\\[\\]\\(\\)=]+";

		/// <summary>Pattern string that matches Attributes of C#</summary>
		protected static readonly string AttributePattern = "(?:\\s*\\[[^\\[\\]]*\\]\\s*)*";

		/// <summary>Pattern string that matches Annotations of Java</summary>
		protected static readonly string AnnotationPattern = $"(?:\\s*@{NamePattern}\\s*(?:\\([^\\(\\)]*\\))?\\s*)*";

		/// <summary>Pattern string that matches type (no grouping)</summary>
		protected static readonly string TypePattern = $"{NamePattern}(?:\\s*<{TypeArgPattern}>\\s*)?(?:\\s*\\[\\s*\\]\\s*)?";

		/// <summary>Pattern string that matches single argument (no grouping)</summary>
		protected static readonly string ArgumentPattern = $"(?:{AttributePattern}{AnnotationPattern}(?:(?:this|in|out|ref)\\s+)?(?:{TypePattern})\\s+(?:{NamePattern})(?:\\s*=[^,]*)?)";
		
		/// <summary>
		/// Regex that matches single argument
		/// <para>- [1] : Type name</para>
		/// <para>- [2] : Argument name</para>
		/// </summary>
		private static readonly Regex ArgumentRegex = new Regex(ArgumentPattern.Replace($"(?:{TypePattern})", $"({TypePattern})").Replace($"(?:{NamePattern})", $"({NamePattern})"));

		/// <summary>
		/// Tries to parse component of source code.
		/// <para>Tries read a line from <paramref name="reader"/>, and if succeeded in parsing, output parsed component.</para>
		/// <para>If succeeded in parsing, the position of the <paramref name="reader"/> is seeked by the number of the read lines.
		/// Otherwise, the position of <paramref name="reader"/> is not changed.</para>
		/// </summary>
		/// <param name="reader"><see cref="SourceCodeReader"/></param>
		/// <param name="obj">[out] Parsed component (only succeeded in parsing)</param>
		/// <returns>Whether succeeded in parsing or not</returns>
		public abstract bool TryParse(SourceCodeReader reader, out T obj);

		/// <summary>
		/// Parse a collection of <see cref="ArgumentInfo"/> from string.
		/// <para>If argument is null or empty, returns a empty collection.</para>
		/// </summary>
		/// <param name="argText">String that indicates arguments</param>
		/// <returns>A collection of <see cref="ArgumentInfo"/></returns>
		protected static IEnumerable<ArgumentInfo> ParseArguments(string argText)
		{
			if(string.IsNullOrEmpty(argText))
				return Enumerable.Empty<ArgumentInfo>();
			
			return argText.Split(",",  "<", ">", d => d == 0)
				.Select(a => ArgumentRegex.Match(a))
				.Where(m => {
					return m.Success;
					})
				.Select(m =>　new ArgumentInfo(ParseType(m.Groups[1].Value), m.Groups[2].Value));
		}

		/// <summary>
		/// Gets a line count from current position of <paramref name="reader"/> to end of line that has more deep <paramref name="depth"/> continuously.
		/// <para>The position of <paramref name="reader"/> after this processing is the same as that of before this processing.</para>
		/// </summary>
		/// <param name="reader">Reader</param>
		/// <param name="depth">Depth threshold (excluding self)</param>
		/// <returns>A line count coutinuous more deep lines</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="reader"/> is null</exception>
		protected static int GetMoreDeepLineCount(SourceCodeReader reader, int depth)
		{
			if(reader == null)
				throw new ArgumentNullException();

			var start = reader.Position;
			var deepLines = 0;

			while(reader.TryRead(out var t) && (t.Depth > depth))
			{
				deepLines++;
			}

			reader.Position = start;
			return deepLines;
		}

		/// <summary>
		/// Parse <see cref="Modifier"/> from string.
		/// </summary>
		/// <param name="modifierText">String that indicates modifier</param>
		/// <returns><see cref="Modifier"/> parsed from string</returns>
		protected virtual Modifier ParseModifiers(string modifierText)
		{
			var words = Regex.Split(modifierText, "\\s+");
			var mod = Modifier.None;

			foreach(var word in words)
			{
				mod |= Structure.Modifiers.Parse(word);
			}

			return mod;
		}

		/// <summary>
		/// Parse <see cref="TypeInfo"/> from string.
		/// <para>If faield to parsing, returns null.</para>
		/// </summary>
		/// <param name="typeText">String that indicates type (that matches <see cref="TypePattern"/>)</param>
		/// <returns><see cref="TypeInfo"/> parsed from string, or null</returns>
		protected static TypeInfo ParseType(string typeText)
		{
			var words = TextAnalyzer.SplitWithDepth(typeText, "<", ">")
				.SplitEach(",")
				.SplitEach("[") // Use '[' as a separator, and ']' is treated as an array descriptor.
				.Where(w => !string.IsNullOrWhiteSpace(w.Text))
				.Select(w => new DepthText(w.Text.Trim(), w.Depth))
				.ToList();

			if(words.Count == 0)
				return null;

			var rightBracketRegex = new Regex("^\\s*\\]\\s*$");
			var rootType = new TypeInfo.Mutable(words[0].Text);
			var rootDepth = words[0].Depth;

			for(var i = 1; i < words.Count; i++)
			{
				var name = words[i].Text;
				var depth = words[i].Depth;
				
				if(rightBracketRegex.IsMatch(name))
				{
					// Gets a type whose depth is the same as an array descriptor
					var arrayType = GetLastTypeWithDepth(rootType, depth - rootDepth);
					arrayType.IsArray = true;
				}
				else
				{
					// Gets a parent type, whose depth is the same as this depth-1
					var parentType = GetLastTypeWithDepth(rootType, depth - rootDepth - 1);
					parentType.TypeArgs.Add(new TypeInfo.Mutable(name));
				}
			}

			return rootType.ToImmutable();
		}

		/// <summary>
		/// Gets a <see cref="TypeInfo.Mutable"/> with the same depth as argument.
		/// If more than one type argument with the same depth is included, returns a last type argument.
		/// </summary>
		/// <param name="rootType"><see cref="TypeInfo.Mutable"/></param>
		/// <param name="depth">Target depth</param>
		/// <returns><see cref="TypeInfo"/> with the same depth as argument</returns>
		private static TypeInfo.Mutable GetLastTypeWithDepth(TypeInfo.Mutable rootType, int depth)
		{
			var type = rootType;

			for(var j = 0; j < depth; j++)
			{
				// If an input arg is correct format, this statement is not passed.
				if(rootType.TypeArgs.LastOrDefault() == null)
					break;

				type = type.TypeArgs.Last();
			}

			return type;
		}
	}
}
