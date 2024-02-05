using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoulWater
{
    internal static class 调试
    {
        public static void 暂停()
        {
            var pr = CAD.Ed.GetInteger("暂停\n");
            if (pr.Status != PromptStatus.OK)
            {
                if (pr.Value == 1)
                {
                    throw new Exception("退出\n");
                }
            }
        }
        public static void 暂停(string str)
        {
            var pr = CAD.Ed.GetInteger("暂停" + str + "\n");
            if (pr.Status != PromptStatus.OK)
            {
                if (pr.Value == 1)
                {
                    throw new Exception("退出\n");
                }
            }
        }
        public static void 输出到cad<T>(this T t)
        {
            if (t == null)
            {
                CAD.Ed.WriteMessage("\nnull");
            }
            else
            {
                CAD.Ed.WriteMessage("\n" + t.ToString());
            }

        }
    }
    internal static class PN
    {
        const int WM_SETTEXT = 0x000C;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        public static void print___NotePad<T>(this T t, string name)
        {
            Process notepadProcess = Process.Start("notepad.exe");

            // 等待记事本窗口加载完毕
            Thread.Sleep(1000);

            // 查找记事本编辑窗口句柄
            IntPtr mainHandle = FindWindow("Notepad", null);
            IntPtr editHandle = FindWindowEx(mainHandle, IntPtr.Zero, "RichEditD2DPT", null);

            // 向编辑窗口发送文本消息
            SendMessage(editHandle, WM_SETTEXT, 0, "Hello, World!");

            // 等待一段时间，以便查看结果
            Thread.Sleep(2000);

            // 关闭记事本进程
            notepadProcess.CloseMainWindow();

        }




        //[DllImport("User32.DLL")]
        //public static extern int SeedMessage(IntPtr hWnd, uint Msg, int wParam, string IParam);
        //[DllImport("User32.DLL")]
        //public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string IpszClass, string IpszWindow);
        //public const uint WM_SETTEXT = 0x000C;
        //static Process vProcess = null;
        //static string STR = "";


        //public static void print___NotePad<T>(this T t,string name)
        //{
        //    var str = t == null ? "null" : t.ToString();
        //    STR += name + "=" + str + "\n";
        //    if(vProcess==null) { vProcess = Process.Start("notepad.exe"); }
        //    //vProcess ??= Process.Start("notepad.exe");
        //    while (vProcess.MainWindowHandle == IntPtr.Zero) { vProcess.Refresh(); }
        //    IntPtr vHandle = (IntPtr)FindWindowEx(vProcess.MainWindowHandle, IntPtr.Zero, "Edit", null);
        //    SeedMessage(vHandle, WM_SETTEXT, 0, STR);


        //}
    }
}
