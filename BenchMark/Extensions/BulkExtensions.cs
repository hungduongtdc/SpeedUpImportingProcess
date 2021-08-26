using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SpeedUpImportingProcess.BenchMark.Extensions.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpeedUpImportingProcess.BenchMark.Extensions
{
    public static class BulkExtensions
    {
        public static Task<T[]> BulkCopyAsync<T>(this DbContext context, BulkCopyParametersDto bulkCopyParametersDto) where T : new()
        {
            return context.Database.GetDbConnection().ConnectionString.BulkCopyAsync<T>(bulkCopyParametersDto);
        }

        public static async Task<T[]> BulkCopyAsync<T>(this string connectionString, BulkCopyParametersDto bulkCopyParametersDto) where T : new()
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            using var cmd = new SqlCommand(string.Empty, connection) { CommandTimeout = 300 };
            using var bulkCopy = new SqlBulkCopy(connection)
            {
                BulkCopyTimeout = 300
            };
            foreach (var tempTable in bulkCopyParametersDto.TempTables)
            {
                cmd.CommandText = tempTable.CreationScript;
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
                bulkCopy.DestinationTableName = tempTable.TempTableName;

                if (tempTable?.DataTableData != null)
                {
                    // Write from the source to the destination.
                    await bulkCopy.WriteToServerAsync(tempTable.DataTableData);
                }
                if (tempTable?.Data?.Any() == true)
                {
                    using DataTable dataTable = MakeDataTableFromListObject(tempTable.Data);
                    // Write from the source to the destination.
                    await bulkCopy.WriteToServerAsync(dataTable);
                }
            }

            cmd.CommandText = bulkCopyParametersDto.FinalizeProcedure;
            cmd.CommandType = CommandType.StoredProcedure;
            AssignParameter(cmd, bulkCopyParametersDto.FinalizeProcedureParameter);
            using var adap = new SqlDataAdapter(cmd);
            using var finalizeResult = new DataSet();
            adap.Fill(finalizeResult);
            if (finalizeResult.Tables.Count > 0)
            {
                return ConvertTableToArrayOfObject<T>(finalizeResult.Tables[0]);
            }
            return Array.Empty<T>();
            //return finalizeResult;
        }

        private static T[] ConvertTableToArrayOfObject<T>(DataTable dataTable) where T : new()
        {
            if (dataTable.Rows.Count == 0)
            {
                return Array.Empty<T>();
            }
            var mapping = GetDataTableToObjectMapping(dataTable, typeof(T));
            return dataTable.Rows.Cast<DataRow>().Select(c => BindDataRowToObject<T>(c, mapping)).ToArray();
        }

        private static List<(PropertyInfo PropertyInfo, string ColumnName)> GetDataTableToObjectMapping(DataTable dataTable, Type type)
        {
            return (from prop in type.GetProperties()
                    join column in dataTable.Columns.Cast<DataColumn>()
                    on prop.Name.ToLower() equals column.ColumnName.ToLower()
                    select (prop, column.ColumnName))
                 .ToList();
        }

        private static T BindDataRowToObject<T>(DataRow dataRow, List<(PropertyInfo PropertyInfo, string ColumnName)> mapping) where T : new()
        {
            var theObject = new T();
            var matchProps = mapping.Select(c => new
            {
                c.PropertyInfo,
                value = dataRow[c.ColumnName]
            }).ToList();
            foreach (var matchProp in matchProps)
            {
                if (matchProp.value != DBNull.Value && matchProp.value != null)
                {
                    if (matchProp.PropertyInfo.PropertyType.IsEnum)
                    {
                        if (Enum.TryParse(matchProp.PropertyInfo.PropertyType, matchProp.value?.ToString(), out object value))
                        {
                            matchProp.PropertyInfo.SetValue(theObject, Convert.ChangeType(value, matchProp.PropertyInfo.PropertyType));
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                        matchProp.PropertyInfo.SetValue(theObject, Convert.ChangeType(matchProp.value, matchProp.PropertyInfo.PropertyType));
                    }
                }
            }
            return theObject;
        }

        public static void AssignParameter(this SqlCommand cmd, object inputParams)
        {
            if (inputParams is Dictionary<string, object> dicParam)
            {
                foreach (var item in dicParam)
                {
                    cmd.Parameters.Add(new SqlParameter($"@{item.Key}", item.Value));
                }
            }
            else
            {
                inputParams?.GetType().GetProperties().Select(c => new { paramName = $"@{c.Name}", val = c.GetValue(inputParams) })
                 .ToList().ForEach(param =>
                 {
                     cmd.Parameters.Add(new SqlParameter(param.paramName, param.val));
                 });
            }
        }

        private static DataTable MakeDataTableFromListObject<T>(IEnumerable<T> objectsToBeBulked)
        {
            var newDataTable = new DataTable();
            bool isFirst = true;

            PropertyInfo[] propInfors = new PropertyInfo[0];
            foreach (var entity in objectsToBeBulked)
            {
                if (isFirst)
                {
                    foreach (var property in entity.GetType().GetProperties())
                    {
                        var propertyType = property.PropertyType;
                        if (Nullable.GetUnderlyingType(property.PropertyType) is Type underlineType)
                        {
                            propertyType = underlineType;
                        }
                        newDataTable.Columns.Add(property.Name, propertyType);
                    }
                    propInfors = (from prop in entity.GetType().GetProperties()
                                  join b in newDataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName)
                                  on prop.Name.ToLower() equals b.ToLower()
                                  select prop).ToArray();
                }
                isFirst = false;
                DataRow newEntry = newDataTable.NewRow();
                foreach (var prop in propInfors)
                {
                    newEntry[prop.Name] = prop.GetValue(entity) ?? DBNull.Value;
                }
                newDataTable.Rows.Add(newEntry);
            }
            return newDataTable;
        }
    }
}
