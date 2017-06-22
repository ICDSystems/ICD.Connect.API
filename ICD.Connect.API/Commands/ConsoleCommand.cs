using System;

namespace ICD.Connect.API.Commands
{
	public sealed class ConsoleCommand : AbstractConsoleCommand
	{
		private readonly Func<string> m_Callback;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="callback"></param>
		public ConsoleCommand(string name, string help, Action callback) : this(name, help, callback, false)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="callback"></param>
		public ConsoleCommand(string name, string help, Func<string> callback) : this(name, help, callback, false)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="callback"></param>
		/// <param name="hidden"></param>
		public ConsoleCommand(string name, string help, Action callback, bool hidden) : base(name, help, hidden)
		{
			m_Callback = () =>
			             {
				             callback();
				             return DEFAULT_RESPONSE;
			             };
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="callback"></param>
		/// <param name="hidden"></param>
		public ConsoleCommand(string name, string help, Func<string> callback, bool hidden) : base(name, help, hidden)
		{
			m_Callback = callback;
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		/// <param name="parameters"></param>
		public override string Execute(params string[] parameters)
		{
			if (!ValidateParamsCount(parameters, 0))
				return string.Format("{0} expects {1} parameters", this.GetSafeConsoleName(), 0);
			return m_Callback();
		}
	}
}
