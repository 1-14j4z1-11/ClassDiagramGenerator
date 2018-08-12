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
	/// The class possessing field (property, indexer) information.
	/// </summary>
	public class FieldInfo
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="modifier">Modifier</param>
		/// <param name="name">Field name</param>
		/// <param name="type">Filed type</param>
		/// <param name="indexerArgs">Collection containing indexer's arguments (Indexer only)</param>
		public FieldInfo(Modifier modifier, string name, TypeInfo type, IEnumerable<ArgumentInfo> indexerArgs = null)
		{
			this.Modifier = modifier;
			this.Name = name;
			this.Type = type;
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
		/// Gets a list of indexer's arguments.
		/// <para>If this is not indexer, return empty list.</para>
		/// </summary>
		public IReadOnlyList<ArgumentInfo> IndexerArguments { get; }

		/// <summary>
		/// Returns a collection of type names related with this field.
		/// </summary>
		/// <returns>A collection of type names related with this field</returns>
		public IEnumerable<string> GetRelatedTypeNames()
		{
			var allTypeNames = new List<string>(this.Type.GetContainedTypeNames());
			allTypeNames.AddRange(this.IndexerArguments.Select(a => a.Type.GetContainedTypeNames()).SelectMany(t => t));
			return allTypeNames;
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
