using System;
using ICD.Connect.API.Info;

namespace ICD.Connect.API.Attributes
{
	[AttributeUsage(AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
	public sealed class ApiParameterAttribute : AbstractApiAttribute
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		public ApiParameterAttribute(string name, string help)
			: base(name, help)
		{
		}

		/// <summary>
		/// Returns the info for the attribute.
		/// </summary>
		/// <returns></returns>
		public override IApiInfo GetInfo()
		{
			return new ApiParameterInfo(this);
		}
	}
}