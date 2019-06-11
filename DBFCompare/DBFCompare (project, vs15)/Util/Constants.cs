using System.Windows.Media;

namespace DBFCompare.Util
{
	internal class Constants
	{
		public const double FontSize = 14.0;
		public const string BuildDateTimePattern = "{0:yyyy.MM.dd \'/\' HH:mm}";

		// ReSharper disable InconsistentNaming
		// ReSharper disable IdentifierTypo      /* Color names: http://chir.ag/projects/name-that-color/ */

		// Text colors
		public static readonly SolidColorBrush ForeColor1_BigStone = Common.BrushHex("#1b293e");     // Dark-blue
		public static readonly SolidColorBrush ForeColor2_PapayaWhip = Common.BrushHex("#ffefd5");   // Beige
		public static readonly SolidColorBrush ForeColor3_Yellow = Common.BrushHex("#ffff00");       // Yellow
		public static readonly SolidColorBrush ForeColor4_Red = Common.BrushHex("#ff0000");          // Red
		public static readonly SolidColorBrush ForeColor5_Lochmara = Common.BrushHex("#007acc");     // Blue
		public static readonly SolidColorBrush ForeColor6_Silver = Common.BrushHex("#cccccc");       // Grey
		public static readonly SolidColorBrush ForeColor7_White = Common.BrushHex("#ffffff");        // White
		public static readonly SolidColorBrush ForeColor8_GuardsmanRed = Common.BrushHex("#ca1000"); // Red
		public static readonly SolidColorBrush ForeColor9_SeaGreen = Common.BrushHex("#317a2e");     // Green

		// Background colors
		public static readonly SolidColorBrush BackColor1_AthensGray = Common.BrushHex("#eeeef2");  // Light-Grey
		public static readonly SolidColorBrush BackColor2_Botticelli = Common.BrushHex("#d6dbe9");  // Grey
		public static readonly SolidColorBrush BackColor3_SanJuan = Common.BrushHex("#364e6f");     // Blue
		public static readonly SolidColorBrush BackColor4_BlueBayoux = Common.BrushHex("#4d6082");  // Blue
		public static readonly SolidColorBrush BackColor5_WaikawaGray = Common.BrushHex("#566c92"); // Blue
		public static readonly SolidColorBrush BackColor6_Lochmara = Common.BrushHex("#007acc");    // Light-Blue
		public static readonly SolidColorBrush BackColor7_BurntOrange = Common.BrushHex("#ca5100"); // Orange
		public static readonly SolidColorBrush BackColor8_BahamaBlue = Common.BrushHex("#005c99");  // Light-Blue
		public static readonly SolidColorBrush BackColor9_DiSerria = Common.BrushHex("#d3a35b");    // Beige

		// Border and line colors
		public static readonly SolidColorBrush LineBorderColor1_BigStone = Common.BrushHex("#1b293e");   // Dark blue
		public static readonly SolidColorBrush LineBorderColor2_Nepal = Common.BrushHex("#8e9bbc");      // Grey
		public static readonly SolidColorBrush LineBorderColor3_SanJuan = Common.BrushHex("#364e6f");    // Blue
		public static readonly SolidColorBrush LineBorderColor4_BlueBayoux = Common.BrushHex("#4d6082"); // Blue
		public static readonly SolidColorBrush LineBorderColor5_Sail = Common.BrushHex("#b8d8f9");       // Light-Blue

		// ReSharper restore IdentifierTypo
		// ReSharper restore InconsistentNaming
	}
}
