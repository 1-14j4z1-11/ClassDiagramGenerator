//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDiagramGenerator.Cui
{
	/// <summary>
	/// The class possessing flag information.
	/// </summary>
	public class CmdFlag
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="required">Whether this flag is always required or not</param>
		/// <param name="dependentArgCount">Number of dependent arguments of this flag</param>
		/// <param name="words">Flag words</param>
		public CmdFlag(bool required, int dependentArgCount, params string[] words)
		{
			this.IsRequired = required;
			this.DependentArgCount = dependentArgCount;
			this.FlagWords = new ReadOnlyCollection<string>(new List<string>(words));
		}

		/// <summary>
		/// Gets a value of whether this flag is always required or not.
		/// </summary>
		public bool IsRequired { get; }

		/// <summary>
		/// Gets number of dependent arguments of this flag.
		/// </summary>
		public int DependentArgCount { get; }

		/// <summary>
		/// Gets a collection of flag words.
		/// </summary>
		public IEnumerable<string> FlagWords { get; }

		/// <summary>
		/// Returns a value of whether <paramref name="flagArg"/> matches this flag or not.
		/// <para>This matching is done with case ignore.</para>
		/// </summary>
		/// <param name="flagArg">One of command line arguments</param>
		/// <returns>Whether <paramref name="flagArg"/> matches this flag or not</returns>
		public bool Matches(string flagArg)
		{
			return this.FlagWords.Any(f => f.ToLower() == flagArg.ToLower());
		}
	}
}
