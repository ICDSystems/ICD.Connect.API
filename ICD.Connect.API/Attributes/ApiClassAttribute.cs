using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
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
		private static readonly Dictionary<Type, ApiClassAttribute[]> s_TypeAttributes;
		private static readonly Dictionary<Type, Type[]> s_TypeProxyTypes;

		private readonly Type m_ProxyType;
		private readonly Type m_Overrides;

		/// <summary>
		/// Gets the proxy type that can represent the decorated class.
		/// </summary>
		[CanBeNull]
		public Type ProxyType { get { return m_ProxyType; } }

		/// <summary>
		/// Used to enforce an inheritance tree through both classes and interfaces.
		/// </summary>
		[CanBeNull]
		public Type Overrides { get { return m_Overrides; } }

		#region Constructors

		/// <summary>
		/// Static constructor.
		/// </summary>
		static ApiClassAttribute()
		{
			s_AttributeCache = new Dictionary<Type, ApiClassAttribute>();
			s_TypeAttributes = new Dictionary<Type, ApiClassAttribute[]>();
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
			: this(proxyType, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="proxyType"></param>
		/// <param name="overrides"></param>
		public ApiClassAttribute(Type proxyType, Type overrides)
			: this(null, null, proxyType, overrides)
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
		/// <param name="overrides"></param>
		public ApiClassAttribute(string name, string help, Type overrides)
			: this(name, help, null, overrides)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="proxyType"></param>
		/// <param name="overrides"></param>
		public ApiClassAttribute(string name, string help, Type proxyType, Type overrides)
			: base(name, help)
		{
			m_ProxyType = proxyType;
			m_Overrides = overrides;
		}

		#endregion

		#region Methods

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
			ApiClassAttribute attribute = type == null ? null : GetAttributes(type).FirstOrDefault();
			return new ApiClassInfo(attribute, type, instance, depth);
		}

		/// <summary>
		/// Returns an ordered sequence of proxy types for the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<Type> GetProxyTypes(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (!s_TypeProxyTypes.ContainsKey(type))
			{
				Type[] proxyTypes =
					GetAttributes(type).Select(a => a.ProxyType)
					                   .Where(t => t != null)
					                   .Distinct()
					                   .ToArray();

				s_TypeProxyTypes.Add(type, proxyTypes);
			}

			return s_TypeProxyTypes[type];
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns the ordered inheritance tree of class attributes.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		private static IEnumerable<ApiClassAttribute> GetAttributes(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (!s_TypeAttributes.ContainsKey(type))
			{
				List<ApiClassAttribute> attributes = new List<ApiClassAttribute>();

				// Get the attribute for the immediate type.
				ApiClassAttribute attribute = GetAttribute(type);
				if (attribute != null)
					attributes.Add(attribute);

				// Recurse over the overridden types.
				Type overrides = attribute == null ? null : attribute.Overrides;
				if (overrides == type)
					throw new InvalidOperationException(string.Format("Cyclic override of {0}", type.Name));

				if (overrides != null)
					attributes.AddRange(GetAttributes(overrides));

				s_TypeAttributes.Add(type, attributes.ToArray(attributes.Count));
			}

			return s_TypeAttributes[type];
		}

		[CanBeNull]
		private static ApiClassAttribute GetAttribute(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (!s_AttributeCache.ContainsKey(type))
			{
				// First see if we can find an attribute on the class
				ApiClassAttribute attribute =
#if SIMPLSHARP
					((CType)type)
#else
					type
#endif
						.GetCustomAttributes<ApiClassAttribute>(true)
						.FirstOrDefault();

				// Try walking the interfaces
				if (attribute == null)
				{
					IEnumerable<Type> interfaces =
						RecursionUtils.BreadthFirstSearch(type, t => t.GetMinimalInterfaces())
						              .Except(type);

					foreach (Type item in interfaces)
					{
						attribute =
#if SIMPLSHARP
							((CType)item)
#else
							item
#endif
								.GetCustomAttributes<ApiClassAttribute>(false)
								.FirstOrDefault();

						if (attribute != null)
							break;
					}
				}

				s_AttributeCache.Add(type, attribute);
			}

			return s_AttributeCache[type];
		}

		#endregion
	}
}
