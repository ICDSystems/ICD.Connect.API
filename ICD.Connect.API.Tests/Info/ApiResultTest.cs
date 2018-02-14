using System;
using ICD.Connect.API.Info;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Info
{
	[TestFixture]
	public sealed class ApiResultTest
	{
		#region Properties

		[TestCase(ApiResult.eErrorCode.Ok)]
		[TestCase(ApiResult.eErrorCode.MissingMember)]
		public void ErrorCodeTest(ApiResult.eErrorCode code)
		{
			Assert.AreEqual(code, new ApiResult {ErrorCode = code}.ErrorCode);
		}

		[TestCase(typeof(int))]
		[TestCase(typeof(string))]
		[TestCase(typeof(ApiResultTest))]
		public void TypeTest(Type type)
		{
			Assert.AreEqual(type, new ApiResult {Type = type}.Type);
		}

		[TestCase(10)]
		[TestCase("test")]
		[TestCase(false)]
		public void ValueTest(object value)
		{
			Assert.AreEqual(value, new ApiResult {Value = value}.Value);
		}

		#endregion

		#region Methods

		[Test]
		public void SetValueGenericTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void SetValueTest()
		{
			Assert.Inconclusive();
		}

		#endregion

		#region Serialization

		[Test]
		public void SerializeTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void SerializeWriterTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void DeserializeTest()
		{
			ApiResult a = new ApiResult
			{
				ErrorCode = ApiResult.eErrorCode.Exception,
				Type = typeof(string),
				Value = "You dun goofed"
			};

			string json = a.Serialize();
			ApiResult b = ApiResult.Deserialize(json);

			Assert.AreEqual(a.ErrorCode, b.ErrorCode);
			Assert.AreEqual(a.Type, b.Type);
			Assert.AreEqual(a.Value, b.Value);
		}

		[Test]
		public void DeserializeToken()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void DeserializeInstanceTest()
		{
			Assert.Inconclusive();
		}

		#endregion
	}
}
