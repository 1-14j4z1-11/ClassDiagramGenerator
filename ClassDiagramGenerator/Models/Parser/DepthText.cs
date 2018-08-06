using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDiagramGenerator.Models.Parser
{
	/// <summary>
	/// The class possessing text and depth.
	/// </summary>
	public class DepthText
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="text">Text</param>
		/// <param name="depth">Depth</param>
		public DepthText(string text, int depth)
		{
			this.Text = text;
			this.Depth = depth;
		}

		/// <summary>
		/// Gets a text.
		/// </summary>
		public string Text { get; }

		/// <summary>
		/// Gets a depth of the text.
		/// </summary>
		public int Depth { get; }

		/// <summary>
		/// Splits the <see cref="Text"/> with <paramref name="separator"/>
		/// <para>Each splitted text has the same depth as original text.</para>
		/// </summary>
		/// <param name="separator">Separator</param>
		/// <returns>Splitted strings</returns>
		public List<DepthText> Split(string separator)
		{
			return this.Text.Split(new[] { separator }, StringSplitOptions.None)
				.Select(s => new DepthText(s, this.Depth)).ToList();
		}

		public override bool Equals(object obj)
		{
			if(!(obj is DepthText other))
				return false;

			return (this.Text == other.Text) && (this.Depth == other.Depth);
		}

		public override int GetHashCode()
		{
			return (this.Text?.GetHashCode() ?? 0) ^ this.Depth.GetHashCode();
		}

		public override string ToString()
		{
			return $"[{this.Depth}] \"{this.Text}\"";
		}
	}
}
