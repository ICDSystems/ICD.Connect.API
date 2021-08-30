#if NETFRAMEWORK
extern alias RealNewtonsoft;
using RealNewtonsoft.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.API.Info.Converters
{
	public sealed class ApiParameterInfoConverter : AbstractApiInfoConverter<ApiParameterInfo>
	{
		private const string PROPERTY_TYPE = "t";
		private const string PROPERTY_VALUE = "v";

		/// <summary>
		/// Creates a new instance of ApiParameterInfo.
		/// </summary>
		/// <returns></returns>
		protected override ApiParameterInfo Instantiate()
		{
			return new ApiParameterInfo();
		}

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, ApiParameterInfo value, JsonSerializer serializer)
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
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, ApiParameterInfo instance,
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

				default:
					base.ReadProperty(property, reader, instance, serializer);
					break;
			}
		}
	}
}
