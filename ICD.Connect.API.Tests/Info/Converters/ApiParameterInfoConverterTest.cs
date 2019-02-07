using ICD.Connect.API.Info;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Info.Converters
{
	[TestFixture]
	public sealed class ApiParameterInfoConverterTest : AbstractApiInfoConverterTest
	{
		[Test]
		public override void WriteJsonTest()
		{
			ApiParameterInfo parameter = new ApiParameterInfo
			{
				Name = "Test",
				Help = "Test test."
			};
			parameter.SetValue("Test");

			string json = JsonConvert.SerializeObject(parameter);

			parameter = JsonConvert.DeserializeObject<ApiParameterInfo>(json);

			Assert.AreEqual("Test", parameter.Name);
			Assert.AreEqual(typeof(string), parameter.Type);
			Assert.AreEqual("Test", parameter.Value);
		}

		[Test]
		public override void ReadJsonTest()
		{
			const string json =
				"{\"n\":\"Test\",\"t\":\"System.String\",\"v\":\"Test\"}";

			ApiParameterInfo parameter = JsonConvert.DeserializeObject<ApiParameterInfo>(json);

			Assert.AreEqual("Test", parameter.Name);
			Assert.AreEqual(typeof(string), parameter.Type);
			Assert.AreEqual("Test", parameter.Value);
		}
	}
}
