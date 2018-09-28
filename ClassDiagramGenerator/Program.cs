//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using ClassDiagramGenerator.Cui;
using ClassDiagramGenerator.Models.Diagram;
using ClassDiagramGenerator.Models.Parser;
using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGenerator
{
	public class Program
	{
		private static readonly CmdFlag InputFlag = new CmdFlag(true, 1, "-i");
		private static readonly CmdFlag OutputFlag = new CmdFlag(true, 1, "-o");
		private static readonly CmdFlag LangFlag = new CmdFlag(true, 1, "-l", "-lang");
		private static readonly CmdFlag AccessLevelFlag = new CmdFlag(false, 1, "-al", "-accesslevel");
		private static readonly CmdFlag ExcludedClassFlag = new CmdFlag(false, 1, "-ec", "-excludedclass");
		private static readonly CmdParser CmdParser = new CmdParser()
			.AddFlag(InputFlag, "Input directory path.")
			.AddFlag(OutputFlag, "Output file path.")
			.AddFlag(LangFlag, "Programming language.", " - C#   : 'cs' or 'csharp'", " - Java : 'java'")
			.AddFlag(AccessLevelFlag, "Access levels of members displayed in a diagram.", "Default value is all access levels.", "Use ',' as separator to specify multiple access levels.")
			.AddFlag(ExcludedClassFlag, "Excluded class names, which is not displayed in a diagram.", "Use ',' as separator to specify multiple classes.");

		private static readonly Language[] Languages = new Language[]
		{
			new Language(new JavaCodeParser(), "*.java", new []{ "java" }),
			new Language(new CSharpCodeParser(), "*.cs", new []{ "cs", "csharp" }),
		};

		public static void Main(string[] args)
		{
			Console.WriteLine($"\nClassDiagramGenerator [Version {GetVersion()}]\n");

			if(!CmdParser.TryParse(args, out var argMap))
			{
				Console.WriteLine(CmdParser.Usage());
				return;
			}

			var inputDir = argMap[InputFlag].First();
			var outputFile = argMap[OutputFlag].First();
			var langValue = argMap[LangFlag].First();
			var lang = Languages.FirstOrDefault(l => l.MatchesCmdValue(langValue));
			var alStr = argMap.TryGetValue(AccessLevelFlag, out var al) ? al.First() : string.Empty;
			var accessLevel = ParseAccessLevel(alStr);
			var ecStr = argMap.TryGetValue(ExcludedClassFlag, out var ec) ? ec.First() : string.Empty;
			var excludedClasses = ParseClasses(ecStr);

			if(lang == null)
			{
				Console.Error.WriteLine($"Invalid language was specified : {langValue}");
				return;
			}

			var sourceFiles = null as IEnumerable<string>;

			try
			{
				sourceFiles = Directory.GetFiles(inputDir, lang.Extension, SearchOption.AllDirectories);
				Console.WriteLine($"Read files ... {sourceFiles.Count()}");
			}
			catch
			{
				Console.Error.WriteLine($"Could not get file list of the directory : {inputDir}");
				return;
			}

			var diagram = GenerateClassDiagram("class-diagram", sourceFiles, lang.Parser, accessLevel, excludedClasses);

			try
			{
				File.WriteAllText(outputFile, diagram);
			}
			catch
			{
				Console.Error.WriteLine($"Could not open output file : {outputFile}");
				return;
			}

			Console.WriteLine($"Completed generating a class diagram >> {outputFile}");
		}

		/// <summary>
		/// Generates a class diagram.
		/// </summary>
		/// <param name="title">Title of class diagram</param>
		/// <param name="filePaths">Source code paths</param>
		/// <param name="parser">Parser to parse source codes</param>
		/// <param name="accessLevel">Access level of members written to a class diagram</param>
		/// <param name="excludedClasses">A collection of class names not to be written to a class diagram</param>
		/// <returns>Class diagram described in PlantUML</returns>
		private static string GenerateClassDiagram(string title, IEnumerable<string> filePaths, ISourceCodeParser parser, Modifier accessLevel, IEnumerable<string> excludedClasses)
		{
			var classes = new List<ClassInfo>();

			foreach(var file in filePaths)
			{
				try
				{
					var content = File.ReadAllText(file);
					classes.AddRange(parser.Parse(content));
				}
				catch
				{
					Console.Error.WriteLine($"Skipped (could not open file) : {file}");
				}
			}

			Console.WriteLine($"Parsed classes ... {classes.Count}");

			var relations = RelationFactory.CreateFromClasses(classes);
			return PumlClassDiagramGenerator.Generate(title ?? string.Empty, classes, relations, accessLevel, excludedClasses);
		}

		/// <summary>
		/// Parses a string describing access levels.
		/// <para>If no access modifier is contained, return all access levels</para>
		/// </summary>
		/// <param name="str">A string describing access levels</param>
		/// <returns><see cref="Modifier"/> indicating access level parsed from an argument</returns>
		private static Modifier ParseAccessLevel(string str)
		{
			if(string.IsNullOrEmpty(str))
				return Modifiers.AllAccessLevels;

			var words = str.Split(',', ' ', '|');
			var modifier = Modifier.None;

			foreach(var word in words)
			{
				modifier |= Modifiers.Parse(word);
			}

			return (modifier == Modifier.None) ? Modifiers.AllAccessLevels : modifier & Modifiers.AllAccessLevels;
		}

		/// <summary>
		/// Parses a string describing class names.
		/// <para>Returns a collection of class names, or null if argument string is null or empty.</para>
		/// </summary>
		/// <param name="str">A string describing class names</param>
		/// <returns>A collection of class names parsed from an argument</returns>
		private static IEnumerable<string> ParseClasses(string str)
		{
			if(string.IsNullOrEmpty(str))
				return Enumerable.Empty<string>();

			return str.Split(',', ' ', '|');
		}
		
		/// <summary>
		/// Gets a version of this application.
		/// </summary>
		/// <returns>A version of this application</returns>
		private static string GetVersion()
		{
			var assembly = Assembly.GetExecutingAssembly();
			return assembly.GetName().Version.ToString(3);
		}

		private class Language
		{
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="parser">Parser to parse this language's source codes</param>
			/// <param name="extension">File extension of this language</param>
			/// <param name="cmdValues">Command argument values that specifies this language</param>
			public Language(ISourceCodeParser parser, string extension, IEnumerable<string> cmdValues)
			{
				this.Parser = parser;
				this.Extension = extension;
				this.CmdValues = cmdValues.Select(s => s.ToLower());
			}

			/// <summary>
			/// Gets a parser to parse this language's source codes.
			/// </summary>
			public ISourceCodeParser Parser { get; }

			/// <summary>
			/// Gets a file extension of this language.
			/// </summary>
			public string Extension { get; }

			/// <summary>
			/// Gets command line argument values that specifies this language.
			/// </summary>
			public IEnumerable<string> CmdValues { get; }

			/// <summary>
			/// Returns a value of whether <paramref name="cmdValue"/> matches this language or not.
			/// </summary>
			/// <param name="cmdValue">dependent argument of a language flag</param>
			/// <returns></returns>
			public bool MatchesCmdValue(string cmdValue)
			{
				return this.CmdValues.Contains(cmdValue?.ToLower());
			}
		}
	}
}
