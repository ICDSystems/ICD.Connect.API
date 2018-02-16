using System;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Json;
using Newtonsoft.Json;

namespace ICD.Connect.API.Info.Converters
{
	public sealed class ApiResultConverter : AbstractGenericJsonConverter<ApiResult>
	{
		private const string PROPERTY_ERRORCODE = "errorCode";
		private const string PROPERTY_TYPE = "type";
		private const string PROPERTY_VALUE = "value";

		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override void WriteJson(JsonWriter writer, ApiResult value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			{
				// Error Code
				writer.WritePropertyName(PROPERTY_ERRORCODE);
				writer.WriteValue(value.ErrorCode);

				// Type
				if (value.Value != null && value.Type != null)
				{
					writer.WritePropertyName(PROPERTY_TYPE);
					writer.WriteType(value.Type);
				}

				// Value
				if (value.Value != null)
				{
					writer.WritePropertyName(PROPERTY_VALUE);
					serializer.Serialize(writer, value.Value);
				}
			}
			writer.WriteEndObject();
		}

		/// <summary>
		/// Reads the JSON representation of the object.
		/// </summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param>
		/// <param name="existingValue">The existing value of object being read.</param>
		/// <param name="serializer">The calling serializer.</param>
		/// <returns>
		/// The object value.
		/// </returns>
		public override ApiResult ReadJson(JsonReader reader, ApiResult existingValue, JsonSerializer serializer)
		{
			ApiResult output = null;

			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.Null || reader.TokenType == JsonToken.EndObject)
				{
					// Read out of the object
					reader.Read();
					break;
				}

				output = output ?? new ApiResult();

				// Get the property
				if (reader.TokenType != JsonToken.PropertyName)
					continue;
				string property = (string)reader.Value;
				
				// Read into the value
				reader.Read();

				switch (property)
				{
					case PROPERTY_ERRORCODE:
						output.ErrorCode = (ApiResult.eErrorCode)Enum.ToObject(typeof(ApiResult.eErrorCode), reader.Value);
						break;

					case PROPERTY_TYPE:
						output.Type = Type.GetType((string)reader.Value, false, true);
						break;

					case PROPERTY_VALUE:
						output.Value = serializer.Deserialize(reader, output.Type);
						break;
				}
			}

			return output;
		}
	}
}
