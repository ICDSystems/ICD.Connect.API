using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;

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
			string first = command.Length == 0 ? ApiConsole.HELP_COMMAND : command[0];
			string[] remaining = command.Skip(command.Length == 0 ? 0 : 1).ToArray();

			// Help
			if (first.Equals(ApiConsole.HELP_COMMAND, StringComparison.CurrentCultureIgnoreCase))
				return extends.PrintConsoleHelp();

			// Child
			uint index;
			bool isIndex = StringUtils.TryParse(first, out index);
			bool all = !isIndex && first.Equals(ApiConsole.ALL_COMMAND, StringComparison.CurrentCultureIgnoreCase);

			// If the user didnt specify an index, the first part of the command is part of the next command
			if (!isIndex && !all)
				remaining = command;

			IConsoleNodeBase[] nodes = extends.GetConsoleNodes(all, index).ToArray();
			if (nodes.Length == 0)
				return string.Format("Unexpected command {0}", first);

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
		/// Convenience method for getting the child nodes based on user selection.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="all"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		private static IEnumerable<IConsoleNodeBase> GetConsoleNodes(this IConsoleNodeGroup extends, bool all, uint index)
		{
			if (all)
				return extends.GetConsoleNodes().OrderValuesByKey();

			IConsoleNodeBase output;
			if (extends.GetConsoleNodes().TryGetValue(index, out output))
				return new[] {output};

			return Enumerable.Empty<IConsoleNodeBase>();
		}

		/// <summary>
		/// Prints the help to the console.
		/// </summary>
		/// <param name="extends"></param>
		public static string PrintConsoleHelp(this IConsoleNodeGroup extends)
		{
			TableBuilder builder = new TableBuilder("Index", "Name", "Type", "Help");

			foreach (KeyValuePair<uint, IConsoleNodeBase> kvp in extends.GetConsoleNodes().OrderByKey())
			{
				IConsoleNodeBase node = kvp.Value;
				string name = node.GetSafeConsoleName();
				string type = node.GetType().Name;
				string help = node.ConsoleHelp;

				builder.AddRow(kvp.Key.ToString(), name, type, help);
			}

			return string.Format("Help for '{0}':{1}{2}", extends.GetSafeConsoleName(), IcdEnvironment.NewLine, builder);
		}
	}
}
