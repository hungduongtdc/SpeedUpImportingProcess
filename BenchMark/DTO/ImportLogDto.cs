using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedUpImportingProcess.BenchMark.DTO
{
    public record ImportLogDto
    {
        public int LineIndex { get; init; }
        public string LogDetail { get; init; }
    }
}
