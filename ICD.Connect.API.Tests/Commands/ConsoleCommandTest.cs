using ICD.Connect.API.Commands;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Commands
{
    [TestFixture]
    public sealed class ConsoleCommandTest : AbstractConsoleCommandTest
    {
        protected override AbstractConsoleCommand InstantiateDefault(string name, string help, bool hidden)
        {
            return new ConsoleCommand(name, help, () => { }, hidden);
        }

        [Test]
        public override void ExecuteTest()
        {
            bool value = false;
            ConsoleCommand command = new ConsoleCommand("test", "test", () => { value = true; });
            command.Execute();

            Assert.AreEqual(true, value);
        }
    }
}
