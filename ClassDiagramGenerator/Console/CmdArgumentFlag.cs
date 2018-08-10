using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDiagramGenerator.Console
{
	/// <summary>
	/// Class possessing flag words.
	/// </summary>
	public class CmdArgumentFlag
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="words">Flag words</param>
		public CmdArgumentFlag(params string[] words)
		{
			this.FlagWords = new ReadOnlyCollection<string>(new List<string>(words));
		}

		/// <summary>
		/// Gets a collection of flag words.
		/// </summary>
		public IEnumerable<string> FlagWords { get; }

		/// <summary>
		/// Returns value whether <paramref name="arg"/> matches this flag or not.
		/// </summary>
		/// <param name="arg">One of command line arguments</param>
		/// <returns>Whether <paramref name="arg"/> matches this flag or not</returns>
		public bool Matches(string arg)
		{
			return this.FlagWords.Contains(arg);
		}
	}
}
