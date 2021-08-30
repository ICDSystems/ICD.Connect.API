#if NETFRAMEWORK
extern alias RealNewtonsoft;
using RealNewtonsoft.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.API.Info.Converters
{
	public sealed class ApiEventInfoConverter : AbstractApiInfoConverter<ApiEventInfo>
	{
		private const string PROPERTY_ACTION = "a";

		/// <summary>
		/// Creates a new instance of T.
		/// </summary>
		/// <returns></returns>
		protected override ApiEventInfo Instantiate()
		{
			return new ApiEventInfo();
		}

		/// <summary>
		/// Override to serialize additional properties to the JSON.
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		protected override void WriteProperties(JsonWriter writer, ApiEventInfo value, JsonSerializer serializer)
		{
			base.WriteProperties(writer, value, serializer);

			if (value.SubscribeAction != ApiEventInfo.eSubscribeAction.None)
			{
				writer.WritePropertyName(PROPERTY_ACTION);
				writer.WriteValue(value.SubscribeAction);
			}
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, ApiEventInfo instance,
		                                     JsonSerializer serializer)
		{
			switch (property)
			{
				case PROPERTY_ACTION:
					instance.SubscribeAction = reader.GetValueAsEnum<ApiEventInfo.eSubscribeAction>();
					break;

				default:
					base.ReadProperty(property, reader, instance, serializer);
					break;
			}
		}
	}
}
