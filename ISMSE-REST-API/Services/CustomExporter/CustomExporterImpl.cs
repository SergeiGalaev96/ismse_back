using ISMSE_REST_API.Contracts.CustomExporter;
using ISMSE_REST_API.Contracts.DataProviders;
using ISMSE_REST_API.Models.CustomExportModels;
using ISMSE_REST_API.Models.Enums;
using ISMSE_REST_API.Services.DataProviders;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web;

namespace ISMSE_REST_API.Services.CustomExporter
{
    public class CustomExporterImpl : ICustomExporter
    {
        private readonly INativeSqlDataProvider _myDataProvider;
        public CustomExporterImpl(INativeSqlDataProvider myDataProvider)
        {
            _myDataProvider = myDataProvider;
        }
        public List<CustomExportItem[]> GetData(Enum[] state, DateTime startDate, DateTime endDate, Guid? msecId = null, int? regionId = null, int? districtId = null)
        {
            return _myDataProvider.FetchData(state, startDate, endDate, msecId, regionId, districtId);
        }
        public byte[] ConvertToFileInByteArray(List<CustomExportItem[]> data)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("List");
                int headerColIndex = 1;
                int headerRowIndex = 1;
                if (data.Count > 0)
                {
                    foreach (var item in data.First())
                    {
                        sheet.Cells[headerRowIndex, headerColIndex].Value = item.Key;
                        sheet.Cells[headerRowIndex, headerColIndex].AutoFitColumns();
                        sheet.Cells[headerRowIndex, headerColIndex].Style.Font.Bold = true;
                        headerColIndex++;
                    }
                }
                int itemRowIndex = headerRowIndex+1;
                foreach (var dataItem in data)
                {
                    int itemColIndex = 1;
                    foreach (var col in dataItem)
                    {
                        var cell = sheet.Cells[itemRowIndex, itemColIndex];

                        if(col.Value is DateTime date)
                        {
                            cell.Style.Numberformat.Format = "dd.mm.yyyy";
                        }

                        if(col.Value is bool val)
                        {
                            col.Value = val ? "ДА" : "НЕТ";
                        }

                        cell.Value = col.Value;
                        itemColIndex++;
                    }
                    itemRowIndex++;
                }
                package.Save();
                return package.GetAsByteArray();
            }
        }
    }
}