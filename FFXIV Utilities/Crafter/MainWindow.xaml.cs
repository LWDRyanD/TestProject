namespace Crafter
{
    using Emgu.CV;
    using Emgu.CV.CvEnum;
    using Emgu.CV.Structure;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Media;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Xml.Serialization;
    using WindowsInput;
    using WindowsInput.Native;
    using CrafterInputSimulator = InputSimulator.InputSimulator;
    using Point = System.Drawing.Point;
    using Size = System.Drawing.Size;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Members

        private IntPtr _ffWindowHandle;
        private WindowsInput.InputSimulator _inputSimulator = new WindowsInput.InputSimulator();
        private Thread _craftThread;
        private CraftingViewModel _viewModel = new CraftingViewModel();
        private Image<Gray, byte> _synthButtonImage;
        private string _patternsfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "FINAL FANTASY XIV - A Realm Reborn", "Patterns.xml");
        private string _synthButtonfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "FINAL FANTASY XIV - A Realm Reborn", "SynthButton.png");

        #endregion Private Members
        
        public MainWindow()
        {
            InitializeCrafterData();
            InitializeComponent();

            FindSynthesisButton();
            // Hook up console output to the text box. Will want to refactor this later into a smarter reporting system.
            Console.SetOut(new ConsoleTextBoxOutput(this.Output));
        }

        /// <summary>
        /// Gets a <see cref="IntPtr"/> window handle to the FXIV process.
        /// </summary>
        public IntPtr FFWindowHandle
        {
            get
            {
                return this._ffWindowHandle != default(IntPtr) ? this._ffWindowHandle : (this._ffWindowHandle = this.GetFFWindowHandle());
            }
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this._craftThread != null && this._craftThread.IsAlive)
            {
                this._craftThread.Abort();
            }
        }

        private IntPtr GetFFWindowHandle()
        {
            Process p = Process.GetProcessesByName("ffxiv_dx11").FirstOrDefault();
            return p != null ? p.MainWindowHandle : default(IntPtr);
        }

        private void InitializeCrafterData()
        {
            // Bail out if there is no patterns file available.
            if (!File.Exists(this._patternsfilePath))
            {
                MessageBox.Show($"The Pattern file, '{this._patternsfilePath}', could not be found. Greedy Crafter will close now. Sorry, friend.");
                this.Close();
            }

            using (var patternStream = File.OpenRead(this._patternsfilePath))
            {
                XmlSerializer xs = new XmlSerializer(typeof(List<CraftPattern>));
                var definedPatterns = (List<CraftPattern>)xs.Deserialize(patternStream);
                foreach (var pattern in definedPatterns)
                {
                    this._viewModel.Patterns.Add(pattern);
                }
            }

            // Warn the user we'll fall back to a more brute force method of starting synthesis if we can't find the file.
            if (!File.Exists(this._synthButtonfilePath))
            {
                MessageBox.Show($"The Synthesis Button template file({this._synthButtonfilePath}) could not be found. Greedy Crafter will use the Confirm button to try to get into synthesis.");
            }
            else
            {
                this._synthButtonImage = new Image<Gray, byte>(this._synthButtonfilePath);
            }

            this.DataContext = this._viewModel;
        }

        private Rectangle? FindSynthesisButton()
        {
            // Grab the location and size information for the FFXIV window.
            Win32Rect ffScreenRect = new Win32Rect();
            GetWindowRect(this.FFWindowHandle, ref ffScreenRect);
            var screenSize = new Size(ffScreenRect.Right - ffScreenRect.Left, ffScreenRect.Bottom - ffScreenRect.Top);

            // Create a GDI+ interface and take a screen shot of the screen.
            using (Graphics ffScreen = Graphics.FromHwnd(this.FFWindowHandle))
            {
                using (Bitmap screenShot = new Bitmap(screenSize.Width, screenSize.Height, ffScreen))
                {
                    using (Graphics screenShotInterface = Graphics.FromImage(screenShot))
                    {
                        screenShotInterface.CopyFromScreen(ffScreenRect.Left, ffScreenRect.Top, 0, 0, screenSize);
                    }

                    // Use the screen shot and find any likely matches.
                    Image<Gray, byte> source = new Image<Gray, byte>(screenShot);
                    using (Image<Gray, float> result = source.MatchTemplate(this._synthButtonImage, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed))
                    {
                        double[] minValues, maxValues;
                        System.Drawing.Point[] minLocations, maxLocations;
                        result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                        // Check the max value, only feel confident if it's over 0.8.
                        if (maxValues.Any() && maxValues[0] > 0.8)
                        {
                            return new Rectangle(maxLocations[0].X, maxLocations[0].Y, this._synthButtonImage.Width, this._synthButtonImage.Height);
                        }
                    }
                }
            }

            // If we got here we didn't find any likely matches.
            return null;
        }

        private void ClickScreen(Point clickPoint)
        {
            var vx = SystemParameters.VirtualScreenWidth;
            var vy = SystemParameters.VirtualScreenHeight;
            SetForegroundWindow(this.FFWindowHandle);

            this._inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(
                (clickPoint.X / vx) * 65535,
                (clickPoint.Y / vy) * 65535);

            this._inputSimulator.Mouse.LeftButtonDown();
            Thread.Sleep(200);
            this._inputSimulator.Mouse.LeftButtonUp();
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this._viewModel.Count < 1)
            {
                SystemSounds.Asterisk.Play();
            }
            else
            {
                this.GoButton.IsEnabled = false;
                this.Output.Text = string.Empty;
                this.RunLoop(this._viewModel.Count, this._viewModel.SelectedPattern, this._viewModel.Collectible);
                this.GoButton.IsEnabled = true;
            }
        }

        private void PressKey(VirtualKeyCode key, TimeSpan sleepTime)
        {
            this.PressKey(key, null, sleepTime);
        }
        
        private bool AcquireFFWindow()
        {
            if (this.FFWindowHandle == default(IntPtr))
            {
                MessageBox.Show("Could not acquire a window handle to the FFXIV process. If FFXIV is running, try running Greedy Crafter as an Admin.");
                return false;
            }

            //if (SetForegroundWindow(this.FFWindowHandle) == 0)
            //{
            //    // Try to reacquire, and return if setting works.
            //    Process p = Process.GetProcessesByName("ffxiv_dx11").FirstOrDefault();
            //    if (p != null)
            //    {
            //        this._ffWindowHandle = p.MainWindowHandle;
            //        return true; // SetForegroundWindow(this._ffWindowHandle) != 0;
            //    }
            //}

            return true;
        }

        private void PressKey(VirtualKeyCode key, VirtualKeyCode? modifier, TimeSpan sleepTime)
        {
            if (this.AcquireFFWindow())
            {
                CrafterInputSimulator.SendKeyPress(this.FFWindowHandle, key, modifier);
                Thread.Sleep(sleepTime);
            }
            else
            {
                throw new InvalidOperationException("Failed to acquire FFXIV window.");
            }
        }

        public void RunLoop(int count, CraftPattern pattern, bool isCollectible)
        {
            Random rand = new Random((int)DateTime.Now.Ticks);

            Console.WriteLine($"-= Crafting {count} {pattern.Name} =-");
                
            this._craftThread = new Thread(
                () =>
                {
                    try
                    {
                        // Loop.
                        for (int i = 0; i < count; ++i)
                        {
                            Console.WriteLine();
                            Console.WriteLine($"Crafting {i + 1} of {count}...");
                            Console.WriteLine();

                            // If we have a synthesis button image, try to find the button dynamically and click it.
                            if (this._synthButtonImage != null)
                            {
                                var buttonRect = this.FindSynthesisButton();
                                if (buttonRect.HasValue)
                                {
                                    // Click the middle of the button.
                                    var clickPoint = new Point(buttonRect.Value.Left + (buttonRect.Value.Width / 2), buttonRect.Value.Top + (buttonRect.Value.Height / 2));
                                    this.ClickScreen(clickPoint);
                                    Thread.Sleep(TimeSpan.FromMilliseconds(1500 + rand.Next(0, 2000)));
                                }
                                else
                                {
                                    Console.WriteLine("Failed to find the Synthesize button. Aborting.");
                                    break;
                                }
                            }
                            else
                            {
                                this.PressKey(VirtualKeyCode.NUMPAD0, TimeSpan.FromMilliseconds(300 + rand.Next(0, 300)));
                                this.PressKey(VirtualKeyCode.NUMPAD0, TimeSpan.FromMilliseconds(300 + rand.Next(0, 300)));
                                this.PressKey(VirtualKeyCode.NUMPAD0, TimeSpan.FromMilliseconds(300 + rand.Next(0, 300)));
                                this.PressKey(VirtualKeyCode.NUMPAD0, TimeSpan.FromMilliseconds(300 + rand.Next(0, 300)));
                                this.PressKey(VirtualKeyCode.NUMPAD0, TimeSpan.FromSeconds(1.5) + TimeSpan.FromMilliseconds(rand.Next(0, 2000)));
                            }
                            
                            foreach (var command in pattern.Commands)
                            {
                                var sleepTime = command.GetSleepTime(rand);
                                Console.WriteLine($"  Pressing {command} then waiting {sleepTime.TotalSeconds} seconds...");
                                PressKey(command.Key, command.Modifier, sleepTime);
                            }

                            if (isCollectible)
                            {
                                PressKey(WindowsInput.Native.VirtualKeyCode.NUMPAD0, TimeSpan.FromSeconds(1) + TimeSpan.FromMilliseconds(rand.Next(0, 500)));
                                PressKey(WindowsInput.Native.VirtualKeyCode.NUMPAD0, TimeSpan.FromSeconds(1) + TimeSpan.FromMilliseconds(rand.Next(500, 2000)));
                            }
                        }

                        Console.WriteLine("-= Crafting complete! =-");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{ex.GetType().Name} encountered: {ex.Message}");
                    }
                });

            this._craftThread.Start();
        }
        
        #region Win32 Externs

        // import the function in your class
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref Win32Rect lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        #endregion Win32 Externs
    }
}
