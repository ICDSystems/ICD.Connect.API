using ICD.Common.Utils.Json;
using Newtonsoft.Json;

namespace ICD.Connect.API.Info.Converters
{
	public abstract class AbstractApiInfoConverter<T> : AbstractGenericJsonConverter<T>
		where T : AbstractApiInfo
	{
		private const string PROPERTY_NAME = "name";
		private const string PROPERTY_HELP = "help";
		private const string PROPERTY_RESULT = "result";

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, T value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			// Name
			if (!string.IsNullOrEmpty(value.Name))
			{
				writer.WritePropertyName(PROPERTY_NAME);
				writer.WriteValue(value.Name);
			}

			// Help
			if (!string.IsNullOrEmpty(value.Help))
			{
				writer.WritePropertyName(PROPERTY_HELP);
				writer.WriteValue(value.Help);
			}

			// Reponse
			if (value.Result != null)
			{
				writer.WritePropertyName(PROPERTY_RESULT);
				serializer.Serialize(writer, value.Result);
			}
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, T instance, JsonSerializer serializer)
		{
			switch (property)
			{
				case PROPERTY_NAME:
					instance.Name = (string)reader.Value;
					break;

				case PROPERTY_HELP:
					instance.Help = (string)reader.Value;
					break;

				case PROPERTY_RESULT:
					instance.Result = serializer.Deserialize<ApiResult>(reader);
					break;

				default:
					base.ReadProperty(property, reader, instance, serializer);
					break;
			}
		}
	}
}
