﻿//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDiagramGenerator.Cui
{
	/// <summary>
	/// The class to parse command line arguments.
	/// </summary>
	public class CmdParser
	{
		private static readonly string NewLine = Environment.NewLine;
		private readonly List<CmdFlag> flags = new List<CmdFlag>();
		private readonly Dictionary<CmdFlag, IEnumerable<string>> descMap = new Dictionary<CmdFlag, IEnumerable<string>>();

		/// <summary>
		/// Adds a command line flag.
		/// </summary>
		/// <param name="flag">Flag to be added</param>
		/// <param name="descriptions">Description texts to be written as usage</param>
		/// <returns>Instance of itself</returns>
		public CmdParser AddFlag(CmdFlag flag, params string[] descriptions)
		{
			this.flags.Add(flag);
			this.descMap[flag] = (descriptions != null) ? new List<string>(descriptions) : Enumerable.Empty<string>();
			
			return this;
		}

		/// <summary>
		/// Tries to parse command line arguments.
		/// </summary>
		/// <param name="args">Command line arguments</param>
		/// <param name="argValueMap">[out] Dictionary containing flag keys and dependent argument values (only succeeded in parsing)</param>
		/// <returns>Whether succeeded in parsing or not</returns>
		public bool TryParse(IEnumerable<string> args, out Dictionary<CmdFlag, List<string>> argValueMap)
		{
			if(args == null)
				throw new ArgumentNullException();

			argValueMap = new Dictionary<CmdFlag, List<string>>();
			var argList = args.ToList();
			var restRequired = new HashSet<CmdFlag>(this.flags.Where(f => f.IsRequired));

			for(var i = 0; i < argList.Count; i++)
			{
				var arg = argList[i];
				var flag = flags.FirstOrDefault(f => f.Matches(arg));

				if(flag == null)
				{
					return false;
				}

				restRequired.Remove(flag);
				argValueMap[flag] = new List<string>();
				var num = flag.DependentArgCount;

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

		/// <summary>
		/// Gets a string to indicate usage.
		/// </summary>
		/// <returns>String to indicate usage</returns>
		public string Usage()
		{
			var builder = new StringBuilder();

			builder.Append("[Arguments]").Append(NewLine);

			foreach(var flag in this.flags)
			{
				var flagText = $"    {string.Join(", ", flag.FlagWords)} "
					+ (flag.IsRequired ? "(Required)" : "(Optional)") + " : ";
				var descIndent = string.Join(string.Empty, Enumerable.Range(0, 8).Select(_ => " "));

				builder.Append("    ")
					.Append(string.Join(",", flag.FlagWords))
					.Append(flag.IsRequired ? " (Required)" : " (Optional)")
					.Append(NewLine + "        ")
					.Append(string.Join(NewLine + "        ", descMap[flag]))
					.Append(NewLine).Append(NewLine);
			}

			return builder.ToString();
		}
	}
}