using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;


namespace JoyTrack
{
    /// <summary>
    /// Logique d'interaction pour Messagebox.xaml
    /// </summary>
    public partial class Messagebox : Window
    {

        public string message { get; set; }

        [DllImport("Dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        DWMWINDOWATTRIBUTE attribute,
        [In] ref bool pvAttribute,
        int cbAttribute);
        private enum DWMWINDOWATTRIBUTE
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        }

        public Messagebox()
        {
            

            InitializeComponent();
            IntPtr hWnd = new WindowInteropHelper(this).EnsureHandle();
            bool value = true;
            int result = DwmSetWindowAttribute(
                hWnd,
                DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
                ref value,
                Marshal.SizeOf<bool>()
            );

            rtb_message.FontFamily = new FontFamily("Segoe UI");
            rtb_message.FocusVisualStyle = null;
            this.Title = Properties.Resources.AppName;
        }

        private void rtb_message_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(message);

            FormatTextFromString(message);
        }

        private void FormatTextFromString(string input)
        {
            // Initialize a new FlowDocument
            FlowDocument document = new FlowDocument();
            Paragraph paragraph = new Paragraph();

            // Example parsing logic (simplified)
            while (!string.IsNullOrEmpty(input))
            {
                int boldStart = input.IndexOf("<b>");
                int boldEnd = input.IndexOf("</b>");
                int colorStart = input.IndexOf("<color=");
                int colorEnd = input.IndexOf("</color>");

                if (boldStart != -1)
                {
                    paragraph.Inlines.Add(new Run(input.Substring(0, boldStart)));
                    string boldText = input.Substring(boldStart + 3, boldEnd - boldStart - 3);
                    Run runBold = new Run(boldText);
                    runBold.FontWeight = FontWeights.Bold;
                    runBold.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF65B6E0"));
                    paragraph.Inlines.Add(runBold);
                    input = input.Substring(boldEnd + 4);
                }
                else if (colorStart != -1)
                {
                    paragraph.Inlines.Add(new Run(input.Substring(0, colorStart)));
                    int colorTagEnd = input.IndexOf(">", colorStart);
                    string color = input.Substring(colorStart + 7, colorTagEnd - colorStart - 7);
                    string colorText = input.Substring(colorTagEnd + 1, colorEnd - colorTagEnd - 1);
                    Run runColor = new Run(colorText);
                    runColor.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
                    paragraph.Inlines.Add(runColor);
                    input = input.Substring(colorEnd + 8);
                }
                else
                {
                    paragraph.Inlines.Add(new Run(input));
                    input = string.Empty;
                }
            }

            // Add the paragraph to the FlowDocument
            document.Blocks.Add(paragraph);

            // Assign the FlowDocument to the RichTextBox
            rtb_message.Document = document;
        }

    }
}
