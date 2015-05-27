using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiddleText.Backend
{
    public class TextFiddler
    {
        public TextFiddlerConfig Config { get; private set; }

        public TextFiddler(TextFiddlerConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            Config = config;
        }
    }
}
