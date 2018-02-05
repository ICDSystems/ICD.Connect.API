using ICD.Common.Utils.Tests.Attributes;
using ICD.Connect.API.Attributes;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Attributes
{
	public abstract class AbstractApiAttributeTest<TAttribute> : AbstractIcdAttributeTest<TAttribute>
		where TAttribute : AbstractApiAttribute
	{
		protected abstract TAttribute Instantiate(string name, string help);

		[TestCase(null, "")]
		[TestCase("  test  ", "Test")]
		[TestCase("test test", "TestTest")]
		public void NameTest(string name, string expected)
		{
			TAttribute instance = Instantiate(name, null);
			Assert.AreEqual(expected, instance.Name);
		}

		[TestCase(null, "")]
		[TestCase("   test   ", "Test.")]
		[TestCase("test test test", "Test test test.")]
		public void HelpTest(string help, string expected)
		{
			TAttribute instance = Instantiate(null, help);
			Assert.AreEqual(expected, instance.Help);
		}
	}
}
