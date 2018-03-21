using System.Collections.Generic;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.API.Comparers
{
	public class ApiPropertyInfoEqualityComparer : IEqualityComparer<PropertyInfo>
	{
		private static readonly ApiPropertyInfoEqualityComparer s_Instance;

		public static ApiPropertyInfoEqualityComparer Instance { get { return s_Instance; } }

		/// <summary>
		/// Static constructor.
		/// </summary>
		static ApiPropertyInfoEqualityComparer()
		{
			s_Instance = new ApiPropertyInfoEqualityComparer();
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
