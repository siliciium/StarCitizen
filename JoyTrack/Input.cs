using freetrack20enhancedwrapper;
using HelixToolkit.Wpf;
using Newtonsoft.Json.Linq;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;



namespace JoyTrack
{
    internal class Input
    {

        private DirectInput directInput;
        private Joystick joystick;

        private static JoystickState previousState;
        private static int previous_X;
        private static int previous_Y;
        private static string log_file;

        private CancellationTokenSource _cts;
        private Freetrack20EnhancedClass freetrack20Enhanced;

        public TextBlock tb_debug { get; set; }
        public TransformGroup tfg_joy_target { get; set; }
        public TransformGroup tfg_head_target { get; set; }
        public TranslateTransform tt_bg { get; set; }
        public AxisAngleRotation3D head_rotationX { get; set; }
        public AxisAngleRotation3D head_rotationY { get; set; }
        public bool use_freetrack20enhanced { get; set; }
        public int config { get; set; }
        public Guid initialized_device_guid { get; set; }
        public string initialized_device_name { get; set; }
        public double curve_exponent { get; set; }
        public double deadzone { get; set; }


        public Input()
        {
            directInput = new DirectInput();
            previousState = new JoystickState();

            initialized_device_guid = Guid.Empty;
            initialized_device_name = string.Empty;

            string exec_path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            log_file = System.IO.Path.Combine(exec_path, "input-log.txt");
        }

        public Dictionary<Guid, Dictionary<string, object>> getDevices()
        {

            Dictionary<Guid, Dictionary<string, object>> devices = new Dictionary<Guid, Dictionary<string, object>>();

            foreach (DeviceInstance deviceInstance in directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly))
            {


                Debug.WriteLine($"Input/GameControl : {deviceInstance.InstanceGuid}");
                Debug.WriteLine($"\tProductName     : {deviceInstance.ProductName}");
                Debug.WriteLine($"\tInstanceName    : {deviceInstance.InstanceName}");
                Debug.WriteLine($"\tType            : {deviceInstance.Type}");
                Debug.WriteLine($"\tSubtype         : {deviceInstance.Subtype.ToString()}"); //.ToString("X4")
                Debug.WriteLine($"\tUsage           : {deviceInstance.Usage}");
                Debug.WriteLine($"\tUsagePage       : {deviceInstance.UsagePage}");
                Debug.WriteLine($"\tIsHID           : {deviceInstance.IsHumanInterfaceDevice}");

                Dictionary<string, object> infos = new Dictionary<string, object>
                {
                    { "ProductName", ReplaceMultipleSpacesWithSingle(deviceInstance.ProductName.Trim()) },
                    { "InstanceName", deviceInstance.InstanceName },
                    { "Type", deviceInstance.Type },
                    { "Subtype", $"{deviceInstance.Subtype.ToString()}" },//.ToString("X4")
                    { "Usage", deviceInstance.Usage },
                    { "UsagePage", deviceInstance.UsagePage },
                    { "IsHumanInterfaceDevice", deviceInstance.IsHumanInterfaceDevice }
                };

                var result = init(deviceInstance.InstanceGuid, true);
                if (result.state)
                {
                    foreach (string key in result.dic.Keys)
                    {
                        infos.Add(key, result.dic[key]);
                    }

                    /* Add only device containing axes */
                    if (result.dic.ContainsKey("AxeCount"))
                    {
                        if (Convert.ToInt32(result.dic["AxeCount"]) > 0)
                        {
                            devices.Add(deviceInstance.InstanceGuid, infos);
                        }
                    }
                }


            }

            return devices;
        }

