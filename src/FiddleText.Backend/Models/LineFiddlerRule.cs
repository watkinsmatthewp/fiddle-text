using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FiddleText.Backend
{
    public class LineFiddlerRule
    {
        public LineAction Action { get; set; }
        public Regex PositiveMatchPattern { get; set; }
        public Regex NegativeMatchPattern { get; set; }
        public string Replacement { get; set; }
    }
}
