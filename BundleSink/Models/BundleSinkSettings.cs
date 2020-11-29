using System;

namespace BundleSink.Models
{
    public class BundleSinkSettings {
        public string PublicOutputPath { get; set; }
        public bool AppendVersion { get; set; }
        public bool PrintAllAttributes { get; set; }
    }
}