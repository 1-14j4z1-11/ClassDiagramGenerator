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
			: this(false, name, typeArgs)
		{ }

		/// <summary>
		/// Constructor.
		/// <para>If <paramref name="isArray"/> is true, this instance indicates an Array type.</para>
		/// <para>For example</para>
		/// <para>- isArray=false, name="string" -> string</para>
		/// <para>- isArray=true, name="string" -> string[]</para>
		/// <para>- isArray=true, name="List", typeArgs=[string] -> List&lt;string&gt;[]</para>
		/// </summary>
		/// <param name="isArray">Value of whether this type indicates an Array type or not.</param>
		/// <param name="name">Type name</param>
		/// <param name="typeArgs">Collection of type arguments</param>
		public TypeInfo(bool isArray, string name, IEnumerable<TypeInfo> typeArgs = null)
		{
			this.Name = name;
			this.IsArray = IsArray;
			this.TypeArgs = new ReadOnlyCollection<TypeInfo>(
				new List<TypeInfo>(typeArgs ?? Enumerable.Empty<TypeInfo>()));
		}

		/// <summary>
		/// Gets a type name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets a value of whether this type indicates an Array type or not.
		/// </summary>
		public bool IsArray { get; }

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
				&& (this.IsArray == other.IsArray)
				&& (this.TypeArgs.SequenceEqual(other.TypeArgs));
		}

		public override int GetHashCode()
		{
			var argsHash = (this.TypeArgs.Count > 0) ? this.TypeArgs.Select(t => t?.GetHashCode() ?? 0).Aggregate((h1, h2) => h1 ^ h2) : 0;
			return this.Name.GetHashCode() ^ this.IsArray.GetHashCode() ^ argsHash;
		}

		public override string ToString()
		{
			var typeArgs = (this.TypeArgs.Count > 0) ? $"<{string.Join(",", this.TypeArgs)}>" : string.Empty;
			return this.Name + typeArgs + (this.IsArray ? "[]" : string.Empty);
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
			/// Gets or sets a value of whether this type indicates an Array type or not.
			/// </summary>
			public bool IsArray { get; set; }

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
				return new TypeInfo(this.IsArray, this.Name, this.TypeArgs.Select(m => m.ToImmutable()));
			}
		}
	}
}
