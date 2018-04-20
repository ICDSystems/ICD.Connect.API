using ICD.Common.Utils.Tests.Json;
using ICD.Connect.API.Info;
using Newtonsoft.Json;
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
				"{\"errorCode\":1,\"type\":\"System.String, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\",\"value\":\"Test\"}";

			ApiResult result = JsonConvert.DeserializeObject<ApiResult>(json);

			Assert.AreEqual(ApiResult.eErrorCode.MissingMember, result.ErrorCode);
			Assert.AreEqual(typeof(string), result.Type);
			Assert.AreEqual("Test", result.Value);
		}
	}
}
