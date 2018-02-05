using ICD.Connect.API.Attributes;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Attributes
{
	[TestFixture]
	public sealed class ApiEventAttributeTest : AbstractApiAttributeTest<ApiEventAttribute>
	{
		protected override ApiEventAttribute Instantiate(string name, string help)
		{
			return new ApiEventAttribute(name, help);
		}
	}
}
