using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Utils;

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
		public static string ExecuteConsoleCommand(this IConsoleNode extends, params string[] command)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			string first = command.FirstOrDefault(ApiConsole.HELP_COMMAND);
			string[] remaining = command.Skip(1).ToArray();

			// Root
			if (first.Equals(ApiConsole.SET_ROOT_COMMAND, StringComparison.CurrentCultureIgnoreCase))
				return ApiConsole.ToggleRoot(extends);

			// Help
			if (first.Equals(ApiConsole.HELP_COMMAND, StringComparison.CurrentCultureIgnoreCase))
				return extends.PrintConsoleHelp();

			// Status
			if (first.Equals(ApiConsole.STATUS_COMMAND, StringComparison.CurrentCultureIgnoreCase))
				return extends.PrintConsoleStatus();

			// Command
			IConsoleCommand nodeCommand = extends.GetConsoleCommandByName(first);
			if (nodeCommand != null)
			{
				try
				{
					return nodeCommand.Execute(remaining);
				}
				catch (Exception e)
				{
					return string.Format("Failed to execute console command {0} - {1}", first, e.Message);
				}
			}

			// Child
			IConsoleNodeBase[] children = extends.GetConsoleNodesBySelector(first).ToArray();
			if (children.Length == 0)
				return string.Format("Unexpected command {0}", StringUtils.ToRepresentation(first));

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

			try
			{
				return extends.GetConsoleCommands()
				              .Append(new ConsoleCommand("Status", "Prints the current status for this item",
				                                         () => PrintConsoleStatus(extends)))
				              .SingleOrDefault(c => c.GetSafeConsoleName()
				                                     .StartsWith(name, StringComparison.OrdinalIgnoreCase));

			}
			catch (InvalidOperationException)
			{
				return null;
			}
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

			IConsoleNodeBase[] nodes = extends.GetConsoleNodes().ToArray();
			IConsoleCommand[] commands =
				extends.GetConsoleCommands()
				       .Append(new ConsoleCommand("Status", "Prints the current status for this item",
				                                  () => PrintConsoleStatus(extends)))
				       .ToArray();
			IEnumerable<string> commandNames = nodes.Select(n => n.GetSafeConsoleName())
			                                        .Concat(commands.Select(c => c.GetSafeConsoleName()));
			string[] formattedCommandNames = ConsoleUtils.FormatMinimalConsoleCommands(commandNames, false).ToArray();

			// Add the nodes
			for (int index = 0; index < nodes.Length; index++)
			{
				IConsoleNodeBase node = nodes[index];
				string name = formattedCommandNames[index];
				builder.AddRow(name, node.ConsoleHelp);
			}

			// Add the console commands
			for (int index = 0; index < commands.Length; index++)
			{
				int indexOffset = index + nodes.Length;
				IConsoleCommand command = commands[index];
				string name = formattedCommandNames[indexOffset];
				builder.AddRow(name, command.ConsoleHelp);
			}

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
			builder.AddRow("Type", extends.GetType().Name);

			AddStatusRowDelegate callback = (name, value) =>
			{
				string valueString = GetStatusString(value);
				builder.AddRow(name, valueString);
			};

			extends.BuildConsoleStatus(callback);

			return string.Format("Status for '{0}':{1}{2}{3}", extends.GetSafeConsoleName(),
			                     IcdEnvironment.NewLine, IcdEnvironment.NewLine, builder);
		}

		private static string GetStatusString(object value)
		{
			string output = value == null ? "NULL" : value.ToString();

			if (value is bool)
			{
				string code = (bool)value ? AnsiUtils.COLOR_GREEN : AnsiUtils.COLOR_RED;
				output = AnsiUtils.Format(output, code);
			}

			return output;
		}
	}
}
