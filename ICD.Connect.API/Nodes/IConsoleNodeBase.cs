using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;

namespace ICD.Connect.API.Nodes
{
	public interface IConsoleNodeBase
	{
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		string ConsoleName { get; }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		string ConsoleHelp { get; }

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IConsoleNodeBase> GetConsoleNodes();
	}

	/// <summary>
	/// Extension methods for IConsoleNodeBase.
	/// </summary>
	public static class ConsoleNodeBaseExtensions
	{
		/// <summary>
		/// Runs the command.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="command"></param>
		public static string ExecuteConsoleCommand(this IConsoleNodeBase extends, string command)
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
		public static string ExecuteConsoleCommand(this IConsoleNodeBase extends, params string[] command)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			IConsoleNode node = extends as IConsoleNode;
			if (node != null)
				return node.ExecuteConsoleCommand(command);

			IConsoleNodeGroup group = extends as IConsoleNodeGroup;
			if (group != null)
				return group.ExecuteConsoleCommand(command);

			throw new ArgumentOutOfRangeException("extends", "Unable to execute console command for type "
			                                                 + extends.GetType().Name);
		}

		/// <summary>
		/// Gets the console name without any whitespace.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		public static string GetSafeConsoleName(this IConsoleNodeBase extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return StringUtils.RemoveWhitespace(extends.ConsoleName);
		}

		/// <summary>
		/// Gets the child console nodes based on the given selector (e.g. index, all, etc).
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="selector"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleNodeBase> GetConsoleNodesBySelector(this IConsoleNodeBase extends, string selector)
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
		private static IEnumerable<IConsoleNodeBase> GetConsoleNodesBySelectorIterator(IConsoleNodeBase node, string selector)
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
				foreach (IConsoleNodeBase child in node.GetConsoleNodes())
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
		private static IConsoleNodeBase GetConsoleNodeByName(this IConsoleNodeBase extends, string name)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetConsoleNodes()
			              .FirstOrDefault(g => name.Equals(g.GetSafeConsoleName(), StringComparison.CurrentCultureIgnoreCase));
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
			}

			IConsoleNode node = extends as IConsoleNode;
			if (node != null)
				return node.GetConsoleNodes().ElementAtOrDefault((int)key);

			throw new ArgumentOutOfRangeException("extends", "Unable to execute console command for type "
			                                                 + extends.GetType().Name);
		}
	}
}
