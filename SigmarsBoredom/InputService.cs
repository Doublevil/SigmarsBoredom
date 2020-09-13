using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace SigmarsBoredom
{
    /// <summary>
    /// Sends input to a given process.
    /// </summary>
    public class InputService
    {
        #region Imports

        //this allows virtual mouse event creation and win32 api calls
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetCursorPos(int x, int y);

        #endregion

        private readonly IntPtr _mainWindowHandle;

        /// <summary>
        /// Builds an input service for the process with the given name.
        /// </summary>
        /// <param name="processName">Name of the target process.</param>
        public InputService(string processName)
        {
            var handle = Process.GetProcessesByName(processName).FirstOrDefault()?.MainWindowHandle;
            if (handle == null)
                throw new Exception($"Cannot find process \"{processName}\". Make sure the process is running before starting.");

            _mainWindowHandle = handle.Value;
        }

        /// <summary>
        /// Sends a click to the given position on the main window of the process.
        /// </summary>
        /// <param name="x">X coordinate of the point to click.</param>
        /// <param name="y">Y coordinate of the point to click.</param>
        public void SendMouseLeftButtonDown(int x, int y)
        {
            SetForegroundWindow(_mainWindowHandle);
            Point newCursorPos = new Point(x, y);
            ClientToScreen(_mainWindowHandle, ref newCursorPos);
            SetCursorPos(newCursorPos.X, newCursorPos.Y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        }

        /// <summary>
        /// Sends a click to the given position on the main window of the process.
        /// </summary>
        /// <param name="x">X coordinate of the point to click.</param>
        /// <param name="y">Y coordinate of the point to click.</param>
        public void SendMouseLeftButtonUp(int x, int y)
        {
            SetForegroundWindow(_mainWindowHandle);
            Point newCursorPos = new Point(x, y);
            ClientToScreen(_mainWindowHandle, ref newCursorPos);
            SetCursorPos(newCursorPos.X, newCursorPos.Y);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        /// <summary>
        /// Sends a mouse left button click on the main window of the process.
        /// </summary>
        /// <param name="x">X coordinate of the point to click.</param>
        /// <param name="y">Y coordinate of the point to click.</param>
        public void SendMouseLeftButtonClick(int x, int y)
        {
            SetForegroundWindow(_mainWindowHandle);
            Point newCursorPos = new Point(x, y);
            ClientToScreen(_mainWindowHandle, ref newCursorPos);
            SetCursorPos(newCursorPos.X, newCursorPos.Y);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
    }
}
