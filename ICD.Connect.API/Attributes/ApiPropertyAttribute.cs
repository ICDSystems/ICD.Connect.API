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
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class ApiPropertyAttribute : AbstractApiAttribute
	{
		private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> s_Cache;

		/// <summary>
		/// Gets the binding flags for API property discovery.
		/// </summary>
		public new static BindingFlags BindingFlags { get { return AbstractApiAttribute.BindingFlags; } }

		/// <summary>
		/// Static constructor.
		/// </summary>
		static ApiPropertyAttribute()
		{
			s_Cache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		public ApiPropertyAttribute(string name, string help)
			: base(name, help)
		{
		}

		#region Methods

		/// <summary>
		/// Returns the info for the attribute.
		/// </summary>
		/// <returns></returns>
		public ApiPropertyInfo GetInfo(PropertyInfo memberInfo)
		{
			return new ApiPropertyInfo(this, memberInfo);
		}

		/// <summary>
		/// Gets the PropertyInfo for the given API info.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static PropertyInfo GetProperty(ApiPropertyInfo info, Type type)
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

			s_Cache[type] = new Dictionary<string, PropertyInfo>();

			foreach (PropertyInfo property in GetProperties(type))
			{
				ApiPropertyAttribute attribute = GetPropertyAttributeForProperty(property);
				if (attribute == null)
					continue;

				s_Cache[type].Add(attribute.Name, property);
			}
		}

		public static IEnumerable<PropertyInfo> GetProperties(Type type)
		{
			return
#if SIMPLSHARP
				((CType)type)
#else
				type.GetTypeInfo()
#endif
					.GetProperties(BindingFlags);
		}

		[CanBeNull]
		public static ApiPropertyAttribute GetPropertyAttributeForProperty(PropertyInfo property)
		{
			return property.GetCustomAttributes<ApiPropertyAttribute>(true).FirstOrDefault();
		}

		#endregion
	}
}
