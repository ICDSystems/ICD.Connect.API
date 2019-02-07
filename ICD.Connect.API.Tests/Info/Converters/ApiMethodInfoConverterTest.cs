using System.Linq;
using ICD.Connect.API.Info;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Info.Converters
{
	[TestFixture]
	public sealed class ApiMethodInfoConverterTest : AbstractApiInfoConverterTest
	{
		[Test]
		public override void WriteJsonTest()
		{
			ApiMethodInfo methodInfo = new ApiMethodInfo
			{
				Execute = true,
				Name = "Test",
				Help = "Test test."
			};

			ApiParameterInfo[] parameters =
			{
				new ApiParameterInfo
				{
					Name = "Param1"
				},
				new ApiParameterInfo
				{
					Name = "Param2"
				},
				new ApiParameterInfo
				{
					Name = "Param3"
				}
			};

			methodInfo.SetParameters(parameters);

			string json = JsonConvert.SerializeObject(methodInfo);

			Assert.AreEqual("{\"n\":\"Test\",\"e\":true,\"ps\":[{\"n\":\"Param1\"},{\"n\":\"Param2\"},{\"n\":\"Param3\"}]}", json);
		}

		[Test]
		public override void ReadJsonTest()
		{
			const string json =
				"{\"n\":\"Test\",\"h\":\"Test test.\",\"e\":true,\"ps\":[{\"n\":\"Param1\"},{\"n\":\"Param2\"},{\"n\":\"Param3\"}]}";

			ApiMethodInfo methodInfo = JsonConvert.DeserializeObject<ApiMethodInfo>(json);

			Assert.AreEqual("Test", methodInfo.Name);
			Assert.AreEqual("Test test.", methodInfo.Help);
			Assert.AreEqual(3, methodInfo.GetParameters().Count());
			Assert.AreEqual("Param1", methodInfo.GetParameters().ElementAt(0).Name);
			Assert.AreEqual("Param2", methodInfo.GetParameters().ElementAt(1).Name);
			Assert.AreEqual("Param3", methodInfo.GetParameters().ElementAt(2).Name);
		}
	}
}
