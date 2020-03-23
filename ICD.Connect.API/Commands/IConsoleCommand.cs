using ICD.Connect.API.Nodes;

namespace ICD.Connect.API.Commands
{
	public interface IConsoleCommand : IConsoleCommon
	{
		/// <summary>
		/// Returns true if the command should be hidden in the console.
		/// </summary>
		bool Hidden { get; }

		/// <summary>
		/// Executes the command.
		/// </summary>
		/// <param name="parameters"></param>
		string Execute(params string[] parameters);
	}
}
