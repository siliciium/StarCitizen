using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;



namespace JoyTrack
{
    /// <summary>
    /// Logique d'interaction pour Configuration.xaml
    /// </summary>
    public partial class Configuration : Window
    {
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



        private Input input;
        private JObject config;
        public string config_file { get; set; }
        private Dictionary<Guid, Dictionary<string, object>> devices;
        public Guid selected_guid {get; set;}

        private double exponent;
        private double deadzone;

        private static Polyline polyline;


        public Configuration()
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

            this.ResizeMode = ResizeMode.CanMinimize;

            ic0.Source = ConvertBitmapToImageSource(Properties.Resources.c0);
            ic1.Source = ConvertBitmapToImageSource(Properties.Resources.c1);
            ic2.Source = ConvertBitmapToImageSource(Properties.Resources.c2);
            ic3.Source = ConvertBitmapToImageSource(Properties.Resources.c3);
            ic4.Source = ConvertBitmapToImageSource(Properties.Resources.c4);
            ic5.Source = ConvertBitmapToImageSource(Properties.Resources.c5);
            ic6.Source = ConvertBitmapToImageSource(Properties.Resources.c6);
            ic7.Source = ConvertBitmapToImageSource(Properties.Resources.c7);

            selected_guid = Guid.Empty;

            input = new Input();
            devices = input.getDevices();            
            foreach (Guid guid in devices.Keys )
            {
                cb_devices.Items.Add(devices[guid]["ProductName"]);
                //tb_dev_desc.Text = string.Format("GUID : {0}", guid.ToString());
            }

