using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Comparers;
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
		private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> s_AttributeNameToProperty;
		private static readonly Dictionary<Type, PropertyInfo[]> s_TypeToProperties;
		private static readonly Dictionary<PropertyInfo, ApiNodeGroupAttribute> s_PropertyToAttribute;

		/// <summary>
		/// Static constructor.
		/// </summary>
		static ApiNodeGroupAttribute()
		{
			s_AttributeNameToProperty = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
			s_TypeToProperties = new Dictionary<Type, PropertyInfo[]>();
			s_PropertyToAttribute = new Dictionary<PropertyInfo, ApiNodeGroupAttribute>();
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
		public static ApiNodeGroupInfo GetInfo(PropertyInfo property)
		{
			return GetInfo(property, null);
		}

		/// <summary>
		/// Returns the info for the attribute.
		/// </summary>
		/// <returns></returns>
		public static ApiNodeGroupInfo GetInfo(PropertyInfo property, object instance)
		{
			return GetInfo(property, instance, int.MaxValue);
		}

		/// <summary>
		/// Returns the info for the attribute.
		/// </summary>
		/// <returns></returns>
		public static ApiNodeGroupInfo GetInfo(PropertyInfo property, object instance, int depth)
		{
			ApiNodeGroupAttribute attribute = property == null ? null : GetAttribute(property);
			return new ApiNodeGroupInfo(attribute, property, instance, depth);
		}

		/// <summary>
		/// Gets the PropertyInfo for the given API info.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[CanBeNull]
		public static PropertyInfo GetProperty(ApiNodeGroupInfo info, Type type)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (type == null)
				throw new ArgumentNullException("type");

			if (!s_AttributeNameToProperty.ContainsKey(type))
				CacheType(type);

			return s_AttributeNameToProperty[type].GetDefault(info.Name, null);
		}

		#endregion

		#region Private Methods

		private static void CacheType(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (s_AttributeNameToProperty.ContainsKey(type))
				return;

			s_AttributeNameToProperty[type] = new Dictionary<string, PropertyInfo>();

			foreach (PropertyInfo property in GetProperties(type))
			{
				if (!property.CanRead)
					continue;

				if (!typeof(IApiNodeGroup).IsAssignableFrom(property.PropertyType))
					continue;

				ApiNodeGroupAttribute attribute = GetAttribute(property);
				if (attribute == null)
					continue;

				s_AttributeNameToProperty[type].Add(attribute.Name, property);
			}
		}

		public static IEnumerable<PropertyInfo> GetProperties(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (!s_TypeToProperties.ContainsKey(type))
			{
				PropertyInfo[] properties =
					type.GetAllTypes()
					    .SelectMany(t =>
#if SIMPLSHARP
					                ((CType)t)
#else
										t.GetTypeInfo()
#endif
						                .GetProperties(BindingFlags))
					    .Where(p => GetAttribute(p) != null)
					    .Distinct(PropertyInfoApiEqualityComparer.Instance)
					    .ToArray();

				s_TypeToProperties.Add(type, properties);
			}

			return s_TypeToProperties[type];
		}

		[CanBeNull]
		public static ApiNodeGroupAttribute GetAttribute(PropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			if (!s_PropertyToAttribute.ContainsKey(property))
			{
				ApiNodeGroupAttribute attribute = property.GetCustomAttributes<ApiNodeGroupAttribute>(true).FirstOrDefault();
				s_PropertyToAttribute.Add(property, attribute);
			}

			return s_PropertyToAttribute[property];
		}

		#endregion
	}
}
