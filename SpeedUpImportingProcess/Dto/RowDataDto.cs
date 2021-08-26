using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedUpImportingProcess.Dto
{
    public record RowDataDto : IFormattable
    {
        public int Index { get; set; }
        public FieldDataDto[] Fields { get; set; }
        public FileDataDto File { get; internal set; }

        public override string ToString()
        {
            return ToString(string.Empty, null);
        }
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Join("\t", this.Fields.Select(c => c.Value));
        }
    }
    [DebuggerDisplay("{ColumnIndex}/ {ColumnName}: {Value}")]
    public class FieldDataDto
    {
        public int ColumnIndex { get; set; }
        public string ColumnName { get; set; }
        public string Value { get; set; }
        public string SQLColumnName => string.IsNullOrEmpty(ColumnName) ? ColumnIndex.ToString() : ColumnName;
    }
}
