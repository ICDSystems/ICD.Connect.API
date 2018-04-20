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
	}
}
