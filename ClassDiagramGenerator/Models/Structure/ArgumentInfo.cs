using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDiagramGenerator.Models.Structure
{
	/// <summary>
	/// The class possessing argument information.
	/// </summary>
	public class ArgumentInfo
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type">Argument type</param>
		/// <param name="name">Argument name</param>
		/// <exception cref="ArgumentNullException">If <paramref name="type"/> is null</exception>
		public ArgumentInfo(TypeInfo type, string name)
		{
			this.Type = type ?? throw new ArgumentNullException();
			this.Name = name;
		}

		/// <summary>
		/// Gets a argument type.
		/// </summary>
		public TypeInfo Type { get; }

		/// <summary>
		/// Gets a argument name.
		/// </summary>
		public string Name { get; }

		public override bool Equals(object obj)
		{
			if(!(obj is ArgumentInfo other))
				return false;

			return this.Type.Equals(other.Type) && (this.Name == other.Name);
		}

		public override int GetHashCode()
		{
			return (this.Type?.GetHashCode() ?? 0) ^ (this.Name?.GetHashCode() ?? 0);
		}

		public override string ToString()
		{
			return $"{this.Type} {this.Name}";
		}
	}
}
