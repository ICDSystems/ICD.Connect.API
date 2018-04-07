using Newtonsoft.Json;

namespace ICD.Connect.API.Info.Converters
{
    public sealed class ApiEventInfoConverter : AbstractApiInfoConverter<ApiEventInfo>
    {
		/// <summary>
		/// Creates a new instance of T.
		/// </summary>
		/// <returns></returns>
		protected override ApiEventInfo Instantiate()
		{
			return new ApiEventInfo();
		}

		/// <summary>
		/// Override to handle the current property value with the given name.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="reader"></param>
		/// <param name="instance"></param>
		/// <param name="serializer"></param>
		protected override void ReadProperty(string property, JsonReader reader, ApiEventInfo instance, JsonSerializer serializer)
		{
		}
	}
}
