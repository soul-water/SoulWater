using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulWater
{
    internal class CAD
    {
        //将常用的文档管理器，当前文档、编辑器，数据库作为静态类的字段，方便使用
        /// <summary>
        /// 文档管理器
        /// </summary>
        public static DocumentCollection Dm { get { return Application.DocumentManager; } }
        /// <summary>
        /// 由文档管理器中得到的当前文档
        /// </summary>
        public static Document Doc { get { return Dm.MdiActiveDocument; } }
        /// <summary>
        /// 数据库
        /// </summary>
        public static Database Db { get { return Doc.Database; } }
        /// <summary>
        /// 编辑器
        /// </summary>
        public static Editor Ed { get { return Doc.Editor; } }
        /// <summary>
        /// 事务
        /// </summary>
        public static Transaction Tr { get { return Db.TransactionManager.StartTransaction(); } }
    }
    internal class 系统变量
    {
        /// <summary>
        /// 线性比例
        /// </summary>
        public static int 当标注比例
        {
            get
            {
                if (System.Convert.ToInt32(Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("DIMSCALE")) == 0)
                {
                    return 1;
                }
                else
                {
                    return System.Convert.ToInt32(Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("DIMSCALE"));
                }
            }
            set { if (value != 当标注比例) { Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("DIMSCALE", value); } }
        }
        /// <summary>
        /// 当前图层
        /// </summary>
        public static string 当前图层 { get { return System.Convert.ToString(Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("CLAYER")); } }
        /// <summary>
        /// 标注样式
        /// </summary>
        public static string 当前标注样式 { get { return System.Convert.ToString(Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("DIMSTYLE")); } }
        /// <summary>
        /// 文字样式
        /// </summary>
        public static string 当前文字样式 { get { return System.Convert.ToString(Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("TEXTSTYLE")); } }
        /// <summary>
        /// 捕捉
        /// </summary>
        public static int 当前捕捉
        {
            get { return System.Convert.ToInt32(Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("OSMODE")); }
            set
            {
                if (value != 当前捕捉) { Autodesk.AutoCAD.ApplicationServices.Application.SetSystemVariable("OSMODE", value); }
            }
        }

    }
}
