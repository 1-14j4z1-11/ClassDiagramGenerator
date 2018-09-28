//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ClassDiagramGenerator.Models.Structure
{
	/// <summary>
	/// Class possessing type information.
	/// </summary>
	public class TypeInfo
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Type name</param>
		/// <param name="typeArgs">Collection of type arguments</param>
		public TypeInfo(string name, IEnumerable<TypeInfo> typeArgs = null)
			: this(name, 0, typeArgs)
		{ }

		/// <summary>
		/// Constructor.
		/// <para>If <paramref name="arrayDimension"/> is not 0, this instance indicates an Array type.</para>
		/// <para>For example</para>
		/// <para>- <paramref name="arrayDimension"/>=0, name="string" -> string</para>
		/// <para>- <paramref name="arrayDimension"/>=1, name="string" -> string[]</para>
		/// <para>- <paramref name="arrayDimension"/>=2, name="List", typeArgs=[string] -> List&lt;string&gt;[][]</para>
		/// </summary>
		/// <param name="name">Type name</param>
		/// <param name="arrayDimension">A dimension of array</param>
		/// <param name="typeArgs">Collection of type arguments</param>
		public TypeInfo(string name, int arrayDimension, IEnumerable<TypeInfo> typeArgs = null)
		{
			this.Name = name;
			this.ArrayDimension = arrayDimension;
			this.TypeArgs = new ReadOnlyCollection<TypeInfo>(
				new List<TypeInfo>(typeArgs ?? Enumerable.Empty<TypeInfo>()));
		}

		/// <summary>
		/// Gets a type name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets or sets a dimension of array.
		/// <para>If this type is not an array type, returns 0.</para>
		/// </summary>
		public int ArrayDimension { get; }

		/// <summary>
		/// Gets a list of type arguments.
		/// <para>If this has no type arguments, returns empty list.</para>
		/// </summary>
		public IReadOnlyList<TypeInfo> TypeArgs { get; }

		/// <summary>
		/// Gets a exact type name consisting of the type name and the number of type parameters.
		/// </summary>
		public string ExactName
		{
			get => this.Name + ((this.TypeArgs.Count > 0) ? $"`{this.TypeArgs.Count}" : string.Empty);
		}

		/// <summary>
		/// Gets a collection containing all <see cref="TypeInfo"/>(s) that make up this type (including this type itself).
		/// </summary>
		/// <returns>A collection containing all <see cref="TypeInfo"/>(s) that make up this type (including this type itself)</returns>
		public IEnumerable<TypeInfo> GetContainedTypes()
		{
			var types = new List<TypeInfo>() { this };
			types.AddRange(this.TypeArgs.Select(t => t.GetContainedTypes()).SelectMany(t => t));
			return types;
		}

		public override bool Equals(object obj)
		{
			if(!(obj is TypeInfo other))
				return false;

			return (this.Name == other.Name)
				&& (this.ArrayDimension == other.ArrayDimension)
				&& (this.TypeArgs.SequenceEqual(other.TypeArgs));
		}

		public override int GetHashCode()
		{
			var argsHash = (this.TypeArgs.Count > 0) ? this.TypeArgs.Select(t => t?.GetHashCode() ?? 0).Aggregate((h1, h2) => h1 ^ h2) : 0;
			return this.Name.GetHashCode() ^ this.ArrayDimension.GetHashCode() ^ argsHash;
		}

		public override string ToString()
		{
			var typeArgs = (this.TypeArgs.Count > 0) ? $"<{string.Join(",", this.TypeArgs)}>" : string.Empty;
			var arrayBrackets = string.Join(string.Empty, Enumerable.Range(0, this.ArrayDimension).Select(_ => "[]"));
			return this.Name + typeArgs + arrayBrackets;
		}

		/// <summary>
		/// Mutable <see cref="TypeInfo"/> Class.
		/// <para>This class is only used intermediate processing.</para>
		/// </summary>
		internal class Mutable
		{
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="name">Type name</param>
			public Mutable(string name)
			{
				this.Name = name;
				this.TypeArgs = new List<Mutable>();
			}

			/// <summary>
			/// Gets or sets a type name.
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// Gets or sets a dimension of array.
			/// <para>If this type is not an array type, returns 0.</para>
			/// </summary>
			public int ArrayDimension { get; set; }

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
				return new TypeInfo(this.Name, this.ArrayDimension, this.TypeArgs.Select(m => m.ToImmutable()));
			}
		}
	}
}
