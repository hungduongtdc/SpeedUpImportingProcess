using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedUpImportingProcess.BenchMark.Extensions.DTO
{
    public class FileDataDto
    {
        public int FileId { get; set; }
        public string FileFullName { get; set; }
        public string FileName => string.IsNullOrEmpty(FileFullName) ? string.Empty : Path.GetFileName(FileFullName);
    }
}
