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
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class ApiPropertyAttribute : AbstractApiAttribute
	{
		private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> s_AttributeNameToProperty;
		private static readonly Dictionary<Type, PropertyInfo[]> s_TypeToProperties;
		private static readonly Dictionary<PropertyInfo, ApiPropertyAttribute> s_PropertyToAttribute;

		private static readonly SafeCriticalSection s_AttributeNameToPropertySection;
		private static readonly SafeCriticalSection s_TypeToPropertiesSection;
		private static readonly SafeCriticalSection s_PropertyToAttributeSection;

		/// <summary>
		/// Static constructor.
		/// </summary>
		static ApiPropertyAttribute()
		{
			s_AttributeNameToProperty = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
			s_TypeToProperties = new Dictionary<Type, PropertyInfo[]>();
			s_PropertyToAttribute = new Dictionary<PropertyInfo, ApiPropertyAttribute>();

			s_AttributeNameToPropertySection = new SafeCriticalSection();
			s_TypeToPropertiesSection = new SafeCriticalSection();
			s_PropertyToAttributeSection = new SafeCriticalSection();
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
			if (info == null)
				throw new ArgumentNullException("info");

			if (type == null)
				throw new ArgumentNullException("type");

			s_AttributeNameToPropertySection.Enter();

			try
			{
				return CacheType(type).GetDefault(info.Name, null);
			}
			finally
			{
				s_AttributeNameToPropertySection.Leave();
			}
		}

		public static IEnumerable<PropertyInfo> GetProperties(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_TypeToPropertiesSection.Enter();

			try
			{
				PropertyInfo[] properties;
				if (!s_TypeToProperties.TryGetValue(type, out properties))
				{
					properties =
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

				return properties;
			}
			finally
			{
				s_TypeToPropertiesSection.Leave();
			}
		}

		[CanBeNull]
		public static ApiPropertyAttribute GetAttribute(PropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			s_PropertyToAttributeSection.Enter();

			try
			{
				ApiPropertyAttribute attribute;
				if (!s_PropertyToAttribute.TryGetValue(property, out attribute))
				{
					attribute = property.GetCustomAttributes<ApiPropertyAttribute>(true).FirstOrDefault();
					s_PropertyToAttribute.Add(property, attribute);
				}

				return attribute;
			}
			finally
			{
				s_PropertyToAttributeSection.Leave();
			}
		}

		#endregion

		#region Private Methods

		[NotNull]
		private static Dictionary<string, PropertyInfo> CacheType(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_AttributeNameToPropertySection.Enter();

			try
			{
				Dictionary<string, PropertyInfo> propertyMap;
				if (!s_AttributeNameToProperty.TryGetValue(type, out propertyMap))
				{
					propertyMap = new Dictionary<string, PropertyInfo>();
					s_AttributeNameToProperty.Add(type, propertyMap);

					foreach (PropertyInfo property in GetProperties(type))
					{
						ApiPropertyAttribute attribute = GetAttribute(property);
						if (attribute == null)
							continue;

						if (propertyMap.ContainsKey(attribute.Name))
							throw new InvalidProgramException(string.Format("{0} has multiple {1}s with name {2}", type.Name,
																			typeof(ApiPropertyAttribute), attribute.Name));

						propertyMap.Add(attribute.Name, property);
					}
				}

				return propertyMap;
			}
			finally
			{
				s_AttributeNameToPropertySection.Leave();
			}
		}

		#endregion
	}
}
