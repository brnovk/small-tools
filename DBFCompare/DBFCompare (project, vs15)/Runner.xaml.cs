using DBFCompare.Util;

namespace DBFCompare
{
	/// <summary>
	/// Application launch class
	/// </summary>
	/// <inheritdoc />
	public partial class Runner
	{
		private Runner()
		{
			// Handling uncaught exceptions
			Dispatcher.UnhandledException += Common.RootExceptionHandler;
		}
	}
}
