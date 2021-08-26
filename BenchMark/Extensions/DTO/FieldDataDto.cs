using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedUpImportingProcess.BenchMark.Extensions.DTO
{
    [DebuggerDisplay("{ColumnIndex}/ {ColumnName}: {Value}")]
    public class FieldDataDto
    {
        public int ColumnIndex { get; set; }
        public string ColumnName { get; set; }
        public string Value { get; set; }
        public string SQLColumnName => string.IsNullOrEmpty(ColumnName) ? ColumnIndex.ToString() : ColumnName;
    }
}
