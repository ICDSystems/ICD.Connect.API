using System;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.API.Commands
{
	[PublicAPI]
	public sealed class EnumConsoleCommand<T> : AbstractConsoleCommand
		where T : struct, IConvertible
	{
		private readonly Func<T, string> m_Callback;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="callback"></param>
		public EnumConsoleCommand(string name, Action<T> callback)
			: this(name, callback, false)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="callback"></param>
		public EnumConsoleCommand(string name, Func<T, string> callback)
			: this(name, callback, false)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="callback"></param>
		/// <param name="hidden"></param>
		public EnumConsoleCommand(string name, Action<T> callback, bool hidden)
			: base(name, GetHelpString(name), hidden)
		{
			m_Callback = a =>
			             {
				             callback(a);
				             return DEFAULT_RESPONSE;
			             };
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="callback"></param>
		/// <param name="hidden"></param>
		public EnumConsoleCommand(string name, Func<T, string> callback, bool hidden)
			: base(name, GetHelpString(name), hidden)
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

			T param;

			if (!EnumUtils.TryParseStrict(parameters[0], true, out param))
				return string.Format("Invalid parameter {0}", parameters[0]);

			return m_Callback(param);
		}

		/// <summary>
		/// Builds the help string from the available enum values.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private static string GetHelpString(string name)
		{
			string array = StringUtils.ArrayFormat(EnumUtils.GetValues<T>().OrderBy(e => e.ToString()));
			return string.Format("{0} <{1}>", name, array);
		}
	}
}
