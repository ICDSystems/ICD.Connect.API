using ICD.Connect.API.Commands;
using NUnit.Framework;

namespace ICD.Connect.API.Tests.Commands
{
    [TestFixture(typeof(string))]
    public sealed class GenericConsoleCommandTest<T> : AbstractConsoleCommandTest
    {
        protected override AbstractConsoleCommand InstantiateDefault(string name, string help, bool hidden)
        {
            return new GenericConsoleCommand<T>(name, help, v => { }, hidden);
        }
    }

    [TestFixture(typeof(string), typeof(int))]
    public sealed class GenericConsoleCommandTest<T1, T2> : AbstractConsoleCommandTest
    {
        protected override AbstractConsoleCommand InstantiateDefault(string name, string help, bool hidden)
        {
            return new GenericConsoleCommand<T1, T2>(name, help, (v1, v2) => { }, hidden);
        }
    }

    [TestFixture(typeof(string), typeof(int), typeof(char))]
    public sealed class GenericConsoleCommandTest<T1, T2, T3> : AbstractConsoleCommandTest
    {
        protected override AbstractConsoleCommand InstantiateDefault(string name, string help, bool hidden)
        {
            return new GenericConsoleCommand<T1, T2, T3>(name, help, (v1, v2, v3) => { }, hidden);
        }
    }

    [TestFixture(typeof(string), typeof(int), typeof(char), typeof(byte))]
    public sealed class GenericConsoleCommandTest<T1, T2, T3, T4> : AbstractConsoleCommandTest
    {
        protected override AbstractConsoleCommand InstantiateDefault(string name, string help, bool hidden)
        {
            return new GenericConsoleCommand<T1, T2, T3, T4>(name, help, (v1, v2, v3, v4) => { }, hidden);
        }
    }
}
