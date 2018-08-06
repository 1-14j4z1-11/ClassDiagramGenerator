using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ClassDiagramGenerator.Models.Diagram;
using ClassDiagramGenerator.Models.Parser;
using ClassDiagramGenerator.Models.Structure;

namespace UmlGenerator
{
	public class Program
	{
		public static void Main(string[] args)
		{
			if(args.Length < 2)
			{
				return;
			}

			var dir = args[0];
			var outputFile = args[1];

			var diagram = GenerateClassDiagram("class-diagram", Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories));
			File.WriteAllText(outputFile, diagram);
		}

		/// <summary>
		/// Generates a class diagram.
		/// </summary>
		/// <param name="title">Title of class diagram</param>
		/// <param name="filePaths">Source code paths</param>
		/// <returns>Class diagram described in PlantUML</returns>
		private static string GenerateClassDiagram(string title, IEnumerable<string> filePaths)
		{
			var parser = new SourceCodeParser();
			var classes = new List<ClassInfo>();

			foreach(var file in filePaths)
			{
				if(!File.Exists(file))
					continue;

				var content = File.ReadAllText(file);
				classes.AddRange(parser.Parse(content));
			}

			var relations = RelationFactory.CreateFromClasses(classes);
			return DiagramGenerator.Generate(title ?? string.Empty, classes, relations, Modifier.Public);
		}
	}
}
