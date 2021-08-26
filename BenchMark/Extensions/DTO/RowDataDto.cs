using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedUpImportingProcess.BenchMark.Extensions.DTO
{
    public record RowDataDto : IFormattable
    {
        public int Index { get; set; }
        public FieldDataDto[] Fields { get; set; }

        public override string ToString()
        {
            return ToString(string.Empty, null);
        }
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Join("\t", this.Fields.Select(c => c.Value));
        }
    }
}
