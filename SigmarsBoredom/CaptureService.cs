using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace SigmarsBoredom
{
    public class CaptureService
    {
        #region Imports

        [DllImport("gdi32", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

        [DllImport("gdi32", EntryPoint = "SelectObject")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32", EntryPoint = "CreateCompatibleDC")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32", EntryPoint = "BitBlt")]
        public static extern bool BitBlt(IntPtr hDestDC, int X, int Y, int nWidth, int nHeight, IntPtr hSrcDC, int SrcX, int SrcY, int Rop);

        [DllImport("gdi32", EntryPoint = "DeleteDC")]
        public static extern IntPtr DeleteDC(IntPtr hDC);

        [DllImport("gdi32", EntryPoint = "DeleteObject")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("user32.dll", EntryPoint = "GetDC")]
        public static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        #endregion

        private const int SRCCOPY = 13369376; // not sure what this is tbh

        private readonly IntPtr _mainWindowHandle;

        /// <summary>
        /// Builds a capture service capturing from the process with the given name.
        /// </summary>
        /// <param name="processName">Name of the process to capture.</param>
        public CaptureService(string processName)
        {
            var handle = Process.GetProcessesByName(processName).FirstOrDefault()?.MainWindowHandle;
            if (handle == null)
                throw new Exception($"Cannot find process \"{processName}\". Make sure the process is running before starting.");

            _mainWindowHandle = handle.Value;
        }

        /// <summary>
        /// Captures and returns an image of the target process' main window, with an option to get only the specified rectangle portion.
        /// </summary>
        /// <param name="rectangleInWindow">If specified, defines the area of the window that will be captured.</param>
        public Bitmap GetWindowImage(Rectangle? rectangleInWindow = null)
        {
            RECT sourceRectangle;
            if (rectangleInWindow == null)
            {
                if (!GetWindowRect(_mainWindowHandle, out sourceRectangle))
                    throw new Exception("Unable to get window dimensions");
            }
            else
            {
                sourceRectangle = new RECT()
                {
                    Left = rectangleInWindow.Value.X, Top = rectangleInWindow.Value.Y,
                    Right = rectangleInWindow.Value.Right, Bottom = rectangleInWindow.Value.Bottom
                };
            }

            int xLoc = sourceRectangle.Right - sourceRectangle.Left;
            int yLoc = sourceRectangle.Bottom - sourceRectangle.Top;

            IntPtr handle = GetDC(_mainWindowHandle);

            IntPtr mem = CreateCompatibleDC(handle);

            IntPtr result = CreateCompatibleBitmap(handle, xLoc, yLoc);

            if (result == IntPtr.Zero)
                throw new Exception("Could not create compatible bitmap from the target process' main window.");

            IntPtr oldBmp = SelectObject(mem, result);
            BitBlt(mem, 0, 0, xLoc, yLoc, handle, sourceRectangle.Left, sourceRectangle.Top, SRCCOPY);
            SelectObject(mem, oldBmp);
            DeleteDC(mem);
            ReleaseDC(_mainWindowHandle, handle);
            Image imgReturn = Image.FromHbitmap(result);

            DeleteObject(oldBmp);
            DeleteObject(handle);
            DeleteObject(mem);

            return (Bitmap)imgReturn;
        }
    }
}
