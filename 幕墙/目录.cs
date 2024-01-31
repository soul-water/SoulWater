using Autodesk.AutoCAD.DatabaseServices;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulWater.幕墙
{
    internal class 目录
    {

        public static void Zml()
        {
            string name = "图名编号";
            List<BlockReference> blocks= 选择.SelectAllEntities<BlockReference>();
            if(blocks.Count == 0 ) { return; }
            List<BlockReference> references = blocks.GetNameBlock(name);
            if( references.Count == 0 ) { CAD.Ed.WriteMessage("没有对应块");return; }
            foreach( BlockReference block in references )
            {

            }
            List<List<string>> list =
            [
                new List<string>() { "A", "B", "C", "123" },
                new List<string>() { "D", "E", "F" },
                new List<string>() { "G", "H", },
            ];

            Epplus表格.WriteToExcel(list);

        }




    }
    class Epplus表格
    {
        public static bool WriteToExcel(List<List<string>> strings)
        {
            if (strings is null)
            {
                return false;
            }
            int rowCount = strings.Count; // 获取行数
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;//指明非商业应用
            string sourceFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "浏览.xlsx");
            //string sourceFile = @"C:\Users\EDUTH\Desktop\浏览.xlsx";
            FileInfo newFile = new(sourceFile);
            using (ExcelPackage package = new(newFile))
            {
                ExcelWorksheet worksheet;
                if (newFile.Exists)
                {
                    worksheet = package.Workbook.Worksheets["目录"];
                }
                else
                {
                    worksheet = package.Workbook.Worksheets.Add("目录");
                }
                int startRow = worksheet.Dimension?.End.Row + 1 ?? 1;
                //int l = 0;
                for (int i = startRow,L=0; i < rowCount + startRow; i++, L++)
                {
                    List<string> rowValues = strings[L];
                    for (int j = 0; j < rowValues.Count; j++)
                    {
                        worksheet.Cells[i, j + 1].Value = rowValues[j];
                    }
                }
                package.Save();

            }

            return true;
        }
    }
    class Mic表格
    {

    }

}
