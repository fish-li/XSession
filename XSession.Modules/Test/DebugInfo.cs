﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XSession.Modules.Test
{
    public class DebugInfo
    {
        public DateTime Time { get; set; }

        public string Url { get; set; }

        public string SessionId { get; set; }

        public List<string> Items { get; set; }

        public long Size { get; set; }
    }
}
