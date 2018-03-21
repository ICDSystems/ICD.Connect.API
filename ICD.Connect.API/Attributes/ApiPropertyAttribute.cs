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
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class ApiPropertyAttribute : AbstractApiAttribute
	{
		private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> s_Cache;

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
		public static ApiPropertyInfo GetInfo(PropertyInfo property)
		{
			ApiPropertyAttribute attribute = property == null ? null : GetAttribute(property);
			return new ApiPropertyInfo(attribute, property);
		}

		/// <summary>
		/// Gets the PropertyInfo for the given API info.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[CanBeNull]
		public static PropertyInfo GetProperty(ApiPropertyInfo info, Type type)
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

			s_Cache[type] = new Dictionary<string, PropertyInfo>();

			foreach (PropertyInfo property in GetProperties(type))
			{
				ApiPropertyAttribute attribute = GetAttribute(property);
				if (attribute == null)
					continue;

				s_Cache[type].Add(attribute.Name, property);
			}
		}

		public static IEnumerable<PropertyInfo> GetProperties(Type type)
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
					                .GetProperties(BindingFlags))
				    .Distinct(PropertyInfoApiEqualityComparer.Instance);
		}

		[CanBeNull]
		public static ApiPropertyAttribute GetAttribute(PropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			return property.GetCustomAttributes<ApiPropertyAttribute>(true).FirstOrDefault();
		}

		#endregion
	}
}
