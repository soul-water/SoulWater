﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

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
        [CommandMethod("Exce")]
        public static void Exce()
        {
            var bankAccounts = new List<Account> {
                 new Account {
                  ID = 345678,
                  Balance = 541.27
                            },
                  new Account {
                  ID = 1230221,
                  Balance = -127.44
                }
            };
            DisplayInExcel(bankAccounts);


        }
        [CommandMethod("Exce2")]
        public static void Exce2()
        {
            // 获取已经打开的 Excel 应用程序对象
            Excel.Application excelApp = (Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");

            // 获取活动工作簿
            Excel.Workbook workbook = excelApp.ActiveWorkbook;

            // 选择要操作的工作表
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.ActiveSheet;

            // 找到要插入数据的单元格
            Excel.Range cell = (Excel.Range)worksheet.Cells[5, 1];

            // 使用 Range 对象的 Value 属性将数据写入单元格
            cell.Value = "Hello, World!";
        }
        static void DisplayInExcel(IEnumerable<Account> accounts)
        {
            var excelApp = new Excel.Application
            {
                // Make the object visible.
                Visible = true
            };

            // Create a new, empty workbook and add it to the collection returned
            // by property Workbooks. The new workbook becomes the active workbook.
            // Add has an optional parameter for specifying a particular template.
            // Because no argument is sent in this example, Add creates a new workbook.
            excelApp.Workbooks.Add();

            // This example uses a single workSheet. The explicit type casting is
            // removed in a later procedure.
            Excel._Worksheet workSheet = (Excel.Worksheet)excelApp.ActiveSheet;
            // Establish column headings in cells A1 and B1.


            workSheet.Cells[1, "A"] = "ID Number";
            workSheet.Cells[1, "B"] = "Current Balance";

            var row = 1;
            foreach (var acct in accounts)
            {
                row++;
                workSheet.Cells[row, "A"] = acct.ID;
                workSheet.Cells[row, "B"] = acct.Balance;
            }

            workSheet.Columns.EntireColumn.AutoFit();
            //workSheet.Columns[1].AutoFit();
            //workSheet.Columns[2].AutoFit();



        }
        public class Account
        {
            public int ID { get; set; }
            public double Balance { get; set; }
        }
        static void CreateIconInWordDoc()
        {
            var wordApp = new Word.Application
            {
                Visible = true
            };

            // The Add method has four reference parameters, all of which are
            // optional. Visual C# allows you to omit arguments for them if
            // the default values are what you want.
            wordApp.Documents.Add();

            // PasteSpecial has seven reference parameters, all of which are
            // optional. This example uses named arguments to specify values
            // for two of the parameters. Although these are reference
            // parameters, you do not need to use the ref keyword, or to create
            // variables to send in as arguments. You can send the values directly.
            wordApp.Selection.PasteSpecial(Link: true, DisplayAsIcon: true);
        }


    }

}
