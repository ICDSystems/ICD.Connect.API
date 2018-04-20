using ICD.Connect.API.Attributes;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Attributes
{
	[TestFixture]
	public sealed class ApiPropertyAttributeTest : AbstractApiAttributeTest<ApiPropertyAttribute>
	{
		protected override ApiPropertyAttribute Instantiate(string name, string help)
		{
			return new ApiPropertyAttribute(name, help);
		}
	}
}
