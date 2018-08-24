//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClassDiagramGenerator.Models.Parser
{
	/// <summary>
	/// The class to read each line from source code.
	/// </summary>
	public class SourceCodeReader
	{
		private int position;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="code">Source code</param>
		/// <exception cref="ArgumentNullException">If argument is null</exception>
		public SourceCodeReader(string code)
		{
			if(code == null)
				throw new ArgumentNullException();

			this.Lines = new ReadOnlyCollection<DepthText>(ConvertCode(code));
		}

		/// <summary>
		/// Gets or sets position of next reading.
		/// <para>Allowed position is range of [0, EndOfLines].</para>
		/// <para>Setting the EndOfLines means the end of all reading.</para>
		/// </summary>
		/// <exception cref="ArgumentException">If value is out of range.</exception>
		public int Position
		{
			get { return this.position; }
			set
			{
				// Allows setting last index + 1 to be able to indicate end of reading
				if((value < 0) && (this.Lines.Count < value))
					throw new ArgumentException();

				this.position = value;
			}
		}
		
		/// <summary>
		/// Gets a value of whether the current position is end of reader.
		/// </summary>
		public bool IsEndOfLines
		{
			get => this.Position >= this.Lines.Count;
		}

		/// <summary>
		/// Gets strings that this instance has.
		/// </summary>
		public IReadOnlyList<DepthText> Lines { get; }

		/// <summary>
		/// Tries to read line with current position.
		/// <para>If this reader has finished all lines, returns false.</para>
		/// </summary>
		/// <param name="line">[out] Line to be read</param>
		/// <returns>Whether succeeded in reading</returns>
		public bool TryRead(out DepthText line)
		{
			if(this.Position >= this.Lines.Count)
			{
				line = null;
				return false;
			}

			line = this.Lines[this.Position++];
			return true;
		}
		
		/// <summary>
		/// Resets the position.
		/// </summary>
		public void Reset()
		{
			this.Position = 0;
		}
		
		/// <summary>
		/// Converts source code into a collection of <see cref="DepthText"/>.
		/// </summary>
		/// <param name="code">Source code</param>
		/// <returns>Collection of <see cref="DepthText"/></returns>
		private static List<DepthText> ConvertCode(string code)
		{
			// Remove " that does not make up a string
			code = code.Replace("'\"'", string.Empty);
			code = Regex.Replace(code, "([^\\\\]?)\\\\\"", $"$1");

			// Remove single character enclosed by ''
			code = Regex.Replace(code, "'.'", string.Empty);

			// Remove string enclosed by " "
			code = RemoveEnclosedText(code, "\"", "\"");

			// Remove comment enclosed by /* */
			code = RemoveEnclosedText(code, "/*", "*/");

			// Remove single line commect and # line
			code = Regex.Replace(code, "//.*(\\r|\\n|\\r\\n)", string.Empty);
			code = Regex.Replace(code, "#.*(\\r|\\n|\\r\\n)", string.Empty);

			// Unify delimiters into a single space.
			code = Regex.Replace(code, "[\\r\\n\\s]+", " ");
			
			return TextAnalyzer.SplitWithDepth(code, "{", "}")
				.Split(";")
				.Where(w => !string.IsNullOrWhiteSpace(w.Text))
				.ToList();
		}

		/// <summary>
		/// Returns a string removed substrings enclosed by <paramref name="open"/> and <paramref name="close"/>.
		/// </summary>
		/// <param name="text">A string</param>
		/// <param name="open">Start string to enclose</param>
		/// <param name="close">End string to enclose</param>
		/// <returns>A string removed substrings enclosed by <paramref name="open"/> and <paramref name="close"/></returns>
		private static string RemoveEnclosedText(string text, string open, string close)
		{
			var index = 0;

			while((index = text.IndexOf(open)) >= 0)
			{
				var pairIndex = text.IndexOf(close, index + open.Length);

				if(pairIndex < 0)
					break;

				text = text.Remove(index, pairIndex - index + close.Length);
			}

			return text;
		}
	}
}
