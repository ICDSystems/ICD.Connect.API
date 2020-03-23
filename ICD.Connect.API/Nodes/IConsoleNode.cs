using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Utils;

namespace ICD.Connect.API.Nodes
{
	public delegate void AddStatusRowDelegate(string name, object value);

	public interface IConsoleNode : IConsoleNodeBase
	{
		#region Methods

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

		#endregion
	}

	/// <summary>
	/// Extension methods for IConsoleNodes.
	/// </summary>
	public static class ConsoleNodeExtensions
	{
		#region Internal Methods

		/// <summary>
		/// Runs the command.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="command"></param>
		internal static string ExecuteConsoleCommand(this IConsoleNode extends, params string[] command)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			string first = command.FirstOrDefault(ApiConsole.HELP_COMMAND);
			string[] remaining = command.Skip(1).ToArray();

			IConsoleCommon[] children = extends.GetChildrenBySelector(first).ToArray();
			if (children.Length == 0)
				return string.Format("Unexpected command {0}", StringUtils.ToRepresentation(first));

			string[] output = children.Select(c => c.ExecuteConsoleCommand(remaining))
			                          .Where(o => !string.IsNullOrEmpty(o))
			                          .ToArray();

			return string.Join(IcdEnvironment.NewLine, output);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the child console nodes based on the given selector (e.g. index, all, etc).
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="selector"></param>
		/// <returns></returns>
		private static IEnumerable<IConsoleCommon> GetChildrenBySelector(this IConsoleNode extends, string selector)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			IConsoleCommon[] children = extends.GetChildren().ToArray();

			// Is there an exact match?
			IConsoleCommon exact = children.FirstOrDefault(c => selector.Equals(c.GetSafeConsoleName(), StringComparison.OrdinalIgnoreCase));
			if (exact != null)
				return exact.Yield();

			// Selector is an abbreviation
			try
			{
				return children.Single(g => g.GetSafeConsoleName()
				                             .StartsWith(selector, StringComparison.CurrentCultureIgnoreCase))
				               .Yield();
			}
			catch (InvalidOperationException)
			{
				return Enumerable.Empty<IConsoleCommon>();
			}
		}

		/// <summary>
		/// Gets the full set of child nodes and commands for the given console node.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		private static IEnumerable<IConsoleCommon> GetChildren(this IConsoleNode extends)
		{
			IEnumerable<IConsoleCommon> nodes =
				extends.GetConsoleNodes()
				       .Cast<IConsoleCommon>();

			IEnumerable<IConsoleCommon> commands =
				extends.GetConsoleCommands()
					// Add the special commands
					   .Append(new ConsoleCommand("Status", "Prints the current status for this item", () => PrintConsoleStatus(extends)))
				       .Append(new ConsoleCommand(ApiConsole.SET_ROOT_COMMAND, null, () => ApiConsole.ToggleRoot(extends), true))
				       .Append(new ConsoleCommand(ApiConsole.HELP_COMMAND, null, () => extends.PrintConsoleHelp(), true))
					   .Cast<IConsoleCommon>();

			return nodes.Concat(commands);
		}

		/// <summary>
		/// Prints the help to the console.
		/// </summary>
		/// <param name="extends"></param>
		private static string PrintConsoleHelp(this IConsoleNode extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			// Get the abbreviations for the child items
			IConsoleCommon[] children = extends.GetChildren().ToArray();
			IEnumerable<string> commandNames = children.Select(c => c.GetSafeConsoleName());
			string[] formattedCommandNames = ConsoleUtils.FormatMinimalConsoleCommands(commandNames, false).ToArray();

			TableBuilder builder = new TableBuilder("Command", "Help");
			foreach (KeyValuePair<string, IConsoleCommon> pair in formattedCommandNames.Zip(children))
			{
				IConsoleCommand command = pair.Value as IConsoleCommand;
				if (command != null && command.Hidden)
					continue;

				builder.AddRow(pair.Key, pair.Value.ConsoleHelp);
			}

			return string.Format("Help for '{0}':{1}{2}{3}", extends.GetSafeConsoleName(),
			                     IcdEnvironment.NewLine, IcdEnvironment.NewLine, builder);
		}

		/// <summary>
		/// Prints the status to the console.
		/// </summary>
		/// <param name="extends"></param>
		private static string PrintConsoleStatus(this IConsoleNode extends)
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

		/// <summary>
		/// Gets the string representation for the given status value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
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

		#endregion
	}
}
