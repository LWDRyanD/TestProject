using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace InputSimulatorTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Process p = Process.GetProcessesByName("ffxiv_dx11").FirstOrDefault();
            if (p != null)
            {
                InputSimulator.InputSimulator.SendKeyPress(p.MainWindowHandle, VirtualKeyCode.VK_U, VirtualKeyCode.CONTROL);
            }
        }
    }
}
