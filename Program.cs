#region 文档说明
/* ****************************************************************************************************
 * 文档作者：无闻
 * 创建日期：2013 年 10 月 16 日
 * 文档用途：七牛云盘程序主入口，用于必要文件检查等操作
 * -----------------------------------------------------------------------------------------------------
 * 修改记录：
 * -----------------------------------------------------------------------------------------------------
 * 参考文献：
 * *****************************************************************************************************/
#endregion

#region 命名空间引用
using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

using CharmCommonMethod;
using CharmControlLibrary;

#endregion

namespace QiNiuDrive
{
    // 程序主入口
    static class Program
    {
        #region 常量
        private const string APP_NAME = "七牛云盘"; // 应用程序名称
        #endregion

        #region 方法
        // 应用程序的主入口点。
        [STAThread]
        static void Main()
        {
            // 检查相同进程是否正在运行
            if (CheckSameProcess())
                return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMain());
        }

        // 检查相同进程是否正在运行
        // 返回：true=有相同进程; false=没有相同进程
        public static bool CheckSameProcess()
        {
            // 判断是否存在禁止检查标识
            if (File.Exists("Data\\UAC.SIGN"))
            {
                File.Delete("Data\\UAC.SIGN");
                // 创建UAC模式标识
                FileStream fs = File.Create("Data\\UACMODE.SIGN");
                fs.Dispose();
            }
            else
            {
                // 获取同名进程数量
                Process[] procs = Process.GetProcessesByName(APP_NAME);
                // 判断返回的进程数组长度，如果数组长度大于1则说明相同程序已经启动
                if (procs.Length <= 1) return false;

                // 判断程序启动模式
                if (File.Exists("Data\\UACMODE.SIGN"))
                {
                    // 相同进程采用UAC模式运行
                    CharmMessageBox msgbox = new CharmMessageBox();
                    msgbox.Show("尊敬的用户您好：\n" +
                                "欢迎您使用 " + APP_NAME + "！\n\n" +
                                "由于您的系统开启了 UAC 安全控制，" +
                                "程序无法进行相关操作，因此您需要手动恢复已打开的程序窗口！\n\n" +
                                "感谢您对 " + APP_NAME + " 的支持与理解！",
                        "温馨提示");
                }
                else
                {
                    Process currentProc = Process.GetCurrentProcess();
                    // 对比进程ID，排除非自身进程，找到已经启动的主程序，并显示主窗口
                    foreach (Process proc in procs)
                    {
                        if (proc.Id == currentProc.Id) continue;
                        IntPtr hwnd = APIOperation.GetWindowHandle(APP_NAME);
                        APIOperation.ShowWindowForeground(hwnd);
                    }
                }
                // 检测到正在运行的相同进程
                return true;
            }

            // 没有检测到正在运行的相同进程
            return false;
        }
        #endregion
    }
}
