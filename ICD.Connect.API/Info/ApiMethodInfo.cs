using ICD.Connect.API.Attributes;

namespace ICD.Connect.API.Info
{
	public sealed class ApiMethodInfo : AbstractApiInfo
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		public ApiMethodInfo(string name, string help)
			: base(name, help)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="attribute"></param>
		public ApiMethodInfo(ApiMethodAttribute attribute)
			: base(attribute)
		{
		}
	}
}