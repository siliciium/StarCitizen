using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;



namespace SC_LogParser_Arena
{

    public partial class MainWindow : Window
    {
        private static readonly int WS_PORT = 8118;

        private static string logPath;
        private static bool stopFlag = false;

        private static Dictionary<string, Dictionary<string, int>> leaderboard;
        private static List<KeyValuePair<string, string>> killfeed;
        private static WebSocketServer ws_server;


        private static readonly List<string> NPCs = new List<string>() {
            "PU_Pilots-Human",
            "PU_Human",
            "NPC_Archetypes",
            "Kopion_",
            "AIModule_Unmanned_"
        };

        private static bool mustExit = false;

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

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        // Make sure RECT is actually OUR defined struct, not the windows rect.
        public static RECT GetWindowRectangle(Window window)
        {
            RECT rect;
            GetWindowRect((new WindowInteropHelper(window)).Handle, out rect);

            return rect;
        }

        public MainWindow()
        {
            InitializeComponent();

            // Windows 11 DarkMode
            IntPtr hWnd = new WindowInteropHelper(this).EnsureHandle();
            bool value = true;
            int result = DwmSetWindowAttribute(
                hWnd,
                DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
                ref value,
                Marshal.SizeOf<bool>()
            );


            leaderboard = new Dictionary<string, Dictionary<string, int>>();
            killfeed = new List<KeyValuePair<string, string>>();

            lstatus.Content = "WebSocket server stopped.";
            ws_server = new WebSocketServer($"http://localhost:{WS_PORT}/", lstatus, leaderboard, killfeed);
            
        }


        private void open_file_Click(object sender, RoutedEventArgs e)
        {
            if (string.Equals(open_file.Content, "Open"))
            {
                open_file.IsEnabled = false;
                chb_pve.IsEnabled = false;

                stopFlag = false;
                tb_file.Clear();
                leaderboard.Clear();
                killfeed.Clear();

                OpenFileDialog openFileDialog = new OpenFileDialog();

                openFileDialog.Filter = "SC log file (*.log)|*.log|All Files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                bool? result = openFileDialog.ShowDialog();

                if (result == true)
                {
                    open_file.Content = "Stop";

                    string filePath = openFileDialog.FileName;                    
                    tb_file.Text = filePath;
                    logPath = filePath;

                    Task serverTask = ws_server.StartAsync();
                    bool? pve = chb_pve.IsChecked;
                    Task.Run(() => MonitorFile(logPath, 2, pve));                        
                    

                }
                else
                {
                    open_file.Content = "Open";
                    chb_pve.IsEnabled = true;                    
                }

                open_file.IsEnabled = true;


            }
            else if (string.Equals(open_file.Content, "Stop"))
            {
                open_file.IsEnabled = false;                
                stopFlag = true;                
            }


        }


        private bool IsNPC(string player)
        {
            bool ok = false;            

            foreach (string NPC in NPCs)
            {
                if (player.StartsWith(NPC))
                {
                    ok = true;
                    break;
                }
            }

            return ok;
        }

        private Dictionary<string, int> NewDic(int kill, int death, int suicide, int crash)
        {
            Dictionary<string, int> d = new Dictionary<string, int>();
            d.Add("kill", kill);
            d.Add("death", death);
            d.Add("suicide", suicide);
            d.Add("crash", crash);
            return d;
        }


        private void MonitorFile(string filename, int intervalSeconds, bool? pve)
        {
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader reader = new StreamReader(fs))
                {
                    string lastLine = string.Empty;
                    long lastPosition = fs.Position;

                    while (!stopFlag)
                    {
                        Thread.Sleep(intervalSeconds * 1000); // Wait for the specified interval

                        fs.Seek(lastPosition, SeekOrigin.Begin);
                        string newLine;

                        while ((newLine = reader.ReadLine()) != null)
                        {
                            if (!string.IsNullOrWhiteSpace(newLine))
                            {
                                //Debug.WriteLine($"{newLine}");

                                if(newLine.Contains("<Actor Death> CActor::Kill:"))
                                {

                                    Debug.WriteLine($"{newLine}");
                                    string[] parts = newLine.Split(' ');

                                    if (parts.Length >= 13)
                                    {
                                        string victim = RemoveChar(parts[5], '\'');
                                        string killer = RemoveChar(parts[12], '\'');

                                        /* PVE ? */
                                        if (IsNPC(victim) || IsNPC(killer))
                                        {
                                            if (pve != true)
                                            {
                                                lastLine = newLine;
                                                lastPosition = fs.Position;
                                                fs.Seek(lastPosition, SeekOrigin.Begin); // Ensure we remain at the correct position
                                                fs.Flush();
                                                continue;
                                            }
                                        }

                                        killfeed.Add(new KeyValuePair<string, string>(killer, victim));

                                        bool isSuicide = false;
                                        if(string.Equals(killer, victim))
                                        {
                                            isSuicide = true;
                                        }

                                        bool isCrash = false;
                                        if (string.Equals(killer, "unknown"))
                                        {
                                            isCrash = true;
                                        }

                                        if (isSuicide)
                                        {
                                            if (!leaderboard.ContainsKey(victim))
                                            {                                                
                                                leaderboard[victim] = NewDic(0,0,1,0);
                                            }
                                            else
                                            {
                                                leaderboard[victim]["suicide"] += 1;
                                            }
                                        }else if (isCrash)
                                        {
                                            if (!leaderboard.ContainsKey(victim))
                                            {
                                                leaderboard[victim] = NewDic(0, 0, 0, 1);
                                            }
                                            else
                                            {
                                                leaderboard[victim]["crash"] += 1;
                                            }
                                        }
                                        else
                                        {
                                            if (!leaderboard.ContainsKey(killer))
                                            {
                                                leaderboard[killer] = NewDic(1, 0, 0, 0);
                                            }
                                            else
                                            {
                                                leaderboard[killer]["kill"] += 1;
                                            }


                                            if (!leaderboard.ContainsKey(victim))
                                            {
                                                leaderboard[victim] = NewDic(0, 1, 0, 0);
                                            }
                                            else
                                            {
                                                leaderboard[victim]["death"] +=  1;
                                            }
                                        }

                                    }
                                    
                                }

                                lastLine = newLine;
                                lastPosition = fs.Position;
                            }
                        }

                        fs.Seek(lastPosition, SeekOrigin.Begin); // Ensure we remain at the correct position
                        fs.Flush();
                    }


                    leaderboard.Clear();
                    killfeed.Clear();


                    this.Dispatcher.Invoke(() =>
                    {
                        //ws_server.Stop();
                        ws_server.Stop();
                        Debug.WriteLine("[MainWindow][MonitorFile][ws_server.Stop]");

                        open_file.Content = "Open";
                        open_file.IsEnabled = true;
                        chb_pve.IsEnabled = true;
                        tb_file.Text = string.Empty;

                        Debug.WriteLine("[MainWindow][MonitorFile] finished !");

                        if (mustExit)
                        {
                            Application.Current.Shutdown();
                        }
                    });

      

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[MainWindow][MonitorFile] Error [MonitorFile]: " + ex.Message);
                Debug.WriteLine(ex);
                MessageBox.Show($"{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        static string RemoveChar(string str, char charToRemove)
        {
            return str.Replace(charToRemove.ToString(), string.Empty);
        }

        

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {           

            if (string.Equals(open_file.Content, "Stop"))
            {

                e.Cancel = true;
                Debug.WriteLine("[MainWindow][Window_Closing] Please wait, cleanning ...");

                mustExit = true;
                stopFlag = true;
            }
            
        }
    }
}
