using System.Collections.Generic;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.API.Comparers
{
	public class PropertyInfoApiEqualityComparer : IEqualityComparer<PropertyInfo>
	{
		private static readonly PropertyInfoApiEqualityComparer s_Instance;

		public static PropertyInfoApiEqualityComparer Instance { get { return s_Instance; } }

		/// <summary>
		/// Static constructor.
		/// </summary>
		static PropertyInfoApiEqualityComparer()
		{
			s_Instance = new PropertyInfoApiEqualityComparer();
		}

		public bool Equals(PropertyInfo a, PropertyInfo b)
		{
			return a.Name == b.Name;
		}

		public int GetHashCode(PropertyInfo info)
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + info.Name.GetHashCode();

				return hash;
			}
		}
	}
}
