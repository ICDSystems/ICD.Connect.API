using System.Linq;
using NUnit.Framework;

namespace ICD.Connect.API.Tests
{
	[TestFixture]
	public sealed class ApiConsoleTest
	{
		[Test]
		public void RegisterChildTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void UnregisterChildTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void ExecuteCommandTest()
		{
			Assert.Inconclusive();
		}

		[Test]
		public void GetConsoleNodesTest()
		{
			Assert.Inconclusive();
		}

		[TestCase("one \"two two\" three \"four four\" five six", "one", "two two", "three", "four four", "five", "six")]
		[TestCase("one t\"wo three", "one", "t\"wo", "three")]
		[TestCase("one t\"wo three fo\"ur", "one", "t\"wo", "three", "fo\"ur")]
		public void SplitTest(string command, params string[] expected)
		{
			string[] split = ApiConsole.Split(command).ToArray();

			Assert.IsTrue(split.SequenceEqual(expected));
		}
	}
}
