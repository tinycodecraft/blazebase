
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GovcoreBse.Shared.Tools;

public class NPOIHelper
{
    public static string ExtractZip(string inpf, string passw, string dir)
    {
        try
        {
            if (string.IsNullOrEmpty(dir))
            {
                dir = Path.GetDirectoryName(inpf);

            }
            else
            {
                inpf = Path.Combine(dir, inpf);
            }
            using (var ins = File.OpenRead(inpf))
            using (var zf = new ZipFile(ins))
            {
                if (!string.IsNullOrEmpty(passw))
                    zf.Password = passw;
                foreach (ZipEntry entry in zf)
                {
                    if (!entry.IsFile)
                    {
                        continue;
                    }
                    var entryname = entry.Name;
                    // to remove the folder from the entry:
                    //entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here
                    // to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    // Manipulate the output filename here as desired.
                    var outf = Path.Combine(dir, entryname);
                    // 4K is optimum
                    var buffer = new byte[4096];

                    // Unzip file in buffered chunks. This is just as fast as unpacking
                    // to a buffer the full size of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (var zipStream = zf.GetInputStream(entry))
                    using (Stream fsOutput = File.Create(outf))
                    {
                        StreamUtils.Copy(zipStream, fsOutput, buffer);
                    }

                }


            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
        return "";
    }
    public static string CreateZipWithPssd(string inpf, string outpf, string paasw, string dir = null)
    {
        try
        {
            var buffer = new byte[4096];
            if (!string.IsNullOrEmpty(dir))
            {
                inpf = Path.Combine(dir, inpf);
                outpf = Path.Combine(dir, outpf);
            }
            using (var fs = File.Create(outpf))
            using (var outs = new ZipOutputStream(fs))
            {
                outs.Password = paasw;
                using (var ins = File.OpenRead(inpf))
                {
                    var entry = new ZipEntry(Path.GetFileName(ZipEntry.CleanName(inpf)));
                    entry.Size = ins.Length;
                    outs.PutNextEntry(entry);

                    int count = ins.Read(buffer, 0, buffer.Length);
                    while (count > 0)
                    {
                        outs.Write(buffer, 0, count);
                        count = ins.Read(buffer, 0, buffer.Length);

                    }
                    fs.Flush();
                }

            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
        return "";
    }

    public static bool WriteDataTable(string filename, string targetfile, DataTable dt, bool isXSSFWorkbook = false)
    {
        try
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            using (FileStream ws = new FileStream(targetfile, FileMode.Create, FileAccess.Write))

            {
                IWorkbook workbook = isXSSFWorkbook ? (IWorkbook)new XSSFWorkbook(fs) : (IWorkbook)new HSSFWorkbook(fs);
                ISheet sheet = workbook.GetSheetAt(0);
                var headerrow = sheet.GetRow(0);
                if (headerrow == null)
                    headerrow = sheet.CreateRow(0);
                var nextrowpos = 1;

                var font = workbook.CreateFont();
                font.FontHeightInPoints = 11;
                font.FontName = "Calibri";
                font.IsBold = true;


                foreach (DataColumn c in dt.Columns)
                {
                    var headercell = headerrow.GetCell(dt.Columns.IndexOf(c));
                    if (headercell == null) headercell = headerrow.CreateCell(dt.Columns.IndexOf(c));
                    headercell.SetCellValue(c.Caption);
                    headercell.CellStyle = workbook.CreateCellStyle();
                    headercell.CellStyle.SetFont(font);

                }

                foreach (DataRow dtr in dt.Rows)
                {
                    var newrow = sheet.GetRow(nextrowpos) ?? sheet.CreateRow(nextrowpos);
                    nextrowpos++;

                    foreach (DataColumn c in dt.Columns)
                    {
                        var colindex = dt.Columns.IndexOf(c);
                        var newcell = newrow.GetCell(colindex) ?? newrow.CreateCell(colindex);
                        if (dtr[c] != null && dtr[c] != DBNull.Value)
                        {
                            if (HelperT.BaseType(c.DataType) == typeof(DateTime))
                            {
                                newcell.SetCellValue(((DateTime)dtr[c]).ToString("dd/MM/yyyy"));
                            }
                            else if (HelperT.BaseType(c.DataType) == typeof(int))
                            {
                                newcell.SetCellValue((int)dtr[c]);
                            }
                            else
                            {
                                newcell.SetCellValue(dtr[c]?.ToString());
                            }

                        }


                    }
                }


                //workbook.GetCreationHelper().CreateFormulaEvaluator().EvaluateAll();

                workbook.Write(ws);
                workbook.Close();

                return true;

            }
        }
        catch (Exception ex)
        {

        }
        return false;
    }

    public static bool WriteDataTable(string filename, string targetfile, Dictionary<int, DataTable> dts, bool isXSSFWorkbook = false, bool eval = true)
    {
        try
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            using (FileStream ws = new FileStream(targetfile, FileMode.Create, FileAccess.Write))

            {
                IWorkbook workbook = isXSSFWorkbook ? (IWorkbook)new XSSFWorkbook(fs) : (IWorkbook)new HSSFWorkbook(fs);
                ISheet sheet = workbook.GetSheetAt(0);
                foreach (var k in dts.Keys)
                {
                    int rx = k;
                    foreach (DataRow r in dts[k].Rows.OfType<DataRow>())
                    {
                        //int cx = 0;
                        rx = dts[k].Rows.IndexOf(r) + k;
                        foreach (DataColumn c in dts[k].Columns.OfType<DataColumn>())
                        {
                            var cx = dts[k].Columns.IndexOf(c);
                            var row = sheet.GetRow(rx);



                            if (r[c] != DBNull.Value)
                            {

                                if (row == null)
                                {
                                    row = sheet.CreateRow(rx);
                                }



                                string cellstr = c.DataType == typeof(string) ? (r.Field<string>(c) ?? "") : null;
                                var isDecimal = c.DataType == typeof(decimal);
                                decimal celldec = cellstr == null && isDecimal ? r.Field<decimal>(c) : -1;
                                int cellint = (cellstr == null && !isDecimal) ? r.Field<int>(c) : -1;

                                var cell = row.GetCell(cx);
                                if (cell == null)
                                    cell = row.CreateCell(cx);

                                //Please don't use continue statement to skip line
                                if (cellstr != null)
                                    cell.SetCellValue(cellstr);
                                else
                                {
                                    if (isDecimal)
                                    {

                                        cell.SetCellValue(Decimal.ToDouble(celldec));
                                    }
                                    else cell.SetCellValue(cellint);
                                }
                            }




                        }


                    }
                }
                if (eval)
                    workbook.GetCreationHelper().CreateFormulaEvaluator().EvaluateAll();

                workbook.Write(ws);
                workbook.Close();

                return true;

            }
        }
        catch (Exception ex)
        {

        }
        return false;
    }

}
