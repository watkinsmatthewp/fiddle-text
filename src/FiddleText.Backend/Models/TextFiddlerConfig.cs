using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FiddleText.Backend
{
    public class TextFiddlerConfig
    {
        public bool RunMultiThreaded { get; set; }
        public bool OverwriteOriginalFile { get; set; }
        public string OutputDirectory { get; set; }
        public List<LineFiddlerRule> Rules { get; private set; }

        public TextFiddlerConfig()
        {
            Rules = new List<LineFiddlerRule>();
        }
    }
}
