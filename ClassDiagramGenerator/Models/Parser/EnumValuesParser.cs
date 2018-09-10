//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ClassDiagramGenerator.Models.Structure;

namespace ClassDiagramGenerator.Models.Parser
{
	public class EnumValuesParser : ComponentParser<IEnumerable<FieldInfo>>
	{
		private static readonly Regex EnumValueRegex = new Regex($"({NamePattern})(?:\\s*=\\s*{NamePattern})?");
		private readonly ClassInfo classInfo;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="classInfo"><see cref="ClassInfo"/> containing enum values</param>
		public EnumValuesParser(ClassInfo classInfo)
		{
			this.classInfo = classInfo;
		}

		public override bool TryParse(SourceCodeReader reader, out IEnumerable<FieldInfo> values)
		{
			if(!reader.TryRead(out var text))
			{
				values = null;
				return false;
			}

			values = text.Text.Split(',')
				.Select(s => EnumValueRegex.Match(s))
				.Where(m => m.Success)
				.Select(m => m.Groups[1].Value)
				.Select(this.CreateFieldFromValueName);

			return true;
		}

		/// <summary>
		/// Creates a <see cref="FieldInfo"/> from enum value name.
		/// </summary>
		/// <param name="name">Enum value name</param>
		/// <returns><see cref="FieldInfo"/></returns>
		private FieldInfo CreateFieldFromValueName(string name)
		{
			return new FieldInfo(Modifier.Public | Modifier.Static, name, new TypeInfo("int"));
		}
	}
}
