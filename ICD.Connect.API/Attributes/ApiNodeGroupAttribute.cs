using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
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

		private static readonly SafeCriticalSection s_AttributeNameToPropertySection;
		private static readonly SafeCriticalSection s_TypeToPropertiesSection;
		private static readonly SafeCriticalSection s_PropertyToAttributeSection;

		/// <summary>
		/// Static constructor.
		/// </summary>
		static ApiNodeGroupAttribute()
		{
			s_AttributeNameToProperty = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
			s_TypeToProperties = new Dictionary<Type, PropertyInfo[]>();
			s_PropertyToAttribute = new Dictionary<PropertyInfo, ApiNodeGroupAttribute>();

			s_AttributeNameToPropertySection = new SafeCriticalSection();
			s_TypeToPropertiesSection = new SafeCriticalSection();
			s_PropertyToAttributeSection = new SafeCriticalSection();
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
		public static ApiNodeGroupAttribute GetAttribute(PropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("property");

			s_PropertyToAttributeSection.Enter();

			try
			{
				ApiNodeGroupAttribute attribute;
				if (!s_PropertyToAttribute.TryGetValue(property, out attribute))
				{
					attribute = property.GetCustomAttributes<ApiNodeGroupAttribute>(true).FirstOrDefault();
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
						if (!property.CanRead)
							continue;

						if (!typeof(IApiNodeGroup).IsAssignableFrom(property.PropertyType))
							continue;

						ApiNodeGroupAttribute attribute = GetAttribute(property);
						if (attribute == null)
							continue;

						if (propertyMap.ContainsKey(attribute.Name))
							throw new InvalidProgramException(string.Format("{0} has multiple {1}s with name {2}", type.Name,
							                                                typeof(ApiNodeGroupAttribute), attribute.Name));

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
