using System;
using ICD.Connect.API.Commands;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Commands
{
    [TestFixture]
    public sealed class EnumConsoleCommandTest : AbstractConsoleCommandTest
    {
        private enum eTestEnum
        {
            None = 0,
            A = 1,
            B = 2,
            C = 3
        }

        protected override AbstractConsoleCommand InstantiateDefault(string name, string help, bool hidden)
        {
            return new EnumConsoleCommand<eTestEnum>(name, e => { }, hidden);
        }

        [TestCase(null)]
        public override void HelpTest(string help)
        {
            EnumConsoleCommand<eTestEnum> command = new EnumConsoleCommand<eTestEnum>("Test", e => { }, false);
            Assert.AreEqual("Test <[A, B, C, None]>", command.ConsoleHelp);
        }
    }
}
