using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Comparers;
using ICD.Connect.API.Info;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.API.Attributes
{
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class ApiNodeAttribute : AbstractApiAttribute
	{
		private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> s_Cache;

		/// <summary>
		/// Static constructor.
		/// </summary>
		static ApiNodeAttribute()
		{
			s_Cache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		public ApiNodeAttribute(string name, string help)
			: base(name, help)
		{
		}

		#region Methods

		/// <summary>
		/// Returns the info for the attribute.
		/// </summary>
		/// <returns></returns>
		public ApiNodeInfo GetInfo(PropertyInfo memberInfo)
		{
			return new ApiNodeInfo(this, memberInfo);
		}

		/// <summary>
		/// Gets the PropertyInfo for the given API info.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[CanBeNull]
		public static PropertyInfo GetProperty(ApiNodeInfo info, Type type)
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
				if (!property.CanRead)
					continue;

				ApiNodeAttribute attribute = GetAttribute(property);
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
				    .Distinct(ApiPropertyInfoEqualityComparer.Instance);
		}

		[CanBeNull]
		public static ApiNodeAttribute GetAttribute(PropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			return property.GetCustomAttributes<ApiNodeAttribute>(true).FirstOrDefault();
		}

		#endregion
	}
}
