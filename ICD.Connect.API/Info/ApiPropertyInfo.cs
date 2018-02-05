using ICD.Connect.API.Attributes;

namespace ICD.Connect.API.Info
{
	public sealed class ApiPropertyInfo : AbstractApiInfo
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		public ApiPropertyInfo(string name, string help)
			: base(name, help)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		public ApiPropertyInfo(ApiPropertyAttribute attribute)
			: base(attribute)
		{
		}
	}
}