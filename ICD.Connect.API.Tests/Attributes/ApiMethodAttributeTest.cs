using ICD.Connect.API.Attributes;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Attributes
{
	[TestFixture]
	public sealed class ApiMethodAttributeTest : AbstractApiAttributeTest<ApiMethodAttribute>
	{
		protected override ApiMethodAttribute Instantiate(string name, string help)
		{
			return new ApiMethodAttribute(name, help);
		}
	}
}
