using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Collections;

namespace ICD.Connect.API.Nodes
{
	public sealed class ConsoleNodeGroup : IConsoleNodeGroup
	{
		private readonly string m_ConsoleName;
		private readonly string m_Help;
		private readonly IEnumerable<KeyValuePair<uint, IConsoleNodeBase>> m_Enumerable;

		private IcdOrderedDictionary<uint, IConsoleNodeBase> m_Nodes;

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
			if (nodes == null)
				throw new ArgumentNullException("nodes");

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
			if (nodes == null)
				throw new ArgumentNullException("nodes");

			// Add 1, the user wants to press 1 for the first item.
			IEnumerable<KeyValuePair<uint, IConsoleNodeBase>> enumerable =
				nodes.Select((node, index) => new KeyValuePair<uint, IConsoleNodeBase>((uint)index + 1, node));

			return new ConsoleNodeGroup(name, help, enumerable);
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
			if (nodes == null)
				throw new ArgumentNullException("nodes");

			if (getKey == null)
				throw new ArgumentNullException("getKey");

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
			if (nodes == null)
				throw new ArgumentNullException("nodes");

			if (getKey == null)
				throw new ArgumentNullException("getKey");

			IEnumerable<KeyValuePair<uint, IConsoleNodeBase>> enumerable =
				nodes.Select(node => new KeyValuePair<uint, IConsoleNodeBase>(getKey(node), node));

			return new ConsoleNodeGroup(name, help, enumerable);
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="help"></param>
		/// <param name="kvps"></param>
		private ConsoleNodeGroup(string name, string help, IEnumerable<KeyValuePair<uint, IConsoleNodeBase>> kvps)
		{
			if (kvps == null)
				throw new ArgumentNullException("kvps");

			m_ConsoleName = name;
			m_Help = help;
			m_Enumerable = kvps;
		}

		#endregion

		/// <summary>
		/// Gets the child console nodes as a keyed collection.
		/// </summary>
		/// <returns></returns>
		public IDictionary<uint, IConsoleNodeBase> GetConsoleNodes()
		{
			if (m_Nodes == null)
			{
				m_Nodes = new IcdOrderedDictionary<uint, IConsoleNodeBase>();

				foreach (KeyValuePair<uint, IConsoleNodeBase> pair in m_Enumerable)
				{
					if (m_Nodes.ContainsKey(pair.Key))
						throw new InvalidOperationException(string.Format("{0} {1} already contains key {2}", GetType().Name,
						                                                  this.GetSafeConsoleName(), pair.Key));

					m_Nodes.Add(pair.Key, pair.Value);
				}
			}

			return new Dictionary<uint, IConsoleNodeBase>(m_Nodes);
		}

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IConsoleNodeBase> IConsoleNodeBase.GetConsoleNodes()
		{
			return GetConsoleNodes().Values;
		}
	}
}
