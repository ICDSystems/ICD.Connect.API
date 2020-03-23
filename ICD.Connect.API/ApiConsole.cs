using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
#if SIMPLSHARP
using Crestron.SimplSharp;
#endif

namespace ICD.Connect.API
{
	/// <summary>
	/// Singleton representing the root node of the console.
	/// </summary>
	public sealed class ApiConsole : IConsoleNode
	{
		public const string ROOT_COMMAND = "ICD";
		private const string ROOT_HELP = "Root node for dynamic console commands";

		public const string ALL_COMMAND = "A";
		public const string HELP_COMMAND = "?";
		public const string SET_ROOT_COMMAND = "/";

		private static readonly ApiConsole s_Singleton;
		private static readonly IcdHashSet<IConsoleNodeBase> s_Children;
		private static readonly SafeCriticalSection s_ChildrenSection;
		private static readonly WeakReference s_CurrentRoot;

		#region Properties

		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public string ConsoleName { get { return ROOT_COMMAND; } }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public string ConsoleHelp { get { return ROOT_HELP; } }

		/// <summary>
		/// Gets/sets the current root for the ICD console.
		/// </summary>
		[CanBeNull]
		public static IConsoleNodeBase CurrentRoot
		{ 
			get { return s_CurrentRoot.Target as IConsoleNodeBase; } 
			set { s_CurrentRoot.Target = value; }
		}

		#endregion

		/// <summary>
		/// Static constructor.
		/// </summary>
		static ApiConsole()
		{
			s_Singleton = new ApiConsole();
			s_Children = new IcdHashSet<IConsoleNodeBase>();
			s_ChildrenSection = new SafeCriticalSection();
			s_CurrentRoot = new WeakReference(null);

			IcdConsole.AddNewConsoleCommand(ExecuteCommand, ROOT_COMMAND, ROOT_HELP, IcdConsole.eAccessLevel.Operator);

			IcdConsole.AddNewConsoleCommand(parameters => IcdConsole.ConsoleCommandResponse(CleanErrorLog(parameters)), "icderr",
			                                "Prints the error log without the added Crestron info",
			                                IcdConsole.eAccessLevel.Operator);
		}

		#region Methods

		/// <summary>
		/// Adds the console node to the root of the dynamic console.
		/// </summary>
		/// <param name="node"></param>
		[PublicAPI]
		public static void RegisterChild([NotNull] IConsoleNodeBase node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			s_ChildrenSection.Execute(() => s_Children.Add(node));
		}

		/// <summary>
		/// Removes the console node to the root of the dynamic console.
		/// </summary>
		/// <param name="node"></param>
		[PublicAPI]
		public static void UnregisterChild([NotNull] IConsoleNodeBase node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			s_ChildrenSection.Execute(() => s_Children.Remove(node));
		}

		/// <summary>
		/// Called when the user runs the ICD command.
		/// </summary>
		/// <param name="command"></param>
		[PublicAPI]
		public static void ExecuteCommand([NotNull] string command)
		{
			if (command == null)
				throw new ArgumentNullException("command");

			string output = ExecuteCommandForResponse(command);
			IcdConsole.ConsoleCommandResponseLine(output);
		}

		/// <summary>
		/// Executes the command and returns the response
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public static string ExecuteCommandForResponse([NotNull] string command)
		{
			if (command == null)
				throw new ArgumentNullException("command");

			command = command.Trim();

			IConsoleNodeBase root = CurrentRoot ?? s_Singleton;
			return root.ExecuteConsoleCommand(command);
		}

#if SIMPLSHARP

		/// <summary>
		/// Called by S+ to execute a console command via UCMD.
		/// </summary>
		/// <param name="command"></param>
		[PublicAPI]
		public static void SPlusUcmd([NotNull] SimplSharpString command)
		{
			if (command == null)
				throw new ArgumentNullException("command");

			string commandString = command.ToString().Trim();
			commandString = string.IsNullOrEmpty(commandString) ? HELP_COMMAND : commandString;

			// User convenience, let them know there's actually a UCMD handler
			if (commandString == HELP_COMMAND)
			{
				IcdConsole.ConsoleCommandResponse("Type \"{0} {1}\" to see commands registered for {2}", ROOT_COMMAND, HELP_COMMAND,
				                                  typeof(IcdConsole).Name);
				return;
			}

			// Only care about commands that start with ICD prefix.
			if (!commandString.Equals(ROOT_COMMAND, StringComparison.OrdinalIgnoreCase) &&
			    !commandString.StartsWith(ROOT_COMMAND + ' ', StringComparison.OrdinalIgnoreCase))
				return;

			// Trim the prefix
			commandString = commandString.Substring(ROOT_COMMAND.Length).Trim();

			ExecuteCommand(commandString);
		}
#endif

