using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Info;
using ICD.Connect.API.Nodes;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.API.Attributes
{
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class ApiNodeGroupAttribute : AbstractApiAttribute
	{
		private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> s_Cache;

		/// <summary>
		/// Static constructor.
		/// </summary>
		static ApiNodeGroupAttribute()
		{
			s_Cache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		public ApiNodeGroupAttribute(string name, string help)
			: base(name, help)
		{
		}

		#region Methods

		/// <summary>
		/// Returns the info for the attribute.
		/// </summary>
		/// <returns></returns>
		public ApiNodeGroupInfo GetInfo(PropertyInfo memberInfo)
		{
			return new ApiNodeGroupInfo(this, memberInfo);
		}

		/// <summary>
		/// Gets the PropertyInfo for the given API info.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static PropertyInfo GetProperty(ApiNodeInfo info, Type type)
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
				if (!property.CanRead)
					continue;

				if (!typeof(IApiNodeGroup).IsAssignableFrom(property.PropertyType))
					continue;

				ApiNodeGroupAttribute attribute = GetNodeGroupAttributeForProperty(property);
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
		public static ApiNodeGroupAttribute GetNodeGroupAttributeForProperty(PropertyInfo property)
		{
			return property.GetCustomAttributes<ApiNodeGroupAttribute>(true).FirstOrDefault();
		}

		#endregion
	}
}
