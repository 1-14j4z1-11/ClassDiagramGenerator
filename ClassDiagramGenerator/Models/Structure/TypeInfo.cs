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
	/// The class possessing type information.
	/// </summary>
	public class TypeInfo
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Type name</param>
		/// <param name="typeArgs">Collection of type arguments</param>
		public TypeInfo(string name, IEnumerable<TypeInfo> typeArgs = null)
		{
			this.Name = name;
			this.TypeArgs = new ReadOnlyCollection<TypeInfo>(
				new List<TypeInfo>(typeArgs ?? Enumerable.Empty<TypeInfo>()));
		}

		/// <summary>
		/// Gets a type name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets a list of type arguments.
		/// <para>If this has no type arguments, returns empty list.</para>
		/// </summary>
		public IReadOnlyList<TypeInfo> TypeArgs { get; }

		/// <summary>
		/// Gets a list containing all type names that make up this type (including this type name).
		/// </summary>
		/// <returns>A list containing all type names that make up this type (including this type name)</returns>
		public IReadOnlyList<string> GetContainedTypeNames()
		{
			var typeNames = new List<string>() { this.Name };
			typeNames.AddRange(this.TypeArgs.Select(t => t.GetContainedTypeNames()).SelectMany(t => t));
			return new ReadOnlyCollection<string>(typeNames);
		}

		public override bool Equals(object obj)
		{
			if(!(obj is TypeInfo other))
				return false;

			return (this.Name == other.Name)
				&& (this.TypeArgs.SequenceEqual(other.TypeArgs));
		}

		public override int GetHashCode()
		{
			var argsHash = (this.TypeArgs.Count > 0) ? this.TypeArgs.Select(t => t?.GetHashCode() ?? 0).Aggregate((h1, h2) => h1 ^ h2) : 0;
			return this.Name.GetHashCode() ^ argsHash;
		}

		public override string ToString()
		{
			if(this.TypeArgs.Count == 0)
			{
				return this.Name;
			}
			else
			{
				return this.Name + "<" + string.Join(",", this.TypeArgs) + ">";
			}
		}

		/// <summary>
		/// Mutable <see cref="TypeInfo"/> Class.
		/// <para>This class is only used intermediate processing</para>
		/// </summary>
		internal class Mutable
		{
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="name">Type name</param>
			/// <param name="typeArgs">Collection of type arguments</param>
			public Mutable(string name, IEnumerable<Mutable> typeArgs = null)
			{
				this.Name = name;
				this.TypeArgs = (typeArgs != null) ? new List<Mutable>(typeArgs) : new List<Mutable>();
			}

			/// <summary>
			/// Gets or sets a type name.
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// Gets a mutable list of type arguments.
			/// </summary>
			public List<Mutable> TypeArgs { get; }

			/// <summary>
			/// Create a immutable <see cref="TypeInfo"/> instance that is the same as this instance.
			/// </summary>
			/// <returns><see cref="TypeInfo"/></returns>
			public TypeInfo ToImmutable()
			{
				return new TypeInfo(this.Name, this.TypeArgs.Select(m => m.ToImmutable()));
			}
		}
	}
}
