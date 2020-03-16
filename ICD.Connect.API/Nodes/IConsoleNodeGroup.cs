using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Utils;

namespace ICD.Connect.API.Nodes
{
	public interface IConsoleNodeGroup : IConsoleNodeBase
	{
		/// <summary>
		/// Gets the child console nodes as a keyed collection.
		/// </summary>
		/// <returns></returns>
		new IDictionary<uint, IConsoleNodeBase> GetConsoleNodes();
	}

	/// <summary>
	/// Extension methods for the IConsoleNodeGroup.
	/// </summary>
	public static class ConsoleNodeGroupExtensions
	{
		/// <summary>
		/// Executes the command on the node group.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="command"></param>
		public static string ExecuteConsoleCommand(this IConsoleNodeGroup extends, params string[] command)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (command == null)
				throw new ArgumentNullException("command");

			string first = command.FirstOrDefault(ApiConsole.HELP_COMMAND);
			string[] remaining = command.Skip(1).ToArray();

			// Root
			if (first.Equals(ApiConsole.SET_ROOT_COMMAND, StringComparison.CurrentCultureIgnoreCase))
				return ApiConsole.ToggleRoot(extends);

			// Help
			if (first.Equals(ApiConsole.HELP_COMMAND, StringComparison.CurrentCultureIgnoreCase))
				return extends.PrintConsoleHelp();

			IConsoleNodeBase[] nodes = extends.GetConsoleNodesBySelector(first).ToArray();
			if (nodes.Length == 0)
				return string.Format("{0} has no item with key {1}", extends.GetSafeConsoleName(),
				                     StringUtils.ToRepresentation(first));

			string output = null;

			foreach (IConsoleNodeBase node in nodes)
			{
				string resp = node.ExecuteConsoleCommand(remaining);
				if (resp != null)
					output = (output ?? string.Empty) + IcdEnvironment.NewLine + resp;
			}

			return output;
		}

		/// <summary>
		/// Gets the child console node with the given key/index.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		internal static IConsoleNodeBase GetConsoleNodeByKey(this IConsoleNodeGroup extends, uint key)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			try
			{
				// Match the trailing digits
				return extends.GetConsoleNodes()
				              .Single(kvp => kvp.Key.ToString().EndsWith(key.ToString(), StringComparison.OrdinalIgnoreCase))
				              .Value;
			}
			catch (InvalidOperationException)
			{
				throw new ArgumentOutOfRangeException("extends", "No item with key " + key);
			}
		}

		/// <summary>
		/// Prints the help to the console.
		/// </summary>
		/// <param name="extends"></param>
		public static string PrintConsoleHelp(this IConsoleNodeGroup extends)
		{
			TableBuilder builder = new TableBuilder("Index", "Name", "Type", "Help");

			KeyValuePair<uint, IConsoleNodeBase>[] kvps =
				extends.GetConsoleNodes()
				       .OrderByKey()
				       .ToArray();

			string[] keysFormatted =
				ConsoleUtils.FormatMinimalConsoleCommands(kvps.Select(kvp => kvp.Key.ToString()), true)
				            .ToArray();

			for (int index = 0; index < kvps.Length; index++)
			{
				IConsoleNodeBase node = kvps[index].Value;
				string key = keysFormatted[index];

				string name = node.GetSafeConsoleName();
				string type = node.GetType().GetSyntaxName();
				string help = node.ConsoleHelp;

				builder.AddRow(key, name, type, help);
			}

			return string.Format("Help for '{0}':{1}{2}", extends.GetSafeConsoleName(), IcdEnvironment.NewLine, builder);
		}
	}
}
