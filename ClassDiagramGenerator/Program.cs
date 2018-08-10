using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClassDiagramGenerator.Models.Diagram;
using ClassDiagramGenerator.Models.Parser;
using ClassDiagramGenerator.Models.Structure;
using ClassDiagramGenerator.Console;

namespace UmlGenerator
{
	public class Program
	{
		private static readonly CmdArgumentFlag InputFlag = new CmdArgumentFlag("-i");
		private static readonly CmdArgumentFlag OutputFlag = new CmdArgumentFlag("-o");
		private static readonly CmdArgumentFlag LangFlag = new CmdArgumentFlag("-l", "-lang");
		private static readonly CmdArgumentParser CmdParser = new CmdArgumentParser()
			.AddArgumentFlag(true, InputFlag, 1, "Input directory path")
			.AddArgumentFlag(true, OutputFlag, 1, "Output file path")
			.AddArgumentFlag(true, LangFlag, 1, "Programming language ( C# : 'cs' or 'csharp', Java : 'java' )");

		private static readonly Language[] Languages = new Language[]
		{
			new Language(new JavaCodeParser(), "*.java", new []{ "java" }),
			new Language(new CSharpCodeParser(), "*.cs", new []{ "cs", "csharp" }),
		};

		public static void Main(string[] args)
		{
			if(!CmdParser.TryParse(args, out var argMap))
			{
				Console.WriteLine(CmdParser.Usage());
				return;
			}

			var inputDir = argMap[InputFlag].First();
			var outputFile = argMap[OutputFlag].First();
			var langValue = argMap[LangFlag].First();

			var lang = Languages.FirstOrDefault(l => l.MatchesCmdValue(langValue));

			if(lang == null)
			{
				Console.WriteLine(CmdParser.Usage());
				return;
			}

			var sourceFiles = null as IEnumerable<string>;

			try
			{
				sourceFiles = Directory.GetFiles(inputDir, lang.Extension, SearchOption.AllDirectories);
			}
			catch
			{
				Console.WriteLine($"Could not get file list : {inputDir}");
				return;
			}

			var diagram = GenerateClassDiagram("class-diagram", sourceFiles, lang.Parser);

			try
			{
				File.WriteAllText(outputFile, diagram);
			}
			catch
			{
				Console.WriteLine($"Could not open output file : {outputFile}");
				return;
			}
		}

		/// <summary>
		/// Generates a class diagram.
		/// </summary>
		/// <param name="title">Title of class diagram</param>
		/// <param name="filePaths">Source code paths</param>
		/// <param name="parser">Parser to parse source codes</param>
		/// <returns>Class diagram described in PlantUML</returns>
		private static string GenerateClassDiagram(string title, IEnumerable<string> filePaths, ISourceCodeParser parser)
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
					Console.WriteLine($"Skipped (could not open file) : {file}");
				}
			}

			var relations = RelationFactory.CreateFromClasses(classes);
			return DiagramGenerator.Generate(title ?? string.Empty, classes, relations, Modifier.Public);
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
			/// Returns a value whether <paramref name="cmdValue"/> matches this language or not.
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
