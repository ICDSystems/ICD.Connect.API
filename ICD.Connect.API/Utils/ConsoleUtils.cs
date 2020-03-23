using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.API.Utils
{
	public static class ConsoleUtils
	{
		/// <summary>
		/// Given a sequence of console commands, formats each item with braces to show the shorthand command.
		/// E.g.
		/// 	ALongCommand
		/// 	AnotherLongCommand
		/// 	ALousyCommand
		/// 	BCommand
		/// Would return:
		/// 	(ALon)gCommand
		/// 	(An)otherLongCommand
		/// 	(ALou)syCommand
		/// 	(B)Command
		/// </summary>
		/// <param name="items"></param>
		/// <param name="reverse"></param>
		/// <returns></returns>
		public static IEnumerable<string> FormatMinimalConsoleCommands([NotNull] IEnumerable<string> items,
		                                                               bool reverse)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			IList<string> itemsList = items as IList<string> ?? items.ToArray();
			return itemsList.Select(item => FormatMinimalConsoleCommand(item, itemsList, reverse));
		}

		/// <summary>
		/// Given a console command and a sequence of console commands, formats the item with braces to show the shorthand command.
		/// E.g.
		/// 	ALongCommand
		/// 	AnotherLongCommand
		/// 	ALousyCommand
		/// 	BCommand
		/// Would return:
		/// 	(ALon)gCommand
		/// 	(An)otherLongCommand
		/// 	(ALou)syCommand
		/// 	(B)Command
		/// </summary>
		/// <param name="item"></param>
		/// <param name="items"></param>
		/// <param name="reverse"></param>
		/// <returns></returns>
		private static string FormatMinimalConsoleCommand(string item, [NotNull] IEnumerable<string> items,
		                                                  bool reverse)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			// Avoid comparing a console command with itself
			string[] itemsArray = items.Where(i => !string.Equals(item, i, StringComparison.OrdinalIgnoreCase)).ToArray();

			int uniqueLength = GetUniqueLength(item, itemsArray, reverse);

			// Easy case - there is no minimal console command
			if (uniqueLength >= item.Length)
				return string.Format("{0}{1}{2}", AnsiUtils.COLOR_BLUE, item, AnsiUtils.ANSI_RESET);

			return reverse
				? string.Format("{0}{1}{2}{3}",
				                item.Substring(0, item.Length - uniqueLength),
								AnsiUtils.COLOR_BLUE,
				                item.Substring(item.Length - uniqueLength),
				                AnsiUtils.ANSI_RESET)
				: string.Format("{0}{1}{2}{3}",
				                AnsiUtils.COLOR_BLUE,
								item.Substring(0, uniqueLength),
				                AnsiUtils.ANSI_RESET,
								item.Substring(uniqueLength));
		}

		/// <summary>
		/// Gets the minimum number of characters of the given item to be unique against the
		/// same number of characters from any item in the given sequence.
		/// E.g.
		///  	FooBar
		///  	FooBaz
		/// Returns
		/// 	6
		/// </summary>
		/// <param name="item"></param>
		/// <param name="items"></param>
		/// <param name="reverse"></param>
		/// <returns></returns>
		private static int GetUniqueLength(string item, [NotNull] IEnumerable<string> items, bool reverse)
		{
			if (items == null)
				throw new ArgumentNullException("items");

			return items.Select(other => GetDeviationIndex(item, other, reverse))
			            .MaxOrDefault() + 1;
		}

		/// <summary>
		/// Returns the index where the two strings start to deviate from each other.
		/// E.g.
		///		FooBar
		///		FooBaz
		/// Returns
		///		5
		/// </summary>
		/// <param name="item"></param>
		/// <param name="other"></param>
		/// <param name="reverse"></param>
		/// <returns></returns>
		private static int GetDeviationIndex([NotNull] string item, [NotNull] string other, bool reverse)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			if (other == null)
				throw new ArgumentNullException("other");

			int output = 0;

			for (int index = 1; index <= item.Length && index <= other.Length; index++)
			{
				string a;
				string b;

				if (reverse)
				{
					a = item.Substring(item.Length - index);
					b = other.Substring(other.Length - index);
				}
				else
				{
					a = item.Substring(0, index);
					b = other.Substring(0, index);
				}

				if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
					output = index;
				else
					break;
			}

			return output;
		}
	}
}
