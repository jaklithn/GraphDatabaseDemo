using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Movies.Entities;
using Newtonsoft.Json;


namespace Movies.Services
{
    public static class MovieParser
    {
        private const string ZipFileName = "MovieContainer.zip";
        private const string JsonFileName = "MovieContainer.json";

        public static MovieContainer ParseFromFile()
        {
            var zipFilePath = Path.Combine(Environment.CurrentDirectory, "Resources", ZipFileName);
            using (var zipStorer = ZipStorer.Open(zipFilePath, FileAccess.Read))
            {
                var zipDir = zipStorer.ReadCentralDir();
                foreach (var zipEntry in zipDir)
                {
                    if (Path.GetFileName(zipEntry.FilenameInZip) == JsonFileName)
                    {
                        using (var ms = new MemoryStream())
                        {
                            zipStorer.ExtractFile(zipEntry, ms);
                            var json = Encoding.UTF8.GetString(ms.ToArray());
                            return JsonConvert.DeserializeObject<MovieContainer>(json);
                        }
                    }
                }
                return null;
            }
        }
    }
}