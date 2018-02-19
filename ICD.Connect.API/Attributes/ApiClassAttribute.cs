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
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public sealed class ApiClassAttribute : AbstractApiAttribute
	{
		private readonly Type[] m_ProxyTypes;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="proxyTypes"></param>
		public ApiClassAttribute(params Type[] proxyTypes)
			: this(null, null, proxyTypes)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="proxyTypes"></param>
		public ApiClassAttribute(string name, string help, params Type[] proxyTypes)
			: base(name, help)
		{
			m_ProxyTypes = proxyTypes.Distinct().ToArray();
		}

		/// <summary>
		/// Gets the proxy types that can control this class.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Type> GetProxyTypes()
		{
			return m_ProxyTypes.ToArray(m_ProxyTypes.Length);
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
		public static ApiClassAttribute GetAttribute(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			return
#if SIMPLSHARP
				((CType)type)
#else
				type
#endif
					.GetCustomAttributes<ApiClassAttribute>(true).FirstOrDefault();
		}
	}
}
