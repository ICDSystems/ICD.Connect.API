using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
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

		private static readonly SafeCriticalSection s_AttributeNameToMethodSection;
		private static readonly SafeCriticalSection s_TypeToMethodsSection;
		private static readonly SafeCriticalSection s_MethodToAttributeSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		static ApiMethodAttribute()
		{
			s_AttributeNameToMethod = new Dictionary<Type, Dictionary<string, MethodInfo>>();
			s_TypeToMethods = new Dictionary<Type, MethodInfo[]>();
			s_MethodToAttribute = new Dictionary<MethodInfo, ApiMethodAttribute>();

			s_AttributeNameToMethodSection = new SafeCriticalSection();
			s_TypeToMethodsSection = new SafeCriticalSection();
			s_MethodToAttributeSection = new SafeCriticalSection();
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

			s_AttributeNameToMethodSection.Enter();

			try
			{
				return CacheType(type).GetDefault(info.Name, null);
			}
			finally
			{
				s_AttributeNameToMethodSection.Leave();
			}
		}

		/// <summary>
		/// Gets the API methods on the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<MethodInfo> GetMethods(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");
			
			s_TypeToMethodsSection.Enter();

			try
			{
				MethodInfo[] methods;
				if (!s_TypeToMethods.TryGetValue(type, out methods))
				{
					methods =
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

				return methods;
			}
			finally
			{
				s_TypeToMethodsSection.Leave();
			}
		}

		[CanBeNull]
		public static ApiMethodAttribute GetAttribute(MethodInfo method)
		{
			if (method == null)
				throw new ArgumentNullException("method");

			s_MethodToAttributeSection.Enter();

			try
			{
				ApiMethodAttribute attribute;
				if (!s_MethodToAttribute.TryGetValue(method, out attribute))
				{
					attribute = method.GetCustomAttributes<ApiMethodAttribute>(true).FirstOrDefault();
					s_MethodToAttribute.Add(method, attribute);
				}

				return attribute;
			}
			finally
			{
				s_MethodToAttributeSection.Leave();
			}
		}

		#endregion

		#region Private Methods

		[NotNull]
		private static Dictionary<string, MethodInfo> CacheType(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_AttributeNameToMethodSection.Enter();

			try
			{
				Dictionary<string, MethodInfo> methodMap;
				if (!s_AttributeNameToMethod.TryGetValue(type, out methodMap))
				{
					methodMap = new Dictionary<string, MethodInfo>();
					s_AttributeNameToMethod.Add(type, methodMap);

					foreach (MethodInfo method in GetMethods(type))
					{
						ApiMethodAttribute attribute = GetAttribute(method);
						if (attribute == null)
							continue;

						if (methodMap.ContainsKey(attribute.Name))
							throw new InvalidProgramException(string.Format("{0} has multiple {1}s with name {2}", type.Name,
							                                                typeof(ApiMethodAttribute), attribute.Name));

						methodMap.Add(attribute.Name, method);
					}
				}

				return methodMap;
			}
			finally
			{
				s_AttributeNameToMethodSection.Leave();
			}
		}

		#endregion
	}
}
