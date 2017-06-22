using System;

namespace ICD.Connect.API.Commands
{
	/// <summary>
	/// Console command that passes the full string[] params to the callback.
	/// </summary>
	public sealed class ParamsConsoleCommand : AbstractConsoleCommand
	{
		private readonly Func<string[], string> m_Callback;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="callback"></param>
		public ParamsConsoleCommand(string name, string help, Action<string[]> callback)
			: this(name, help, callback, false)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="callback"></param>
		public ParamsConsoleCommand(string name, string help, Func<string[], string> callback)
			: this(name, help, callback, false)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="callback"></param>
		/// <param name="hidden"></param>
		public ParamsConsoleCommand(string name, string help, Action<string[]> callback, bool hidden)
			: base(name, help, hidden)
		{
			m_Callback = s =>
			             {
				             callback(s);
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
		public ParamsConsoleCommand(string name, string help, Func<string[], string> callback, bool hidden)
			: base(name, help, hidden)
		{
			m_Callback = callback;
		}

		/// <summary>
		/// Executes the command.
		/// </summary>
		/// <param name="parameters"></param>
		public override string Execute(params string[] parameters)
		{
			return m_Callback(parameters);
		}
	}
}
