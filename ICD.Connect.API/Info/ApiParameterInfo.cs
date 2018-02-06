﻿using System;
using ICD.Common.Utils.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
#else
using System.Reflection;
#endif
using ICD.Connect.API.Attributes;

namespace ICD.Connect.API.Info
{
	public sealed class ApiParameterInfo : AbstractApiInfo
	{
		private const string PROPERTY_TYPE = "type";
		private const string PROPERTY_VALUE = "value";

		/// <summary>
		/// Gets/sets the data type for the parameter.
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// Gets/sets the value for the parameter.
		/// </summary>
		public object Value { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public ApiParameterInfo()
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		/// <param name="parameter"></param>
		public ApiParameterInfo(ApiParameterAttribute attribute, ParameterInfo parameter)
			: base(attribute)
		{
			Type = parameter.ParameterType;
		}

		#region Serialization

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteProperties(JsonWriter writer)
		{
			base.WriteProperties(writer);

			if (Type != null)
			{
				writer.WritePropertyName(PROPERTY_TYPE);
				writer.WriteValue(Type.Name);
			}

			if (Value != null)
			{
				writer.WritePropertyName(PROPERTY_VALUE);
				writer.WriteValue(Value);
			}
		}

		/// <summary>
		/// Deserializes the JSON string to an ApiParameterInfo instance.
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static ApiParameterInfo Deserialize(string json)
		{
			JObject jObject = JObject.Parse(json);
			return Deserialize(jObject);
		}

		/// <summary>
		/// Instanties a new instance and applies the JSON object.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public static ApiParameterInfo Deserialize(JToken token)
		{
			ApiParameterInfo instance = new ApiParameterInfo();
			Deserialize(instance, token);
			return instance;
		}

		/// <summary>
		/// Applies the JSON object info to the given instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public static void Deserialize(ApiParameterInfo instance, JToken token)
		{
			// Type
			string typeName = (string)token[PROPERTY_TYPE];
			instance.Type = typeName == null ? null : Type.GetType(typeName, true, true);

			// Value
			instance.Value = JsonUtils.Deserialize(instance.Type, token[PROPERTY_VALUE]);

			AbstractApiInfo.Deserialize(instance, token);
		}

		#endregion
	}
}