using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Comparers;
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
		public static ApiMethodInfo GetInfo(MethodInfo method)
		{
			return GetInfo(method, null);
		}

		/// <summary>
		/// Gets the method info for the given member.
		/// </summary>
		/// <param name="method"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static ApiMethodInfo GetInfo(MethodInfo method, object instance)
		{
			return GetInfo(method, instance, int.MaxValue);
		}

		/// <summary>
		/// Gets the method info for the given member.
		/// </summary>
		/// <param name="method"></param>
		/// <param name="instance"></param>
		/// <param name="depth"></param>
		/// <returns></returns>
		public static ApiMethodInfo GetInfo(MethodInfo method, object instance, int depth)
		{
			ApiMethodAttribute attribute = method == null ? null : GetAttribute(method);
			return new ApiMethodInfo(attribute, method, instance, depth);
		}

		/// <summary>
		/// Gets the MethodInfo for the given API info.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[CanBeNull]
		public static MethodInfo GetMethod(ApiMethodInfo info, Type type)
		{
			if (!s_Cache.ContainsKey(type))
				CacheType(type);

			return s_Cache[type].GetDefault(info.Name, null);
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
				ApiMethodAttribute attribute = GetAttribute(method);
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
				    .Distinct(ApiMethodInfoEqualityComparer.Instance);
		}

		

		[CanBeNull]
		public static ApiMethodAttribute GetAttribute(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			return method.GetCustomAttributes<ApiMethodAttribute>(true).FirstOrDefault();
		}

		#endregion
	}
}
