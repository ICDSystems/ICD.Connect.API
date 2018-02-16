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
		/// Creates a new instance of T.
		/// </summary>
		/// <returns></returns>
		protected abstract T Instantiate();

		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		public sealed override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			{
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

				WriteProperties(writer, value, serializer);
			}
			writer.WriteEndObject();
		}

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected virtual void WriteProperties(JsonWriter writer, T value, JsonSerializer serializer)
		{
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
		public override T ReadJson(JsonReader reader, T existingValue, JsonSerializer serializer)
		{
			T output = null;

			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.Null || reader.TokenType == JsonToken.EndObject)
				{
					// Read out of the object
					reader.Read();
					break;
				}

				output = output ?? Instantiate();

				// Get the property
				if (reader.TokenType != JsonToken.PropertyName)
					continue;
				string property = (string)reader.Value;

				// Read into the value
				reader.Read();

				switch (property)
				{
					case PROPERTY_NAME:
						output.Name = (string)reader.Value;
						break;

					case PROPERTY_HELP:
						output.Help = (string)reader.Value;
						break;

					case PROPERTY_RESULT:
						output.Result = serializer.Deserialize<ApiResult>(reader);
						break;

					default:
						ReadProperty(property, reader, output, serializer);
						break;
				}
			}

			return output;
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected abstract void ReadProperty(string property, JsonReader reader, T instance, JsonSerializer serializer);
	}
}
