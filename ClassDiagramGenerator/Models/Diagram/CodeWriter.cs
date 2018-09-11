//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDiagramGenerator.Models.Diagram
{
	/// <summary>
	/// The class to write codes.
	/// </summary>
	public class CodeWriter
	{
		private static readonly string Indentation = "\t";
		private readonly string newLine;
		private readonly StringBuilder builder = new StringBuilder();
		private bool isNewLine = true;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="newLine">String used as new line.</param>
		public CodeWriter(string newLine)
		{
			this.newLine = newLine ?? string.Empty;
		}

		/// <summary>
		/// Gets a number of indent.
		/// <para>If this is greater than 0, adds indentation from a next line.</para>
		/// </summary>
		public int Indent { get; private set; }

		/// <summary>
		/// Gets or sets a symbol string to be written at each new line head (after indentation).
		/// <para>If writing a symbol is not needed, sets a null.</para>
		/// </summary>
		public string HeaderSymbol { get; set; }

		/// <summary>
		/// Increases indent.
		/// <para>If an argument is omitted, increases indent by 1.</para>
		/// </summary>
		/// <param name="count">Number of indent to be increased</param>
		/// <returns>This instance itself</returns>
		public CodeWriter IncreaseIndent(int count = 1)
		{
			this.Indent += count;
			return this;
		}

		/// <summary>
		/// Decreases indent.
		/// <para>If an argument is omitted, decreases indent by 1.</para>
		/// </summary>
		/// <param name="count">Number of indent to be decreased</param>
		/// <returns>This instance itself</returns>
		public CodeWriter DecreaseIndent(int count = 1)
		{
			this.Indent -= count;
			return this;
		}

		/// <summary>
		/// Resets indent.
		/// </summary>
		/// <returns>This instance itself</returns>
		public CodeWriter ResetIndent()
		{
			this.Indent = 0;
			return this;
		}

		/// <summary>
		/// Writes a new line.
		/// </summary>
		/// <returns>This instance itself</returns>
		public CodeWriter NewLine()
		{
			this.builder.Append(this.newLine);
			this.isNewLine = true;
			return this;
		}

		/// <summary>
		/// Writes a text.
		/// </summary>
		/// <param name="text">Text to be written</param>
		/// <returns>This instance itself</returns>
		public CodeWriter Write(string text)
		{
			if(this.isNewLine)
			{
				for(var i = 0; i < this.Indent; i++)
				{
					this.builder.Append(Indentation);
				}

				this.builder.Append(this.HeaderSymbol ?? string.Empty);
				this.isNewLine = false;
			}

			this.builder.Append(text);
			return this;
		}

		public override string ToString()
		{
			return this.builder.ToString();
		}
	}
}
