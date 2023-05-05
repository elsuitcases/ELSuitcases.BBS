using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELSuitcases.BBS.Library.Server
{
    public static class FileHelper
    {
        public static async Task<ZipArchive?> CompressAsZip(string pathZipFile, List<FileInfo> files)
        {
            if (files == null)
                return null;

            string funcName = "FileHelper.CompressAsZip";
            ZipArchive? archive = null;

            try
            {
                using (FileStream fs = new FileStream(pathZipFile, FileMode.Create, FileAccess.Write, FileShare.Write, Constants.DEFAULT_VALUE_BUFFER_SIZE, true))
                {
                    archive = new ZipArchive(fs, ZipArchiveMode.Create, false, Encoding.UTF8);

                    foreach (var f in files)
                    {
                        if (!f.Exists)
                            continue;

                        string fileName = f.Name;
                        if (archive.Entries.Any(e => e.Name == fileName))
                            fileName += "_(1)";

                        archive.CreateEntryFromFile(f.FullName, fileName);
                    }
                }

                Common.PrintDebugInfo(string.Format("ZIP 압축 성공 : ZIP_FILE = {0} / ENTRIES_COUNT = {1}", pathZipFile, archive.Entries.Count), funcName);
            }
            catch (Exception ex)
            {
                archive?.Dispose();

                Common.ThrowException(ex, true, funcName, Constants.MESSAGE_ERROR_FUNCTION_ON_RUNNING);
            }

            await Task.Delay(10);

            return archive;
        }

        public static async Task<Stream?> GetEntryFromZipArchive(string pathZipFile, DirectoryInfo folderTempExtract, string entryNameToGet)
        {
            if (!File.Exists(pathZipFile))
                return null;

            string funcName = "FileHelper.GetEntryFromZipArchive";
            Stream? dfs = null;

            try
            {
                using (FileStream fs = new FileStream(pathZipFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read, false))
                    {
                        if (zip.GetEntry(entryNameToGet) is ZipArchiveEntry entry)
                        {
                            if (!Directory.Exists(folderTempExtract.FullName))
                                Directory.CreateDirectory(folderTempExtract.FullName);

                            string pathEntry = Path.Combine(folderTempExtract.FullName, entryNameToGet);
                            entry.ExtractToFile(pathEntry, true);
                            dfs = new FileStream(pathEntry, FileMode.Open, FileAccess.Read, FileShare.Read, Constants.DEFAULT_VALUE_BUFFER_SIZE, true);

                            Common.PrintDebugInfo(string.Format("ZIP 엔트리 읽기 성공 : ZIP_FILE = {0} / ENTRY = {1}", pathZipFile, pathEntry), funcName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.ThrowException(ex, true, funcName, Constants.MESSAGE_ERROR_FUNCTION_ON_RUNNING);
            }

            await Task.Delay(1);

            return dfs;
        }
    }
}
