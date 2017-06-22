using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.API.Nodes
{
	public sealed class ConsoleNodeGroup : IConsoleNodeGroup
	{
		private readonly string m_ConsoleName;
		private readonly string m_Help;
		private readonly Dictionary<uint, IConsoleNodeBase> m_Nodes;

		#region Properties

		/// <summary>
		/// Gets the name of the group.
		/// </summary>
		public string ConsoleName { get { return m_ConsoleName; } }

		/// <summary>
		/// Gets the help for the group.
		/// </summary>
		public string ConsoleHelp { get { return m_Help; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Default mapping of index to node.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="nodes"></param>
		/// <returns></returns>
		[PublicAPI]
		public static ConsoleNodeGroup IndexNodeMap<T>(string name, IEnumerable<T> nodes)
			where T : IConsoleNodeBase
		{
			return IndexNodeMap(name, string.Empty, nodes);
		}

		/// <summary>
		/// Default mapping of index to node.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="nodes"></param>
		/// <returns></returns>
		[PublicAPI]
		public static ConsoleNodeGroup IndexNodeMap<T>(string name, string help, IEnumerable<T> nodes)
			where T : IConsoleNodeBase
		{
			Dictionary<uint, IConsoleNodeBase> output = new Dictionary<uint, IConsoleNodeBase>();

			// Add 1, the user wants to press 1 for the first item.
			nodes.ForEach((item, index) => output[(uint)index + 1] = item);

			return new ConsoleNodeGroup(name, help, output);
		}

		/// <summary>
		/// Custom mapping of key to node.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="nodes"></param>
		/// <param name="getKey"></param>
		/// <returns></returns>
		[PublicAPI]
		public static ConsoleNodeGroup KeyNodeMap<T>(string name, IEnumerable<T> nodes, Func<T, uint> getKey)
			where T : IConsoleNodeBase
		{
			return KeyNodeMap(name, string.Empty, nodes, getKey);
		}

		/// <summary>
		/// Custom mapping of key to node.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="nodes"></param>
		/// <param name="getKey"></param>
		/// <returns></returns>
		[PublicAPI]
		public static ConsoleNodeGroup KeyNodeMap<T>(string name, string help, IEnumerable<T> nodes, Func<T, uint> getKey)
			where T : IConsoleNodeBase
		{
			Dictionary<uint, IConsoleNodeBase> dict = new Dictionary<uint, IConsoleNodeBase>();

			foreach (T item in nodes)
			{
				uint key = getKey(item);
				dict.Add(key, item);
			}

			return new ConsoleNodeGroup(name, help, dict);
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="nodes"></param>
		private ConsoleNodeGroup(string name, string help, IDictionary<uint, IConsoleNodeBase> nodes)
		{
			m_ConsoleName = name;
			m_Help = help;
			m_Nodes = new Dictionary<uint, IConsoleNodeBase>(nodes);
		}

		#endregion

		/// <summary>
		/// Gets the child console nodes as a keyed collection.
		/// </summary>
		/// <returns></returns>
		public IDictionary<uint, IConsoleNodeBase> GetConsoleNodes()
		{
			return new Dictionary<uint, IConsoleNodeBase>(m_Nodes);
		}

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IConsoleNodeBase> IConsoleNodeBase.GetConsoleNodes()
		{
			return GetConsoleNodes().OrderValuesByKey();
		}
	}
}
