//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ClassDiagramGenerator.Models.Structure
{
	/// <summary>
	/// Class possessing field (property, indexer) information.
	/// </summary>
	public class FieldInfo
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="modifier">Modifier</param>
		/// <param name="name">Field name</param>
		/// <param name="type">Filed type</param>
		/// <param name="propertyType">Property type</param>
		/// <param name="indexerArgs">Collection containing indexer's arguments (Indexer only)</param>
		public FieldInfo(Modifier modifier, string name, TypeInfo type, PropertyType propertyType = PropertyType.None, IEnumerable<ArgumentInfo> indexerArgs = null)
		{
			this.Modifier = modifier;
			this.Name = name;
			this.Type = type;
			this.PropertyType = propertyType;
			this.IndexerArguments = new ReadOnlyCollection<ArgumentInfo>(
				new List<ArgumentInfo>(indexerArgs ?? Enumerable.Empty<ArgumentInfo>()));
		}

		/// <summary>
		/// Gets a modifier of field.
		/// </summary>
		public Modifier Modifier { get; }

		/// <summary>
		/// Gets a field name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets a field type.
		/// </summary>
		public TypeInfo Type { get; }

		/// <summary>
		/// Gets a <see cref="PropertyType"/>.
		/// </summary>
		public PropertyType PropertyType { get; }

		/// <summary>
		/// Gets a list of indexer's arguments.
		/// <para>If this is not indexer, return empty list.</para>
		/// </summary>
		public IReadOnlyList<ArgumentInfo> IndexerArguments { get; }

		/// <summary>
		/// Returns a collection of types related with this field.
		/// </summary>
		/// <returns>A collection of types related with this field</returns>
		public IEnumerable<TypeInfo> GetRelatedTypes()
		{
			var allTypes = new List<TypeInfo>(this.Type.GetContainedTypes());
			allTypes.AddRange(this.IndexerArguments.Select(a => a.Type.GetContainedTypes()).SelectMany(t => t));
			return allTypes;
		}

		public override bool Equals(object obj)
		{
			if(!(obj is FieldInfo other))
				return false;

			return (this.Modifier == other.Modifier)
				&& (this.Name == other.Name)
				&& Equals(this.Type, other.Type)
				&& (this.IndexerArguments.SequenceEqual(other.IndexerArguments));
		}

		public override int GetHashCode()
		{
			var argsHash = (this.IndexerArguments.Count > 0) ? this.IndexerArguments.Select(a => a?.GetHashCode() ?? 0).Aggregate((h1, h2) => h1 ^ h2) : 0;

			return this.Modifier.GetHashCode()
				^ (this.Name?.GetHashCode() ?? 0)
				^ (this.Type?.GetHashCode() ?? 0)
				^ argsHash;
		}
	}
}
