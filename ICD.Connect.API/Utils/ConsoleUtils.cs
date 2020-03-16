using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.API.Utils
{
	public static class ConsoleUtils
	{
		/// <summary>
		/// Given a sequence of console commands, formats each item with braces to show the shorthand command.
		/// E.g.
		///		ALongCommand
		///		AnotherLongCommand
		///		ALousyCommand
		///		BCommand
		///		
		/// Would return:
		///		(ALon)gCommand
		///		(An)otherLongCommand
		///		(ALou)syCommand
		///		(B)Command
		/// </summary>
		/// <param name="items"></param>
		/// <returns></returns>
		public static IEnumerable<string> FormatMinimalConsoleCommands([NotNull] IEnumerable<string> items)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			IList<string> itemsList = items as IList<string> ?? items.ToArray();
			return itemsList.Select(item => FormatMinimalConsoleCommand(item, itemsList));
		}

		///  <summary>
		///  Given a console command and a sequence of console commands, formats the item with braces to show the shorthand command.
		///  E.g.
		/// 		ALongCommand
		/// 		AnotherLongCommand
		/// 		ALousyCommand
		/// 		BCommand
		/// 		
		///  Would return:
		/// 		(ALon)gCommand
		/// 		(An)otherLongCommand
		/// 		(ALou)syCommand
		/// 		(B)Command
		///  </summary>
		/// <param name="item"></param>
		/// <param name="items"></param>
		///  <returns></returns>
		private static string FormatMinimalConsoleCommand(string item, [NotNull] IEnumerable<string> items)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			// Avoid comparing a console command with itself
			items = items.Where(i => !string.Equals(item, i, StringComparison.OrdinalIgnoreCase));

			int leadingMatch = GetLeadingMatch(item, items);

			// Easy case - there is no minimal console command
			if (leadingMatch >= item.Length)
				return item;

			// Insert braces
			return string.Format("({0}){1}", item.Substring(0, leadingMatch + 1), item.Substring(leadingMatch + 1));
		}

		/// <summary>
		/// Gets the min length from the start of the item to each string, finding the
		/// point where they deviate from the given.
		/// E.g.
		///		FooBar
		///		FooBaz
		/// Returns
		///		5
		/// </summary>
		/// <param name="item"></param>
		/// <param name="items"></param>
		/// <returns></returns>
		private static int GetLeadingMatch(string item, [NotNull] IEnumerable<string> items)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			return items.Select(other => GetLeadingMatch(item, other))
			            .MaxOrDefault();
		}

		/// <summary>
		/// Gets the length from the start of each string to the point where they deviate.
		/// E.g.
		///		FooBar
		///		FooBaz
		/// Returns
		///		5
		/// </summary>
		/// <param name="item"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		private static int GetLeadingMatch([NotNull] string item, [NotNull] string other)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			if (other == null)
				throw new ArgumentNullException("other");

			int output = 0;

			for (int index = 1; index <= item.Length && index <= other.Length; index++)
			{
				string a = item.Substring(0, index);
				string b = other.Substring(0, index);

				if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
					output = index;
				else
					break;
			}

			return output;
		}
	}
}