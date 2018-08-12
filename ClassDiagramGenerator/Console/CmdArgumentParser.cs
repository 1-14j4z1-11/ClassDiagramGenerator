//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDiagramGenerator.Console
{
	/// <summary>
	/// Class to parse Command line arguments.
	/// </summary>
	public class CmdArgumentParser
	{
		private static readonly string NewLine = Environment.NewLine;
		private readonly Dictionary<CmdArgumentFlag, int> flagMap = new Dictionary<CmdArgumentFlag, int>();
		private readonly Dictionary<CmdArgumentFlag, string> descMap = new Dictionary<CmdArgumentFlag, string>();
		private readonly HashSet<CmdArgumentFlag> requiredFlags = new HashSet<CmdArgumentFlag>();

		/// <summary>
		/// Adds a command line flag.
		/// </summary>
		/// <param name="required">Whether <paramref name="flag"/> is required</param>
		/// <param name="flag">Flag to be added</param>
		/// <param name="dependentArgs">Number of dependent arguments of <paramref name="flag"/></param>
		/// <param name="description">Description to be written as usage</param>
		/// <returns>Instance of itself</returns>
		public CmdArgumentParser AddArgumentFlag(bool required, CmdArgumentFlag flag, int dependentArgs, string description = null)
		{
			this.flagMap[flag] = dependentArgs;
			this.descMap[flag] = description ?? string.Empty;

			if(required)
			{
				this.requiredFlags.Add(flag);
			}
			
			return this;
		}

		/// <summary>
		/// Tries to parse command line arguments.
		/// </summary>
		/// <param name="args">Command line arguments</param>
		/// <param name="argValueMap">[out] Dictionary containing flag keys and dependent argument values (only succeeded in parsing)</param>
		/// <returns>Whether succeeded in parsing or not</returns>
		public bool TryParse(IEnumerable<string> args, out Dictionary<CmdArgumentFlag, List<string>> argValueMap)
		{
			if(args == null)
				throw new ArgumentNullException();

			argValueMap = new Dictionary<CmdArgumentFlag, List<string>>();
			var argList = args.ToList();
			var restRequired = new HashSet<CmdArgumentFlag>(this.requiredFlags);

			for(var i = 0; i < argList.Count; i++)
			{
				var arg = argList[i];
				var flag = flagMap.Keys.FirstOrDefault(f => f.Matches(arg));

				if(flag == null)
				{
					return false;
				}

				restRequired.Remove(flag);
				argValueMap[flag] = new List<string>();
				var num = flagMap[flag];

				if(i + num >= argList.Count)
				{
					return false;
				}

				for(var j = 0; j < num; j++)
				{
					argValueMap[flag].Add(argList[++i]);
				}
			}
			
			return (restRequired.Count == 0);
		}

		public string Usage()
		{
			var builder = new StringBuilder();

			builder.Append("Arguments").Append(NewLine);

			foreach(var flag in this.flagMap.Keys)
			{
				builder.Append("    ")
					.Append(string.Join(" , ", flag.FlagWords))
					.Append(" : ").Append(descMap[flag])
					.Append(NewLine);
			}

			return builder.ToString();
		}
	}
}
