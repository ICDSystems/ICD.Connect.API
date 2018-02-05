using ICD.Connect.API.Attributes;

namespace ICD.Connect.API.Info
{
	public sealed class ApiEventInfo : AbstractApiInfo
	{
		public ApiEventInfo(string name, string help)
			: base(name, help)
		{
		}

		public ApiEventInfo(ApiEventAttribute attribute)
			: base(attribute)
		{
		}
	}
}