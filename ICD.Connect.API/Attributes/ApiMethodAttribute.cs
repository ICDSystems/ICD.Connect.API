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
		private static readonly Dictionary<Type, Dictionary<string, MethodInfo>> s_AttributeNameToMethod;
		private static readonly Dictionary<Type, MethodInfo[]> s_TypeToMethods;
		private static readonly Dictionary<MethodInfo, ApiMethodAttribute> s_MethodToAttribute;

		/// <summary>
		/// Constructor.
		/// </summary>
		static ApiMethodAttribute()
		{
			s_AttributeNameToMethod = new Dictionary<Type, Dictionary<string, MethodInfo>>();
			s_TypeToMethods = new Dictionary<Type, MethodInfo[]>();
			s_MethodToAttribute = new Dictionary<MethodInfo, ApiMethodAttribute>();
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
			if (info == null)
				throw new ArgumentNullException("info");

			if (type == null)
				throw new ArgumentNullException("type");

			if (!s_AttributeNameToMethod.ContainsKey(type))
				CacheType(type);

			return s_AttributeNameToMethod[type].GetDefault(info.Name, null);
		}

		#endregion

		#region Private Methods

		private static void CacheType(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (s_AttributeNameToMethod.ContainsKey(type))
				return;

			s_AttributeNameToMethod[type] = new Dictionary<string, MethodInfo>();

			foreach (MethodInfo method in GetMethods(type))
			{
				ApiMethodAttribute attribute = GetAttribute(method);
				if (attribute == null)
					continue;

				s_AttributeNameToMethod[type].Add(attribute.Name, method);
			}
		}

		public static IEnumerable<MethodInfo> GetMethods(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (!s_TypeToMethods.ContainsKey(type))
			{
				MethodInfo[] methods =
					type.GetAllTypes()
					    .SelectMany(t =>
#if SIMPLSHARP
					                ((CType)t)
#else
										t.GetTypeInfo()
#endif
						                .GetMethods(BindingFlags))
					    .Where(m => !m.IsGenericMethod)
					    .Where(m => GetAttribute(m) != null)
					    .Distinct(MethodInfoApiEqualityComparer.Instance)
					    .ToArray();

				s_TypeToMethods.Add(type, methods);
			}

			return s_TypeToMethods[type];
		}

		[CanBeNull]
		public static ApiMethodAttribute GetAttribute(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			if (!s_MethodToAttribute.ContainsKey(method))
			{
				ApiMethodAttribute attribute = method.GetCustomAttributes<ApiMethodAttribute>(true).FirstOrDefault();
				s_MethodToAttribute.Add(method, attribute);
			}

			return s_MethodToAttribute[method];
		}

		#endregion
	}
}
