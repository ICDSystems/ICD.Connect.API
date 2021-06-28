using System;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;

namespace ICD.Connect.API.Nodes
{
	public interface IConsoleCommon
	{
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		[NotNull]
		string ConsoleName { get; }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		[CanBeNull]
		string ConsoleHelp { get; }
	}

	public static class ConsoleCommonExtensions
	{
		/// <summary>
		/// Gets the console name without any whitespace.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		[NotNull]
		internal static string GetSafeConsoleName([NotNull] this IConsoleCommon extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			string consoleName = StringUtils.RemoveWhitespace(extends.ConsoleName);
			if (string.IsNullOrEmpty(consoleName))
				throw new InvalidOperationException(extends.GetType().Name + " console name is null or empty");

			return consoleName;
		}

		/// <summary>
		/// Runs the command.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="command"></param>
		internal static string ExecuteConsoleCommand([NotNull] this IConsoleCommon extends, [NotNull] string command)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (command == null)
				throw new ArgumentNullException("command");

			string[] split = ApiConsole.Split(command).ToArray();

			try
			{
				return extends.ExecuteConsoleCommand(split);
			}
			catch (Exception e)
			{
				ServiceProvider.TryGetService<ILoggerService>()
							   .AddEntry(eSeverity.Error, e, "Failed to execute console command \"{0}\" - {1}", command, e.Message);
				return string.Format("Failed to execute console command \"{0}\" - {1}", command, e.Message);
			}
		}

		/// <summary>
		/// Runs the command.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="command"></param>
		internal static string ExecuteConsoleCommand([NotNull] this IConsoleCommon extends, params string[] command)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			// Console command
			IConsoleCommand consoleCommand = extends as IConsoleCommand;
			if (consoleCommand != null)
				return consoleCommand.Execute(command);

			// Console node
			IConsoleNode node = extends as IConsoleNode;
			if (node != null)
				return node.ExecuteConsoleCommand(command);

			// Console group
			IConsoleNodeGroup group = extends as IConsoleNodeGroup;
			if (group != null)
				return group.ExecuteConsoleCommand(command);

			throw new ArgumentOutOfRangeException("extends", "Unable to execute console command for type "
			                                                 + extends.GetType().Name);
		}
	}
}
