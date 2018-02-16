using System.Linq;
using ICD.Connect.API.Info;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Info.Converters
{
	[TestFixture]
	public sealed class ApiNodeGroupInfoConverterTest : AbstractApiInfoConverterTest
	{
		[Test]
		public override void WriteJsonTest()
		{
			ApiNodeGroupInfo info = new ApiNodeGroupInfo
			{
				Name = "Test",
				Help = "Test test."
			};

			info.AddNode(1, new ApiClassInfo { Name = "Node 1" });
			info.AddNode(2, new ApiClassInfo { Name = "Node 2" });
			info.AddNode(3, new ApiClassInfo { Name = "Node 3" });

			string json = JsonConvert.SerializeObject(info);

			Assert.Inconclusive();
		}

		[Test]
		public override void ReadJsonTest()
		{
			const string json =
				"{\"name\":\"Test\",\"help\":\"Test test.\",\"nodes\":{\"1\":{\"name\":\"Node 1\"},\"2\":{\"name\":\"Node 2\"},\"3\":{\"name\":\"Node 3\"}}}";

			ApiNodeGroupInfo info = JsonConvert.DeserializeObject<ApiNodeGroupInfo>(json);

			Assert.AreEqual("Test", info.Name);
			Assert.AreEqual("Test test.", info.Help);
			Assert.AreEqual(3, info.GetNodes().Count());
		}
	}
}
