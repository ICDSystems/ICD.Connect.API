using ICD.Connect.API.Attributes;

namespace ICD.Connect.API.Info
{
	public sealed class ApiClassInfo : AbstractApiInfo
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		public ApiClassInfo(string name, string help)
			: base(name, help)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		public ApiClassInfo(ApiClassAttribute attribute)
			: base(attribute)
		{
		}
	}
}