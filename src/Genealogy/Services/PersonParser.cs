using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Genealogy.Entities;
using Newtonsoft.Json;


namespace Genealogy.Services
{
    public static class PersonParser
    {
        private const string ZipFileName = "PersonContainer.zip";
        private const string JsonFileName = "Persons.json";

        public static PersonContainer ParseFromFile()
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
                            return JsonConvert.DeserializeObject<PersonContainer>(json);
                        }
                    }
                }
                return null;
            }
        }
    }
}