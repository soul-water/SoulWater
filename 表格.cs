using Autodesk.AutoCAD.Runtime;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using Exception = System.Exception;
using Word = Microsoft.Office.Interop.Word;

namespace SoulWater
{
    internal class 表格T
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
    internal class 表格
    {
        [CommandMethod("Extex")]
        public static void Extex()
        {
            List<List<string>> list =
            [
                ["A", "B", "C", "A"],
                ["D", "E", "F"],
                ["G", "H",],
            ];
            输出到Excel(list);
        }

        [CommandMethod("Extex2")]
        public static void Extex2()
        {
            List<List<string>> list =
            [
                ["1", "2", "3", "4"],
                ["5", "6", "7"],
                ["8", "9",],
            ];
            输出到打开的Excel(list);
        }

        /// <summary>
        /// 判断是否打开了一个excel程序
        /// </summary>
        /// <returns></returns>
        public static bool Ex是否打开()
        {
            try
            {
                Excel.Application excelApp = (Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
            }
            catch (Exception)
            {
                // 获取对象失败，表示Excel程序未打开
                Console.WriteLine("Excel程序未打开。");
                return false;
            }
            return true;

        }
        public static void 输出到Excel(List<List<String>> strings)
        {
            var excelApp = new Excel.Application
            {
                // Make the object visible.
                Visible = true
            };
            excelApp.Workbooks.Add();

            // This example uses a single workSheet. The explicit type casting is
            // removed in a later procedure.
            Excel._Worksheet workSheet = (Excel.Worksheet)excelApp.ActiveSheet;
            // Establish column headings in cells A1 and B1.


            //workSheet.Cells[1, "A"] = "ID Number";
            //workSheet.Cells[1, "B"] = "Current Balance";
            for (int i = 0; i < strings.Count; i++)
            {
                for (int j = 0; j < strings[i].Count; j++)
                {
                    workSheet.Cells[i + 1, j + 1] = strings[i][j];
                }
            }
            workSheet.Columns.EntireColumn.AutoFit();
        }
        public static void 输出到打开的Excel(List<List<String>> strings)
        {
            if (!Ex是否打开())
            {
                CAD.Ed.WriteMessage("没打开excel");
                return;
            };
            // 获取已经打开的 Excel 应用程序对象
            Excel.Application excelApp = (Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");

            // 获取活动工作簿
            Excel.Workbook workbook = excelApp.ActiveWorkbook;

            // 选择要操作的工作表
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.ActiveSheet;

            // 获取已使用的区域
            Excel.Range usedRange = worksheet.UsedRange;

            int rowCount = usedRange.Rows.Count;
            //int columnCount = usedRange.Columns.Count;
            for (int i = 0; i < strings.Count; i++)
            {
                for (int j = 0; j < strings[i].Count; j++)
                {
                    worksheet.Cells[i + rowCount + 1, j + 1] = strings[i][j];
                }
            }
            worksheet.Columns.EntireColumn.AutoFit();
        }

    }
}