		/// <summary>
		/// Splits the given command into its individual parts.
		/// Enquoted sequences are not split.
		/// 
		/// E.g.
		///		"one \"two three\" four"
		/// Yields
		///		"one"
		///		"two three"
		///		"four"
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public static IEnumerable<string> Split([NotNull] string command)
		{
			if (command == null)
				throw new ArgumentNullException("command");

			return Regex.Matches(command, @"[\""].+?[\""]|[^ ]+")
			            .Cast<Match>()
			            .Select(m => m.Value)
			            .Where(s => !string.IsNullOrEmpty(s))
			            .Select(s => StringUtils.UnEnquote(s));
		}

		/// <summary>
		/// Returns a table with the program install date, current uptime, & time since last restart.
		/// </summary>
		/// <returns></returns>
		public static string Uptime()
		{
			DateTime installDateTime = ProgramUtils.ProgramInstallDate;
			string installDate = installDateTime.ToString("f");

			TimeSpan progUptime = ProcessorUtils.GetProgramUptime();
			string uptime = string.Format("{0} days {1:D2}:{2:D2}:{3:D2}",
			                              progUptime.Days,
			                              progUptime.Hours,
			                              progUptime.Minutes,
			                              progUptime.Seconds);

			TimeSpan systemUptime = ProcessorUtils.GetSystemUptime();
			string lastRestart = string.Format("{0} days {1:D2}:{2:D2}:{3:D2}",
			                                   systemUptime.Days,
			                                   systemUptime.Hours,
			                                   systemUptime.Minutes,
			                                   systemUptime.Seconds);

			TableBuilder builder = new TableBuilder("Item", "Value");

			builder.AddRow("Install Date", installDate);
			builder.AddRow("Current Uptime", uptime);
			builder.AddRow("Time Since Last Restart", lastRestart);

			return builder.ToString();
		}

		#endregion

		/// <summary>
		/// Removes crestron junk from the error log.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		private static string CleanErrorLog(params string[] args)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			string errLog = string.Empty;
			IcdConsole.SendControlSystemCommand("err " + string.Join(" ", args), ref errLog);

			string cleaned = Regex.Replace(errLog,
			                               @"(^|\n)(?:\d+\. )?(Error|Notice|Info|Warning|Ok): (?:\w*)\.exe (?:\[(App \d+)\])? *# (.+?)  # ?",
			                               "$1$4 - $3 $2:: ");
			cleaned = Regex.Replace(cleaned,
			                        @"(?<!(^|\n)\d*)(?:\d+\. )?(Error|Notice|Info|Warning|Ok): SimplSharpPro\.exe ?(?:\[App (\d+)\] )?# (.+?)  # ?",
			                        "");
			return cleaned;
		}

		#region Console

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			return s_ChildrenSection.Execute(() => s_Children.ToArray());
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			ILoggerService logger = ServiceProvider.TryGetService<ILoggerService>();
			if (logger != null)
				addRow("Logging Severity", logger.SeverityLevel);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			ILoggerService logger = ServiceProvider.TryGetService<ILoggerService>();
			if (logger != null)
			{
				string help = string.Format("SetLoggingSeverity <{0}>",
				                            StringUtils.ArrayFormat(EnumUtils.GetValues<eSeverity>().OrderBy(e => (int)e)));
				yield return new GenericConsoleCommand<eSeverity>("SetLoggingSeverity", help, s => logger.SeverityLevel = s);
			}

			yield return new ConsoleCommand("PrintThreads", "Prints a table of the known active threads", () => ThreadingUtils.PrintThreads());

			yield return new ConsoleCommand("Uptime",
			                                "Prints a table with the program install date, current uptime, & time since last restart",
			                                () => Uptime());

		}

		#endregion

		/// <summary>
		/// If the given node matches the current root, clears the root.
		/// Otherwise, sets the current root.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		public static string ToggleRoot([CanBeNull] IConsoleNodeBase node)
		{
			if (node == CurrentRoot)
				node = null;

			CurrentRoot = node;

			string output =
				node == null
					? "Cleared current root"
					: "Set current root to " + node.GetSafeConsoleName();

			// Hack - Lets include the help text for the current node
			string help = ExecuteCommandForResponse(HELP_COMMAND);

			return string.Format("{0}{1}{1}{2}", output, IcdEnvironment.NewLine, help);
		}
	}
}
