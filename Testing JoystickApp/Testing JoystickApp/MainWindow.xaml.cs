using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpDX.DirectInput;
using System.Runtime.InteropServices;

namespace Testing_JoystickApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public MainWindow()
        {
            InitializeComponent();
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
                joystickGuid = deviceInstance.InstanceGuid;

            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                    joystickGuid = deviceInstance.InstanceGuid;

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                Console.WriteLine("No joystick/Gamepad found.");
                Console.ReadKey();
                Environment.Exit(1);
            }

            // Instantiate the joystick
            var joystick = new Joystick(directInput, joystickGuid);

            Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid);

            // Query all suported ForceFeedback effects
            var allEffects = joystick.GetEffects();
            foreach (var effectInfo in allEffects)
                Console.WriteLine("Effect available {0}", effectInfo.Name);

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();

            // Poll events from joystick
            double x = 700;
            double y = 360;
            double theX = 500; 
            double theY = 500;
            Boolean clickOnce = false;
            while (true)
            {
                joystick.Poll();
                var datas = joystick.GetBufferedData();
                Boolean click = false;
                foreach (var state in datas)
                {
                    Console.WriteLine(state);
                    if (state.Offset == JoystickOffset.X)
                    {
                        x = state.Value;
                    }
                    if (state.Offset == JoystickOffset.Y)
                    {
                        y = state.Value;
                    }
                    if (state.Offset == JoystickOffset.Buttons0)
                    {
                        click = true;
                        clickOnce = !clickOnce;
                    }

                }
             
                //Console.WriteLine(theX + "," + theY);
                if (x >= 44000)
                {
                    if(theX < 1366)
                        NativeMethods.SetCursorPos((int)theX++, (int)theY);
                }
                if (x <= 20000)
                {
                    if(theX > 0)
                        NativeMethods.SetCursorPos((int)theX--, (int)theY);
                }
                if (y >= 44000)
                {
                    if(theY < 768)
                        NativeMethods.SetCursorPos((int)theX, (int)theY++);
                }
                if (y <= 20000)
                {
                    if(theY > 0)
                        NativeMethods.SetCursorPos((int)theX, (int)theY--);
                }

                System.Threading.Thread.Sleep(9);
                if (click)
                {
                    if (clickOnce)
                    {
                        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (int)theX, (int)theY, 0, 0);
                    }
                }
            }
        }

        public partial class NativeMethods
        {
            /// Return Type: BOOL->int  
            ///X: int  
            ///Y: int  
            [System.Runtime.InteropServices.DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
            [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
            public static extern bool SetCursorPos(int X, int Y);
        }



    }
}
