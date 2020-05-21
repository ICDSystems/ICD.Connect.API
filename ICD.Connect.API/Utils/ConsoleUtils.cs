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

			// List of items as they came in
			IList<string> itemsList = items as IList<string> ?? items.ToArray();

			// List of the items sorted forwards/backwards
			IComparer<string> comparer =
				reverse
					? (IComparer<string>)ReverseOrdinalIgnoreCaseComparer.Instance
					: StringComparer.OrdinalIgnoreCase;
			string[] sorted = itemsList.OrderBy(i => i, comparer).ToArray();

			// Compare each item with adjacent items.
			foreach (string item in itemsList)
			{
				int index = sorted.BinarySearch(item, comparer);

				string previous = index == 0 ? null : sorted[index - 1];
				string next = index == itemsList.Count - 1 ? null : sorted[index + 1];

				int maxDeviation = 0;

				if (previous != null)
				{
					int deviation = GetDeviationIndex(item, previous, reverse);
					maxDeviation = Math.Max(deviation, maxDeviation);
				}

				if (next != null)
				{
					int deviation = GetDeviationIndex(item, next, reverse);
					maxDeviation = Math.Max(deviation, maxDeviation);
				}

				yield return FormatMinimalConsoleCommand(item, maxDeviation, reverse);
			}
		}

		/// <summary>
		/// Given a console command and an index, formats the item to highlight the shorthand command.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index"></param>
		/// <param name="reverse"></param>
		/// <returns></returns>
		[NotNull]
		private static string FormatMinimalConsoleCommand([NotNull] string item, int index, bool reverse)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			int length = index + 1;

			// Easy case - there is no minimal console command
			if (length >= item.Length)
				return string.Format("{0}{1}{2}", AnsiUtils.COLOR_BLUE, item, AnsiUtils.ANSI_RESET);

			return reverse
				? string.Format("{0}{1}{2}{3}",
				                item.Substring(0, item.Length - length),
				                AnsiUtils.COLOR_BLUE,
				                item.Substring(item.Length - length),
				                AnsiUtils.ANSI_RESET)
				: string.Format("{0}{1}{2}{3}",
				                AnsiUtils.COLOR_BLUE,
				                item.Substring(0, length),
				                AnsiUtils.ANSI_RESET,
				                item.Substring(length));
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

			item = item.ToUpper();
			other = other.ToUpper();

			int output = 0;

			for (int index = 0; index < item.Length && index < other.Length; index++)
			{
				char a = reverse ? item[item.Length - (index + 1)] : item[index];
				char b = reverse ? other[other.Length - (index + 1)] : other[index];

				if (a == b)
					output = index + 1;
				else
					break;
			}

			return output;
		}

		public sealed class ReverseOrdinalIgnoreCaseComparer : IComparer<string>
		{
			private static ReverseOrdinalIgnoreCaseComparer s_Instance;

			public static ReverseOrdinalIgnoreCaseComparer Instance
			{
				get { return s_Instance ?? (s_Instance = new ReverseOrdinalIgnoreCaseComparer()); }
			}

			public int Compare(string x, string y)
			{
				x = x == null ? null : StringUtils.Reverse(x);
				y = y == null ? null : StringUtils.Reverse(y);

				return StringComparer.OrdinalIgnoreCase.Compare(x, y);
			}
		}
	}
}
