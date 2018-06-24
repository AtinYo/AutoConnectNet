using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.IO;
namespace AutoConnectNet
{
    class DLLWrapper
    {
        //导入dll
        [DllImport("wininet.dll", EntryPoint = "InternetGetConnectedState")]
        //判断网络状况的方法,返回值true为连接，false为未连接
        public extern static bool InternetGetConnectedState(out int conState, int reder);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }

    class Program
    {
        static int n = 1;
        static int failedToConnectTimes = 0;//已经失败的重连次数
        static int MaxConnectTimes = 0;//最大重连次数
        static void Main(string[] args)
        {
            HideWindow();
            MaxConnectTimes = 10;
            string filepath = System.AppDomain.CurrentDomain.BaseDirectory + "bat\\connect.bat";
            while (true)
            {
                Thread.Sleep(500);
                if(!DLLWrapper.InternetGetConnectedState(out n, 0))
                {
                    if (failedToConnectTimes <= MaxConnectTimes)
                        ConnectNet(filepath);
                    else
                    {
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 通过bat重连
        /// </summary>
        /// <param name="filepath"></param>
        static void ConnectNet(string filepath)
        {
            Process proc = null;
            try
            {
                proc = new Process();
                proc.StartInfo.FileName = filepath;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();
                bool result = proc.WaitForExit(10000);//等待10s
                if(!result || !DLLWrapper.InternetGetConnectedState(out n, 0))
                {
                    if (!result)
                    {
                        proc.Close();
                    }
                    failedToConnectTimes++;
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
            }
        }

        static void HideWindow()
        {
            Console.Title = "AutoConnectNet";
            IntPtr intptr = DLLWrapper.FindWindow("ConsoleWindowClass", "AutoConnectNet");
            if (intptr != IntPtr.Zero)
            {
                DLLWrapper.ShowWindow(intptr, 0);//隐藏这个窗口
            }
        }
    }
}
