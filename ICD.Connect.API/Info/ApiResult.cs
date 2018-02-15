using System;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ICD.Connect.API.Info
{
	public sealed class ApiResult
	{
		private const string PROPERTY_ERRORCODE = "errorCode";
		private const string PROPERTY_TYPE = "type";
		private const string PROPERTY_VALUE = "value";

		public enum eErrorCode
		{
			Ok = 0,
			MissingMember = 1,
			MissingNode = 2,
			InvalidParameter = 3,
			Exception = 4
		}

		#region Properties

		/// <summary>
		/// The error code for this response.
		/// </summary>
		public eErrorCode ErrorCode { get; set; }

		/// <summary>
		/// Gets/sets the data type for the parameter.
		/// </summary>
		public Type Type { get; set; }

		/// <summary>
		/// Gets/sets the value for the parameter.
		/// </summary>
		public object Value { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Creates a recursive copy of the API response.
		/// </summary>
		/// <returns></returns>
		public ApiResult DeepCopy()
		{
			return new ApiResult
			{
				ErrorCode = ErrorCode,
				Type = Type,
				Value = Value
			};
		}

		/// <summary>
		/// Sets the value and type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		public void SetValue<T>(T value)
		{
// ReSharper disable once CompareNonConstrainedGenericWithNull
			Type type = value == null ? typeof(T) : value.GetType();
			SetValue(type, value);
		}

		/// <summary>
		/// Sets the value and type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		public void SetValue(Type type, object value)
		{
			Type = type;
			Value = value;
		}

		#endregion

		#region Serialization

		/// <summary>
		/// Serializes the instance to JSON.
		/// </summary>
		/// <returns></returns>
		public string Serialize()
		{
			return JsonUtils.Serialize(Serialize);
		}

		/// <summary>
		/// Serializes the instance to JSON.
		/// </summary>
		/// <param name="writer"></param>
		public void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();
			{
				// Error Code
				writer.WritePropertyName(PROPERTY_ERRORCODE);
				writer.WriteValue(ErrorCode);

				// Type
				if (Value != null && Type != null)
				{
					writer.WritePropertyName(PROPERTY_TYPE);
					writer.WriteType(Type);
				}

				// Value
				if (Value != null)
				{
					writer.WritePropertyName(PROPERTY_VALUE);

					IApiInfo info = Value as IApiInfo;
					if (info == null)
						writer.WriteValue(Value);
					else
						info.Serialize(writer);
				}
			}
			writer.WriteEndObject();
		}

		/// <summary>
		/// Deserializes the JSON string to an ApiParameterInfo instance.
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static ApiResult Deserialize(string json)
		{
			JObject jObject = JObject.Parse(json);
			return Deserialize(jObject);
		}

		/// <summary>
		/// Instanties a new instance and applies the JSON object.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public static ApiResult Deserialize(JToken token)
		{
			ApiResult instance = new ApiResult();
			Deserialize(instance, token);
			return instance;
		}

		/// <summary>
		/// Applies the JSON object info to the given instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		private static void Deserialize(ApiResult instance, JToken token)
		{
			// Error Code
			JToken errorCodeToken = token[PROPERTY_ERRORCODE];
			instance.ErrorCode = errorCodeToken == null ? eErrorCode.Ok : (eErrorCode)(int)errorCodeToken;

			// Type
			string typeName = (string)token[PROPERTY_TYPE];
			instance.Type = typeName == null ? null : Type.GetType(typeName, false, true);

			// Value
			JToken valueToken = token[PROPERTY_VALUE];
			instance.Value = valueToken == null ? null : GetValueFromToken(instance.Type, valueToken);
		}

		private static object GetValueFromToken(Type type, JToken token)
		{
			if (type == typeof(ApiClassInfo))
				return ApiClassInfo.Deserialize(token);

			if (type == typeof(ApiNodeInfo))
				return ApiNodeInfo.Deserialize(token);

			if (type == typeof(ApiNodeGroupInfo))
				return ApiNodeGroupInfo.Deserialize(token);

			if (type == typeof(ApiMethodInfo))
				return ApiMethodInfo.Deserialize(token);

			if (type == typeof(ApiParameterInfo))
				return ApiParameterInfo.Deserialize(token);

			return JsonUtils.Deserialize(type, token);
		}

		#endregion
	}
}
