using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NativeDownloader
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No file specified to load.");
                return 1;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File specifed does not exist.");
                return 1;
            }

            //Load our json
            List<ConfigInfo> configInfo = JsonConvert.DeserializeObject<List<ConfigInfo>>(File.ReadAllText(args[0]));

            Task<ConfigInfo[]> checkMD5 = ForEashAsync(configInfo, CheckFileMD5);

            List<ConfigInfo> configToDownload = checkMD5.Result.Where(r => r != null).ToList();

            if (configToDownload.Count == 0) return 0;

            Task<bool[]> ret = ForEashAsync(configToDownload, DownloadFile);

            if (ret.Result.Any(b => b == false))
            {
                Console.WriteLine("Failed to download a file.");
                Console.ReadLine();
                return 1;
            }

            return 0;

        }

        public static Task<T2[]> ForEashAsync<T, T2>(IEnumerable<T> source, Func<T, Task<T2>> body)
        {
            return Task.WhenAll(
                from item in source
                select Task.Run(() => body(item)));
        }

        public static async Task<ConfigInfo> CheckFileMD5(ConfigInfo info)
        {
            bool ret = await Task.Run(() => CheckFileValid(info));
            //True if file correct
            //That means return null so it does not get redownloaded
            return ret ? null : info;
        }

        public static bool CheckFileValid(ConfigInfo info)
        {
            string fileSum = Md5Sum(info.OutputLocation);

            return fileSum != null && fileSum.Equals(info.MD5);
        }

        private static string Md5Sum(string fileName)
        {
            byte[] fileMd5Sum = null;


            if (File.Exists(fileName))
            {
                using (FileStream stream = new FileStream(fileName, FileMode.Open))
                {
                    using (MD5 md5 = new MD5CryptoServiceProvider())
                    {
                        fileMd5Sum = md5.ComputeHash(stream);
                    }
                }
            }

            if (fileMd5Sum == null)
            {
                return null;
            }

            StringBuilder builder = new StringBuilder();
            foreach (var b in fileMd5Sum)
            {
                builder.Append(b);
            }
            return builder.ToString();
        }

        public static async Task<bool> DownloadFile(ConfigInfo info)
        {
            string webUrl = Path.Combine(info.Site, info.Version, info.FileName);
            string localFileName = info.OutputLocation;

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;
                    await client.DownloadFileTaskAsync(new Uri(webUrl), localFileName);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
