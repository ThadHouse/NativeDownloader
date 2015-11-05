using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NativeDownloader
{
    public class ConfigInfo
    {
        public string Version { get; set; }
        public string Site { get; set; }
        public string FileName { get; set; }
        public string OutputLocation { get; set; }
        public string MD5 { get; set; }

        public ConfigInfo(string version, string site, string filename, string outputLocation, string md5)
        {
            Version = version;
            Site = site;
            FileName = filename;
            OutputLocation = outputLocation;
            MD5 = md5;
        }
    }
}
