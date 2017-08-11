using System;
using ICD.Connect.API.Commands;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Commands
{
    [TestFixture]
    public sealed class ParamsConsoleCommandTest : AbstractConsoleCommandTest
    {
        protected override AbstractConsoleCommand InstantiateDefault(string name, string help, bool hidden)
        {
            return new ParamsConsoleCommand(name, help, v => { }, hidden);
        }
    }
}
