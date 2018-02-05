using ICD.Connect.API.Attributes;

namespace ICD.Connect.API.Info
{
	public sealed class ApiParameterInfo : AbstractApiInfo
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		public ApiParameterInfo(string name, string help)
			: base(name, help)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		public ApiParameterInfo(ApiParameterAttribute attribute)
			: base(attribute)
		{
		}
	}
}