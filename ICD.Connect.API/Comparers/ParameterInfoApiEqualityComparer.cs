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

		public bool Equals(ParameterInfo a, ParameterInfo b)
		{
			return a.Position == b.Position && a.ParameterType == b.ParameterType;
		}

		public int GetHashCode(ParameterInfo info)
		{
			unchecked
			{
				int hash = 17;

				hash = hash * 23 + info.Position;
				hash = hash * 23 + info.ParameterType.GetHashCode();

				return hash;
			}
		}
	}
}
