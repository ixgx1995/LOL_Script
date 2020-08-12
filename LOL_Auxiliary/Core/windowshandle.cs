using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LOL_Auxiliary
{
    public class windowshandle
    {
        [DllImportAttribute("gdi32.dll")]
        public static extern bool BitBlt
(
    IntPtr hdcDest,    //目标DC的句柄
    int nXDest,        //目标DC的矩形区域的左上角的x坐标
    int nYDest,        //目标DC的矩形区域的左上角的y坐标
    int nWidth,        //目标DC的句型区域的宽度值
    int nHeight,       //目标DC的句型区域的高度值
    IntPtr hdcSrc,     //源DC的句柄
    int nXSrc,         //源DC的矩形区域的左上角的x坐标
    int nYSrc,         //源DC的矩形区域的左上角的y坐标
    System.Int32 dwRo  //光栅的处理数值
);
        #region 光栅操作代码

        //BLACKNESS：表示使用与物理调色板的索引0相关的色彩来填充目标矩形区域，（对缺省的物理调色板而言，该颜色为黑色）。
        public const int BLACKNESS = 0x42;
        //DSTINVERT：表示使目标矩形区域颜色取反。
        public const int DSTINVERT = 0x550009;
        //MERGECOPY：表示使用布尔型的AND（与）操作符将源矩形区域的颜色与特定模式组合一起。
        public const int MERGECOPY = 0xC000CA;
        //MERGEPAINT：通过使用布尔型的OR（或）操作符将反向的源矩形区域的颜色与目标矩形区域的颜色合并。
        public const int MERGEPAINT = 0xBB0226;
        //NOTSRCCOPY：将源矩形区域颜色取反，于拷贝到目标矩形区域。
        public const int NOTSRCCOPY = 0x330008;
        //NOTSRCERASE：使用布尔类型的OR（或）操作符组合源和目标矩形区域的颜色值，然后将合成的颜色取反。
        public const int NOTSRCERASE = 0x1100A6;
        //PATCOPY：将特定的模式拷贝到目标位图上。
        public const int PATCOPY = 0xF00021;
        //PATPAINT：通过使用布尔OR（或）操作符将源矩形区域取反后的颜色值与特定模式的颜色合并。然后使用OR（或）操作符将该操作的结果与目标矩形区域内的颜色合并。
        public const int PATINVERT = 0x5A0049;
        //PATINVERT：通过使用XOR（异或）操作符将源和目标矩形区域内的颜色合并。
        public const int PATPAINT = 0xFB0A09;
        //SRCAND：通过使用AND（与）操作符来将源和目标矩形区域内的颜色合并。
        public const int SRCAND = 0x8800C6;
        //SRCCOPY：将源矩形区域直接拷贝到目标矩形区域。
        public const int SRCCOPY = 0xCC0020;
        //SRCERASE：通过使用AND（与）操作符将目标矩形区域颜色取反后与源矩形区域的颜色值合并。
        public const int SRCERASE = 0x440328;
        //SRCINVERT：通过使用布尔型的XOR（异或）操作符将源和目标矩形区域的颜色合并。
        public const int SRCINVERT = 0x660046;
        //SRCPAINT：通过使用布尔型的OR（或）操作符将源和目标矩形区域的颜色合并。
        public const int SRCPAINT = 0xEE0086;
        //WHITENESS：使用与物理调色板中索引1有关的颜色填充目标矩形区域。（对于缺省物理调色板来说，这个颜色就是白色）。
        public const int WHITENESS = 0xFF0062;

        #endregion

        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        //设置此窗体为活动窗体
        [DllImport("user32", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetFocus(IntPtr hWnd);

        //设置进程窗口到最前
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        
        /// <summary>
        /// 激活窗体(0 关闭 1正常 2最小化 3最大化等)
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="nCmdShow"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImportAttribute("user32.dll")]
        public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// 获取窗口大小以及位置
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpRect"></param>
        /// <returns></returns>
        [DllImportAttribute("user32.dll")]
        public static extern int GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);

        /// <summary>
        /// 获取窗口大小以及位置
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lpRect"></param>
        /// <returns></returns>
        [DllImportAttribute("user32.dll")]
        public static extern int GetWindowRect(IntPtr hWnd, ref Rect lpRect);

        /// <summary>
        /// 获得窗口的DC驱动
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImportAttribute("user32.dll")]
        public extern static IntPtr GetDC(IntPtr hWnd);

        [DllImportAttribute("user32.dll")]
        public extern static int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        //获取活动窗口句柄
        [DllImport("User32.dll")]
        public static extern IntPtr GetForegroundWindow();
    }
}
