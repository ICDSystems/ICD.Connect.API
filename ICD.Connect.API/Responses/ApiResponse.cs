using System;
using ICD.Common.Utils.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ICD.Connect.API.Responses
{
	public sealed class ApiResponse
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
		/// Sets the value and type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		public void SetValue<T>(T value)
		{
			SetValue(typeof(T), value);
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
				if (ErrorCode != eErrorCode.Ok)
				{
					writer.WritePropertyName(PROPERTY_ERRORCODE);
					writer.WriteValue(ErrorCode);
				}

				// Type
				if (Type != null)
				{
					writer.WritePropertyName(PROPERTY_TYPE);
					writer.WriteValue(Type.FullName);
				}

				// Value
				if (Value != null)
				{
					writer.WritePropertyName(PROPERTY_VALUE);
					writer.WriteValue(Value);
				}
			}
			writer.WriteEndObject();
		}

		/// <summary>
		/// Deserializes the JSON string to an ApiParameterInfo instance.
		/// </summary>
		/// <param name="json"></param>
		/// <returns></returns>
		public static ApiResponse Deserialize(string json)
		{
			JObject jObject = JObject.Parse(json);
			return Deserialize(jObject);
		}

		/// <summary>
		/// Instanties a new instance and applies the JSON object.
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public static ApiResponse Deserialize(JToken token)
		{
			ApiResponse instance = new ApiResponse();
			Deserialize(instance, token);
			return instance;
		}

		/// <summary>
		/// Applies the JSON object info to the given instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="token"></param>
		/// <returns></returns>
		public static void Deserialize(ApiResponse instance, JToken token)
		{
			// Error Code
			JToken errorCodeToken = token[PROPERTY_ERRORCODE];
			instance.ErrorCode = errorCodeToken == null ? eErrorCode.Ok : (eErrorCode)(int)errorCodeToken;

			// Type
			string typeName = (string)token[PROPERTY_TYPE];
			instance.Type = typeName == null ? null : Type.GetType(typeName, false, true);

			// Value
			instance.Value = JsonUtils.Deserialize(instance.Type, token[PROPERTY_VALUE]);
		}

		#endregion
	}
}
