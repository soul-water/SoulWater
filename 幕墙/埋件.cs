using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulWater.幕墙
{
    class 命令
    {

    }
    class 埋件图
    {
        class 埋件布置图
        {
            internal BlockReference 埋件布置 {  get; set; }
            //internal BlockTableRecord BlockTb { get; }
            
            
            internal Dictionary<string, int> MJ { get; set; }
            internal int count { get; set; }
        }
        class 埋件剖面
        {
            internal List<BlockReference> Blocks { get; set; }
            internal string MJPM { get; set; }
            internal Dictionary<string, int> MJ { get; set; }
            internal int count { get; set; }
        }



    }
}
