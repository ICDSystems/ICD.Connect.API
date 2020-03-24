using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Utils;

namespace ICD.Connect.API.Nodes
{
	public interface IConsoleNodeGroup : IConsoleNodeBase
	{
		#region Methods

		/// <summary>
		/// Gets the child console nodes as a keyed collection.
		/// </summary>
		/// <returns></returns>
		new IDictionary<uint, IConsoleNodeBase> GetConsoleNodes();

		#endregion
	}

	/// <summary>
	/// Extension methods for the IConsoleNodeGroup.
	/// </summary>
	public static class ConsoleNodeGroupExtensions
	{
		#region Internal Methods

		/// <summary>
		/// Executes the command on the node group.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="command"></param>
		internal static string ExecuteConsoleCommand(this IConsoleNodeGroup extends, params string[] command)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (command == null)
				throw new ArgumentNullException("command");

			string first = command.FirstOrDefault(ApiConsole.HELP_COMMAND);
			string[] remaining = command.Skip(1).ToArray();

			// Child
			bool isAll;
			IConsoleCommon[] children = extends.GetChildrenBySelector(first, out isAll).ToArray();
			if (children.Length == 0 && !isAll)
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
		/// <param name="isAll"></param>
		/// <returns></returns>
		private static IEnumerable<IConsoleCommon> GetChildrenBySelector(this IConsoleNodeGroup extends, string selector, out bool isAll)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			isAll = false;
			KeyValuePair<string, IConsoleCommon>[] children = extends.GetChildren().ToArray();

			// Special "all" case
			if (ApiConsole.ALL_COMMAND.Equals(selector, StringComparison.OrdinalIgnoreCase))
			{
				// We only care about child nodes when using the "all" command
				isAll = true;
				return children.Select(kvp => kvp.Value)
				               .OfType<IConsoleNode>()
				               .Cast<IConsoleCommon>();
			}

			// Is there an exact match?
			IConsoleCommon exact = children.Where(kvp => kvp.Key.Equals(selector, StringComparison.OrdinalIgnoreCase))
			                               .Select(kvp => kvp.Value)
			                               .FirstOrDefault();
			if (exact != null)
				return exact.Yield();

			// Selector is an abbreviation
			try
			{
				return children.Where(kvp => kvp.Key.EndsWith(selector, StringComparison.OrdinalIgnoreCase))
				               .Select(kvp => kvp.Value)
				               .Single()
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
		private static IEnumerable<KeyValuePair<string, IConsoleCommon>> GetChildren(this IConsoleNodeGroup extends)
		{
			IEnumerable<KeyValuePair<string, IConsoleCommon>> nodes =
				extends.GetConsoleNodes()
				       .Select(kvp => new KeyValuePair<string, IConsoleCommon>(kvp.Key.ToString(), kvp.Value));

			IEnumerable<KeyValuePair<string, IConsoleCommon>> commands = new []
			{
				new ConsoleCommand(ApiConsole.SET_ROOT_COMMAND, null, () => ApiConsole.ToggleRoot(extends), true),
				new ConsoleCommand(ApiConsole.HELP_COMMAND, null, () => extends.PrintConsoleHelp(), true),
				new ConsoleCommand(ApiConsole.ALL_COMMAND, null, () => { }, true) // Special case
			}.Select(c => new KeyValuePair<string, IConsoleCommon>(c.GetSafeConsoleName(), c));

			return nodes.Concat(commands);
		}

		/// <summary>
		/// Prints the help to the console.
		/// </summary>
		/// <param name="extends"></param>
		private static string PrintConsoleHelp(this IConsoleNodeGroup extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			// Get the abbreviations for the child items
			KeyValuePair<string, IConsoleCommon>[] children = extends.GetChildren().ToArray();
			IEnumerable<string> commandNames = children.Select(kvp => kvp.Key);
			string[] formattedCommandNames = ConsoleUtils.FormatMinimalConsoleCommands(commandNames, true).ToArray();

			TableBuilder builder = new TableBuilder("Index", "Name", "Type", "Help");
			foreach (KeyValuePair<string, KeyValuePair<string, IConsoleCommon>> pair in formattedCommandNames.Zip(children))
			{
				IConsoleNodeBase node = pair.Value.Value as IConsoleNodeBase;
				if (node == null)
					continue;

				string key = pair.Key;
				string name = node.GetSafeConsoleName();
				string type = node.GetType().GetSyntaxName();
				string help = node.ConsoleHelp;

				builder.AddRow(key, name, type, help);
			}

			return string.Format("Help for '{0}':{1}{2}{3}", extends.GetSafeConsoleName(),
								 IcdEnvironment.NewLine, IcdEnvironment.NewLine, builder);
		}

		#endregion
	}
}
