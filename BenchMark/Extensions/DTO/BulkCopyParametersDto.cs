using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedUpImportingProcess.BenchMark.Extensions.DTO
{
    public record BulkCopyParametersDto
    {
        public BulkCopyTempTableDto[] TempTables { get; init; }
        public string FinalizeProcedure { get; init; }
        public object FinalizeProcedureParameter { get; init; }
        public BulkCopyOption Option { get; init; }
    }
    public record BulkCopyTempTableDto
    {
        public string TempTableName { get; init; }
        public string CreationScript { get; init; }
        public IEnumerable<object> Data { get; init; }
        public DataTable DataTableData { get; init; }
    }
    public record BulkCopyOption
    {
    }
}
