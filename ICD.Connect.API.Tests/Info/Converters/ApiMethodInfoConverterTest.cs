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
			ApiMethodInfo info = new ApiMethodInfo
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

			info.SetParameters(parameters);

			string json = JsonConvert.SerializeObject(info);

			Assert.Inconclusive();
		}

		[Test]
		public override void ReadJsonTest()
		{
			const string json =
				"{\"name\":\"Test\",\"help\":\"Test test.\",\"execute\":true,\"params\":[{\"name\":\"Param1\"},{\"name\":\"Param2\"},{\"name\":\"Param3\"}]}";

			ApiMethodInfo methodInfo = JsonConvert.DeserializeObject<ApiMethodInfo>(json);

			Assert.AreEqual("Test", methodInfo.Name);
			Assert.AreEqual("Test test.", methodInfo.Help);
			Assert.AreEqual(3, methodInfo.GetParameters().Count());
		}
	}
}
