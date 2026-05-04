using System;
using System.Collections.Generic;
using System.Text;

namespace Downloader
{
    public class SessionData
    {
        public string Active { get; set; } = "";       
        public List<string> Saved { get; set; } = new(); 
    }
}
