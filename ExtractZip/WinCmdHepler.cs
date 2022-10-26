using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Utils.Common;

namespace ExtractZip
{
    public class WinCmdHelper
    {
        public string ReadData(string filePath, Encoding encoding)
        {
            return string.Join(Environment.NewLine, ReadDataAllLine(filePath, encoding));
        }

        public IEnumerable<string> ReadDataAllLine(string filePath, Encoding encoding)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader streamReader = new StreamReader(fileStream, encoding))
                {
                    StringBuilder sb = new StringBuilder();
                    //---------------------------------------------------
                    int symbol = streamReader.Peek();
                    while (true)
                    {
                        symbol = streamReader.Read();
                        if (symbol.Equals(-1))
                        {
                            break;
                        }
                        // Check line delimiter
                        if ((symbol == 13 && streamReader.Peek() == 10) || // "\r\n": //Windows
                            symbol == 13 || // "\r": //Macintosh
                            symbol == 10) // "\n": //UnixLinux
                        {
                            // If line delimiter == windows, will read next current
                            if (symbol == 13 && streamReader.Peek() == 10)
                            {
                                streamReader.Read();
                            }
                            //---------------------
                            string line = sb.ToString();
                            sb.Clear();
                            //---------------------
                            yield return line;
                        }
                        else
                        {
                            sb.Append((char)symbol);
                        }
                    }
                    //---------------------------------------------------
                    if (!string.IsNullOrWhiteSpace(sb.ToString()))
                    {
                        string line = sb.ToString();
                        sb.Clear();
                        yield return line;
                    }
                }
            }
        }

        public void WriteTextFile(string filePath, string content, string lineDelimiterEnum, bool startNewLine, bool endNewLine, Encoding encoding, bool append = false)
        {
            StringBuilder temp = new StringBuilder();
            //-----------------------------------------
            if (startNewLine)
            {
                temp.Append(lineDelimiterEnum);
            }
            //----------------------------------------
            temp.Append(content);
            //----------------------------------------
            if (endNewLine)
            {
                temp.Append(lineDelimiterEnum);
            }
            //-----------------------------------------
            WriteTextFile(filePath, temp.ToString(), encoding, append);
        }

        public void WriteTextFile(string filePath, string content, Encoding encoding, bool append = false)
        {
            FileInfo info = new FileInfo(filePath);
            //-----------------------------------------
            using (FileSystemWatcher fw = new FileSystemWatcher(info.DirectoryName, info.Name))
            {
                using (ManualResetEventSlim mre = new ManualResetEventSlim())
                {
                    fw.EnableRaisingEvents = true;
                    fw.Changed += (s, e) =>
                    {
                        mre.Set();
                    };
                    FileSystem.WriteAllText(filePath, content, append, encoding ?? Encoding.Default);
                    mre.Wait();
                }
            }
        }

        public void RenameDirectory(string path, string newName)
        {
            FileSystem.RenameDirectory(path, newName);
            //------------------------------------------------------------
            WaitDirectoryInProcessCompleted(new DirectoryInfo(Path.Combine(Path.GetDirectoryName(path), newName)), true);
        }

        public void RenameFile(string path, string newName)
        {
            FileInfo info = new FileInfo(path);

            using (FileSystemWatcher fw = new FileSystemWatcher(info.DirectoryName, info.Name))
            {
                using (ManualResetEventSlim mre = new ManualResetEventSlim())
                {
                    fw.EnableRaisingEvents = true;
                    fw.Renamed += (object sender, RenamedEventArgs e) =>
                    {
                        mre.Set();
                    };
                    FileSystem.RenameFile(path, newName);
                    mre.Wait();
                }
            }
        }

        public void CopyFile(string source, string destination, bool isOverwrite = false)
        {
            FileSystem.CopyFile(source, destination, isOverwrite);
            //-----------------------------------------
            WaitFileInProcessCompleted(new FileInfo(destination));
        }

        public void CopyDirectory(string source, string destination, bool isOverwrite = false)
        {
            string[] getDirectoriesFromSource = GetDirectories(source, System.IO.SearchOption.AllDirectories).ToArray();
            string[] getFilesFromSource = GetFiles(source, System.IO.SearchOption.AllDirectories).ToArray();
            //------------------------------------------
            FileSystem.CopyDirectory(source, destination, isOverwrite);
            //------------------------------------------
            WaitDirectoryInProcessCompleted(new DirectoryInfo(destination), true);
            //------------------------------------------
            string[] getDirectoriesFromDestination = GetDirectories(destination, System.IO.SearchOption.AllDirectories).ToArray();
            string[] getFilesFromDestination = GetFiles(destination, System.IO.SearchOption.AllDirectories).ToArray();
            // Check Directory
            while (getDirectoriesFromSource.Length != getDirectoriesFromDestination.Length)
            {
                Thread.Sleep(100);
            }
            // Check File
            while (getFilesFromSource.Length != getFilesFromDestination.Length)
            {
                Thread.Sleep(100);
            }
            //InProcessCompleted
            foreach (string item in getFilesFromDestination)
            {
                WaitFileInProcessCompleted(new FileInfo(item));
            }
        }

        public void MoveFile(string source, string destination, bool isOverwrite = false)
        {
            FileSystem.MoveFile(source, destination, isOverwrite);
            //-----------------------------------------
            WaitFileInProcessCompleted(new FileInfo(destination));
        }

        public void MoveDirectory(string source, string destination, bool isOverwrite = false)
        {
            string[] getDirectoriesFromSource = GetDirectories(source, System.IO.SearchOption.AllDirectories).ToArray();
            string[] getFilesFromSource = GetFiles(source, System.IO.SearchOption.AllDirectories).ToArray();
            //------------------------------------------
            FileSystem.MoveDirectory(source, destination, isOverwrite);
            //------------------------------------------
            WaitDirectoryInProcessCompleted(new DirectoryInfo(source), false);
            //------------------------------------------
            string[] getDirectoriesFromDestination = GetDirectories(destination, System.IO.SearchOption.AllDirectories).ToArray();
            string[] getFilesFromDestination = GetFiles(destination, System.IO.SearchOption.AllDirectories).ToArray();
            // Check Directory
            while (getDirectoriesFromSource.Length != getDirectoriesFromDestination.Length)
            {
                Thread.Sleep(100);
            }
            // Check File
            while (getFilesFromSource.Length != getFilesFromDestination.Length)
            {
                Thread.Sleep(100);
            }
            //InProcessCompleted
            foreach (string item in getFilesFromDestination)
            {
                WaitFileInProcessCompleted(new FileInfo(item));
            }
        }

        public string GetFileName(string pathFile)
        {
            return Path.GetFileName(pathFile);
        }

        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public string GetFileNameWithoutExtension(string pathFile)
        {
            return Path.GetFileNameWithoutExtension(pathFile);
        }

        public string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        public IEnumerable<string> GetFiles(string pathFile, System.IO.SearchOption searchOption, string searchPattern = "*")
        {
            foreach (string item in Directory.GetFiles(pathFile, searchPattern, searchOption))
            {
                yield return item;
            }
        }

        public IEnumerable<string> GetDirectories(string pathFile, System.IO.SearchOption searchOption, string searchPattern = "*")
        {
            foreach (string item in Directory.GetDirectories(pathFile, searchPattern, searchOption))
            {
                yield return item;
            }
        }

        public string Combine(string path1, string path2)
        {
            return Path.Combine(path1, path2);
        }

        public string GetExtension(string pathFile)
        {
            return Path.GetExtension(pathFile);
        }

        public void CreateDirectory(string pathDirectory)
        {
            Directory.CreateDirectory(pathDirectory);
            //------------------------------------------------------------
            WaitDirectoryInProcessCompleted(new DirectoryInfo(pathDirectory), true);
        }

        public void CreateFile(string pathFile)
        {
            using (FileStream temp = File.Create(pathFile)) ;
            //-----------------------------------------
            WaitFileInProcessCompleted(new FileInfo(pathFile));
        }

        public void DeleteDirectory(string pathDirectory, bool isRecycleBin = false)
        {
            FileSystem.DeleteDirectory(pathDirectory,
                UIOption.OnlyErrorDialogs,
                isRecycleBin ? RecycleOption.DeletePermanently : RecycleOption.SendToRecycleBin);
            //-----------------------------------------
            WaitDirectoryInProcessCompleted(new DirectoryInfo(pathDirectory), false);
        }

        public void DeleteFile(string pathFile, bool isRecycleBin = false)
        {
            FileInfo info = new FileInfo(pathFile);

            using (FileSystemWatcher fw = new FileSystemWatcher(info.DirectoryName, info.Name))
            {
                using (ManualResetEventSlim mre = new ManualResetEventSlim())
                {
                    fw.EnableRaisingEvents = true;
                    fw.Deleted += (object sender, FileSystemEventArgs e) =>
                    {
                        mre.Set();
                    };
                    FileSystem.DeleteFile(pathFile,
                        UIOption.OnlyErrorDialogs,
                        isRecycleBin ? RecycleOption.DeletePermanently : RecycleOption.SendToRecycleBin);
                    mre.Wait();
                }
            }
        }

        public bool CheckExistsFile(string pathFile)
        {
            if (string.IsNullOrWhiteSpace(pathFile))
            {
                throw new StringEmptyException("The file path specified cannot be empty.");
            }
            //--------------------------------------------------------------------
            return File.Exists(pathFile);
        }

        public bool CheckExistsDirectory(string pathDirectory)
        {
            if (string.IsNullOrWhiteSpace(pathDirectory))
            {
                throw new StringEmptyException("The file path specified cannot be empty.");
            }
            //--------------------------------------------------------------------
            return Directory.Exists(pathDirectory);
        }

        public string CreatePathDestination(string destination, string fileName)
        {
            return string.Format("{0}{1}{2}",
                    destination,
                    destination.Last().Equals(@"\") ? string.Empty : @"\",
                    fileName);
        }

        public void OpenWebSite(string url)
        {
            ProcessStartInfo procStartInfo = new ProcessStartInfo(url)
            {
                CreateNoWindow = true
            };
            Process.Start(procStartInfo);
        }

        /// <summary>
        /// Build File Name for Destination folder.
        /// </summary>
        /// <param name="source">Source File Path.</param>
        /// <param name="destination">Destination Folder Path.</param>
        /// <param name="isOverwrite">Flag to set overwrite file destination folder.</param>
        /// <returns></returns>
        public string BuildFileNameForDestination(string source, string destination, bool isOverwrite)
        {
            var fileName = this.GetFileNameWithoutExtension(source);
            var extension = this.GetExtension(source);
            var destinationPath = $"{destination}\\{fileName}{extension}";

            if (isOverwrite)
            {
                return destinationPath;
            }
            else
            {
                bool isExists = true;
                int index = 0;
                string fileNameDestination = string.Empty;
                while (isExists)
                {
                    fileNameDestination = $"{destination}\\{fileName}" + "{0}" + extension;
                    if (index == 0)
                    {
                        fileNameDestination = string.Format(fileNameDestination, string.Empty);
                    }

                    if (index == 1)
                    {
                        fileNameDestination = string.Format(fileNameDestination, " - Copy");
                    }

                    if (index > 1)
                    {
                        fileNameDestination = string.Format(fileNameDestination, $" - Copy ({index})");
                    }

                    isExists = this.CheckExistsFile(fileNameDestination);

                    index++;
                }
                return fileNameDestination;
            }
        }

        /// <summary>
        /// Build Folder Name for Destination folder.
        /// </summary>
        /// <param name="source">Source Folder Path.</param>
        /// <param name="destination">Destination Folder Path.</param>
        /// <returns></returns>
        public string BuildFolderNameForDestination(string source, string destination)
        {
            var folderName = Path.GetFileNameWithoutExtension(source);
            bool isExists = true;
            int index = 0;
            string folderNameDestination = string.Empty;
            while (isExists)
            {
                folderNameDestination = $"{destination}\\{folderName}" + "{0}";
                if (index == 0)
                {
                    folderNameDestination = string.Format(folderNameDestination, string.Empty);
                }

                if (index == 1)
                {
                    folderNameDestination = string.Format(folderNameDestination, " - Copy");
                }

                if (index > 1)
                {
                    folderNameDestination = string.Format(folderNameDestination, $" - Copy ({index})");
                }

                isExists = this.CheckExistsDirectory(folderNameDestination);

                index++;
            }
            return folderNameDestination;
        }

        //--------------------------------------------------------------------------------

        private const int ERROR_SHARING_VIOLATION = 32;
        private const int ERROR_LOCK_VIOLATION = 33;

        private void WaitFileInProcessCompleted(FileInfo file)
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
                        if ((ex is IOException) && (errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION))
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

        private void WaitDirectoryInProcessCompleted(DirectoryInfo file, bool isExists)
        {
            Action<DirectoryInfo> checkExistsDirectory = null;
            using (ManualResetEventSlim resetEvent = new ManualResetEventSlim(false))
            {
                // Setup checkExistsDirectory
                checkExistsDirectory = (item) =>
                {
                    if (isExists ? CheckExistsDirectory(item.FullName) : !CheckExistsDirectory(item.FullName))
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

        #region private

        #endregion
    }
}
