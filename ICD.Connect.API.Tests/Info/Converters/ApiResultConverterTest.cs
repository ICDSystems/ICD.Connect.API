#if NETFRAMEWORK
extern alias RealNewtonsoft;
using RealNewtonsoft.Newtonsoft.Json;
#else
using Newtonsoft.Json;
#endif
using ICD.Common.Utils.Tests.Json;
using ICD.Connect.API.Info;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Info.Converters
{
	[TestFixture]
	public sealed class ApiResultConverterTest : AbstractGenericJsonConverterTest
	{
		[Test]
		public override void WriteJsonTest()
		{
			ApiResult result = new ApiResult
			{
				ErrorCode = ApiResult.eErrorCode.MissingMember
			};
			result.SetValue("Test");

			string json = JsonConvert.SerializeObject(result);

			result = JsonConvert.DeserializeObject<ApiResult>(json);

			Assert.AreEqual(ApiResult.eErrorCode.MissingMember, result.ErrorCode);
			Assert.AreEqual(typeof(string), result.Type);
			Assert.AreEqual("Test", result.Value);
		}

		[Test]
		public override void ReadJsonTest()
		{
			const string json =
				"{\"e\":1,\"t\":\"System.String\",\"v\":\"Test\"}";

			ApiResult result = JsonConvert.DeserializeObject<ApiResult>(json);

			Assert.AreEqual(ApiResult.eErrorCode.MissingMember, result.ErrorCode);
			Assert.AreEqual(typeof(string), result.Type);
			Assert.AreEqual("Test", result.Value);
		}
	}
}
