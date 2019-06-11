using System;
using System.IO;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Threading;
using System.Security.Principal;

using FindMissingCharsetInDbf.View.Util;

namespace FindMissingCharsetInDbf.Util
{
	/// <summary>
	/// General utility methods of the application.
	/// </summary>
	internal class Common
	{
		/// <summary>
		/// Getting the title of the main application window.
		/// In the title: name, version, build date/time, current user, etc.
		/// </summary>
		public static string GetApplicationTitle(Assembly assembly)
		{
			var currentIdentity = WindowsIdentity.GetCurrent();                         // current user
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			var user = currentIdentity != null ? currentIdentity.Name : string.Empty;   // username
			var versionInfo = GetFileVersionInfo(assembly);
			var appName = versionInfo.ProductName;
			var productVersion = versionInfo.ProductVersion;
			var buildDate = GetBuildDateTime(assembly);
			var titlePattern = GenerateTitlePattern(buildDate != null);
			return string.Format(titlePattern, appName, productVersion, buildDate != null
				? string.Format(Constants.BuildDateTimePattern, buildDate)
				: string.Empty, user);
		}

		/// <summary>
		/// Forming an application window title template, depending on the presence of DateTime application assembly
		/// </summary>
		private static string GenerateTitlePattern(bool isBuildDateExist)
		{
			const string titlePatternPart1 = "{0}, {1}";
			const string titlePatternPart2WithDate = " [Build: {2}]";
			const string titlePatternPart2WithoutDate = "{2}";
			const string titlePatternPart3 = " [{3}]";
			return titlePatternPart1 + (isBuildDateExist
					   ? titlePatternPart2WithDate
					   : titlePatternPart2WithoutDate) + titlePatternPart3;
		}

		/// <summary>
		/// Getting an object of class FileVersionInfo from the specified Assembly
		/// (Required to read the application assembly attributes: name, version, etc.)
		/// </summary>
		private static FileVersionInfo GetFileVersionInfo(Assembly assembly)
		{
			return FileVersionInfo.GetVersionInfo(assembly.Location);
		}

		/// <summary>
		/// The method for obtaining the date/time of compilation of the specified assembly
		/// </summary>
		private static DateTime? GetBuildDateTime(Assembly assembly)
		{
			var current = DateTime.Now;
			var universal = current.ToUniversalTime();
			var gmtOffset = (current - universal).TotalHours;
			try
			{
				var file = assembly.Location;
				const int headerOffset = 60;
				const int linkerTimestampOffset = 8;
				var buffer = new byte[2048];
				using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
				{
					stream.Read(buffer, 0, 2048);
				}
				var offset = BitConverter.ToInt32(buffer, headerOffset);
				var startIndex = offset + linkerTimestampOffset;
				var secondsSince1970 = BitConverter.ToInt32(buffer, startIndex);
				var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				var linkTimeUtc = epoch.AddSeconds(secondsSince1970);
				return linkTimeUtc.AddHours(gmtOffset);
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		/// Obtaining an object of a class of color of visual elements, using a hex color code
		/// </summary>
		public static SolidColorBrush BrushHex(string hexColor)
		{
			var solidColorBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(hexColor);
			if (solidColorBrush != null && !solidColorBrush.IsFrozen)
			{
				// NOTE: We make the color un-editable, because without this, the performance is very low.
				// Detailed (Freezable Objects): https://msdn.microsoft.com/en-us/library/aa970683(v=vs.85).aspx
				solidColorBrush.Freeze();
			}
			return solidColorBrush;
		}

		/// <summary>
		/// Handling unrecognized exceptions in the application's start class (Runner)
		/// At the end of the method execution, the application is terminated, and the exception with
		/// the stack trace is written to a text error file in the user directory of the operating system.
		/// [ C:\Users\username\Documents\ ] 
		/// </summary>
		public static void RootExceptionHandler(object senderIsDispatcher, DispatcherUnhandledExceptionEventArgs ex)
		{
			const string filenamePattern = "Error-log [{0}, {1}].txt";
			const string fileMessagePattern = "{0} {1} {2} {1} {3} {1} {4} {5} {1}";
			const string messageBoxHeader = PageLiterals.HeaderCriticalError;
			const string messageBoxPattern = "Unhandled exceptional situation:" +
											 "{0} {1} {0} {2} {0} " +
											 "A copy of the message is written to a file: {3} {4} {0}" +
											 "The application will be completed";
			var newLine = Environment.NewLine;
			var doubleNewLine = newLine + newLine;

			var versionInfo = GetFileVersionInfo(Assembly.GetExecutingAssembly());
			var appName = versionInfo.ProductName;
			var productVersion = versionInfo.ProductVersion;
			var errorsLogFile = string.Format(filenamePattern, appName, productVersion);

			// Write uncaught exceptions with stack trace in error-file
			var exceptionMessage = ex.Exception.Message;
			var exceptionStackTrace = ex.Exception.StackTrace;
			var personalFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var pathErrorLogFile = Path.Combine(personalFolder, errorsLogFile);
			var messageToFile = string.Format(fileMessagePattern, DateTime.Now, newLine, ex, exceptionMessage,
				exceptionStackTrace, doubleNewLine);
			AppendToBeginFile(pathErrorLogFile, messageToFile);

			// Showing the problem to the user
			const MessageBoxImage messageBoxType = MessageBoxImage.Error;
			const MessageBoxButton messageBoxButtons = MessageBoxButton.OK;
			var messageToMessageBox = string.Format(messageBoxPattern, doubleNewLine, exceptionMessage,
				exceptionStackTrace, newLine, pathErrorLogFile);
			MessageBox.Show(messageToMessageBox, messageBoxHeader, messageBoxButtons, messageBoxType);

			ex.Handled = true;
			Application.Current.Shutdown();
		}

		/// <summary>
		/// Add text to the beginning of a specified text file
		/// </summary>
		private static void AppendToBeginFile(string filePath, string message)
		{
			var oldFileContent = string.Empty;
			if (File.Exists(filePath))
			{
				oldFileContent = File.ReadAllText(filePath);
			}
			File.WriteAllText(filePath, message + oldFileContent);
		}
	}
}
