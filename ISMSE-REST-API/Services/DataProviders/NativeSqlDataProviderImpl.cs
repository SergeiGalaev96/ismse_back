using ISMSE_REST_API.Contracts.Builders;
using ISMSE_REST_API.Contracts.DataProviders;
using ISMSE_REST_API.Extensions;
using ISMSE_REST_API.Models.Address;
using ISMSE_REST_API.Models.CustomExportModels;
using ISMSE_REST_API.Models.Enums;
using ISMSE_REST_API.Services.Builders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ISMSE_REST_API.Services.DataProviders
{
    public class NativeSqlDataProviderImpl : INativeSqlDataProvider
    {
        private readonly ITSqlQueryBuilder _queryBuilder;
        public NativeSqlDataProviderImpl(ITSqlQueryBuilder queryBuilder)
        {
            _queryBuilder = queryBuilder;
        }
        private static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        public List<CustomExportItem[]> FetchData(
            Enum[] state,
            DateTime startDate,
            DateTime endDate,
            Guid? msecId = null,
            int? regionId = null,
            int? districtId = null)
        {
            var model = new ConcurrentBag<CustomExportItem[]>();
            _semaphoreSlim.Wait();
            try
            {
                /*GetAddressRegions();
                GetAddressDistricts();*/
                var addressSoate = GetAddressSoate().GetAwaiter().GetResult();
                DataTable dt = new DataTable();
                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["cissaDataDbClone"].ConnectionString))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = _queryBuilder.BuildSql(state, startDate, endDate, msecId, regionId, districtId);
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 600000;

                        conn.Open();
                        dt.Load(cmd.ExecuteReader());
                        conn.Close();
                    }
                }
                var columns = new List<string>();

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    columns.Add(dt.Columns[i].ColumnName);
                }
                var dataRows = dt.AsEnumerable();
                Parallel.ForEach(dataRows, row =>
                {
                    var rowModel = new List<CustomExportItem>();
                    for (int i = 0; i < columns.Count; i++)
                    {
                        var colName = columns[i];
                        var val = row[i] is DBNull ? null : row[i];
                        var hasRef = colName.EndsWith("-ref");

                        if (hasRef && val != null && !string.IsNullOrEmpty(val.ToString()))
                        {
                            try
                            {
                                //PARSE REGION
                                if (colName == REGION_REF && val is string regionCode)
                                {
                                    var item = addressSoate.FirstOrDefault(x => x.RegionCode == regionCode);
                                    if (item != null)
                                        val = item.RegionName;
                                }//PARSE DISTRICT
                                else if (colName == DISTRICT_REF && val is string districtCode)
                                {
                                    var item = addressSoate.FirstOrDefault(x => x.DistrictCode.Trim().ToLower() == districtCode.Trim().ToLower());
                                    if (item != null)
                                        val = item.DistrictName;
                                }
                                //PARSE DJAMOAT
                                else if (colName == DJAMOAT_REF && val is string djamoatCode)
                                {
                                    var item = addressSoate.FirstOrDefault(x => x.DjamoatCode.Trim().ToLower() == djamoatCode.Trim().ToLower());
                                    if (item != null)
                                        val = item.DjamoatName;
                                }
                                //PARSE VILLAGE
                                else if (colName == VILLAGE_REF && val is string villageCode)
                                {
                                    var item = addressSoate.FirstOrDefault(x => x.VillageCode.Trim().ToLower() == villageCode.Trim().ToLower());
                                    if (item != null)
                                        val = item.VillageName;
                                }
                            }
                            catch (Exception)
                            {

                                throw;
                            }
                        }

                        var hasByte = colName.EndsWith(BYTE_ANCHOR_SUFFIX);
                        if(hasByte && val != null && val is byte[] byteVal)
                        {
                            var normalText = Encoding.UTF8.GetString(byteVal);
                            rowModel.Add(new CustomExportItem { Key = colName, Value = normalText });
                            continue;
                        }
                        rowModel.Add(new CustomExportItem { Key = colName, Value = val });
                    }
                    //rowModel.Add(new CustomExportItem { Key = "Статус", Value = $"{string.Join(",", state.SelectMany(x => x.GetValueText()))}" });
                    model.Add(rowModel.ToArray());
                });
                dt.Dispose();

                return model.ToList();
            }
            catch (Exception e)
            {
                //TODO: Override exceptions
                throw;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private const string REGION_REF = "Область-ref";
        private const string DISTRICT_REF = "Район-ref";
        private const string DJAMOAT_REF = "Джамоат-ref";
        private const string VILLAGE_REF = "Населенный пункт-ref";
        const string BYTE_ANCHOR_SUFFIX = "-byte";

        private static ImmutableList<addressSoateItemDTO> addressSoateItemDTOs = null;
        private static SemaphoreSlim _semaphoreSlimAddress = new SemaphoreSlim(1, 1);
        private async Task<ImmutableList<addressSoateItemDTO>> GetAddressSoate()
        {
            if (addressSoateItemDTOs != null) return addressSoateItemDTOs;
            await _semaphoreSlimAddress.WaitAsync();
            try
            {
                var items = new List<addressSoateItemDTO>();
                using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["cissaMetaDb"].ConnectionString))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = ADDRESS_SOATE_T_SQL;
                        cmd.CommandType = CommandType.Text;

                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            items = new List<addressSoateItemDTO>();
                            while (reader.Read())
                            {
                                items.Add(
                                    new addressSoateItemDTO
                                    {
                                        RegionCode = reader.GetValue(0).ToString(),
                                        RegionName = reader.GetString(1),
                                        DistrictCode = reader.GetValue(2).ToString(),
                                        DistrictName = reader.GetString(3),
                                        DjamoatCode = reader.IsDBNull(4) ? "" : reader.GetValue(4).ToString(),
                                        DjamoatName = reader.IsDBNull(5) ? "" : reader.GetValue(5).ToString(),
                                        VillageCode = reader.IsDBNull(6) ? "" : reader.GetValue(6).ToString(),
                                        VillageName = reader.IsDBNull(7) ? "" : reader.GetValue(7).ToString()
                                    }
                                    );
                            }
                        }
                        conn.Close();
                    }
                }
                addressSoateItemDTOs = ImmutableList.Create(items.ToArray());
                return addressSoateItemDTOs;
            }
            finally
            {
                _semaphoreSlimAddress.Release();
            }
        }
        private const string ADDRESS_SOATE_T_SQL = @"
SELECT [RegionCode]
      ,[RegionName]
      ,[DistrictCode]
      ,[DistrictName]
      ,[DjamoatCode]
      ,[DjamoatName]
      ,[VillageCode]
      ,[VillageName]
  FROM [msec-meta].[dbo].[address_soate]
";
    }
}