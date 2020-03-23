using System;
using ICD.Common.Utils;

namespace ICD.Connect.API.Nodes
{
	public interface IConsoleCommon
	{
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		string ConsoleName { get; }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		string ConsoleHelp { get; }
	}

	public static class ConsoleCommonExtensions
	{
		/// <summary>
		/// Gets the console name without any whitespace.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		public static string GetSafeConsoleName(this IConsoleCommon extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return StringUtils.RemoveWhitespace(extends.ConsoleName);
		}
	}
}
