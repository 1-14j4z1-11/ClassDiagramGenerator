//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDiagramGenerator.Models.Structure
{
	/// <summary>
	/// The class possessing method information.
	/// </summary>
	public class MethodInfo
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="modifier">Modifier</param>
		/// <param name="name">Method name</param>
		/// <param name="returnType">Return value type, or null to indicate a constructor</param>
		/// <param name="arguments">Collection containing arguments</param>
		public MethodInfo(Modifier modifier, string name, TypeInfo returnType, IEnumerable<ArgumentInfo> arguments)
		{
			this.Modifier = modifier;
			this.Name = name;
			this.ReturnType = returnType;
			this.Arguments = new ReadOnlyCollection<ArgumentInfo>(
				new List<ArgumentInfo>(arguments ?? Enumerable.Empty<ArgumentInfo>()));
		}

		/// <summary>
		/// Gets a modifier of method.
		/// </summary>
		public Modifier Modifier { get; }

		/// <summary>
		/// Gets a method name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets a return value type.
		/// <para>If this is constructor, returns null.</para>
		/// </summary>
		public TypeInfo ReturnType { get; }

		/// <summary>
		/// Gets a list of arguments.
		/// <para>If this has no arguments, return empty list.</para>
		/// </summary>
		public IReadOnlyList<ArgumentInfo> Arguments { get; }

		/// <summary>
		/// Returns a collection of type names related with this method.
		/// </summary>
		/// <returns>A collection of type names related with this method</returns>
		public IEnumerable<string> GetRelatedTypeNames()
		{
			var allTypeNames = new List<string>(this.ReturnType?.GetContainedTypeNames() ?? Enumerable.Empty<string>());
			allTypeNames.AddRange(this.Arguments.Select(a => a.Type.GetContainedTypeNames()).SelectMany(t => t));
			return allTypeNames;
		}

		public override bool Equals(object obj)
		{
			if(!(obj is MethodInfo other))
				return false;

			return (this.Modifier == other.Modifier)
				&& (this.Name == other.Name)
				&& Equals(this.ReturnType, other.ReturnType)
				&& (this.Arguments.SequenceEqual(other.Arguments));
		}

		public override int GetHashCode()
		{
			var argsHash = (this.Arguments.Count > 0) ? this.Arguments.Select(a => a?.GetHashCode() ?? 0).Aggregate((h1, h2) => h1 ^ h2) : 0;

			return this.Modifier.GetHashCode()
				^ (this.Name?.GetHashCode() ?? 0)
				^ (this.ReturnType?.GetHashCode() ?? 0)
				^ argsHash;
		}
	}
}
