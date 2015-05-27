using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FiddleText.Backend
{
    class Program
    {
        static void Main(string[] args)
        {
            // Test
            TextFiddlerConfig config = new TextFiddlerConfig()
            {
                OverwriteOriginalFile = true
            };
            config.Rules.Add(new LineFiddlerRule() 
            { 
                Action = LineAction.Modify,
                PositiveMatchPattern = new Regex(@"(.+)SKU=(.+)\s>>(.+)"),
                Replacement = "$2"
            });
            TextFiddler fiddler = new TextFiddler(config);
            ProcessFileResult fileResult = fiddler.ProcessFile(@"C:\Users\matthew.watkins\Desktop\16a9fd6b-4bc0-4290-b901-4400f13158c4.log");
        }
    }
}
