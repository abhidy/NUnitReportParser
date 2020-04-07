using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Core;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace ResultParser
{
    class GenerateReport
    {
        Excel.Application xlApp;
        Excel.Workbook xlWorkBook;
        Excel.Worksheet xlWorkSheet;
        int RowCount = 1;
        private static GenerateReport instance = null;

        private GenerateReport()
        {
        }
        public static GenerateReport Instance
        {
            get
            {

                if (instance == null)
                {
                    instance = new GenerateReport();
                }
                return instance;
            }
        }
        public void CreateNewExcelHandle(string Path)
        {
            this.xlApp = new Microsoft.Office.Interop.Excel.Application();
            if (xlApp != null)
            {
                object misValue = System.Reflection.Missing.Value;
                xlWorkBook = xlApp.Workbooks.Add(misValue);
                xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

                FileInfo fin = new FileInfo(Path);

                if (fin.Exists)
                {
                    File.Delete(Path);
                }
            }
        }
        public void WriteToExcel(string[] ArrayData)
        {
            for (int ColCount = 1; ColCount <= ArrayData.Length; ColCount++)
            {
                xlWorkSheet.Cells[RowCount, ColCount] = ArrayData[ColCount - 1];
            }
            RowCount++;
        }
        public void ClearExcelHandler(string path)
        {
            object misValue = System.Reflection.Missing.Value;

            xlWorkBook.SaveAs(path, Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorkSheet);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);
        }
    }
}