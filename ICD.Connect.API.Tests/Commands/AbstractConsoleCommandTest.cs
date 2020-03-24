using ICD.Connect.API.Commands;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Commands
{
    public abstract class AbstractConsoleCommandTest
    {
        protected abstract AbstractConsoleCommand InstantiateDefault(string name, string help, bool hidden);

        [TestCase("Test")]
        public void ConsoleNameTest(string name)
        {
            AbstractConsoleCommand command = InstantiateDefault(name, null, false);
            Assert.AreEqual(name, command.ConsoleName);
        }

        [TestCase("Test")]
        public virtual void HelpTest(string help)
        {
            AbstractConsoleCommand command = InstantiateDefault(null, help, false);
            Assert.AreEqual(help, command.ConsoleHelp);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void HiddenTest(bool hidden)
        {
            AbstractConsoleCommand command = InstantiateDefault(null, null, hidden);
            Assert.AreEqual(hidden, command.Hidden);
        }

        [Test]
        public virtual void ExecuteTest()
        {
            // Override in subclass
            Assert.Inconclusive("{0} does not implement test", GetType().Name);
        }
    }
}
