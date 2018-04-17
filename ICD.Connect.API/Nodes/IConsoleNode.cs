using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;

namespace ICD.Connect.API.Nodes
{
	public delegate void AddStatusRowDelegate(string name, object value);

	public interface IConsoleNode : IConsoleNodeBase
	{
		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		void BuildConsoleStatus(AddStatusRowDelegate addRow);

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IConsoleCommand> GetConsoleCommands();
	}

	/// <summary>
	/// Extension methods for IConsoleNodes.
	/// </summary>
	public static class ConsoleNodeExtensions
	{
		/// <summary>
		/// Runs the command.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="command"></param>
		public static string ExecuteConsoleCommand(this IConsoleNode extends, string command)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (command == null)
				throw new ArgumentNullException("command");

			string[] split = command.Split()
			                        .Where(s => !string.IsNullOrEmpty(s))
			                        .ToArray();

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
		public static string ExecuteConsoleCommand(this IConsoleNode extends, params string[] command)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			string first = command.FirstOrDefault(ApiConsole.HELP_COMMAND);
			string[] remaining = command.Skip(1).ToArray();

			// Help
			if (first.Equals(ApiConsole.HELP_COMMAND, StringComparison.CurrentCultureIgnoreCase))
				return extends.PrintConsoleHelp();

			// Status
			if (first.Equals(ApiConsole.STATUS_COMMAND, StringComparison.CurrentCultureIgnoreCase))
				return extends.PrintConsoleStatus();

			// Command
			IConsoleCommand nodeCommand = extends.GetConsoleCommandByName(first);
			if (nodeCommand != null)
				return nodeCommand.Execute(remaining);

			// Child
			IConsoleNodeBase[] children = extends.GetConsoleNodesBySelector(first).ToArray();
			if (children.Length == 0)
				return string.Format("Unexpected command {0}", first);

			foreach (IConsoleNodeBase child in children)
			{
				string resp = child.ExecuteConsoleCommand(remaining);
				if (resp != null)
					return resp;
			}
			return null;
		}

		/// <summary>
		/// Gets the console command with the given name. Otherwise returns null.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		[PublicAPI]
		public static IConsoleCommand GetConsoleCommandByName(this IConsoleNode extends, string name)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetConsoleCommands()
			              .FirstOrDefault(c => name.Equals(c.GetSafeConsoleName(), StringComparison.CurrentCultureIgnoreCase));
		}

		/// <summary>
		/// Prints the help to the console.
		/// </summary>
		/// <param name="extends"></param>
		public static string PrintConsoleHelp(this IConsoleNode extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			TableBuilder builder = new TableBuilder("Command", "Help");

			foreach (IConsoleNodeBase node in extends.GetConsoleNodes() ?? new IConsoleNodeBase[0])
				builder.AddRow(node.GetSafeConsoleName(), node.ConsoleHelp);

			foreach (IConsoleCommand command in (extends.GetConsoleCommands() ?? new IConsoleCommand[0]).Where(n => !n.Hidden))
				builder.AddRow(command.GetSafeConsoleName(), command.Help);

			builder.AddRow(ApiConsole.STATUS_COMMAND, "Prints the current status for this item");

			return string.Format("Help for '{0}':{1}{2}{3}", extends.GetSafeConsoleName(),
			                     IcdEnvironment.NewLine, IcdEnvironment.NewLine, builder);
		}

		/// <summary>
		/// Prints the status to the console.
		/// </summary>
		/// <param name="extends"></param>
		public static string PrintConsoleStatus(this IConsoleNode extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			TableBuilder builder = new TableBuilder("Property", "Value");
			AddStatusRowDelegate callback = (name, value) => builder.AddRow(name, string.Format("{0}", value));

			extends.BuildConsoleStatus(callback);

			return string.Format("Status for '{0}':{1}{2}{3}", extends.GetSafeConsoleName(),
			                     IcdEnvironment.NewLine, IcdEnvironment.NewLine, builder);
		}
	}
}
