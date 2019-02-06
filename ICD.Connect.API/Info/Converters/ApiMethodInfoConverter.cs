using ICD.Common.Utils.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ICD.Connect.API.Info.Converters
{
	public sealed class ApiMethodInfoConverter : AbstractApiInfoConverter<ApiMethodInfo>
	{
		private const string PROPERTY_PARAMETERS = "ps";
		private const string PROPERTY_EXECUTE = "e";

		/// <summary>
		/// Creates a new instance of ApiMethodInfo.
		/// </summary>
		/// <returns></returns>
		protected override ApiMethodInfo Instantiate()
		{
			return new ApiMethodInfo();
		}

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, ApiMethodInfo value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			// Execute
			if (value.Execute)
			{
				writer.WritePropertyName(PROPERTY_EXECUTE);
				writer.WriteValue(value.Execute);
			}

			// Parameters
			if (value.ParameterCount > 0)
			{
				writer.WritePropertyName(PROPERTY_PARAMETERS);
				serializer.SerializeArray(writer, value.GetParameters());
			}
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, ApiMethodInfo instance,
		                                     JsonSerializer serializer)
		{
			switch (property)
			{
				case PROPERTY_EXECUTE:
					instance.Execute = (bool)reader.Value;
					break;

				case PROPERTY_PARAMETERS:
					IEnumerable<ApiParameterInfo> parameters =
						serializer.DeserializeArray<ApiParameterInfo>(reader)
						          .Where(p => p != null);
					instance.SetParameters(parameters);
					break;

				default:
					base.ReadProperty(property, reader, instance, serializer);
					break;
			}
		}
	}
}
