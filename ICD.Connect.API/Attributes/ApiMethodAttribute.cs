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
		/// Gets the binding flags for API method discovery.
		/// </summary>
		public new static BindingFlags BindingFlags { get { return AbstractApiAttribute.BindingFlags; } }

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
#if SIMPLSHARP
				((CType)type)
#else
				type.GetTypeInfo()
#endif
					.GetMethods(BindingFlags);
		}

		[CanBeNull]
		public static ApiMethodAttribute GetMethodAttributeForMethod(MethodInfo property)
		{
			return property.GetCustomAttributes<ApiMethodAttribute>(true).FirstOrDefault();
		}

		#endregion
	}
}
