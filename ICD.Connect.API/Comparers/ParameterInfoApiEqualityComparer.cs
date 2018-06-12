using System.Collections.Generic;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.API.Comparers
{
	public sealed class ParameterInfoApiEqualityComparer : IEqualityComparer<ParameterInfo>
	{
		private static ParameterInfoApiEqualityComparer s_Instance;

		public static ParameterInfoApiEqualityComparer Instance
		{
			get { return s_Instance ?? (s_Instance = new ParameterInfoApiEqualityComparer()); }
		}

		public bool Equals(ParameterInfo x, ParameterInfo y)
		{
			return x.Position == y.Position && x.ParameterType == y.ParameterType;
		}

		public int GetHashCode(ParameterInfo obj)
		{
			unchecked
			{
				int hash = 17;

				hash = hash * 23 + obj.Position;
				hash = hash * 23 + obj.ParameterType.GetHashCode();

				return hash;
			}
		}
	}
}
