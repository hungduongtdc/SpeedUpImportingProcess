using CsvHelper.Configuration.Attributes;
using System;

namespace SpeedUpImportingProcess.Dto
{
    public class AttendanceCsvDto
    {
        [Index(1)]
        public string Id { get; set; }
        [Index(4)]
        public DateTime Att_Date { get; set; }

        [Index(7)]
        public int Attendance_CodeID { get; set; }

    }
}
