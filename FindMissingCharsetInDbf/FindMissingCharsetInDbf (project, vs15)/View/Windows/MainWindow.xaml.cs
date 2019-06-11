using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Documents;
using System.Collections.Generic;

using FindMissingCharsetInDbf.Util;
using FindMissingCharsetInDbf.View.Util;

namespace FindMissingCharsetInDbf.View.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	/// <inheritdoc cref="Window" />
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();
			AdditionalInitializeComponent();
			VisualInitializeComponent();
			RefreshCountFoundedAndSetActivityExportButton();
		}

		private void AdditionalInitializeComponent()
		{
			Title = Common.GetApplicationTitle(Assembly.GetExecutingAssembly());

			var lastPath = Properties.Settings.Default.LastPath;
			if (!string.IsNullOrWhiteSpace(lastPath))
			{
				SourcePathTextBox.Text = lastPath;
			}
		}

		private void VisualInitializeComponent()
		{
			Background = Constants.BackColor2_Botticelli;
			Foreground = Constants.ForeColor2_PapayaWhip;
			FontSize = Constants.FontSize;
			CountLabel.FontFamily = new FontFamily(Constants.MonospacedFontFamily);
		}

		private void ExecuteFindAllButton_OnClick(object senderIsButton, RoutedEventArgs eventArgs)
		{
			const string fileExtensionPattern = "*.dbf";
			var sourceFolderPath = SourcePathTextBox.Text.Trim();
			FilesRichTextBox.Document.Blocks.Clear();
			if (string.IsNullOrWhiteSpace(sourceFolderPath))
			{
				const string message = PageLiterals.ValidationRootPathNotSpecified;
				MessageBox.Show(message, PageLiterals.HeaderError,
					MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				return;
			}
			if (!Directory.Exists(sourceFolderPath))
			{
				var message = string.Format(PageLiterals.ValidationDirectoryInvalidPattern, sourceFolderPath);
				MessageBox.Show(message, PageLiterals.HeaderError,
					MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				return;
			}
			var enumerable = SafeGetDirectoryFiles(sourceFolderPath, fileExtensionPattern, 
				SearchOption.AllDirectories);
			var dbfFiles = enumerable as string[] ?? enumerable.ToArray();
			if (!dbfFiles.Any())
			{
				MessageBox.Show(PageLiterals.ValidationDbfNotFound, PageLiterals.HeaderInformationOrWarning,
					MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
				return;
			}

			var fileDescriptions = new List<FileDescription>(dbfFiles.Length);
			foreach (var dbfFile in dbfFiles)
			{
				if (!IsAccessToFile(dbfFile))
				{
					continue;
				}
				var tableTypeFlag = ReadOneByteFromFile(dbfFile, 0);
				var tableEncodingFlag = ReadOneByteFromFile(dbfFile, 29);
				if (!TableTypes.ContainsKey(tableTypeFlag) || !TableEncodings.ContainsKey(tableEncodingFlag))
				{
					var errorDescription = new FileDescription
					{
						Path = dbfFile,
						TypeFlag = tableTypeFlag,
						Db = "error",
						DbDescription = "error",
						EncodingFlag = tableEncodingFlag,
						EncodingCode = "error",
						EncodingName = "error"
					};
					fileDescriptions.Add(errorDescription);

					FilesRichTextBox.AppendText(dbfFile);
					FilesRichTextBox.AppendText(Environment.NewLine);
					continue;
				}
				var tableTypeDescription = TableTypes[tableTypeFlag];
				var tableEncodingDescription = TableEncodings[tableEncodingFlag];
				var description = new FileDescription
				{
					Path = dbfFile,
					TypeFlag = tableTypeFlag,
					Db = tableTypeDescription.Item1,
					DbDescription = tableTypeDescription.Item2,
					EncodingFlag = tableEncodingFlag,
					EncodingCode = tableEncodingDescription.Item1,
					EncodingName = tableEncodingDescription.Item2
				};
				fileDescriptions.Add(description);

				FilesRichTextBox.AppendText(dbfFile);
				FilesRichTextBox.AppendText(Environment.NewLine);
			}
			MainDataGrid.ItemsSource = fileDescriptions;
			RefreshCountFoundedAndSetActivityExportButton();
			SaveLastPath();
		}

		private void ExecuteFindErrorsButton_OnClick(object senderIsButton, RoutedEventArgs eventArgs)
		{
			const string fileExtensionPattern = "*.dbf";
			var sourceFolderPath = SourcePathTextBox.Text.Trim();
			FilesRichTextBox.Document.Blocks.Clear();
			if (string.IsNullOrWhiteSpace(sourceFolderPath))
			{
				const string message = PageLiterals.ValidationRootPathNotSpecified;
				MessageBox.Show(message, PageLiterals.HeaderError,
					MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				return;
			}
			if (!Directory.Exists(sourceFolderPath))
			{
				var message = string.Format(PageLiterals.ValidationDirectoryInvalidPattern, sourceFolderPath);
				MessageBox.Show(message, PageLiterals.HeaderError,
					MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				return;
			}

			var enumerable = SafeGetDirectoryFiles(sourceFolderPath, fileExtensionPattern, 
				SearchOption.AllDirectories);
			var dbfFiles = enumerable as string[] ?? enumerable.ToArray();
			if (!dbfFiles.Any())
			{
				MessageBox.Show(PageLiterals.ValidationDbfNotFound, PageLiterals.HeaderInformationOrWarning,
					MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
				return;
			}

			var fileDescriptions = new List<FileDescription>(dbfFiles.Length);
			foreach (var dbfFile in dbfFiles)
			{
				if (!IsAccessToFile(dbfFile))
				{
					continue;
				}
				var tableTypeFlag = ReadOneByteFromFile(dbfFile, 0);
				var tableEncodingFlag = ReadOneByteFromFile(dbfFile, 29);
				if (TableTypes.ContainsKey(tableTypeFlag) && TableEncodings.ContainsKey(tableEncodingFlag))
				{
					continue;
				}
				var errorDescription = new FileDescription
				{
					Path = dbfFile,
					TypeFlag = tableTypeFlag,
					Db = "error",
					DbDescription = "error",
					EncodingFlag = tableEncodingFlag,
					EncodingCode = "error",
					EncodingName = "error"
				};
				fileDescriptions.Add(errorDescription);

				FilesRichTextBox.AppendText(dbfFile);
				FilesRichTextBox.AppendText(Environment.NewLine);
			}
			MainDataGrid.ItemsSource = fileDescriptions;
			RefreshCountFoundedAndSetActivityExportButton();
			SaveLastPath();
		}

		private void ExecuteFindWithoutEncodingButton_OnClick(object senderIsButton, RoutedEventArgs eventArgs)
		{
			const string fileExtensionPattern = "*.dbf";
			var sourceFolderPath = SourcePathTextBox.Text.Trim();
			FilesRichTextBox.Document.Blocks.Clear();
			if (string.IsNullOrWhiteSpace(sourceFolderPath))
			{
				const string message = PageLiterals.ValidationRootPathNotSpecified;
				MessageBox.Show(message, PageLiterals.HeaderError,
					MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				return;
			}
			if (!Directory.Exists(sourceFolderPath))
			{
				var message = string.Format(PageLiterals.ValidationDirectoryInvalidPattern, sourceFolderPath);
				MessageBox.Show(message, PageLiterals.HeaderError, 
					MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
				return;
			}
			var enumerable = SafeGetDirectoryFiles(sourceFolderPath, fileExtensionPattern, 
				SearchOption.AllDirectories);
			var dbfFiles = enumerable as string[] ?? enumerable.ToArray();
			if (!dbfFiles.Any())
			{
				MessageBox.Show(PageLiterals.ValidationDbfNotFound);
				MessageBox.Show(PageLiterals.ValidationDbfNotFound, PageLiterals.HeaderInformationOrWarning, 
					MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
				return;
			}

			var fileDescriptions = new List<FileDescription>(dbfFiles.Length);
			foreach (var dbfFile in dbfFiles)
			{
				if (!IsAccessToFile(dbfFile))
				{
					continue;
				}
				var tableTypeFlag = ReadOneByteFromFile(dbfFile, 0);
				var tableEncodingFlag = ReadOneByteFromFile(dbfFile, 29);
				if (!TableTypes.ContainsKey(tableTypeFlag) || !TableEncodings.ContainsKey(tableEncodingFlag))
				{
					var errorDescription = new FileDescription
					{
						Path = dbfFile,
						TypeFlag = tableTypeFlag,
						Db = "error",
						DbDescription = "error",
						EncodingFlag = tableEncodingFlag,
						EncodingCode = "error",
						EncodingName = "error"
					};
					fileDescriptions.Add(errorDescription);

					FilesRichTextBox.AppendText(dbfFile);
					FilesRichTextBox.AppendText(Environment.NewLine);
					continue;
				}
				var tableTypeDescription = TableTypes[tableTypeFlag];
				var tableEncodingDescription = TableEncodings[tableEncodingFlag];
				var description = new FileDescription
				{
					Path = dbfFile,
					TypeFlag = tableTypeFlag,
					Db = tableTypeDescription.Item1,
					DbDescription = tableTypeDescription.Item2,
					EncodingFlag = tableEncodingFlag,
					EncodingCode = tableEncodingDescription.Item1,
					EncodingName = tableEncodingDescription.Item2
				};
				if (description.EncodingFlag != 0)
				{
					continue;
				}
				fileDescriptions.Add(description);
				FilesRichTextBox.AppendText(dbfFile);
				FilesRichTextBox.AppendText(Environment.NewLine);
			}
			MainDataGrid.ItemsSource = fileDescriptions;
			RefreshCountFoundedAndSetActivityExportButton();
			SaveLastPath();
		}

		private void SaveListToFileButton_OnClick(object senderIsButton, RoutedEventArgs eventArgs)
		{
			var saveFileDialog = new Microsoft.Win32.SaveFileDialog
			{
				FileName = "document",
				DefaultExt = ".txt",
				Filter = "Text Files(*.txt)|*.txt|All(*.*)|*"
			};
			var result = saveFileDialog.ShowDialog();
			if (result != true)
			{
				return;
			}
			File.WriteAllText(saveFileDialog.FileName,
				new TextRange(FilesRichTextBox.Document.ContentStart, FilesRichTextBox.Document.ContentEnd).Text);
			Process.Start(saveFileDialog.FileName);
		}

		private static byte ReadOneByteFromFile(string file, long offset)
		{
			var buffer = new byte[1];
			using (var reader = new BinaryReader(new FileStream(file, FileMode.Open)))
			{
				reader.BaseStream.Seek(offset, SeekOrigin.Begin);
				reader.Read(buffer, 0, 1);
				return buffer[0];
			}
		}

		private static bool IsAccessToFile(string file)
		{
			try
			{
				using (new BinaryReader(new FileStream(file, FileMode.Open))) { }
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private void RefreshCountFoundedAndSetActivityExportButton()
		{
			CountLabel.Content = string.Format("Count: {0, 5}", MainDataGrid.Items.Count);
			SaveListToFileButton.IsEnabled = MainDataGrid.Items.Count > 0;
		}

		private void SaveLastPath()
		{
			if (string.IsNullOrWhiteSpace(SourcePathTextBox.Text) || !Directory.Exists(SourcePathTextBox.Text))
			{
				return;
			}
			Properties.Settings.Default.LastPath = SourcePathTextBox.Text;
			Properties.Settings.Default.Save();
		}

		/// <summary>
		/// A safe way to get all the files in a directory and sub directory without
		/// crashing on UnauthorizedException or PathTooLongException
		/// </summary>
		/// <param name="rootPath">Starting directory</param>
		/// <param name="patternMatch">Filename pattern match</param>
		/// <param name="searchOption">Search subdirectories or only top level directory for files</param>
		/// <returns>List of files</returns>
		public static IEnumerable<string> SafeGetDirectoryFiles(string rootPath, string patternMatch,
			SearchOption searchOption)
		{
			// ReSharper disable PossibleMultipleEnumeration
			var foundFiles = Enumerable.Empty<string>();
			if (searchOption == SearchOption.AllDirectories)
			{
				try
				{
					var subDirs = Directory.EnumerateDirectories(rootPath);
					// ReSharper disable once LoopCanBeConvertedToQuery
					foreach (var dir in subDirs)
					{
						// Add files in subdirectories recursively to the list
						foundFiles = foundFiles.Concat(SafeGetDirectoryFiles(dir, patternMatch, searchOption));
					}
				}
				catch (UnauthorizedAccessException) { }
				catch (PathTooLongException) { }
			}

			try
			{
				// Add files from the current directory
				foundFiles = foundFiles.Concat(Directory.EnumerateFiles(rootPath, patternMatch));
			}
			catch (UnauthorizedAccessException) { }
			return foundFiles;
			// ReSharper restore PossibleMultipleEnumeration
		}

		private static readonly Dictionary<byte, Tuple<string, string>> TableTypes =
			new Dictionary<byte, Tuple<string, string>>
		{
			{ 2, new Tuple<string, string>("FoxBASE", "Table without memo fields") },
			{ 3, new Tuple<string, string>("dBASE III, dBASE IV, dBASE 5, dBASE 7, FoxPro, FoxBASE+",
				"Table without memo fields") },
			{ 4, new Tuple<string, string>("dBASE 7", "Table without memo fields") },
			{ 48, new Tuple<string, string>("Visual FoxPro",
				"Table (there is no indication of the presence of the memo field .FPT)") },
			{ 49, new Tuple<string, string>("Visual FoxPro", "Table with auto-increment fields") },
			{ 67, new Tuple<string, string>("dBASE IV, dBASE 5", "dBASE IV SQL table without memo fields") },
			{ 99, new Tuple<string, string>("dBASE IV, dBASE 5", "System dBASE IV SQL table without memo fields") },
			{ 131, new Tuple<string, string>("dBASE III, FoxBASE+, FoxPro", "A table with memo fields .DBT") },
			{ 139, new Tuple<string, string>("dBASE IV, dBASE 5", "Table with Memo fields .DBT format dBASE IV") },
			{ 140, new Tuple<string, string>("dBASE 7", "Table with Memo fields .DBT format dBASE IV") },
			{ 203, new Tuple<string, string>("dBASE IV, dBASE 5", "dBASE IV SQL Table with Memo fields .DBT") },
			{ 229, new Tuple<string, string>("SMT", "Table with Memo fields .SMT") },
			{ 235, new Tuple<string, string>("dBASE IV, dBASE 5", "dBASE IV System SQL table with memo fields .DBT")},
			{ 245, new Tuple<string, string>("FoxPro", "Table with Memo fields .FPT") },
			{ 251, new Tuple<string, string>("FoxBASE", "Table with Memo fields .???") }
		};

		private static readonly Dictionary<byte, Tuple<string, string>> TableEncodings =
			new Dictionary<byte, Tuple<string, string>>
		{
			// ReSharper disable StringLiteralTypo
			{ 0, new Tuple<string, string>("0", "Not specified") },
			{ 1, new Tuple<string, string>("437", "US MS-DOS") },
			{ 2, new Tuple<string, string>("850", "International MS-DOS") },
			{ 3, new Tuple<string, string>("1252", "Windows ANSI Latin I") },
			{ 4, new Tuple<string, string>("10000", "Standard Macintosh") },
			{ 8, new Tuple<string, string>("865", "Danish OEM") },
			{ 9, new Tuple<string, string>("437", "Dutch OEM") },
			{ 10, new Tuple<string, string>("850", "Dutch OEM*") },
			{ 11, new Tuple<string, string>("437", "Finnish OEM") },
			{ 13, new Tuple<string, string>("437", "French OEM") },
			{ 14, new Tuple<string, string>("850", "French OEM*") },
			{ 15, new Tuple<string, string>("437", "German OEM") },
			{ 16, new Tuple<string, string>("850", "German OEM*") },
			{ 17, new Tuple<string, string>("437", "Italian OEM") },
			{ 18, new Tuple<string, string>("850", "Italian OEM*") },
			{ 19, new Tuple<string, string>("932", "Japanese Shift-JIS") },
			{ 20, new Tuple<string, string>("850", "Spanish OEM*") },
			{ 21, new Tuple<string, string>("437", "Swedish OEM") },
			{ 22, new Tuple<string, string>("850", "Swedish OEM*") },
			{ 23, new Tuple<string, string>("865", "Norwegian OEM") },
			{ 24, new Tuple<string, string>("437", "Spanish OEM") },
			{ 25, new Tuple<string, string>("437", "English OEM (Great Britain)") },
			{ 26, new Tuple<string, string>("850", "English OEM (Great Britain)*") },
			{ 27, new Tuple<string, string>("437", "English OEM (US)") },
			{ 28, new Tuple<string, string>("863", "French OEM (Canada)") },
			{ 29, new Tuple<string, string>("850", "French OEM*") },
			{ 31, new Tuple<string, string>("852", "Czech OEM") },
			{ 34, new Tuple<string, string>("852", "Hungarian OEM") },
			{ 35, new Tuple<string, string>("852", "Polish OEM") },
			{ 36, new Tuple<string, string>("860", "Portuguese OEM") },
			{ 37, new Tuple<string, string>("850", "Portuguese OEM*") },
			{ 38, new Tuple<string, string>("866", "Russian OEM") },
			{ 55, new Tuple<string, string>("850", "English OEM (US)*") },
			{ 64, new Tuple<string, string>("852", "Romanian OEM") },
			{ 77, new Tuple<string, string>("936", "Chinese GBK (PRC)") },
			{ 78, new Tuple<string, string>("949", "Korean (ANSI/OEM)") },
			{ 79, new Tuple<string, string>("950", "Chinese Big5 (Taiwan)") },
			{ 80, new Tuple<string, string>("874", "Thai (ANSI/OEM)") },
			{ 87, new Tuple<string, string>("Current ANSI CP", "ANSI") },
			{ 88, new Tuple<string, string>("1252", "Western European ANSI") },
			{ 89, new Tuple<string, string>("1252", "Spanish ANSI") },
			{ 100, new Tuple<string, string>("852", "Eastern European MS-DOS") },
			{ 101, new Tuple<string, string>("866", "Russian MS-DOS") },
			{ 102, new Tuple<string, string>("865", "Nordic MS-DOS") },
			{ 103, new Tuple<string, string>("861", "Icelandic MS-DOS") },
			{ 104, new Tuple<string, string>("895", "Kamenicky (Czech) MS-DOS") },
			{ 105, new Tuple<string, string>("620", "Mazovia (Polish) MS-DOS") },
			{ 106, new Tuple<string, string>("737", "Greek MS-DOS (437G)") },
			{ 107, new Tuple<string, string>("857", "Turkish MS-DOS") },
			{ 108, new Tuple<string, string>("863", "French-Canadian MS-DOS") },
			{ 120, new Tuple<string, string>("950", "Taiwan Big 5") },
			{ 121, new Tuple<string, string>("949", "Hangul (Wansung)") },
			{ 122, new Tuple<string, string>("936", "PRC GBK") },
			{ 123, new Tuple<string, string>("932", "Japanese Shift-JIS") },
			{ 124, new Tuple<string, string>("874", "Thai Windows/MS–DOS") },
			{ 134, new Tuple<string, string>("737", "Greek OEM") },
			{ 135, new Tuple<string, string>("852", "Slovenian OEM") },
			{ 136, new Tuple<string, string>("857", "Turkish OEM") },
			{ 150, new Tuple<string, string>("10007", "Russian Macintosh") },
			{ 151, new Tuple<string, string>("10029", "Eastern European Macintosh") },
			{ 152, new Tuple<string, string>("10006", "Greek Macintosh") },
			{ 200, new Tuple<string, string>("1250", "Eastern European Windows") },
			{ 201, new Tuple<string, string>("1251", "Russian Windows") },
			{ 202, new Tuple<string, string>("1254", "Turkish Windows") },
			{ 203, new Tuple<string, string>("1253", "Greek Windows") },
			{ 204, new Tuple<string, string>("1257", "Baltic Windows") }
			// ReSharper restore StringLiteralTypo
		};
	}

	internal class FileDescription
	{
		public string Path { get; set; }
		public byte TypeFlag { get; set; }
		public string Db { get; set; }
		public string DbDescription { get; set; }
		public byte EncodingFlag { get; set; }
		public string EncodingCode { get; set; }
		public string EncodingName { get; set; }
	}
}
