using System;

namespace BundleSink.Models
{
    public class BundleSinkSettings {
        public string PublicOutputPath { get; set; }
        public bool AppendVersion { get; set; }
        public bool PrintAllAttributes { get; set; }
        public bool PrintComments { get; set; }
        public bool CheckIntegrity { get; set; }
        public bool UsePlainIOptions { get; set; }
        public bool RewriteOutput { get; set; }
    }
}