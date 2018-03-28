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
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
	public sealed class ApiClassAttribute : AbstractApiAttribute
	{
		private static readonly Dictionary<Type, ApiClassAttribute> s_AttributeCache;
		private static readonly Dictionary<Type, Type[]> s_TypeProxyTypes;

		private readonly Type m_ProxyType;

		/// <summary>
		/// Gets the proxy type that can represent the decorated class.
		/// </summary>
		[CanBeNull]
		public Type ProxyType { get { return m_ProxyType; } }

		/// <summary>
		/// Static constructor.
		/// </summary>
		static ApiClassAttribute()
		{
			s_AttributeCache = new Dictionary<Type, ApiClassAttribute>();
			s_TypeProxyTypes = new Dictionary<Type, Type[]>();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiClassAttribute()
			: this(null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiClassAttribute(Type proxyType)
			: this(null, null, proxyType)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		public ApiClassAttribute(string name, string help)
			: this(name, help, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="proxyType"></param>
		public ApiClassAttribute(string name, string help, Type proxyType)
			: base(name, help)
		{
			m_ProxyType = proxyType;
		}

		public static IEnumerable<Type> GetProxyTypes(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (!s_TypeProxyTypes.ContainsKey(type))
			{
				// First get the proxy type for the immediate type
				ApiClassAttribute attribute = GetAttribute(type);
				Type typeProxyType = attribute == null ? null : attribute.ProxyType;

				// Then get the interface proxy types
				IEnumerable<Type> interfaceProxyTypes =
					type.GetMinimalInterfaces()
					    .SelectMany(i => GetProxyTypes(i));

				// Finally get the proxy types from base classes
				IEnumerable<Type> classProxyTypes =
					type.IsClass
						? type.GetBaseTypes()
						      .Prepend(type)
						      .Select(t => GetAttribute(t))
						      .Where(a => a != null)
						      .Select(a => a.ProxyType)
						: Enumerable.Empty<Type>();

				// Class defined proxy types override interface proxy types
				Type[] proxyTypes =
					classProxyTypes.Concat(interfaceProxyTypes)
					               .Prepend(typeProxyType)
					               .Where(t => t != null)
					               .Distinct()
					               .ToArray();

				s_TypeProxyTypes.Add(type, proxyTypes);
			}

			return s_TypeProxyTypes[type];
		}

		/// <summary>
		/// Gets the class info for the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ApiClassInfo GetInfo(Type type)
		{
			return GetInfo(type, null);
		}

		/// <summary>
		/// Gets the class info for the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static ApiClassInfo GetInfo(Type type, object instance)
		{
			return GetInfo(type, instance, int.MaxValue);
		}

		/// <summary>
		/// Gets the class info for the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="depth"></param>
		/// <returns></returns>
		public static ApiClassInfo GetInfo(Type type, object instance, int depth)
		{
			ApiClassAttribute attribute = type == null ? null : GetAttribute(type);
			return new ApiClassInfo(attribute, type, instance, depth);
		}

		[CanBeNull]
		private static ApiClassAttribute GetAttribute(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (!s_AttributeCache.ContainsKey(type))
			{
				ApiClassAttribute attribute =
#if SIMPLSHARP
					((CType)type)
#else
					type
#endif
						.GetCustomAttributes<ApiClassAttribute>(true)
						.FirstOrDefault();

				s_AttributeCache.Add(type, attribute);
			}

			return s_AttributeCache[type];
		}
	}
}
