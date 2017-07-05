using System;
using ICD.Common.Properties;

namespace ICD.Connect.API.Commands
{
	public sealed class GenericConsoleCommand<T1> : AbstractConsoleCommand
	{
		private readonly Func<T1, string> m_Callback;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="callback"></param>
		public GenericConsoleCommand(string name, string help, Action<T1> callback)
			: this(name, help, callback, false)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="callback"></param>
		public GenericConsoleCommand(string name, string help, Func<T1, string> callback)
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
		public GenericConsoleCommand(string name, string help, Action<T1> callback, bool hidden)
			: base(name, help, hidden)
		{
			m_Callback = t1 =>
			             {
				             callback(t1);
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
		public GenericConsoleCommand(string name, string help, Func<T1, string> callback, bool hidden)
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
			if (!ValidateParamsCount(parameters, 1))
				return string.Format("{0} expects {1} parameters", this.GetSafeConsoleName(), 1);

			T1 param = Convert<T1>(parameters[0]);
			return m_Callback(param);
		}
	}

	public sealed class GenericConsoleCommand<T1, T2> : AbstractConsoleCommand
	{
		private readonly Func<T1, T2, string> m_Callback;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="callback"></param>
		public GenericConsoleCommand(string name, string help, Action<T1, T2> callback)
			: this(name, help, callback, false)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="callback"></param>
		public GenericConsoleCommand(string name, string help, Func<T1, T2, string> callback)
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
		public GenericConsoleCommand(string name, string help, Action<T1, T2> callback, bool hidden)
			: base(name, help, hidden)
		{
			m_Callback = (t1, t2) =>
			             {
				             callback(t1, t2);
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
		public GenericConsoleCommand(string name, string help, Func<T1, T2, string> callback, bool hidden)
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
			if (!ValidateParamsCount(parameters, 2))
				return string.Format("{0} expects {1} parameters", this.GetSafeConsoleName(), 2);

			T1 param1 = Convert<T1>(parameters[0]);
			T2 param2 = Convert<T2>(parameters[1]);

			return m_Callback(param1, param2);
		}
	}

	public sealed class GenericConsoleCommand<T1, T2, T3> : AbstractConsoleCommand
	{
		private readonly Func<T1, T2, T3, string> m_Callback;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="callback"></param>
		public GenericConsoleCommand(string name, string help, Action<T1, T2, T3> callback)
			: this(name, help, callback, false)
		{
			m_Callback = (t1, t2, t3) =>
			             {
				             callback(t1, t2, t3);
				             return DEFAULT_RESPONSE;
			             };
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="callback"></param>
		public GenericConsoleCommand(string name, string help, Func<T1, T2, T3, string> callback)
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
		public GenericConsoleCommand(string name, string help, Action<T1, T2, T3> callback, bool hidden)
			: base(name, help, hidden)
		{
			m_Callback = (t1, t2, t3) =>
			             {
				             callback(t1, t2, t3);
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
		public GenericConsoleCommand(string name, string help, Func<T1, T2, T3, string> callback, bool hidden)
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
			if (!ValidateParamsCount(parameters, 3))
				return string.Format("{0} expects {1} parameters", this.GetSafeConsoleName(), 3);

			T1 param1 = Convert<T1>(parameters[0]);
			T2 param2 = Convert<T2>(parameters[1]);
			T3 param3 = Convert<T3>(parameters[2]);

			return m_Callback(param1, param2, param3);
		}
	}

	[PublicAPI]
	public sealed class GenericConsoleCommand<T1, T2, T3, T4> : AbstractConsoleCommand
	{
		private readonly Func<T1, T2, T3, T4, string> m_Callback;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="callback"></param>
		public GenericConsoleCommand(string name, string help, Action<T1, T2, T3, T4> callback)
			: this(name, help, callback, false)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="callback"></param>
		public GenericConsoleCommand(string name, string help, Func<T1, T2, T3, T4, string> callback)
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
		public GenericConsoleCommand(string name, string help, Action<T1, T2, T3, T4> callback, bool hidden)
			: base(name, help, hidden)
		{
			m_Callback = (t1, t2, t3, t4) =>
			             {
				             callback(t1, t2, t3, t4);
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
		public GenericConsoleCommand(string name, string help, Func<T1, T2, T3, T4, string> callback, bool hidden)
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
			if (!ValidateParamsCount(parameters, 4))
				return string.Format("{0} expects {1} parameters", this.GetSafeConsoleName(), 4);

			T1 param1 = Convert<T1>(parameters[0]);
			T2 param2 = Convert<T2>(parameters[1]);
			T3 param3 = Convert<T3>(parameters[2]);
			T4 param4 = Convert<T4>(parameters[3]);

			return m_Callback(param1, param2, param3, param4);
		}
	}
}
