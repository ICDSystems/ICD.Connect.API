using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
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
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (command == null)
				throw new ArgumentNullException("command");

			string first = command.FirstOrDefault(ApiConsole.HELP_COMMAND);
			string[] remaining = command.Skip(1).ToArray();

			// Help
			if (first.Equals(ApiConsole.HELP_COMMAND, StringComparison.CurrentCultureIgnoreCase))
				return extends.PrintConsoleHelp();

			IConsoleNodeBase[] nodes;

			try
			{
				nodes = GetConsoleNodesBySelector(extends, first).ToArray();
			}
			catch (Exception)
			{
				return string.Format("{0} has no item with key {0}", extends.GetSafeConsoleName(),
				                     StringUtils.ToRepresentation(first));
			}

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
		/// Gets the child console nodes based on the given selector (e.g. index, all, etc).
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleNodeBase> GetConsoleNodesBySelector(this IConsoleNodeGroup extends, string selector)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return GetConsoleNodesBySelectorIterator(extends, selector);
		}

		/// <summary>
		/// Gets the child console nodes based on the given selector (e.g. index, all, etc).
		/// </summary>
		/// <param name="node"></param>
		/// <param name="selector"></param>
		/// <returns></returns>
		private static IEnumerable<IConsoleNodeBase> GetConsoleNodesBySelectorIterator(IConsoleNodeGroup node, string selector)
		{
			// Selector is an index.
			uint index;
			if (StringUtils.TryParse(selector, out index))
			{
				IConsoleNodeBase indexed = node.GetConsoleNodeByKey(index);
				if (indexed != null)
					yield return indexed;
				yield break;
			}

			// Selector is all
			if (selector.Equals(ApiConsole.ALL_COMMAND, StringComparison.CurrentCultureIgnoreCase))
			{
				foreach (IConsoleNodeBase child in node.GetConsoleNodes().Values)
					yield return child;
				yield break;
			}

			// Selector is a name
			IConsoleNodeBase named = node.GetConsoleNodeByName(selector);
			if (named != null)
				yield return named;
		}

		/// <summary>
		/// Gets the child console node with the given name. Otherwise returns null.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		[CanBeNull]
		private static IConsoleNodeBase GetConsoleNodeByName(this IConsoleNodeGroup extends, string name)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetConsoleNodes()
			              .Values
			              .FirstOrDefault(v => name.Equals(v.GetSafeConsoleName(), StringComparison.CurrentCultureIgnoreCase));
		}

		/// <summary>
		/// Gets the child console node with the given key/index.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		private static IConsoleNodeBase GetConsoleNodeByKey(this IConsoleNodeBase extends, uint key)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			IConsoleNodeGroup group = extends as IConsoleNodeGroup;
			if (group != null)
			{
				IConsoleNodeBase output;
				if (group.GetConsoleNodes().TryGetValue(key, out output))
					return output;

				throw new ArgumentOutOfRangeException("extends", "No child with the given key");
			}

			IConsoleNode node = extends as IConsoleNode;
			if (node != null)
				return node.GetConsoleNodes().ElementAtOrDefault((int)key);

			throw new ArgumentOutOfRangeException("extends", "Unable to execute console command for type "
			                                                 + extends.GetType().Name);
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
				string type = node.GetType().GetSyntaxName();
				string help = node.ConsoleHelp;

				builder.AddRow(kvp.Key.ToString(), name, type, help);
			}

			return string.Format("Help for '{0}':{1}{2}", extends.GetSafeConsoleName(), IcdEnvironment.NewLine, builder);
		}
	}
}