        private void Log(string message)
        {
            Debug.WriteLine(message);

            try
            {
                if (!File.Exists(log_file)) { File.Create(log_file).Dispose(); }

                using (StreamWriter writer = new StreamWriter(log_file, true))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                }

            }
            catch (Exception ex)
            {

                Debug.WriteLine($"An error occurred while logging the message: {ex.Message}");

            }
        }

        private static string ReplaceMultipleSpacesWithSingle(string input)
        {
            // Use Regex to replace multiple spaces with a single space
            return Regex.Replace(input, @"\s+", " ");
        }


        public (bool state, string description, Dictionary<string, object> dic) init(Guid guid, bool unacquire = false)
        {
            List<JoystickOffset> joystickAxisOffsets = new List<JoystickOffset>() {
                JoystickOffset.X,
                JoystickOffset.Y,
                JoystickOffset.Z,
                JoystickOffset.RotationX,
                JoystickOffset.RotationY,
                JoystickOffset.RotationZ,
                JoystickOffset.Sliders0,
                JoystickOffset.Sliders1
            };

            bool state = false;
            string description = string.Empty;
            Dictionary<string, object> dic = new Dictionary<string, object>();

            try
            {
                if (unacquire)
                {
                    Log($"[Input][init] GUID:{guid} (retrieve infos)");
                }
                else
                {
                    Log($"[Input][init] GUID:{guid}");
                }

                if (directInput != null)
                {
                    // Initialize DirectInput
                    joystick = new Joystick(directInput, guid);

                    joystick.SetCooperativeLevel(IntPtr.Zero, CooperativeLevel.Background | CooperativeLevel.NonExclusive);

                    Log("[Input][init][DirectInput]\tJoystick infos:");
                    joystick.Properties.BufferSize = 128;
                    Log($"[Input][init][DirectInput]\t\tBufferSize : {joystick.Properties.BufferSize}");

                    joystick.Acquire();

                    Log($"[Input][init][DirectInput]\t\tType        : {joystick.Information.Type}");
                    Log($"[Input][init][DirectInput]\t\tSubtype     : {joystick.Information.Subtype}");
                    Log($"[Input][init][DirectInput]\t\tUsage       : {joystick.Information.Usage}");
                    Log($"[Input][init][DirectInput]\t\tUsagePage   : {joystick.Information.UsagePage}");

                    SharpDX.DirectInput.DeviceProperties properties = joystick.Properties;
                    Log("[Input][init][DirectInput]\tJoystick Properties:");
                    Log($"[Input][init][DirectInput]\t\tJoystickId  : {properties.JoystickId}");
                    Log($"[Input][init][DirectInput]\t\tAxisMode    : {properties.AxisMode}");
                    Log($"[Input][init][DirectInput]\t\tProductName : {properties.ProductName}");
                    Log($"[Input][init][DirectInput]\t\tProductId   : 0x{properties.ProductId.ToString("X4")}");
                    Log($"[Input][init][DirectInput]\t\tVendorId    : 0x{properties.VendorId.ToString("X4")}");

                    /* NOT IMPLEMENTED 
                    //Debug.WriteLine($"\t\tAutoCenter : {properties.AutoCenter}");
                    //Debug.WriteLine($"\t\tRange      : {properties.Range}");
                    //Debug.WriteLine($"\t\tPortDisplayName : {properties.PortDisplayName}");
                    //Debug.WriteLine($"\t\tSaturation  : {properties.Saturation}");
                    //Debug.WriteLine($"\t\tDeadZone    : {properties.DeadZone}");*/

                    SharpDX.DirectInput.Capabilities capabilities = joystick.Capabilities;
                    Log("[Input][init][DirectInput]\tJoystick Capabilities:");
                    Log($"[Input][init][DirectInput]\t\tAxeCount    : {capabilities.AxeCount}");
                    Log($"[Input][init][DirectInput]\t\tButtonCount : {capabilities.ButtonCount}");
                    Log($"[Input][init][DirectInput]\t\tPovCount    : {capabilities.PovCount}");



                    List<int> sai = new List<int>();
                    List<string> sas = new List<string>();
                    for (int i = 0; i < joystickAxisOffsets.Count; i++)
                    {
                        try
                        {
                            var mightGoBoom = joystick.GetObjectInfoByName(joystickAxisOffsets[i].ToString());
                            switch (joystickAxisOffsets[i])
                            {
                                case JoystickOffset.X:
                                    {
                                        sas.Add("X");
                                        break;
                                    }
                                case JoystickOffset.Y:
                                    {
                                        sas.Add("Y");
                                        break;
                                    }
                                case JoystickOffset.Z:
                                    {
                                        sas.Add("Z");
                                        break;
                                    }
                                case JoystickOffset.RotationX:
                                    {
                                        sas.Add("RotationX");
                                        break;
                                    }
                                case JoystickOffset.RotationY:
                                    {
                                        sas.Add("RotationY");
                                        break;
                                    }
                                case JoystickOffset.RotationZ:
                                    {
                                        sas.Add("RotationZ");
                                        break;
                                    }
                                case JoystickOffset.Sliders0:
                                    {
                                        sas.Add("Sliders0");
                                        break;
                                    }
                                case JoystickOffset.Sliders1:
                                    {
                                        sas.Add("Sliders1");
                                        break;
                                    }
                            }
                            sai.Add(i + 1);
                        }
                        catch { }
                    }
                    int[] SupportedAxes = sai.ToArray();
                    Log($"[Input][init][DirectInput] Joystick {properties.JoystickId} ({properties.ProductName.Trim()}) SupportedAxes: {SupportedAxes.Length}");
                    foreach (string s in sas)
                    {
                        Log($"[Input][init][DirectInput]\t\tAxe: {s}");
                    }

                    dic.Add("JoystickId", properties.JoystickId);
                    dic.Add("VendorId", $"0x{properties.VendorId.ToString("X4")}");
                    dic.Add("ProductId", $"0x{properties.ProductId.ToString("X4")}");
                    dic.Add("AxisMode", properties.AxisMode);
                    dic.Add("AxeCount", capabilities.AxeCount);
                    dic.Add("ButtonCount", capabilities.ButtonCount);
                    dic.Add("PovCount", capabilities.PovCount);


                    if (unacquire)
                    {
                        joystick.Unacquire();
                    }
                    else
                    {
                        initialized_device_guid = guid;
                        initialized_device_name = properties.ProductName.Trim();
                    }
                }
                else
                {
                    Log($"[Input][init] directInput is null");
                }

                state = true;
            }
            catch (Exception ex)
            {
                Log($"[Input][init] {ex.Message}");

                if (ex.Message.Contains("ApiCode: [DIERR_DEVICENOTREG/DeviceNotRegistered]"))
                {
                    description = "Your last used device <b>{0}</b> seems to be not attached. Please choose another device.";
                }
            }

            return (state, description, dic);
        }


        public Task StartPoll()
        {

            if (use_freetrack20enhanced)
            {
                freetrack20Enhanced = new Freetrack20EnhancedClass();
                if (freetrack20Enhanced.initialize())
                {
                    Log("[Input][StartPoll] freetrack20Enhanced initialized !");
                }
                else
                {
                    Log("[Input][StartPoll] freetrack20Enhanced NOT initialized !");
                }
            }

            _cts = new CancellationTokenSource();
            return Task.Run(() => PollLoop(_cts.Token), _cts.Token);
        }

        private void PollLoop(CancellationToken token)
        {

            try
            {
                Log($"[Input][PollLoop] START (config:{config} curve_exponent:{curve_exponent} deadzone:{deadzone})");


                /* CURVE */
                /// ushort.MinValue : 0
                double min = Convert.ToDouble(ushort.MinValue);
                 /// ushort.MaxValue : 65535
                double max = Convert.ToDouble(ushort.MaxValue);
                double center = Math.Floor(max / 2);
                int icenter = Convert.ToInt32(center);

                double deadzone_step = Math.Floor(center * deadzone);
                double deadzone_min = center - deadzone_step;
                double deadzone_max = center + deadzone_step;
                Log($"[Input][PollLoop] {min} - {deadzone_min} , {deadzone_max} - {max}");


                while (!token.IsCancellationRequested)
                {

                    string infos = string.Empty;

                    joystick.Poll();
                    JoystickState state = joystick.GetCurrentState();
                    previous_X = state.X;
                    previous_Y = state.Y;


                    /* CURVE & DEADZONE */                    

                    if (state.X > icenter && state.X >= deadzone_max)
                    {                        
                        previous_X = MathUtilities.ConvertRange(previous_X, deadzone_max, max, center, max);
                        previous_X = MathUtilities.ApplyCurveToMax(previous_X, center, max, curve_exponent);
                    }
                    else if (state.X < icenter && state.X <= deadzone_min)
                    {                        
                        previous_X = MathUtilities.ConvertRange(previous_X, min, deadzone_min, min, center);
                        previous_X = MathUtilities.ApplyCurveToZero(previous_X, min, center, curve_exponent);
                    }
                    else
                    {
                        previous_X = icenter;
                    }

                    if (state.Y > icenter && state.Y >= deadzone_max)
                    {
                        previous_Y = MathUtilities.ConvertRange(previous_Y, deadzone_max, max, center, max);
                        previous_Y = MathUtilities.ApplyCurveToMax(previous_Y, center, max, curve_exponent);
                    }
                    else if (state.Y < icenter && state.Y <= deadzone_min)
                    {
                        previous_Y = MathUtilities.ConvertRange(previous_Y, min, deadzone_min, min, center);
                        previous_Y = MathUtilities.ApplyCurveToMax(previous_Y, min, center, curve_exponent);
                    }
                    else
                    {
                        previous_Y = icenter;
                    }




                    /*JoystickUpdate[] datas = joystick.GetBufferedData(); 
                    foreach (var state in datas)
                    {
                        if (state.Offset == JoystickOffset.X)
                        {
                            if (state.Value != 0)
                            {
                                previous_X = state.Value;
                            }                                
                        }
                        else if (state.Offset == JoystickOffset.Y)
                        {
                            if (state.Value != 0)
                            {
                                previous_Y = state.Value;
                            }                                
                        }

                    }*/

                    double joy_target_X = MathUtilities.RawToPixels_TargetJoy(previous_X);                    
                    double joy_target_Y = MathUtilities.RawToPixels_TargetJoy(previous_Y);

                    double head_target_X = 0, head_target_Y = 0;

                    double head_X = 0, head_Y = 0;

                    double bg_X = 0, bg_Y = 0;

                    double freetrack_pitch = 0, freetrack_yaw = 0;
                    
                    switch (config)
                    {
                        case 0:
                            { /* URDL */
                                head_target_X = MathUtilities.RawToPixels_TargetHead(previous_X, 0, 964);
                                head_target_Y = MathUtilities.RawToPixels_TargetHead(previous_Y, 0, 708);

                                head_X = MathUtilities.RawToPixels_Head(previous_Y);
                                head_Y = MathUtilities.InvertValue(MathUtilities.RawToPixels_Head(previous_X));

                                bg_X = MathUtilities.InvertValue(MathUtilities.RawToPixels_TargetJoy(previous_X));
                                bg_Y = MathUtilities.InvertValue(MathUtilities.RawToPixels_TargetJoy(previous_Y));

                                freetrack_pitch = MathUtilities.InvertValue(MathUtilities.ToDegrees(previous_Y, 180));
                                freetrack_yaw = MathUtilities.ToDegrees(previous_X, 180);
                                break;
                            }
                        case 1: /* ULDR */
                            {
                                head_target_X = MathUtilities.RawToPixels_TargetHead(previous_X, 964, 0);
                                head_target_Y = MathUtilities.RawToPixels_TargetHead(previous_Y, 0, 708);

                                head_X = MathUtilities.RawToPixels_Head(previous_Y);
                                head_Y = MathUtilities.RawToPixels_Head(previous_X);

                                bg_X = MathUtilities.RawToPixels_TargetJoy(previous_X);
                                bg_Y = MathUtilities.InvertValue(MathUtilities.RawToPixels_TargetJoy(previous_Y));

                                freetrack_pitch = MathUtilities.InvertValue(MathUtilities.ToDegrees(previous_Y, 180));
                                freetrack_yaw = MathUtilities.InvertValue(MathUtilities.ToDegrees(previous_X, 180));
                                break;
                            }
                        case 2: /* DRUL */
                            {
                                head_target_X = MathUtilities.RawToPixels_TargetHead(previous_X, 0, 964);
                                head_target_Y = MathUtilities.RawToPixels_TargetHead(previous_Y, 708, 0);

                                head_X = MathUtilities.InvertValue(MathUtilities.RawToPixels_Head(previous_Y));
                                head_Y = MathUtilities.InvertValue(MathUtilities.RawToPixels_Head(previous_X));

                                bg_X = MathUtilities.InvertValue(MathUtilities.RawToPixels_TargetJoy(previous_X));
                                bg_Y = MathUtilities.RawToPixels_TargetJoy(previous_Y);

                                freetrack_pitch = MathUtilities.ToDegrees(previous_Y, 180);
                                freetrack_yaw = MathUtilities.ToDegrees(previous_X, 180);
                                break;
                            }
                        case 3: /* DLUR */
                            {
                                head_target_X = MathUtilities.RawToPixels_TargetHead(previous_X, 964, 0);
                                head_target_Y = MathUtilities.RawToPixels_TargetHead(previous_Y, 708, 0);

                                head_X = MathUtilities.InvertValue(MathUtilities.RawToPixels_Head(previous_Y));
                                head_Y = MathUtilities.RawToPixels_Head(previous_X);

                                bg_X = MathUtilities.RawToPixels_TargetJoy(previous_X);
                                bg_Y = MathUtilities.RawToPixels_TargetJoy(previous_Y);

                                freetrack_pitch = MathUtilities.ToDegrees(previous_Y, 180);
                                freetrack_yaw = MathUtilities.InvertValue(MathUtilities.ToDegrees(previous_X, 180));
                                break;
                            }
                        case 4: /* RULD */
                            {
                                head_target_X = MathUtilities.RawToPixels_TargetHead(previous_Y, 964, 0);
                                head_target_Y = MathUtilities.RawToPixels_TargetHead(previous_X, 708, 0);

                                head_X = MathUtilities.InvertValue(MathUtilities.RawToPixels_Head(previous_X));
                                head_Y = MathUtilities.RawToPixels_Head(previous_Y);

                                bg_X = MathUtilities.RawToPixels_TargetJoy(previous_Y);
                                bg_Y = MathUtilities.RawToPixels_TargetJoy(previous_X);

                                freetrack_pitch = MathUtilities.ToDegrees(previous_X, 180);
                                freetrack_yaw = MathUtilities.InvertValue(MathUtilities.ToDegrees(previous_Y, 180));
                                break;
                            }
                        case 5: /* RDLU */
                            {
                                head_target_X = MathUtilities.RawToPixels_TargetHead(previous_Y, 964, 0);
                                head_target_Y = MathUtilities.RawToPixels_TargetHead(previous_X, 0, 708);

                                head_X = MathUtilities.RawToPixels_Head(previous_X);
                                head_Y = MathUtilities.RawToPixels_Head(previous_Y);

                                bg_X = MathUtilities.RawToPixels_TargetJoy(previous_Y);
                                bg_Y = MathUtilities.InvertValue(MathUtilities.RawToPixels_TargetJoy(previous_X));

                                freetrack_pitch = MathUtilities.InvertValue(MathUtilities.ToDegrees(previous_X, 180));
                                freetrack_yaw = MathUtilities.InvertValue(MathUtilities.ToDegrees(previous_Y, 180));
                                break;
                            }
                        case 6: /* LURD */
                            {
                                head_target_X = MathUtilities.RawToPixels_TargetHead(previous_Y, 964, 0);
                                head_target_Y = MathUtilities.RawToPixels_TargetHead(previous_X, 708, 0);

                                head_X = MathUtilities.InvertValue(MathUtilities.RawToPixels_Head(previous_X));
                                head_Y = MathUtilities.RawToPixels_Head(previous_Y);

                                bg_X = MathUtilities.RawToPixels_TargetJoy(previous_Y);
                                bg_Y = MathUtilities.RawToPixels_TargetJoy(previous_X);

                                freetrack_pitch = MathUtilities.ToDegrees(previous_X, 180);
                                freetrack_yaw = MathUtilities.InvertValue(MathUtilities.ToDegrees(previous_Y, 180));
                                break;
                            }
                        case 7: /* LDRU */
                            {
                                head_target_X = MathUtilities.RawToPixels_TargetHead(previous_Y, 964, 0);
                                head_target_Y = MathUtilities.RawToPixels_TargetHead(previous_X, 0, 708);

                                head_X = MathUtilities.RawToPixels_Head(previous_X);
                                head_Y = MathUtilities.RawToPixels_Head(previous_Y);

                                bg_X = MathUtilities.RawToPixels_TargetJoy(previous_Y);
                                bg_Y = MathUtilities.InvertValue(MathUtilities.RawToPixels_TargetJoy(previous_X));

                                freetrack_pitch = MathUtilities.InvertValue(MathUtilities.ToDegrees(previous_X, 180));
                                freetrack_yaw = MathUtilities.InvertValue(MathUtilities.ToDegrees(previous_Y, 180));
                                break;
                            }
                    }




                    if (tb_debug != null)
                    {

                        infos += string.Format("RAW        \t X:{0} Y:{1}{2}", state.X, state.Y, System.Environment.NewLine);
                        infos += string.Format("RAW smooth \t X:{0} Y:{1}{2}", previous_X, previous_Y, System.Environment.NewLine);
                        infos += string.Format("FreeTrack  \t Pitch:{0} Yaw:{1}{2}", Math.Round(freetrack_pitch, 2), Math.Round(freetrack_yaw, 2), System.Environment.NewLine);
                        //infos += string.Format("JOY Target   \t X:{0} Y:{1}{2}", Math.Round(joy_target_X, 2), Math.Round(joy_target_Y, 2), System.Environment.NewLine);
                        //infos += string.Format("HEAD Target  \t X:{0} Y:{1}{2}", Math.Round(head_target_X, 2), Math.Round(head_target_Y, 2), System.Environment.NewLine);
                        //infos += string.Format("HEAD         \t X:{0} Y:{1}{2}", Math.Round(head_X, 2), Math.Round(head_Y, 2), System.Environment.NewLine);

                        tb_debug.Dispatcher.Invoke(() =>
                        {
                            tb_debug.Text = infos;
                        });

                    }

                    if (tfg_joy_target != null)
                    {
                        tfg_joy_target.Dispatcher.Invoke(() =>
                        {
                            MathUtilities.MoveShapes(tfg_joy_target, joy_target_X, joy_target_Y);
                        });
                    }

                    if (tfg_head_target != null)
                    {
                        tfg_head_target.Dispatcher.Invoke(() =>
                        {
                            MathUtilities.MoveShapes(tfg_head_target, head_target_X, head_target_Y);
                        });
                    }

                    if (tt_bg != null)
                    {
                        tt_bg.Dispatcher.Invoke(() =>
                        {
                            tt_bg.X = bg_X * 30.0;
                            tt_bg.Y = bg_Y * 30.0;
                        });
                    }

                    if (head_rotationX != null)
                    {
                        head_rotationX.Dispatcher.Invoke(() =>
                        {
                            head_rotationX.Angle = head_X;
                        });
                    }

                    if (head_rotationY != null)
                    {
                        head_rotationY.Dispatcher.Invoke(() =>
                        {
                            head_rotationY.Angle = head_Y;
                        });
                    }

                    if (use_freetrack20enhanced)
                    {
                        if (freetrack20Enhanced != null)
                        {
                            double headpose_pitch = freetrack_pitch;
                            double headpose_yaw = freetrack_yaw;
                            double headpose_roll = 0;
                            double headpose_X = 0;
                            double headpose_Y = 0;
                            double headpose_Z = 0;

                            double raw_pitch = freetrack_pitch;
                            double raw_yaw = freetrack_yaw;
                            double raw_roll = 0;
                            double raw_X = 0;
                            double raw_Y = 0;
                            double raw_Z = 0;

                            freetrack20Enhanced.pose(headpose_pitch, headpose_yaw, headpose_roll, headpose_X, headpose_Y, headpose_Z, raw_pitch, raw_yaw, raw_roll, raw_X, raw_Y, raw_Z);
                        }
                    }

                    


                    System.Threading.Thread.Sleep(8);
                    
                }


                // Release access to the device
                joystick.Unacquire();
                joystick.Dispose();

                Log($"[Input][PollLoop] END (joystick Unacquired/Disposedd)");


            }
            catch (Exception ex)
            {
                Log($"[Input][PollLoop][ERROR] {ex.Message}");
                Log($"[Input][PollLoop][ERROR] config:{config} curve_exponent:{curve_exponent}");
            }
        }

        public void StopPoll()
        {
            if (_cts != null)
            {
                Log($"[Input][StopPoll]");
                _cts.Cancel();
            }
        }




    }
}
