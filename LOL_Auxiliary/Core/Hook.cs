using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Script
{
    /// <summary>
    /// 钩子类（监控用户输入）
    /// </summary>
    public class Hook
    {
        #region 用户处理变量集
        /// <summary>
        /// 用户处理类（为了封装成类，传到线程中处理）
        /// </summary>
        class CustomerHandle
        {
            /// <summary>
            /// 用户函数
            /// </summary>
            public Function function;
            /// <summary>
            /// 键值
            /// </summary>
            public int KeysCode;
            /// <summary>
            /// 鼠标坐标
            /// </summary>
            public int MouseXY;
            /// <summary>
            /// 键值状态
            /// </summary>
            public KeyMouseState state;
        }
        /// <summary>
        /// 用户处理对象
        /// </summary>
        CustomerHandle customerhandle;

        /// <summary>
        /// 用户的回掉函数委托
        /// </summary>
        public delegate void Function(int KeysCode, int MouseXY, KeyMouseState state);

        /// <summary>
        /// 鼠标键盘值状态
        /// </summary>
        public enum KeyMouseState { Down, Up, Double, Move, none }

        #endregion

        Thread t;//全局线程对象

        #region 键盘钩子值集

        public const int WM_KEYDOWN = 0x100;//按下

        public const int WM_KEYUP = 0x101;//释放

        public const int WM_SYSKEYDOWN = 0x104;//系统按键按下

        public const int WM_SYSKEYUP = 0x105;//系统按键释放

        #endregion

        #region 鼠标钩子键值集

        public const int WM_MOUSEMOVE = 0x200;//移动
        public const int WM_LBUTTONDOWN = 0x201;//左键按下
        public const int WM_RBUTTONDOWN = 0x204;//右键按下
        public const int WM_MBUTTONDOWN = 0x207;//中键按下
        public const int WM_LBUTTONUP = 0x202;//左键释放
        public const int WM_RBUTTONUP = 0x205;//右键释放
        public const int WM_MBUTTONUP = 0x208;//中键释放
        public const int WM_LBUTTONDBLCLK = 0x203;//左键双击
        public const int WM_RBUTTONDBLCLK = 0x206;//右键双击
        public const int WM_MBUTTONDBLCLK = 0x209;//中键双击

        #endregion

        #region 存储的类结构集

        [StructLayout(LayoutKind.Sequential)]
        public class KeyBoardHookStruct
        {
            public int vkCode;//按键的虚拟键码。键盘上的每个按键对应一个虚拟键码
            public int scanCode;//硬件的扫描码
            public int flags;//按键消息的详细信息。是一些标识位的组合
            public int time;//时间
            public int dwExtraInfo;//扩展到按键消息的信息        
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }

        #endregion

        #region 函数声明
        //钩子委托：
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        //钩子安装：
        [DllImport("user32.dll")]
        public static extern int SetWindowsHookEx(
        int idHook, //钩子的类型，即它处理的消息类型，可以用整型变量表示 
        HookProc lpfn, //钩子发挥作用时的委托类型的回调函数（当钩子得到系统消息时调用这个函数）
        IntPtr hInstance, //应用程序实例的模块句柄 （要设置钩子的应用程序的句柄）
        int threadId //与安装的钩子子程相关联的线程的标识符（如果为0即为全局钩子）
        );

        //卸载钩子：
        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(
        int idHook //钩子的类型
        );

        //调用下一个钩子：
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(
        int idHook, //当前的钩子的类型，由SetWindowsHookEx函数返回
        int nCode,//传给钩子过程的事件代码
        IntPtr wParam,//传给钩子子程的值，其具体含义与钩子类型有关
        IntPtr lParam //传给钩子子程的值，其具体含义与钩子类型有关
        );
        
        #endregion

        #region 钩子必要变量
        //开始安装钩子：
        public static int hHookKey = 0;
        public static int hHookMouse = 0;
        static HookProc KeyboardHookProcedure;
        static HookProc KeyboardHookProceduremouse;//mouse
        private object Hoodlock = new object();//钩子线程锁
        #endregion

        /// <summary>
        /// 键盘钩子安装与卸载
        /// </summary>
        /// <param name="f">用户回掉函数</param>
        /// <param name="timer">用户定时函数对象</param>
        /// <param name="Keystart">用户启动记录模式</param>
        /// <param name="keyEnd">用户结束记录模式</param>
        public void HookKey_StartEnd(Function f) // 定义一个用来安装钩子的方法
        {
            if (hHookKey == 0)
            {
                //初始化回掉函数
                KeyboardHookProcedure = new HookProc((nCode, wParam, lParam) =>
                {
                    //获取键盘输入信息
                    KeyBoardHookStruct input = (KeyBoardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyBoardHookStruct));
                    
                    //按下按键,并且有按键状态，是控制的窗体
                    if (nCode >= 0 && ((int)wParam == WM_KEYDOWN || (int)wParam == WM_KEYUP))
                    {
                        #region 封装必要信息和执行用户函数
                        customerhandle = new CustomerHandle()
                        {
                            function = f,
                            KeysCode = input.vkCode,
                            MouseXY = -1,
                            state = ((int)wParam == WM_KEYDOWN ? KeyMouseState.Down : KeyMouseState.Up)
                        };
                        //while (t.ThreadState == ThreadState.Background) ;//防止记录顺序冲突，有可能影响性能

                        t = new Thread(new ParameterizedThreadStart(KeyMouseThread));
                        t.IsBackground = true;
                        t.ApartmentState = ApartmentState.STA;
                        t.Start((object)customerhandle);
                        #endregion
                    }
                    return CallNextHookEx(hHookMouse, nCode, wParam, lParam);
                });//给委托变量赋初值

                //安装钩子
                hHookKey = SetWindowsHookEx(
                13, //此钩子的类型为全局键盘钩子
                KeyboardHookProcedure,//钩子子程（委托变量）
                IntPtr.Zero,//GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName),//表示全局钩子
                0 //表示全局钩子
                );
            }
            else //卸载键盘钩子
            {
                UnhookWindowsHookEx(hHookKey); // 卸载钩子
                hHookKey = 0;
            }
        }

        /// <summary>
        /// 鼠标钩子安装与卸载
        /// </summary>
        /// <param name="f">用户回掉函数</param>
        /// <param name="timer">用户定时函数对象</param>
        /// <param name="mouse"></param>
        public void HookMouse_StartEnd(Function f) // 定义一个用来安装钩子的方法
        {
            if (hHookMouse == 0)
            {
                //初始化回掉函数
                KeyboardHookProceduremouse = new HookProc((nCode, wParam, lParam) =>
                {
                    //获得到鼠标的信息
                    MouseHookStruct input = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));
                    // 假设正常执行而且用户要监听鼠标的消息
                    if ((nCode >= 0) && ((int)wParam == WM_MOUSEMOVE
                        || (int)wParam == WM_LBUTTONDOWN || (int)wParam == WM_RBUTTONDOWN || (int)wParam == WM_MBUTTONDOWN
                        || (int)wParam == WM_LBUTTONUP || (int)wParam == WM_RBUTTONUP || (int)wParam == WM_MBUTTONUP))
                    {
                        KeyMouseState Mousestate = KeyMouseState.none;
                        MouseButtons button = MouseButtons.None;
                        int clickCount = 0;
                        #region 键值状态的判断与封装
                        switch ((int)wParam)
                        {
                            case WM_MOUSEMOVE:
                                button = MouseButtons.None;
                                Mousestate = KeyMouseState.Move;
                                clickCount = 0;
                                break;
                            case WM_LBUTTONDOWN:
                                button = MouseButtons.Left;
                                Mousestate = KeyMouseState.Down;
                                clickCount = 1;
                                break;
                            case WM_LBUTTONUP:
                                button = MouseButtons.Left;
                                Mousestate = KeyMouseState.Up;
                                clickCount = 1;
                                break;
                            case WM_LBUTTONDBLCLK:
                                button = MouseButtons.Left;
                                Mousestate = KeyMouseState.Double;
                                clickCount = 2;
                                break;
                            case WM_RBUTTONDOWN:
                                button = MouseButtons.Right;
                                Mousestate = KeyMouseState.Down;
                                clickCount = 1;
                                break;
                            case WM_RBUTTONUP:
                                button = MouseButtons.Right;
                                Mousestate = KeyMouseState.Up;
                                clickCount = 1;
                                break;
                            case WM_RBUTTONDBLCLK:
                                button = MouseButtons.Right;
                                Mousestate = KeyMouseState.Double;
                                clickCount = 2;
                                break;
                        }
                        #endregion
                        MouseEventArgs e = new MouseEventArgs(button, clickCount, input.pt.x, input.pt.y, 0);

                        // 假设想要限制鼠标在屏幕中的移动区域能够在此处设置
                        // 后期须要考虑实际的x、y的容差
                        if (!Screen.PrimaryScreen.Bounds.Contains(e.X, e.Y))
                        {
                            //return 1;
                        }

                        #region 封装必要信息和执行用户函数
                        customerhandle = new CustomerHandle()
                        {
                            function = f,
                            KeysCode = Mousestate == KeyMouseState.Move ? -1 : (int)button,
                            MouseXY = (e.Y << 16) | e.X,
                            state = Mousestate
                        };
                        t = new Thread(new ParameterizedThreadStart(KeyMouseThread));
                        t.IsBackground = true;
                        t.Start((object)customerhandle);
                        #endregion
                    }

                    return CallNextHookEx(hHookMouse, nCode, wParam, lParam);
                });//给委托变量赋初值

                //安装钩子
                hHookMouse = SetWindowsHookEx(
                14, //此钩子的类型为全局键盘钩子
                KeyboardHookProceduremouse,//钩子子程（委托变量）
                IntPtr.Zero,//GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName),//表示全局钩子
                0 //表示全局钩子
                );
            }
            else //卸载键盘钩子
            {
                UnhookWindowsHookEx(hHookMouse); // 卸载钩子
                hHookMouse = 0;
            }
        }

        /// <summary>
        /// 客户运行函数线程（防止操作期间堵塞系统全局线程）
        /// </summary>
        /// <param name="obj"></param>
        private void KeyMouseThread(object obj)
        {
            try
            {
                CustomerHandle customerhandle = (CustomerHandle)obj;
                customerhandle.function(customerhandle.KeysCode, customerhandle.MouseXY, customerhandle.state);
            }
            catch (Exception ex)
            {
                MessageBox.Show("记录" + (customerhandle.MouseXY == -1 ? "键盘" : "鼠标")
                    + ((Keys)customerhandle.KeysCode) + "失败！状态为：" + customerhandle.state + "！错误信息为：" + ex.ToString());
            }

        }
    }
}
