using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Windows;
using System.Data.OleDb;
using System.Reflection;

using DBFCompare.Util;
using DBFCompare.View.Util;

namespace DBFCompare.View.Windows
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
			FillingLastPaths();
		}

		private void AdditionalInitializeComponent()
		{
			Title = Common.GetApplicationTitle(Assembly.GetExecutingAssembly());
		}

		private void VisualInitializeComponent()
		{
			Background = Constants.BackColor2_Botticelli;
			Foreground = Constants.ForeColor2_PapayaWhip;
			FontSize = Constants.FontSize;
		}

		private void FillingLastPaths()
		{
			var path1 = Properties.Settings.Default.LastPath1;
			var path2 = Properties.Settings.Default.LastPath2;
			var file1 = Properties.Settings.Default.LastFile1;
			var file2 = Properties.Settings.Default.LastFile2;

			if (string.IsNullOrWhiteSpace(path1) || string.IsNullOrWhiteSpace(path2) ||
			    string.IsNullOrWhiteSpace(file1) || string.IsNullOrWhiteSpace(file2))
			{
				return;
			}
			DbfPath1TextBox.Text = path1;
			DbfPath2TextBox.Text = path2;
			DbfFilename1TextBox.Text = file1;
			DbfFilename2TextBox.Text = file2;
		}

		private void SwapPathButton_OnClick(object senderIsButton, RoutedEventArgs eventArgs)
		{
			// Clearing DataGrids
			DbfSameDataGrid.ItemsSource = null;
			DbfDifferencesDataGrid.ItemsSource = null;
			DbfAddedDataGrid.ItemsSource = null;
			DbfRemovedDataGrid.ItemsSource = null;

			var buffer = DbfPath1TextBox.Text;
			DbfPath1TextBox.Text = DbfPath2TextBox.Text;
			DbfPath2TextBox.Text = buffer;
		}

		private void CompareButton_OnClick(object senderIsButton, RoutedEventArgs eventArgs)
		{
			// Clearing DataGrids
			DbfSameDataGrid.ItemsSource = null;
			DbfDifferencesDataGrid.ItemsSource = null;
			DbfAddedDataGrid.ItemsSource = null;
			DbfRemovedDataGrid.ItemsSource = null;

			var dbfPath1 = DbfPath1TextBox.Text.Trim();
			var dbfPath2 = DbfPath2TextBox.Text.Trim();

			if (string.IsNullOrWhiteSpace(dbfPath1) || string.IsNullOrWhiteSpace(dbfPath2))
			{
				MessageBox.Show(PageLiterals.ValidationDbfPathsNotSpecified, 
					PageLiterals.HeaderInformationOrWarning, MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			var dbfFile1 = DbfFilename1TextBox.Text.Trim();
			var dbfFile2 = DbfFilename2TextBox.Text.Trim();

			if (string.IsNullOrWhiteSpace(dbfFile1) || string.IsNullOrWhiteSpace(dbfFile2))
			{
				MessageBox.Show(PageLiterals.ValidationDbfFilesNotSpecified, 
					PageLiterals.HeaderInformationOrWarning, MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			if (!File.Exists(Path.Combine(dbfPath1, dbfFile1)) || !File.Exists(Path.Combine(dbfPath2, dbfFile2)))
			{
				MessageBox.Show(PageLiterals.ValidationFileNotFound, 
					PageLiterals.HeaderInformationOrWarning, MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			DataTable dataTable1;
			DataTable dataTable2;
			using (var connection = new OleDbConnection(FoxProGetConnectionRow(dbfPath1)))
			{
				connection.Open();
				dataTable1 = GetDataTable(connection, dbfFile1);
				// DbfDiff1DataGrid.ItemsSource = dataTable1.AsDataView();
			}
			using (var connection = new OleDbConnection(FoxProGetConnectionRow(dbfPath2)))
			{
				connection.Open();
				dataTable2 = GetDataTable(connection, dbfFile2);
				// DbfDiff2DataGrid.ItemsSource = dataTable2.AsDataView();
			}

			var dataTableSame = new DataTable();
			var dataTableDifferences = new DataTable();
			var dataTableAdded = new DataTable();
			var dataTableRemoved = new DataTable();

			GetTableDiff(dataTable1, dataTable2, ref dataTableSame, ref dataTableDifferences, 
				ref dataTableAdded, ref dataTableRemoved);

			DbfSameDataGrid.ItemsSource = dataTableSame == null ? null : dataTableSame.AsDataView();
			DbfDifferencesDataGrid.ItemsSource = dataTableDifferences == null 
				? null 
				: dataTableDifferences.AsDataView();
			DbfAddedDataGrid.ItemsSource = dataTableAdded == null ? null : dataTableAdded.AsDataView();
			DbfRemovedDataGrid.ItemsSource = dataTableRemoved == null ? null : dataTableRemoved.AsDataView();

			Properties.Settings.Default.LastPath1 = dbfPath1;
			Properties.Settings.Default.LastPath2 = dbfPath2;
			Properties.Settings.Default.LastFile1 = dbfFile1;
			Properties.Settings.Default.LastFile2 = dbfFile2;
			Properties.Settings.Default.Save();
		}

		private static string FoxProGetConnectionRow(string dataSource)
		{
			const string connectionPattern = "Provider={0};Data Source={1};";
			// + "codepage={2}";
			// ReSharper disable once StringLiteralTypo
			const string provider = "VFPOLEDB.1";
			// ReSharper disable once CommentTypo
			// const string extendedProperties = "DBASE IV";
			// const string persistSecurityInfo = "MACHINE";
			// const string codepage = "1251";
			return string.Format(connectionPattern, provider, dataSource); //, codepage);
		}

		private static DataTable GetDataTable(OleDbConnection connection, string file)
		{
			var dataTable = new DataTable();
			var query = string.Format("SELECT * FROM [{0}]", file);
			using (var command = new OleDbCommand(query, connection))
			{
				using (var reader = command.ExecuteReader())
				{
					if (reader != null)
					{
						dataTable.Load(reader);
					}
				}
			}
			return dataTable;
		}

		// TODO The method only works if the columns are no larger than 32 - is deprecated!
		// ReSharper disable once UnusedMember.Local
		private DataTable DifferentDataTableRecords(DataTable firstDataTable, DataTable secondDataTable)
		{

			// Create Empty Table 
			var resultDataTable = new DataTable("ResultDataTable");

			// use a DataSet to make use of a DataRelation object 
			using (var ds = new DataSet())
			{
				// Add tables 
				ds.Tables.AddRange(new[] { firstDataTable.Copy(), secondDataTable.Copy() });

				// Get Columns for DataRelation 
				var firstColumns = new DataColumn[ds.Tables[0].Columns.Count];

				for (var i = 0; i < firstColumns.Length; i++)
				{
					firstColumns[i] = ds.Tables[0].Columns[i];
				}

				var secondColumns = new DataColumn[ds.Tables[1].Columns.Count];
				for (var i = 0; i < secondColumns.Length; i++)
				{
					secondColumns[i] = ds.Tables[1].Columns[i];
				}

				// Create DataRelation 
				var r1 = new DataRelation(string.Empty, firstColumns, secondColumns, false);
				ds.Relations.Add(r1);

				var r2 = new DataRelation(string.Empty, secondColumns, firstColumns, false);
				ds.Relations.Add(r2);

				// Create columns for return table 
				for (var i = 0; i < firstDataTable.Columns.Count; i++)
				{
					resultDataTable.Columns.Add(firstDataTable.Columns[i].ColumnName, 
						firstDataTable.Columns[i].DataType);
				}

				// If firstDataTable Row not in secondDataTable, Add to ResultDataTable. 
				resultDataTable.BeginLoadData();
				foreach (DataRow parentRow in ds.Tables[0].Rows)
				{
					var childRows = parentRow.GetChildRows(r1);
					if (childRows.Length == 0)
					{
						resultDataTable.LoadDataRow(parentRow.ItemArray, true);
					}
				}

				// If secondDataTable Row not in firstDataTable, Add to ResultDataTable. 
				foreach (DataRow parentRow in ds.Tables[1].Rows)
				{
					var childRows = parentRow.GetChildRows(r2);
					if (childRows.Length == 0)
					{
						resultDataTable.LoadDataRow(parentRow.ItemArray, true);
					}
				}
				resultDataTable.EndLoadData();
			}
			return resultDataTable;
		}

		/// <summary>
		/// https://stackoverflow.com/a/45620698/2390270
		/// Compare a source and target dataTables and return the row that are the same, different, added, and removed
		/// </summary>
		/// <param name="dtOld">DataTable to compare</param>
		/// <param name="dtNew">DataTable to compare to dtOld</param>
		/// <param name="dtSame">DataTable that would give you the common rows in both</param>
		/// <param name="dtDifferences">DataTable that would give you the difference</param>
		/// <param name="dtAdded">DataTable that would give you the rows added going from dtOld to dtNew</param>
		/// <param name="dtRemoved">DataTable that would give you the rows removed going from dtOld to dtNew</param>
		private static void GetTableDiff(DataTable dtOld, DataTable dtNew, ref DataTable dtSame, 
			ref DataTable dtDifferences, ref DataTable dtAdded, ref DataTable dtRemoved)
		{
			if (dtSame == null)
			{
				throw new ArgumentNullException("dtSame");
			}

			// try
			// {
			dtAdded = dtOld.Clone();
			dtAdded.Clear();
			dtRemoved = dtOld.Clone();
			dtRemoved.Clear();
			dtSame = dtOld.Clone();
			dtSame.Clear();
			if (dtNew.Rows.Count > 0)
			{
				var enumerable = dtNew.AsEnumerable().Except(dtOld.AsEnumerable(), DataRowComparer.Default);
				var bufferDataRows = enumerable as DataRow[] ?? enumerable.ToArray();
				if (bufferDataRows.Any())
				{
					dtDifferences.Merge(bufferDataRows.CopyToDataTable());
				}
			}
			if (dtOld.Rows.Count > 0)
			{
				var enumerable = dtOld.AsEnumerable().Except(dtNew.AsEnumerable(), DataRowComparer.Default);
				var bufferDataRows = enumerable as DataRow[] ?? enumerable.ToArray();
				if (bufferDataRows.Any())
				{
					dtDifferences.Merge(bufferDataRows.CopyToDataTable());
				}
			}
			if (dtOld.Rows.Count > 0 && dtNew.Rows.Count > 0)
			{
				var enumerable = dtOld.AsEnumerable().Intersect(dtNew.AsEnumerable(), DataRowComparer.Default);
				var bufferDataRows = enumerable as DataRow[] ?? enumerable.ToArray();
				if (bufferDataRows.Any())
				{
					dtSame = bufferDataRows.CopyToDataTable();
				}
			}
			foreach (DataRow row in dtDifferences.Rows)
			{
				if (dtOld.AsEnumerable().Any(r => r.ItemArray.SequenceEqual(row.ItemArray))
					&& !dtNew.AsEnumerable().Any(r => r.ItemArray.SequenceEqual(row.ItemArray)))
				{
					dtRemoved.Rows.Add(row.ItemArray);
				}
				else if (dtNew.AsEnumerable().Any(r => r.ItemArray.SequenceEqual(row.ItemArray))
					&& !dtOld.AsEnumerable().Any(r => r.ItemArray.SequenceEqual(row.ItemArray)))
				{
					dtAdded.Rows.Add(row.ItemArray);
				}
			}
			// }
			// catch (Exception ex)
			// {
			//     MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			// }
		}
	}
}
