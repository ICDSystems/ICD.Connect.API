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
		/// Creates a new instance of T.
		/// </summary>
		/// <returns></returns>
		protected override ApiResult Instantiate()
		{
			return new ApiResult();
		}

		/// <summary>
		/// Override to write properties to the writer.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, ApiResult value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			// Error Code
			writer.WritePropertyName(PROPERTY_ERRORCODE);
			writer.WriteValue(value.ErrorCode);

			// Type
			if (value.Type != null)
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

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, ApiResult instance, JsonSerializer serializer)
		{
			switch (property)
			{
				case PROPERTY_ERRORCODE:
					instance.ErrorCode = reader.GetValueAsEnum<ApiResult.eErrorCode>();
					break;

				case PROPERTY_TYPE:
					instance.Type = reader.GetValueAsType();
					break;

				case PROPERTY_VALUE:
					instance.Value = serializer.Deserialize(reader, instance.Type);
					break;

				default:
					base.ReadProperty(property, reader, instance, serializer);
					break;
			}
		}
	}
}
