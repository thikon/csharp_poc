using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CopyDirectoryRecursively
{
    class Program
    {
        static void Main(string[] args)
        {
            string source = @"C:\Users\Dew\AppData\Local\Google\Chrome\User Data\Profile 1";
            string destination = @"C:\Users\Dew\AppData\Local\Google\Chrome\User Data\COPY_Default";

            string[] getDirectoriesFromSource = GetDirectories(source, System.IO.SearchOption.AllDirectories).ToArray();
            string[] getFilesFromSource = GetFiles(source, System.IO.SearchOption.AllDirectories).ToArray();

            // Console.WriteLine($"start: {DateTime.Now}");

            // CopyFilesRecursively(source, destination);

            // CopyFileToZip(source, destination);

            //Console.WriteLine(Path.GetFileName(source));

            //Console.WriteLine($"done: {DateTime.Now}");

            string browserType = "GoogleChrome";

            BrowserTypeDriver eBrowserTypeName;
            if (Enum.TryParse(browserType, out eBrowserTypeName))
            {
                switch (eBrowserTypeName)
                {
                    case BrowserTypeDriver.GoogleChrome:
                        Console.WriteLine(browserType);
                        break;
                    default:
                        break;
                }
            }


            Console.ReadKey();


        }


        public enum BrowserTypeDriver
        {
            [Description("Google Chrome")]
            GoogleChrome,
            [Description("Mozilla Firefox")]
            MozillaFirefox,
            [Description("Microsoft Edge")]
            MicrosoftEdge,
            [Description("Microsoft Internet Explorer")]
            InternetExplorer
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        private static void CopyParallel(string sourcePath, string targetPath)
        {
            try
            {
                Parallel.ForEach(Directory.GetDirectories(sourcePath), dirPath =>
                {
                    string currentFolder = Path.GetFileName(dirPath);
                    string tmp = dirPath.Replace(sourcePath, targetPath);
                    Directory.CreateDirectory(tmp);

                    Parallel.ForEach(Directory.GetFiles(dirPath), filePath =>
                    {
                        string currentFile = Path.GetFileName(filePath);
                        File.Copy(filePath, Path.Combine(tmp, currentFile), true);
                    });

                    CopyParallel(dirPath, tmp);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void CopyFileToZip(string sourcePath, string targetPath)
        {
            string zipname = @"c:\temp\profile_1.zip";

            ZipFile.CreateFromDirectory(sourcePath, zipname, CompressionLevel.Fastest, false);
            
            // ZipFile.ExtractToDirectory(zipname, targetPath);
        }

        private static IEnumerable<string> ReadFile(string sourcePath)
        {
            return GetFiles(sourcePath, System.IO.SearchOption.AllDirectories).ToArray();
        }

        private static IEnumerable<string> GetDirectories(string pathFile, System.IO.SearchOption searchOption, string searchPattern = "*")
        {
            foreach (string item in Directory.GetDirectories(pathFile, searchPattern, searchOption))
            {
                yield return item;
            }
        }
        private static IEnumerable<string> GetFiles(string pathFile, System.IO.SearchOption searchOption, string searchPattern = "*")
        {
            foreach (string item in Directory.GetFiles(pathFile, searchPattern, searchOption))
            {
                yield return item;
            }
        }
        private static void WaitDirectoryInProcessCompleted(DirectoryInfo file, bool isExists)
        {
            Action<DirectoryInfo> checkExistsDirectory = null;
            using (ManualResetEventSlim resetEvent = new ManualResetEventSlim(false))
            {
                // Setup checkExistsDirectory
                checkExistsDirectory = (item) =>
                {
                    if (isExists ? Directory.Exists(item.FullName) : !Directory.Exists(item.FullName))
                    {
                        resetEvent.Set();
                    }
                    else
                    {
                        Thread.Sleep(100);
                        checkExistsDirectory(item);
                    }
                };
                Thread thread = new Thread(new ThreadStart(delegate
                {
                    checkExistsDirectory(file);
                }));
                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();
                //With file
                resetEvent.Wait();
            }
        }
        private static void WaitFileInProcessCompleted(FileInfo file)
        {
            Action<FileInfo> checkOpenFile = null;
            using (ManualResetEventSlim resetEvent = new ManualResetEventSlim(false))
            {
                // Setup checkOpenFile
                checkOpenFile = (item) =>
                {
                    FileStream stream = null;

                    try
                    {
                        stream = item.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    }
                    catch (Exception ex)
                    {
                        //the file is unavailable because it is:
                        //still being written to
                        //or being processed by another thread
                        //or does not exist (has already been processed)
                        int errorCode = Marshal.GetHRForException(ex) & ((1 << 16) - 1);
                        if ((ex is IOException) && (errorCode == 32 || errorCode == 33))
                        {
                            Thread.Sleep(100);
                            checkOpenFile(item);
                        }
                        else
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        if (stream != null)
                        {
                            stream.Close();
                        }
                        resetEvent.Set();
                    }
                };
                Thread thread = new Thread(new ThreadStart(delegate
                {
                    checkOpenFile(file);
                }));
                thread.SetApartmentState(ApartmentState.STA);
                thread.IsBackground = true;
                thread.Start();
                //With file
                resetEvent.Wait();
            }
        }

        // -------------

        public static InMemoryFile LoadFromFile(string path)
        {
            using (var fs = File.OpenRead(path))
            {
                using(var memFile = new MemoryStream())
                {
                    fs.CopyTo(memFile);
                    memFile.Seek(0, SeekOrigin.Begin);
                    return new InMemoryFile() { Content = memFile.ToArray(), FileName = Path.GetFileName(path) };
                }
            }
        }

        public static byte[] GetZipArchive(params InMemoryFile[] files)
        {
            byte[] archiveFile;
            using (var archiveStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        var zipArchiveEntry = archive.CreateEntry(file.FileName, CompressionLevel.Optimal);

                        using (var zipStream = zipArchiveEntry.Open())
                        {
                            zipStream.Write(file.Content, 0, file.Content.Length);
                        } 
                    }
                }

                archiveFile = archiveStream.ToArray();
            }

            return archiveFile;
        }
    }

    public class InMemoryFile
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
    }
}
