using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
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
		private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> s_AttributeNameToProperty;
		private static readonly Dictionary<Type, PropertyInfo[]> s_TypeToProperties;
		private static readonly Dictionary<PropertyInfo, ApiNodeAttribute> s_PropertyToAttribute;

		private static readonly SafeCriticalSection s_AttributeNameToPropertySection;
		private static readonly SafeCriticalSection s_TypeToPropertiesSection;
		private static readonly SafeCriticalSection s_PropertyToAttributesSection;

		/// <summary>
		/// Static constructor.
		/// </summary>
		static ApiNodeAttribute()
		{
			s_AttributeNameToProperty = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
			s_TypeToProperties = new Dictionary<Type, PropertyInfo[]>();
			s_PropertyToAttribute = new Dictionary<PropertyInfo, ApiNodeAttribute>();

			s_AttributeNameToPropertySection = new SafeCriticalSection();
			s_TypeToPropertiesSection = new SafeCriticalSection();
			s_PropertyToAttributesSection = new SafeCriticalSection();
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
			if (info == null)
				throw new ArgumentNullException("info");

			if (type == null)
				throw new ArgumentNullException("type");

			s_AttributeNameToPropertySection.Enter();
			try
			{
				if (!s_AttributeNameToProperty.ContainsKey(type))
					CacheType(type);

				return s_AttributeNameToProperty[type].GetDefault(info.Name, null);
			}
			finally
			{
				s_AttributeNameToPropertySection.Leave();
			}
		}

		#endregion

		#region Private Methods

		private static void CacheType(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			s_AttributeNameToPropertySection.Enter();
			try
			{

				if (s_AttributeNameToProperty.ContainsKey(type))
					return;

				s_AttributeNameToProperty[type] = new Dictionary<string, PropertyInfo>();

				foreach (PropertyInfo property in GetProperties(type))
				{
					if (!property.CanRead)
						continue;

					ApiNodeAttribute attribute = GetAttribute(property);
					if (attribute == null)
						continue;

					s_AttributeNameToProperty[type].Add(attribute.Name, property);
				}
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
				if (!s_TypeToProperties.TryGetValue(type,out properties))
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
				s_TypeToPropertiesSection.Enter();
			}
		}

		[CanBeNull]
		public static ApiNodeAttribute GetAttribute(PropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("property");
			
			s_PropertyToAttributesSection.Enter();
			try
			{
				ApiNodeAttribute attribute;
				if (!s_PropertyToAttribute.TryGetValue(property, out attribute))
				{
					attribute = property.GetCustomAttributes<ApiNodeAttribute>(true).FirstOrDefault();
					s_PropertyToAttribute.Add(property, attribute);
				}

				return attribute;
			}
			finally
			{
				s_PropertyToAttributesSection.Leave();
			}
		}

		#endregion
	}
}
