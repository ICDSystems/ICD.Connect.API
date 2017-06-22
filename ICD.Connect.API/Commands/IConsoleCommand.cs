using ICD.Common.Utils;

namespace ICD.Connect.API.Commands
{
	public interface IConsoleCommand
	{
		/// <summary>
		/// Gets the name of the command.
		/// </summary>
		string ConsoleName { get; }

		/// <summary>
		/// Gets the help for the command.
		/// </summary>
		string Help { get; }

		bool Hidden { get; }

		/// <summary>
		/// Executes the command.
		/// </summary>
		/// <param name="parameters"></param>
		string Execute(params string[] parameters);
	}

	/// <summary>
	/// Extension methods for console commands.
	/// </summary>
	public static class ConsoleCommandExtensions
	{
		/// <summary>
		/// Gets the console name without any whitespace.
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		public static string GetSafeConsoleName(this IConsoleCommand extends)
		{
			return StringUtils.RemoveWhitespace(extends.ConsoleName);
		}
	}
}
