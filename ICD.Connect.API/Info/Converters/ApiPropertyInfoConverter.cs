#if NETFRAMEWORK
extern alias RealNewtonsoft;
using RealNewtonsoft.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.API.Info.Converters
{
	public sealed class ApiPropertyInfoConverter : AbstractApiInfoConverter<ApiPropertyInfo>
	{
		private const string PROPERTY_TYPE = "t";
		private const string PROPERTY_VALUE = "v";
		private const string PROPERTY_READ_WRITE = "rw";

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

			if (value.Value != null)
			{
				writer.WritePropertyName(PROPERTY_VALUE);
				serializer.Serialize(writer, value.Value);
			}

			if (value.ReadWrite != ApiPropertyInfo.eReadWrite.None)
			{
				writer.WritePropertyName(PROPERTY_READ_WRITE);
				writer.WriteValue(value.ReadWrite);
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

				case PROPERTY_READ_WRITE:
					instance.ReadWrite = reader.GetValueAsEnum<ApiPropertyInfo.eReadWrite>();
					break;

				default:
					base.ReadProperty(property, reader, instance, serializer);
					break;
			}
		}
	}
}
