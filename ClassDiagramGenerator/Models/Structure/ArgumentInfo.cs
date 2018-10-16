//
// Copyright (c) 2018 Yasuhiro Hayashi
//

using System;

namespace ClassDiagramGenerator.Models.Structure
{
	/// <summary>
	/// Class possessing argument information.
	/// </summary>
	public class ArgumentInfo
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type">Argument type</param>
		/// <param name="name">Argument name</param>
		/// <param name="modifier">Argument modifier (Default is <see cref="ArgumentModifier.None"/>)</param>
		/// <exception cref="ArgumentNullException">If <paramref name="type"/> is null</exception>
		public ArgumentInfo(TypeInfo type, string name, ArgumentModifier modifier = ArgumentModifier.None)
		{
			this.Type = type ?? throw new ArgumentNullException();
			this.Name = name;
			this.Modifier = modifier;
		}

		/// <summary>
		/// Gets an argument type.
		/// </summary>
		public TypeInfo Type { get; }

		/// <summary>
		/// Gets an argument name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets an argument modifier.
		/// </summary>
		public ArgumentModifier Modifier { get; }

		public override bool Equals(object obj)
		{
			if(!(obj is ArgumentInfo other))
				return false;

			return this.Type.Equals(other.Type)
				&& (this.Name == other.Name)
				&& (this.Modifier == other.Modifier);
		}

		public override int GetHashCode()
		{
			return (this.Type?.GetHashCode() ?? 0)
				^ (this.Name?.GetHashCode() ?? 0)
				^ this.Modifier.GetHashCode();
		}

		public override string ToString()
		{
			return $"{this.Modifier.ToModifierString()} {this.Type} {this.Name}";
		}
	}
}
