﻿#if NETFRAMEWORK
extern alias RealNewtonsoft;
using RealNewtonsoft.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.API.Info.Converters
{
	public sealed class ApiNodeGroupKeyInfoConverter : AbstractApiInfoConverter<ApiNodeGroupKeyInfo>
	{
		private const string PROPERTY_VALUE = "v";
		private const string PROPERTY_KEY = "k";

		/// <summary>
		/// Creates a new instance of T.
		/// </summary>
		/// <returns></returns>
		protected override ApiNodeGroupKeyInfo Instantiate()
		{
			return new ApiNodeGroupKeyInfo();
		}

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, ApiNodeGroupKeyInfo value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			if (value.Key != 0)
			{
				writer.WritePropertyName(PROPERTY_KEY);
				writer.WriteValue(value.Key);
			}

			if (value.Node != null)
			{
				writer.WritePropertyName(PROPERTY_VALUE);
				serializer.Serialize(writer, value.Node);
			}
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, ApiNodeGroupKeyInfo instance,
											 JsonSerializer serializer)
		{
			switch (property)
			{
				case PROPERTY_KEY:
					instance.Key = reader.GetValueAsUInt();
					break;

				case PROPERTY_VALUE:
					instance.Node = serializer.Deserialize<ApiClassInfo>(reader);
					break;

				default:
					base.ReadProperty(property, reader, instance, serializer);
					break;
			}
		}
	}
}
