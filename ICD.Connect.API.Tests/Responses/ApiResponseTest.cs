using System;
using ICD.Connect.API.Responses;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Responses
{
	[TestFixture]
	public sealed class ApiResponseTest
	{
		#region Properties

		[TestCase(ApiResponse.eErrorCode.Ok)]
		[TestCase(ApiResponse.eErrorCode.MissingMember)]
		public void ErrorCodeTest(ApiResponse.eErrorCode code)
		{
			Assert.AreEqual(code, new ApiResponse {ErrorCode = code}.ErrorCode);
		}

		[TestCase(typeof(int))]
		[TestCase(typeof(string))]
		[TestCase(typeof(ApiResponseTest))]
		public void TypeTest(Type type)
		{
			Assert.AreEqual(type, new ApiResponse {Type = type}.Type);
		}

		[TestCase(10)]
		[TestCase("test")]
		[TestCase(false)]
		public void ValueTest(object value)
		{
			Assert.AreEqual(value, new ApiResponse {Value = value}.Value);
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
			ApiResponse a = new ApiResponse
			{
				ErrorCode = ApiResponse.eErrorCode.Exception,
				Type = typeof(string),
				Value = "You dun goofed"
			};

			string json = a.Serialize();
			ApiResponse b = ApiResponse.Deserialize(json);

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
