using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
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
    internal static class 输出到记事本
    {
        [DllImport("User32.DLL")]
        public static extern int SeedMessage(IntPtr hWnd, uint Msg, int wParam, string IParam);
        [DllImport("User32.DLL")]
        public static extern int FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string IpszClass, string IpszWindow);
        public const uint WM_SETTEXT = 0x000C;
        static Process vProcess = null;
        static string STR = "";
       

        public static void print___NotePad<T>(this T t,string name)
        {
            var str=t==null ? "null" : t.ToString();
            STR+=name+"="+ str+"\n";
            vProcess ??= Process.Start("notepad.exe");
            while (vProcess.MainWindowHandle == IntPtr.Zero) { vProcess.Refresh(); }
            IntPtr vHandle = (IntPtr)FindWindowEx(vProcess.MainWindowHandle, IntPtr.Zero, "Edit", null);
            SeedMessage(vHandle, WM_SETTEXT, 0, STR);


        }
    } }
