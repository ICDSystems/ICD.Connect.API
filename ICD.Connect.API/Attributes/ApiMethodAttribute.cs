using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.API.Info;

namespace ICD.Connect.API.Attributes
{
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public sealed class ApiMethodAttribute : AbstractApiAttribute
	{
		private static readonly Dictionary<Type, Dictionary<string, MethodInfo>> s_Cache;

		/// <summary>
		/// Constructor.
		/// </summary>
		static ApiMethodAttribute()
		{
			s_Cache = new Dictionary<Type, Dictionary<string, MethodInfo>>();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		public ApiMethodAttribute(string name, string help)
			: base(name, help)
		{
		}

		#region Methods

		/// <summary>
		/// Gets the method info for the given member.
		/// </summary>
		/// <param name="method"></param>
		/// <returns></returns>
		public ApiMethodInfo GetInfo(MethodInfo method)
		{
			return new ApiMethodInfo(this, method);
		}

		/// <summary>
		/// Gets the MethodInfo for the given API info.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static MethodInfo GetMethod(ApiMethodInfo info, Type type)
		{
			if (!s_Cache.ContainsKey(type))
				CacheType(type);

			return s_Cache[type][info.Name];
		}

		#endregion

		#region Private Methods

		private static void CacheType(Type type)
		{
			if (s_Cache.ContainsKey(type))
				return;

			s_Cache[type] = new Dictionary<string, MethodInfo>();

			foreach (MethodInfo method in GetMethods(type))
			{
				ApiMethodAttribute attribute = GetMethodAttributeForMethod(method);
				if (attribute == null)
					continue;

				s_Cache[type].Add(attribute.Name, method);
			}
		}

		public static IEnumerable<MethodInfo> GetMethods(Type type)
		{
			return
				type.GetBaseTypes()
				    .Prepend(type)
				    .SelectMany(t =>
#if SIMPLSHARP
				                ((CType)t)
#else
						        t.GetTypeInfo()
#endif
					                .GetMethods(BindingFlags))
				    .Distinct(MethodComparer, GetMethodHashCode);
		}

		private static bool MethodComparer(MethodInfo a, MethodInfo b)
		{
			return a.Name == b.Name && a.GetParameters().SequenceEqual(b.GetParameters(), ParamComparer);
		}

		private static bool ParamComparer(ParameterInfo arg1, ParameterInfo arg2)
		{
			return arg1.Position == arg2.Position && arg1.ParameterType == arg2.ParameterType;
		}

		private static int GetMethodHashCode(MethodInfo info)
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

		[CanBeNull]
		public static ApiMethodAttribute GetMethodAttributeForMethod(MethodInfo property)
		{
			return property.GetCustomAttributes<ApiMethodAttribute>(true).FirstOrDefault();
		}

		#endregion
	}
}
