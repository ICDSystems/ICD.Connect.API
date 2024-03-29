﻿#if NETFRAMEWORK
extern alias RealNewtonsoft;
using RealNewtonsoft.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif

namespace ICD.Connect.API.Info.Converters
{
	public sealed class ApiNodeInfoConverter : AbstractApiInfoConverter<ApiNodeInfo>
	{
		private const string PROPERTY_NODE = "no";

		/// <summary>
		/// Creates a new instance of ApiNodeInfo.
		/// </summary>
		/// <returns></returns>
		protected override ApiNodeInfo Instantiate()
		{
			return new ApiNodeInfo();
		}

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, ApiNodeInfo value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			if (value.Node != null)
			{
				writer.WritePropertyName(PROPERTY_NODE);
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
		protected override void ReadProperty(string property, JsonReader reader, ApiNodeInfo instance,
		                                     JsonSerializer serializer)
		{
			switch (property)
			{
				case PROPERTY_NODE:
					instance.Node = serializer.Deserialize<ApiClassInfo>(reader);
					break;

				default:
					base.ReadProperty(property, reader, instance, serializer);
					break;
			}
		}
	}
}
