using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiddleText.Backend
{
    public class ProcessFileResult
    {
        public string OriginalFilePath { get; set; }
        public string OutputFilePath { get; set; }
        public Exception Exception { get; set; }
        public TimeSpan Duration { get; set; }

        public Dictionary<LineAction, int> LineChangeCounts { get; set; }
        
        public bool Success
        {
            get
            {
                return Exception == null;
            }
        }

        public long OriginalLineNumberCount
        {
            get
            {
                return LineChangeCounts.Values.Sum();
            }
        }

        public long NewLineNumberCount
        {
            get
            {
                return OriginalLineNumberCount - LineChangeCounts[LineAction.Delete];
            }
        }

        public ProcessFileResult()
        {
            LineChangeCounts = ((LineAction[])(Enum.GetValues(typeof(LineAction)))).ToDictionary(a => a, a => 0);
        }
    }
}
