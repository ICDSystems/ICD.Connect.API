using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.API.Comparers
{
	public class ApiMethodInfoEqualityComparer : IEqualityComparer<MethodInfo>
	{
		private static readonly ApiMethodInfoEqualityComparer s_Instance;

		public static ApiMethodInfoEqualityComparer Instance { get { return s_Instance; } }

		/// <summary>
		/// Static constructor.
		/// </summary>
		static ApiMethodInfoEqualityComparer()
		{
			s_Instance = new ApiMethodInfoEqualityComparer();
		}

		public bool Equals(MethodInfo a, MethodInfo b)
		{
			return a.Name == b.Name && a.GetParameters().SequenceEqual(b.GetParameters(), ParamComparer);
		}

		private static bool ParamComparer(ParameterInfo arg1, ParameterInfo arg2)
		{
			return arg1.Position == arg2.Position && arg1.ParameterType == arg2.ParameterType;
		}

		public int GetHashCode(MethodInfo info)
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + info.Name.GetHashCode();

				foreach (ParameterInfo param in info.GetParameters())
				{
					hash = hash * 23 + param.Position;
					hash = hash * 23 + param.ParameterType.GetHashCode();
				}

				return hash;
			}
		}
	}
}
