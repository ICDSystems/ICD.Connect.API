#if NETFRAMEWORK
extern alias RealNewtonsoft;
using RealNewtonsoft.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using ICD.Connect.API.Attributes;
using ICD.Connect.API.Info;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Info
{
	[TestFixture]
	public sealed class ApiClassInfoTest : AbstractApiInfoTest<ApiClassInfo>
	{
		[Test]
		public void SerializeTest()
		{
			ApiClassInfo info = ApiClassAttribute.GetInfo(typeof(ConcreteClass));
			string json = JsonConvert.SerializeObject(info);

			Assert.Inconclusive();
		}

		[ApiClass("TestClass", "Simple test class for seeing serialization.")]
		private sealed class ConcreteClass : AbstractClass
		{
		}

		private abstract class AbstractClass
		{
			[ApiMethod("TestMethodA", "Empty test method with parameters.")]
			public void TestMethodA(
				[ApiParameter("ParamA", "Example int param.")] int a,
				[ApiParameter("ParamB", "Example string param.")] string b,
				ushort unnamed)
			{
				
			}
		}
	}
}
