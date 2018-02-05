using ICD.Connect.API.Attributes;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Attributes
{
	[TestFixture]
	public sealed class ApiClassAttributeTest : AbstractApiAttributeTest<ApiClassAttribute>
	{
		protected override ApiClassAttribute Instantiate(string name, string help)
		{
			return new ApiClassAttribute(name, help);
		}
	}
}
