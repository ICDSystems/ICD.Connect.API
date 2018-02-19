using ICD.Common.Utils.Extensions;
using Newtonsoft.Json;
using System;

namespace ICD.Connect.API.Info.Converters
{
	public sealed class ApiPropertyInfoConverter : AbstractApiInfoConverter<ApiPropertyInfo>
	{
		private const string PROPERTY_TYPE = "type";
		private const string PROPERTY_VALUE = "value";
		private const string PROPERTY_READ = "read";
		private const string PROPERTY_WRITE = "write";

		/// <summary>
		/// Creates a new instance of ApiPropertyInfo.
		/// </summary>
		/// <returns></returns>
		protected override ApiPropertyInfo Instantiate()
		{
			return new ApiPropertyInfo();
		}

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, ApiPropertyInfo value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			if (value.Type != null)
			{
				writer.WritePropertyName(PROPERTY_TYPE);
				writer.WriteType(value.Type);
			}

			// We want to allow serializing null values in a write context
			if (value.Write)
			{
				writer.WritePropertyName(PROPERTY_VALUE);
				serializer.Serialize(writer, value.Value);
			}

			if (value.Read)
			{
				writer.WritePropertyName(PROPERTY_READ);
				writer.WriteValue(value.Read);
			}

			if (value.Write)
			{
				writer.WritePropertyName(PROPERTY_WRITE);
				writer.WriteValue(value.Write);
			}
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, ApiPropertyInfo instance,
		                                     JsonSerializer serializer)
		{
			switch (property)
			{
				case PROPERTY_TYPE:
					instance.Type = reader.GetValueAsType();
					break;

				case PROPERTY_VALUE:
					instance.Value = serializer.Deserialize(reader, instance.Type);
					break;

				case PROPERTY_READ:
					instance.Read = (bool)reader.Value;
					break;

				case PROPERTY_WRITE:
					instance.Write = (bool)reader.Value;
					break;
			}
		}
	}
}
