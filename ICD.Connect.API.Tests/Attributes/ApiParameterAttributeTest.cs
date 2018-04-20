using System.Linq;
using System.Reflection;
using ICD.Connect.API.Attributes;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Attributes
{
	[TestFixture]
	public sealed class ApiParameterAttributeTest : AbstractApiAttributeTest<ApiParameterAttribute>
	{
		protected override ApiParameterAttribute Instantiate(string name, string help)
		{
			return new ApiParameterAttribute(name, help);
		}

		[Test]
		public void InheritanceTest()
		{
			MethodInfo method = typeof(ConcreteParameterClass).GetMethod("TestMethod",
			                                                             BindingFlags.Instance | BindingFlags.Public,
			                                                             null,
			                                                             new [] {typeof(int), typeof(int), typeof(int)},
			                                                             null);

			ApiParameterAttribute[] attributes = method.GetParameters()
			                                           .Select(p => p.GetCustomAttribute<ApiParameterAttribute>())
			                                           .ToArray();

			Assert.AreEqual(3, attributes.Length);

			Assert.AreEqual("Param1", attributes[0].Name);
			Assert.AreEqual("The first parameter.", attributes[0].Help);

			Assert.AreEqual("Param2", attributes[1].Name);
			Assert.AreEqual("The second parameter.", attributes[1].Help);

			Assert.AreEqual("Param3", attributes[2].Name);
			Assert.AreEqual("The third parameter.", attributes[2].Help);
		}

		private sealed class ConcreteParameterClass : AbstractParameterClass
		{
			public override void TestMethod(int param1, int param2, int param3)
			{
			}
		}

		private abstract class AbstractParameterClass
		{
			public virtual void TestMethod(
				[ApiParameter("Param1", "The first parameter.")] int param1,
				[ApiParameter("Param2", "The second parameter.")] int param2,
				[ApiParameter("Param3", "The third parameter.")] int param3)
			{
			}
		}
	}
}