            if(devices.Count > 0)
            {
                cb_devices.SelectedIndex = 0 ;
            }
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.IO.File.Exists(config_file))
            {
                Debug.WriteLine($"[Window_Loaded] Configuration : {config_file}");

                try
                {
                    config = JObject.Parse(System.IO.File.ReadAllText(config_file));

                    if (config != null)
                    {
                        Debug.WriteLine(config);
                        Guid guid = Guid.Parse(config["GUID"].ToString());
                        if(!Guid.Equals(guid, Guid.Empty))
                        {
                            cb_devices.SelectedIndex = GetIndexFromGuid(guid);
                        }

                        chb_freetrack.IsChecked = System.Convert.ToBoolean(config["Use_Freetrack20Enhanced"]);
                        
                        CheckFromIndex(System.Convert.ToInt32(config["Config"]));

                        exponent = Convert.ToDouble(config["CurveExponent"]);
                        slider_curve.Value = exponent;

                        deadzone = Convert.ToDouble(config["Deadzone"]);
                        slider_deadzone.Value = deadzone;
                    }

                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(string.Format("Error: unable to parse config file {0}{1}{2}", config_file, System.Environment.NewLine, ex.Message), this.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                Debug.WriteLine($"Configuration [Window_Loaded] not exists {config_file}");
                config = new JObject()
                {
                    { "GUID", Guid.Empty.ToString() },
                    { "ProductName", String.Empty },                    
                    { "Use_Freetrack20Enhanced", true },
                    { "Config", 0 },
                    { "CurveExponent", 1.0 },
                    { "Deadzone", 0.0 },
                };
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {

            Debug.WriteLine($"[Window_Closed] use freetrack : {chb_freetrack.IsChecked}");

            if (config != null)
            {
                bool err = false;

                try
                {
                    string productName = String.Empty;
                    if(cb_devices.SelectedValue != null)
                    {
                        productName = cb_devices.SelectedValue.ToString();
                    }
                                       
                    if (productName.Length > 0)
                    {
                        config["GUID"] = GetGuidFromProductName(productName);
                        config["ProductName"] = productName;
                    }
                    else
                    {
                        config["GUID"] = Guid.Empty;
                        config["ProductName"] = String.Empty;
                    }                    
                    config["Use_Freetrack20Enhanced"] = (bool)chb_freetrack.IsChecked;
                    config["Config"] = GetConfigIndex();
                    config["CurveExponent"] = Math.Round(slider_curve.Value,2);
                    config["Deadzone"] = Math.Round(slider_deadzone.Value, 2);
                }
                catch (Exception ex)
                {
                    err = true;
                    System.Windows.MessageBox.Show(string.Format("An error occurred while saving the configuration. The configuration was not saved."), this.Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    if (!err)
                    {
                        using (StreamWriter writer = new StreamWriter(config_file))
                        {
                            writer.WriteLine(config.ToString(Newtonsoft.Json.Formatting.Indented));
                        }
                    }
                }
            }
        }

        private Dictionary<string, string> GetInfosFromProductName(string productName)
        {

            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (Guid guid in devices.Keys)
            {
                if (string.Equals(devices[guid]["ProductName"], productName))
                {
                    dic.Add("ProductId  ", devices[guid]["ProductId"].ToString());
                    dic.Add("VendorId   ", devices[guid]["VendorId"].ToString());

                    dic.Add("GUID", guid.ToString());
                    //dic.Add("InstanceName", devices[guid]["InstanceName"].ToString());
                    dic.Add("Type       ", devices[guid]["Type"].ToString());
                    dic.Add("Subtype    ", devices[guid]["Subtype"].ToString());
                    dic.Add("Usage      ", devices[guid]["Usage"].ToString());
                    dic.Add("UsagePage  ", devices[guid]["UsagePage"].ToString());
                    
                    //dic.Add("HID        ", devices[guid]["IsHumanInterfaceDevice"].ToString());
                    dic.Add("JoystickId ", devices[guid]["JoystickId"].ToString());
                    dic.Add("AxisMode   ", devices[guid]["AxisMode"].ToString());
                    dic.Add("AxeCount   ", devices[guid]["AxeCount"].ToString());
                    dic.Add("ButtonCount", devices[guid]["ButtonCount"].ToString());
                    dic.Add("PovCount   ", devices[guid]["PovCount"].ToString());
                    break ;
                }
            }
            return dic;
        }

        private int GetIndexFromGuid(Guid guid)
        {
            int n = 0;
            foreach (Guid _guid in devices.Keys)
            {
                if (Guid.Equals(_guid, guid))
                {                   
                    break;
                }

                n++;
            }
            return n;
        }

        private Guid GetGuidFromProductName(string productName)
        {
            Guid ret = Guid.Empty;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (Guid guid in devices.Keys)
            {
                if (string.Equals(devices[guid]["ProductName"], productName))
                {
                    ret = guid;
                    break;
                }
            }
            return ret;
        }

        private void cb_devices_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            ComboBox cb = sender as ComboBox;

            if(cb != null)
            {                
                Dictionary<string, string> infos = GetInfosFromProductName(cb.SelectedItem as string);

                if(infos.Count > 0)
                {
                    string desc = string.Empty;
                    foreach (string k in infos.Keys)
                    {
                        if(!String.Equals(k, "GUID"))
                        {
                            desc += string.Format("{0} : {1}{2}", k, infos[k], System.Environment.NewLine);
                        }                        
                    }
                    tb_dev_desc.Text = desc;
                                        
                    selected_guid = Guid.Parse(infos["GUID"].ToString());
                    Debug.WriteLine($"[cb_devices_SelectionChanged] {selected_guid}");

                }
            }

            
        }

        private ImageSource ConvertBitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private int GetConfigIndex()
        {
            int index = 0;

            if ((bool)chb_Config0.IsChecked)
            {
                index = 0;
            }else if ((bool)chb_Config1.IsChecked)
            {
                index = 1;
            }
            else if ((bool)chb_Config2.IsChecked)
            {
                index = 2;
            }
            else if ((bool)chb_Config3.IsChecked)
            {
                index = 3;
            }
            else if ((bool)chb_Config4.IsChecked)
            {
                index = 4;
            }
            else if ((bool)chb_Config5.IsChecked)
            {
                index = 5;
            }
            else if ((bool)chb_Config6.IsChecked)
            {
                index = 6;
            }
            else if ((bool)chb_Config7.IsChecked)
            {
                index = 7;
            }

            return index;
        }

        private void CheckFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                    {
                        chb_Config0.IsChecked = true;
                        break;
                    }
                case 1:
                    {
                        chb_Config1.IsChecked = true;
                        break;
                    }
                case 2:
                    {
                        chb_Config2.IsChecked = true;
                        break;
                    }
                case 3:
                    {
                        chb_Config3.IsChecked = true;
                        break;
                    }
                case 4:
                    {
                        chb_Config4.IsChecked = true;
                        break;
                    }
                case 5:
                    {
                        chb_Config5.IsChecked = true;
                        break;
                    }
                case 6:
                    {
                        chb_Config6.IsChecked = true;
                        break;
                    }
                case 7:
                    {
                        chb_Config7.IsChecked = true;
                        break;
                    }
            }
                        
        }

        

        private void UncheckAll(List<CheckBox> lst)
        {
            foreach (CheckBox c in lst)
            {
                c.IsChecked = false;
            }
        }

        private void chb_Config0_Checked(object sender, RoutedEventArgs e)
        {
            UncheckAll(new List<CheckBox>() { chb_Config1, chb_Config2, chb_Config3, chb_Config4, chb_Config5, chb_Config6, chb_Config7 });
        }

        private void chb_Config1_Checked(object sender, RoutedEventArgs e)
        {
            UncheckAll(new List<CheckBox>() { chb_Config0, chb_Config2, chb_Config3, chb_Config4, chb_Config5, chb_Config6, chb_Config7 });
        }

        private void chb_Config2_Checked(object sender, RoutedEventArgs e)
        {
            UncheckAll(new List<CheckBox>() { chb_Config0, chb_Config1, chb_Config3, chb_Config4, chb_Config5, chb_Config6, chb_Config7 });
        }

        private void chb_Config3_Checked(object sender, RoutedEventArgs e)
        {
            UncheckAll(new List<CheckBox>() { chb_Config0, chb_Config1, chb_Config2, chb_Config4, chb_Config5, chb_Config6, chb_Config7 });
        }

        private void chb_Config4_Checked(object sender, RoutedEventArgs e)
        {
            UncheckAll(new List<CheckBox>() { chb_Config0, chb_Config1, chb_Config2, chb_Config3, chb_Config5, chb_Config6, chb_Config7 });
        }

        private void chb_Config5_Checked(object sender, RoutedEventArgs e)
        {
            UncheckAll(new List<CheckBox>() { chb_Config0, chb_Config1, chb_Config2, chb_Config3, chb_Config4, chb_Config6, chb_Config7 });
        }

        private void chb_Config6_Checked(object sender, RoutedEventArgs e)
        {
            UncheckAll(new List<CheckBox>() { chb_Config0, chb_Config1, chb_Config2, chb_Config3, chb_Config4, chb_Config5, chb_Config7 });
        }

        private void chb_Config7_Checked(object sender, RoutedEventArgs e)
        {
            UncheckAll(new List<CheckBox>() { chb_Config0, chb_Config1, chb_Config2, chb_Config3, chb_Config4, chb_Config5, chb_Config6 });
        }



        private void DrawDashedGrid(double spacing)
        {
            // Clear previous lines
            canva_curve.Children.Clear();
            //System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(255, 35, 35, 35);
            System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(255, 10,57,103);

            double width = canva_curve.ActualWidth;
            double height = canva_curve.ActualHeight;

            // Draw vertical lines
            for (double x = 0; x < width; x += spacing)
            {
                if (x > 0 && x < width)
                {
                    Debug.WriteLine($"DrawDashedGrid x:{x} / {width}");
                    Line verticalLine = new Line
                    {
                        X1 = x,
                        Y1 = 0,
                        X2 = x,
                        Y2 = height,
                        Stroke = new SolidColorBrush(color),
                        StrokeThickness = 1,
                        StrokeDashArray = new DoubleCollection() { 4, 0 }
                    };
                    canva_curve.Children.Add(verticalLine);
                }
            }

            // Draw horizontal lines
            for (double y = 0; y < height; y += spacing)
            {
                if (y > 0 && y < height)
                {
                    Debug.WriteLine($"DrawDashedGrid y:{y} / {height}");
                    Line horizontalLine = new Line
                    {
                        X1 = 0,
                        Y1 = y,
                        X2 = width,
                        Y2 = y,
                        Stroke = new SolidColorBrush(color),
                        StrokeThickness = 1,
                        StrokeDashArray = new DoubleCollection() { 4, 0 }
                    };
                    canva_curve.Children.Add(horizontalLine);
                }
                
            }
        }

        private void DrawDiagonalDashedLine()
        {
            //System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(255, 80, 80, 80);
            System.Windows.Media.Color color = System.Windows.Media.Color.FromArgb(255, 127, 127, 127);

            Line diagonalDashedLine = new Line
            {
                X1 = 0,
                Y1 = canva_curve.ActualHeight,
                X2 = canva_curve.ActualWidth,
                Y2 = 0,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 1
            };

            // Set the dash pattern
            diagonalDashedLine.StrokeDashArray = new DoubleCollection() { 5, 4 };

            canva_curve.Children.Add(diagonalDashedLine);
        }
        
        private void DrawExponentialCurve()
        {

            double minY = Math.Floor( Convert.ToDouble(ushort.MaxValue) / 2); 
            double maxY = Convert.ToDouble(ushort.MaxValue);

            double canvasWidth = canva_curve.ActualWidth;
            double canvasHeight = canva_curve.ActualHeight;

            // Clear previous drawings
            //canva_curve.Children.Clear();

            polyline = new Polyline
            {
                Stroke = System.Windows.Media.Brushes.DodgerBlue,
                StrokeThickness = 2
            };

            double x = 0;
            double canvasX = 0;
            double canvasY = 0;


            for (x = 0; x <= 1; x += 0.01)
            {
                double y = Math.Pow(x, exponent);

                // Normalize the y value to the range [minY, maxY]
                double value = minY + y * (maxY - minY);

                // Scale the y value to fit the canvas height
                canvasX = x * canvasWidth;
                canvasY = canvasHeight - ((value - minY) / (maxY - minY) * canvasHeight);

                polyline.Points.Add(new System.Windows.Point(canvasX, canvasY));
            }


            if (exponent > 1.85)
            {
                for (double nx = Math.Round(canvasY, 2); nx > 0.0; nx -= 0.01)
                {
                    if (canvasX < canvasWidth - 2.0)
                    {
                        canvasX = x * canvasWidth;
                    }

                    double Y = nx / Math.Round(canvasY, 2);

                    polyline.Points.Add(new System.Windows.Point(canvasX, Y));
                }
            }

            // Add the polyline to the canvas
            canva_curve.Children.Add(polyline);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawDashedGrid(31.25);
            DrawDiagonalDashedLine();
            DrawExponentialCurve();
        }

        private void slider_curve_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                exponent = Math.Round(e.NewValue, 2);
                slider_curve.SelectionStart = 0;
                slider_curve.SelectionEnd = exponent;
                label_curve.Content = String.Format("⠕ Curve {0}", exponent.ToString("0.00").Replace(",", "."));

                if (polyline != null && canva_curve.Children.Contains(polyline))
                {
                    canva_curve.Children.Remove(polyline);
                }

                DrawExponentialCurve();

            }
            catch (Exception ex)
            {

            }
            

        }

        private void slider_deadzone_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                slider_deadzone.SelectionStart = 0;
                slider_deadzone.SelectionEnd = e.NewValue;
                label_deadzone.Content = String.Format("⠶ Deadzone {0}", e.NewValue.ToString("0.00").Replace(",", "."));
            }
            catch (Exception ex)
            {

            }

        }
    }
}
