using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Diagnostics;
using System;
using HelixToolkit.Wpf;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;




namespace JoyTrack
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Color head_color = Color.FromArgb(255, 31, 32, 35);
        private double defaut_opacity = 1.0;
        private Model3DGroup model;
        private AxisAngleRotation3D head_rotationX;
        private AxisAngleRotation3D head_rotationY;
        private AxisAngleRotation3D head_rotationZ;
        private TransformGroup tfg_head_target;
        private TransformGroup tfg_joy_target;
        private TranslateTransform tt_bg;
        


        private string exec_path;
        private string config_file;
        private string head_obj;
        private string npclient64;
        private Configuration wconfig;
        private Input input;
        private Task _workerTask;        
        private bool use_freetrack20enhanced = false;


        private Messagebox wmessagebox;


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


        public MainWindow()
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


            wmessagebox = new Messagebox();
            wmessagebox.Owner = this;

            slider_opacity.Value = defaut_opacity;
            

            //CreateBackgroundImage();

            exec_path   = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            config_file = System.IO.Path.Combine(exec_path, "config.json");
            head_obj    = System.IO.Path.Combine(exec_path, "RESSOURCES", "human_head_perso.obj");
            npclient64  = System.IO.Path.Combine(exec_path, "RESSOURCES", "THIRDPARTY", "NPClient64.dll");

            Load3DModelHead();
            //Create3DArrow(new Point3D(0, 0, 50), new Point3D(350, 0, 50));
            DrawGraduatedHLine();
            DrawGraduatedVLine();
            DrawTarget();

            tfg_joy_target = new TransformGroup();
            tfg_joy_target.Children.Add(new TranslateTransform(0, 0));
            target_joy.RenderTransform = tfg_joy_target;
            
            tt_bg = new TranslateTransform();
            BackgroundImageBrush.Transform = tt_bg;

            input = new Input();
            

        }

        

        private void PlaceDeviceNameLabel()
        {            
            //Debug.WriteLine(String.Format("{0}x{1}", tb_device_name.ActualWidth, tb_device_name.ActualHeight));
            //Debug.WriteLine(rect_mouse.Height);

            double Y = ((rect_mouse.Height - tb_device_name.ActualWidth) / 2) + tb_device_name.ActualWidth;
            //Debug.WriteLine(Y);
            Canvas.SetTop(tb_device_name, Y);
            Canvas.SetLeft(tb_device_name, 35);
        }

        


        private void Load3DModelHead() {


            if (System.IO.File.Exists(head_obj))
            {
                ModelImporter importer = new ModelImporter();

                try
                {
                    model = importer.Load(head_obj);

                    if (model != null)
                    {
                        // Change the color of the model
                        ApplyTransparentMaterial(model, head_color, defaut_opacity);
                        // Apply a translation transform to position the model
                        TranslateTransform3D translateTransform = new TranslateTransform3D(new Vector3D(0, 0, 0));
                        // Apply a rotation transform to rotate the model

                        head_rotationX = new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0);
                        RotateTransform3D rotateTransformX = new RotateTransform3D(head_rotationX);

                        head_rotationY = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
                        RotateTransform3D rotateTransformY = new RotateTransform3D(head_rotationY);

                        head_rotationZ = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0);
                        RotateTransform3D rotateTransformZ = new RotateTransform3D(head_rotationZ);


                        // Combine the transforms (rotation followed by translation)

                        // Apply scale transform to resize the object
                        //ScaleTransform3D scaleTransform = new ScaleTransform3D(0.5, 0.5, 0.5);
                        Transform3DGroup transformGroup = new Transform3DGroup();
                        //transformGroup.Children.Add(scaleTransform);
                        transformGroup.Children.Add(rotateTransformX);
                        transformGroup.Children.Add(rotateTransformY);
                        transformGroup.Children.Add(rotateTransformZ);
                        transformGroup.Children.Add(translateTransform);
                        model.Transform = transformGroup;
                        Head_ModelVisual3D.Content = model;
                    }



                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Error loading model {0}.{1}{2}", head_obj, System.Environment.NewLine, ex.Message));
                }
            }
            else
            {
                MessageBox.Show(string.Format("Error loading model ! File not found {0}.", head_obj));
            }

            
        }

        public SolidColorBrush CreateBrushFromArgb(byte alpha, byte red, byte green, byte blue) { 
            Color color = Color.FromArgb(alpha, red, green, blue); 
            return new SolidColorBrush(color);
        }

        private void Create3DArrow(Point3D startPoint, Point3D endPoint)
        {

            // Create a MeshBuilder and add the arrow
            MeshBuilder meshBuilder = new MeshBuilder(true, true);
            meshBuilder.AddArrow(startPoint, endPoint, 2.0, 5.0);

            // Create a GeometryModel3D and add it to the Model3DGroup
            GeometryModel3D arrowModel = new GeometryModel3D
            {
                Geometry = meshBuilder.ToMesh(),
                Material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(255, 0, 172, 233)))
            };

            ModelVisual3D modelVisual3DArrow = new ModelVisual3D { Content = arrowModel };

            helixViewport.Children.Add(modelVisual3DArrow);

        }

        private void ApplyTransparentMaterial(Model3D model, Color color, double opacity)
        {
            if (model is Model3DGroup modelGroup)
            {
                foreach (Model3D child in modelGroup.Children)
                {
                    ApplyTransparentMaterial(child, color, opacity);
                }
            }
            else if (model is GeometryModel3D geometryModel)
            {
                var brush = new SolidColorBrush(color) { Opacity = opacity };
                var material = new DiffuseMaterial(brush);

                // Create a MaterialGroup to apply the material to both sides
                var materialGroup = new MaterialGroup();
                materialGroup.Children.Add(material); // Front face
                materialGroup.Children.Add(new DiffuseMaterial(new SolidColorBrush(color) { Opacity = opacity })); // Back face

                geometryModel.Material = materialGroup;
                geometryModel.BackMaterial = materialGroup;
            }
        }

        /*private Point3D GetCurrentPosition(Model3DGroup modelGroup)
        {
            if (modelGroup.Transform is Transform3DGroup transformGroup) { 
                foreach (var transform in transformGroup.Children) { 
                    if (transform is TranslateTransform3D translateTransform) { 
                        return new Point3D(translateTransform.OffsetX, translateTransform.OffsetY, translateTransform.OffsetZ); 
                    } 
                } 
            }
            Debug.WriteLine("no translation is found");
            return new Point3D(0, 0, 0); // Default position if no translation is found
        }*/

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            e.Cancel = true;


            /*Debug.WriteLine("====== CAMERA ======");
            Debug.WriteLine(camera.Position.ToString().Replace(",", ".").Replace(";", ","));
            Debug.WriteLine(camera.LookDirection.ToString().Replace(",", ".").Replace(";", ","));
            Debug.WriteLine(camera.UpDirection.ToString().Replace(",", ".").Replace(";", ","));
            

            if (model != null)
            {
                Debug.WriteLine("======== OBJ =======");
                Point3D currentPosition = GetCurrentPosition(model);
                Debug.WriteLine(currentPosition);
            }*/

            ReleasePoll();

            e.Cancel = false;

        }

        private void SetCameraPosition(Point3D newPosition, Vector3D newLookDirection, Vector3D newUpDirection)
        { 
            // Set new camera position
            camera.Position = newPosition; 
            // Set new look direction
            camera.LookDirection = newLookDirection; 

            camera.UpDirection = newUpDirection;
        }

        


        private void init_cam_position()
        {
            double pX = -0.735218668244702;
            double pY = 227.147905613218;
            double pZ = -810.451375826972;

            double ldX = 1.80299164972825;
            double ldY = -206.602379336761;
            double ldZ = 798.902545379646;

            double udX = 0;
            double udY = 0;
            double udZ = 1;

            SetCameraPosition(new Point3D(pX, pY, pZ), new Vector3D(ldX, ldY, ldZ), new Vector3D(udX, udY, udZ));
        }

        private void helixViewport_Loaded(object sender, RoutedEventArgs e)
        {
            init_cam_position();

        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {            
            InitJoy();
        }

        private void reinit_cam_pos_Click(object sender, RoutedEventArgs e)
        {
            init_cam_position();
        }

        private void slider_opacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Debug.WriteLine($"opacity : {e.NewValue}");
            ApplyTransparentMaterial(model, head_color, e.NewValue);
        }

        /*private void slider_rota_X_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Debug.WriteLine($"head_rotationX.Angle : {Math.Round(e.NewValue, 2)} -> {InvertValue(Math.Round(e.NewValue, 2))}°");
            head_rotationX.Angle = InvertValue(e.NewValue);

            MoveShapesY(tfg_head_target, DegreesToPixelsY_head(InvertValue(e.NewValue)));
            MoveShapesY(tfg_joy_target, DegreesToPixelsY_joy(InvertValue(e.NewValue)));

            tt_bg.Y = DegreesToPixelsY_joy(e.NewValue) * 30.0;
        }

        private void slider_rota_Y_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Debug.WriteLine($"head_rotationY.Angle : {Math.Round(e.NewValue, 2)} -> {InvertValue(Math.Round(e.NewValue, 2))}°");
            head_rotationY.Angle = InvertValue(e.NewValue);

            MoveShapesX(tfg_head_target, DegreesToPixelsX_head(e.NewValue));
            MoveShapesX(tfg_joy_target, DegreesToPixelsX_joy(e.NewValue));

            tt_bg.X = InvertValue(DegreesToPixelsX_joy(e.NewValue)*30.0); 
        }

        private void slider_rota_Z_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Debug.WriteLine($"head_rotationZ.Angle : {Math.Round(e.NewValue, 2)} -> {InvertValue(Math.Round(e.NewValue, 2))}°");
            head_rotationZ.Angle = e.NewValue;
        }*/

        


        private void DrawGraduatedHLine()
        {
            var brushLine = CreateBrushFromArgb(255, 55, 55, 55);
            var brushTick = CreateBrushFromArgb(255, 50, 50, 50);
            var brushText = CreateBrushFromArgb(255, 55, 55, 55);

            double Y = 768 / 2.0;
            double W = 1024;
            double margin = 30;


            // Draw the main horizontal line
            Line mainLine = new Line
            {
                X1 = margin,
                Y1 = Y,
                X2 = W - margin,
                Y2 = Y,
                Stroke = brushLine,
                StrokeThickness = 0.6
            };
            canva.Children.Add(mainLine);

            // Add graduation marks
            int minDegree = -180;
            int maxDegree = 180;
            int numberOfGraduations = (maxDegree - minDegree) / 20; // Adjust the number of graduations
            double graduationSpacing = (mainLine.X2 - mainLine.X1) / numberOfGraduations;

            //Debug.WriteLine($"HLine:{(mainLine.X2 - mainLine.X1)}");

            for (int i = 0; i <= numberOfGraduations; i++)
            {
                double x = mainLine.X1 + i * graduationSpacing;
                int degree = minDegree + (i * 20);

                // Draw each tick mark
                Line tick = new Line
                {
                    X1 = x,
                    Y1 = Y - 5,
                    X2 = x,
                    Y2 = Y + 5,
                    Stroke = brushTick,
                    StrokeThickness = 0.4
                };
                canva.Children.Add(tick);

                if (degree != 0)
                {
                    // Add labels for the tick marks
                    TextBlock label = new TextBlock
                    {
                        Text = degree.ToString(),//.Replace("-", ""),
                        Foreground = brushText,
                        FontSize = 10,
                        RenderTransform = new TranslateTransform(x - 5, Y - 20)
                    }; 
                    canva.Children.Add(label);
                }
            }
        }


        private void DrawGraduatedVLine()
        {
            var brushLine = CreateBrushFromArgb(255, 55, 55, 55);
            var brushTick = CreateBrushFromArgb(255, 50, 50, 50);
            var brushText = CreateBrushFromArgb(255, 55, 55, 55);

            double Y = 768 / 2.0;
            double W = 1024;
            double H = 768;
            double margin = 30;

            // Draw the main horizontal line
            Line mainLine = new Line
            {
                X1 = W / 2.0,
                Y1 = margin,
                X2 = W / 2.0,
                Y2 = H - margin,
                Stroke = brushLine,
                StrokeThickness = 0.6
            };
            canva.Children.Add(mainLine);

            // Add graduation marks
            int minDegree = -180;
            int maxDegree = 180;
            int numberOfGraduations = (maxDegree - minDegree) / 20; // Adjust the number of graduations
            double graduationSpacing = (mainLine.Y2 - mainLine.Y1) / numberOfGraduations;

            //Debug.WriteLine($"VLine:{(mainLine.Y2 - mainLine.Y1)}");

            for (int i = 0; i <= numberOfGraduations; i++)
            {
                double y = mainLine.Y1 + i * graduationSpacing;
                int degree = minDegree + (i * 20);

                // Draw each tick mark
                Line tick = new Line
                {
                    X1 = W / 2.0 - 5,
                    Y1 = y,
                    X2 = W / 2.0 + 5,
                    Y2 = y,
                    Stroke = brushTick,
                    StrokeThickness = 0.4
                };
                canva.Children.Add(tick);

                // Add labels for the tick marks
                TextBlock label = new TextBlock
                {
                    Text = MathUtilities.InvertValue(degree).ToString(),//.Replace("-", ""),
                    Foreground = brushText,
                    FontSize = 10,
                    RenderTransform = new TranslateTransform(W / 2.0+10, y-10)
                };
                canva.Children.Add(label);
            }
        }


        private void DrawTarget()
        {

            byte alpha = 255;
            byte red   = 30;
            byte green = 144;
            byte blue  = 255;

            //byte red  = 255;
            //byte green = 255;
            //byte blue  = 0;

            var brush = CreateBrushFromArgb(alpha, red, green, blue);
            Color color = Color.FromArgb(alpha, red, green, blue);

            double cW = 1024;
            double cH = 768;
            double Dl = 50.0;
            double Ds = 10.0;

            double Xl = /*(cW / 2.0)*/30 - (Dl / 2.0);
            double Yl = /*(cH / 2.0)*/30 - (Dl / 2.0);

            double Xs = /*(cW / 2.0)*/30 - (Ds / 2.0);
            double Ys = /*(cH / 2.0)*/30 - (Ds / 2.0);


            Ellipse circle_s = new Ellipse
            {
                Width = Ds,
                Height = Ds,
                Stroke = brush,
                StrokeThickness = 0.6,
                Fill = Brushes.Transparent
            };

            Canvas.SetLeft(circle_s, Xs); // Adjust position on canvas
            Canvas.SetTop(circle_s, Ys);  // Adjust position on canvas

            //RenderOptions.SetEdgeMode(circle, EdgeMode.Aliased); 
            RenderOptions.SetBitmapScalingMode(circle_s, BitmapScalingMode.Fant);

            // Create the glow effect
            DropShadowEffect glowEffects = new DropShadowEffect
            {
                Color = color, // Change this to your desired glow color
                BlurRadius = 10,
                ShadowDepth = 0, // Set to 0 to center the shadow evenly around the object
                Opacity = 1
            };
            circle_s.Effect = glowEffects;

            canva.Children.Add(circle_s);


            Ellipse circle_l = new Ellipse
            {
                Width = Dl,
                Height = Dl,
                Stroke = brush,
                StrokeThickness = 1.2,
                Fill = CreateBrushFromArgb(31, 0, 0, 0)
            };

            Canvas.SetLeft(circle_l, Xl); // Adjust position on canvas
            Canvas.SetTop(circle_l, Yl);  // Adjust position on canvas

            //RenderOptions.SetEdgeMode(circle, EdgeMode.Aliased); 
            RenderOptions.SetBitmapScalingMode(circle_l, BitmapScalingMode.Fant);


            // Create the glow effect
            DropShadowEffect glowEffectl = new DropShadowEffect
            {
                Color = color, // Change this to your desired glow color
                BlurRadius = 20,
                ShadowDepth = 5, // Set to 0 to center the shadow evenly around the object
                Opacity = 1
            };
            circle_l.Effect = glowEffectl;

            canva.Children.Add(circle_l);


            Line Hline = new Line
            {
                X1 = Xl - 10,
                Y1 = (Yl + (Dl / 2.0)),
                X2 = Xl+ Dl + 10,
                Y2 = (Yl + (Dl / 2.0)),
                Stroke = brush,
                StrokeThickness = 0.6
            };
            canva.Children.Add(Hline);


            Line Vline = new Line
            {
                X1 = 30,
                Y1 = Yl - 10,
                X2 = 30,
                Y2 = Yl + Dl + 10,
                Stroke = brush,
                StrokeThickness = 0.6
            };
            canva.Children.Add(Vline);

            tfg_head_target = new TransformGroup();
            tfg_head_target.Children.Add(new TranslateTransform(0, 0));
            circle_s.RenderTransform = tfg_head_target;
            circle_l.RenderTransform = tfg_head_target;
            Hline.RenderTransform = tfg_head_target;
            Vline.RenderTransform = tfg_head_target;

            double middle = Math.Floor(Convert.ToDouble(ushort.MaxValue) / 2);
            double head_target_X = MathUtilities.RawToPixels_TargetHead(middle, 0, 964);
            double head_target_Y = MathUtilities.RawToPixels_TargetHead(middle, 0, 708);
            MathUtilities.MoveShapes(tfg_head_target, head_target_X, head_target_Y);
        }


        private void btn_config_Click(object sender, RoutedEventArgs e)
        {
            wconfig = new Configuration();
            wconfig.Owner = this;
            wconfig.config_file = config_file;
            wconfig.ShowDialog();

            Debug.WriteLine($"[Wconfig_Closed] selected_guid : {wconfig.selected_guid})");

            ReleasePoll(true);

        }


        private static (bool state, object value) RegistryValueExists(RegistryKey rootKey, string keyName, string valueName)
        {
            bool state = false;
            object value = null;

            using (RegistryKey key = rootKey.OpenSubKey(keyName))
            {
                if (key != null)
                {
                    value = key.GetValue(valueName);
                    state = true;
                }
                return (state, value);
            }

        }

        private static bool UpdateRegistryValue(RegistryKey rootKey, string keyName, string valueName, object valueData)
        {
            try
            {
                using (RegistryKey key = rootKey.OpenSubKey(keyName, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, valueData);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateRegistryValue] An error occurred: {ex.Message}");
            }
            return false;
        }


        private static bool CreateRegistryKeyAndSetValue(RegistryKey rootKey, string keyName, string valueName, object valueData)
        {
            try
            {
                using (RegistryKey key = rootKey.CreateSubKey(keyName))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, valueData);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateRegistryKeyAndSetValue] An error occurred: {ex.Message}");
            }
            return false;
        }




        private void InitJoy()
        {
            if (System.IO.File.Exists(config_file))
            {
                try
                {
                    JObject config = JObject.Parse(System.IO.File.ReadAllText(config_file));
                    if(config != null)
                    {

                        if (config.ContainsKey("Use_Freetrack20Enhanced"))
                        {
                            if (Convert.ToBoolean(config["Use_Freetrack20Enhanced"]))
                            {


                                // check if NPClient64.dll exists
                                if (System.IO.File.Exists(npclient64))
                                {
                                    bool success = false;

                                    string reg_dll_path = String.Format("{0}/", System.IO.Path.GetDirectoryName(npclient64).Replace('\\', '/'));

                                    string regKeyName = @"Software\NaturalPoint\NATURALPOINT\NPClient Location";

                                    var result = RegistryValueExists(Registry.CurrentUser, regKeyName, "Path");

                                    if (!result.state)
                                    {
                                        if (CreateRegistryKeyAndSetValue(Registry.CurrentUser, regKeyName, "Path", reg_dll_path))
                                        {
                                            success = true;
                                        }
                                    }
                                    else
                                    {
                                        
                                        if (!System.IO.Path.Equals(result.value.ToString(), reg_dll_path))
                                        {

                                            if(UpdateRegistryValue(Registry.CurrentUser, regKeyName, "Path", reg_dll_path))
                                            {
                                                success = true;
                                            }
                                        }
                                        else
                                        {
                                            success = true;
                                        }
                                        
                                    }

                                    if (success)
                                    {
                                        Debug.WriteLine($"RegKey {regKeyName}\\Path exists");
                                        use_freetrack20enhanced = true;
                                    }
                                    else
                                    {
                                        System.IO.FileInfo fi = new System.IO.FileInfo(npclient64);
                                        wmessagebox.message = String.Format("An error was occured when trying to set registry key for <b>{0}</b>.", fi.Name);
                                        wmessagebox.ShowDialog();
                                    }
                                    
                                }
                                else
                                {
                                    System.IO.FileInfo fi = new System.IO.FileInfo(npclient64);
                                    wmessagebox.message = String.Format("Required library <b>{0}</b> is missing !{1}You can download here : <b>https://github.com/opentrack/opentrack/blob/master/bin/NPClient64.dll</b>", fi.Name, System.Environment.NewLine);
                                    wmessagebox.ShowDialog();                                    
                                }

                                

                            }
                        }

                        if (config.ContainsKey("GUID") && 
                            config.ContainsKey("ProductName") && 
                            config.ContainsKey("Config") && 
                            config.ContainsKey("CurveExponent") &&
                            config.ContainsKey("Deadzone"))
                        {
                            string ProductName = config["ProductName"].ToString();
                            tb_device_name.Text = ProductName;

                            Guid guid = Guid.Parse(config["GUID"].ToString());                            

                            if (!Guid.Equals(guid, Guid.Empty))
                            {

                                var result = input.init(guid);

                                if (result.state)
                                {
                                    input.tb_debug        = tb_debug;
                                    input.tfg_joy_target  = tfg_joy_target;
                                    input.tfg_head_target = tfg_head_target;
                                    input.tt_bg           = tt_bg;
                                    input.head_rotationX  = head_rotationX;
                                    input.head_rotationY  = head_rotationY;
                                    input.config          = Convert.ToInt32(config["Config"]);
                                    input.curve_exponent  = Math.Round(Convert.ToDouble(config["CurveExponent"]),2);
                                    input.deadzone        = Math.Round(Convert.ToDouble(config["Deadzone"]), 2);

                                    input.use_freetrack20enhanced = use_freetrack20enhanced;

                                    _workerTask = input.StartPoll();
                                }
                                else
                                {
                                    tb_device_name.Text = String.Empty;
                                    wmessagebox.message = String.Format(result.description, ProductName);
                                    wmessagebox.ShowDialog();
                                }

                            }
                        }

                        
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[InitJoy] {ex.Message}");
                }
            }
        }


        private async void ReleasePoll(bool reinit=false)
        {
            Debug.WriteLine($"[ReleasePoll] reinit={reinit}");

            if (input != null)
            {
                input.StopPoll();

                if (_workerTask != null)
                {
                    await _workerTask;

                    _workerTask.Dispose();
                    
                }

                if (reinit)
                {
                    InitJoy();
                }

            }
        }

        private void tb_device_name_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PlaceDeviceNameLabel();
        }

        
    }
}
