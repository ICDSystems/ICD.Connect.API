using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Connect.API.Attributes;
using ICD.Common.Utils.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif

namespace ICD.Connect.API.Info
{
	public sealed class ApiClassInfo : AbstractApiInfo
	{
		private const string PROPERTY_METHODS = "methods";
		private const string PROPERTY_PROPERTIES = "properties";

		private readonly List<ApiMethodInfo> m_Methods;
		private readonly List<ApiPropertyInfo> m_Properties;

		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiClassInfo()
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="type"></param>
		public ApiClassInfo(ApiClassAttribute attribute, Type type)
			: base(attribute)
		{
			IEnumerable<ApiMethodInfo> parameters = GetMethodInfo(type);
			m_Methods = new List<ApiMethodInfo>(parameters);

			IEnumerable<ApiPropertyInfo> properties = GetPropertyInfo(type);
			m_Properties = new List<ApiPropertyInfo>(properties);
		}

		#region Methods

		/// <summary>
		/// Gets the methods for this class.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ApiMethodInfo> GetMethods()
		{
			return m_Methods.ToArray(m_Methods.Count);
		}

		/// <summary>
		/// Sets the methods for this class.
		/// </summary>
		/// <param name="methods"></param>
		public void SetMethods(IEnumerable<ApiMethodInfo> methods)
		{
			m_Methods.Clear();
			m_Methods.AddRange(methods);
		}

		/// <summary>
		/// Gets the properties for this class.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ApiPropertyInfo> GetProperties()
		{
			return m_Properties.ToArray(m_Properties.Count);
		}

		/// <summary>
		/// Sets the properties for this class.
		/// </summary>
		/// <param name="properties"></param>
		public void SetProperties(IEnumerable<ApiPropertyInfo> properties)
		{
			m_Properties.Clear();
			m_Properties.AddRange(properties);
		}

		#endregion

		#region Private Methods

		private IEnumerable<ApiPropertyInfo> GetPropertyInfo(Type type)
		{
			foreach (PropertyInfo property in
#if SIMPLSHARP
				((CType)type)
#else
				type.GetTypeInfo()
#endif
					.GetProperties(ApiPropertyAttribute.BindingFlags))
			{
				ApiPropertyAttribute attribute = GetPropertyAttributeForMethod(property);
				if (attribute == null)
					continue;

				yield return new ApiPropertyInfo(attribute, property) ;
			}
		}

		private ApiPropertyAttribute GetPropertyAttributeForMethod(PropertyInfo property)
		{
			return property.GetCustomAttributes<ApiPropertyAttribute>(true).FirstOrDefault();
		}

		private IEnumerable<ApiMethodInfo> GetMethodInfo(Type type)
		{
			foreach (MethodInfo method in
#if SIMPLSHARP
				((CType)type)
#else
				type.GetTypeInfo()
#endif
				    .GetMethods(ApiMethodAttribute.BindingFlags))
			{
				ApiMethodAttribute attribute = GetMethodAttributeForMethod(method);
				if (attribute == null)
					continue;

				yield return new ApiMethodInfo(attribute, method);
			}
		}

		[CanBeNull]
		private ApiMethodAttribute GetMethodAttributeForMethod(MethodInfo method)
		{
			return method.GetCustomAttributes<ApiMethodAttribute>(true).FirstOrDefault();
		}

		#endregion

		#region Serialization

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteProperties(JsonWriter writer)
		{
			base.WriteProperties(writer);

			// Methods
			if (m_Methods.Count > 0)
			{
				writer.WritePropertyName(PROPERTY_METHODS);
				writer.WriteStartArray();
				{
					foreach (ApiMethodInfo method in m_Methods)
						method.Serialize(writer);
				}
				writer.WriteEndArray();
			}

			// Properties
			if (m_Properties.Count > 0)
			{
				writer.WritePropertyName(PROPERTY_PROPERTIES);
				writer.WriteStartArray();
				{
					foreach (ApiPropertyInfo property in m_Properties)
						property.Serialize(writer);
				}
				writer.WriteEndArray();
			}
		}

		/// <summary>
		/// Deserializes the JSON string to an ApiClassInfo instance.
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static ApiClassInfo Deserialize(string json)
		{
			JObject jObject = JObject.Parse(json);
			return Deserialize(jObject);
		}

		/// <summary>
		/// Instanties a new instance and applies the JSON object.
		/// </summary>
		/// <param name="jObject"></param>
		/// <returns></returns>
		public static ApiClassInfo Deserialize(JObject jObject)
		{
			ApiClassInfo instance = new ApiClassInfo();
			Deserialize(instance, jObject);
			return instance;
		}

		/// <summary>
		/// Applies the JSON object info to the given instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="json"></param>
		/// <returns></returns>
		public static void Deserialize(ApiClassInfo instance, JObject json)
		{
			// Methods
			JToken methods = json[PROPERTY_METHODS];
			if (methods != null)
			{
				IEnumerable<ApiMethodInfo> methodInfo = methods.Select(m => ApiMethodInfo.Deserialize(m));
				instance.SetMethods(methodInfo);
			}

			// Properties
			JToken properties = json[PROPERTY_PROPERTIES];
			if (properties != null)
			{
				IEnumerable<ApiPropertyInfo> propertyInfo = properties.Select(p => ApiPropertyInfo.Deserialize(p));
				instance.SetProperties(propertyInfo);
			}

			AbstractApiInfo.Deserialize(instance, json);
		}

		#endregion
	}
}