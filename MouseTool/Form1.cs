using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MouseTool
{
    public partial class Form1 : Form
    {
        private bool bStopMsg = false; 
        private int hHook = 0;
        private GCHandle gc;


        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(int hHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool UnhookWindowsHookEx(int hHook);

        [DllImport("user32.dll")]
        public static extern int SetWindowsHookEx(int idHook, HookProc hProc, IntPtr hMod, int dwThreadId);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        //移动鼠标 
        const int MOUSEEVENTF_MOVE = 0x0001;
        //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        //模拟鼠标右键按下 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //模拟鼠标右键抬起 
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //模拟鼠标中键按下 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //模拟鼠标中键抬起 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //标示是否采用绝对坐标 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000; 
        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            Stop();
        }

        void Start()
        {
            string s_seconds = textBox1.Text;
            decimal d_seconds = decimal.Parse(s_seconds);
            timer1.Interval = (int)(d_seconds * 1000);
            timer1.Start();

            lbTip.Text = string.Format(lbTip.Tag.ToString(), comboBox1.Text);
            SetEnabled(false);
        }
        void Stop()
        {
            timer1.Stop();
            lbTip.Text = string.Format("按[{0}]键启动", comboBox1.Text);

            SetEnabled(true);
        }

        void SetEnabled(bool enabled)
        {
            textBox1.Enabled = enabled;
            radioButton1.Enabled = enabled;
            radioButton2.Enabled = enabled;
            comboBox1.Enabled = enabled;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }
            else
            {
                mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
            }
        }

        public int MethodHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KeyInfoStruct struct2 = (KeyInfoStruct)Marshal.PtrToStructure(lParam, typeof(KeyInfoStruct));
                if ((wParam == ((IntPtr)0x100)) && (((Keys)struct2.vkCode).ToString() == this.comboBox1.Text))
                {
                    if (this.timer1.Enabled)
                    {
                        Stop();
                    }
                    else
                    {
                        Start();
                    }
                }
                if (this.bStopMsg)
                {
                    return 1;
                }
            }
            return CallNextHookEx(this.hHook, nCode, wParam, lParam);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyInfoStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (0 == this.hHook)
            {
                HookProc hProc = new HookProc(this.MethodHookProc);
                this.hHook = SetWindowsHookEx(13, hProc, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
                if (this.hHook == 0)
                {
                    MessageBox.Show("设置Hook失败");
                }
                else
                {
                    this.gc = GCHandle.Alloc(hProc);
                }
            }
            else if (UnhookWindowsHookEx(this.hHook))
            {
                this.hHook = 0;
                this.gc.Free();
            }
            else
            {
                MessageBox.Show("卸载失败");
            }

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Stop();
        }

    }
}
