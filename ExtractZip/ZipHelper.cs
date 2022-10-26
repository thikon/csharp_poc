using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils.Common;

namespace ExtractZip
{
    public class ZipHelper
    {
        private readonly ZipApplication _application;
        private readonly WinCmdHelper _cmdHelper;

        private ZipHelper(WinCmdHelper cmdHelper)
        {
            _cmdHelper = cmdHelper;
        }

        public ZipHelper(string path, string cacheTempPath, int level) : this(new WinCmdHelper())
        {
            _application = ZipApplication.CreateApplication(path, cacheTempPath, level);
        }

        public ZipHelper(ZipApplication application) : this(new WinCmdHelper())
        {
            _application = application;
        }

        public void ExtractFile(string path, string[] passwords, out string messageWarning, string filter = null, string filterExt = null, bool directoryStructure = true)
        {
            using (ZipFile zf = ZipReader(false))
            {
                messageWarning = default;
                if (zf.Count > 0)
                {
                    uint extrackCount = 0;
                    uint extrackSuccessCount = 0;
                    uint extrackFailCount = 0;
                    string[] passwordsRecheck = new string[] { null };
                    if (passwords != null)
                    {
                        passwords = passwords.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray();
                        if (passwords.Length > 0)
                        {
                            string[] tempPasswords = new string[passwordsRecheck.Length + passwords.Length];
                            Array.Copy(passwordsRecheck, tempPasswords, passwordsRecheck.Length);
                            Array.Copy(passwords, 0, tempPasswords, 1, passwords.Length);
                            passwordsRecheck = tempPasswords;
                        }
                    }
                    path = _cmdHelper.GetFullPath(path);
                    //--------------------------------
                    foreach (ZipEntry zipEntry in zf)
                    {
                        if (!zipEntry.IsFile)
                        {
                            // Ignore directories
                            continue;
                        }
                        string entryFileName = zipEntry.Name;
                        if (!directoryStructure)
                        {
                            string[] tempWord = zipEntry.Name.Segmentation(@"/");
                            //------------------------------------------------------
                            if (tempWord.Length > 0)
                            {
                                entryFileName = tempWord[tempWord.Length - 1];
                            }
                        }
                        // Manipulate the output filename here as desired.
                        string fullZipToPath = _cmdHelper.Combine(path, entryFileName).Replace(@"\\", @"\");
                        if (directoryStructure)
                        {
                            string directoryName = _cmdHelper.GetDirectoryName(fullZipToPath);
                            if (!_cmdHelper.CheckExistsDirectory(directoryName))
                            {
                                _cmdHelper.CreateDirectory(directoryName);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(filterExt))
                        {
                            if (_cmdHelper.GetExtension(fullZipToPath).IndexOf(filterExt.Trim(), StringComparison.OrdinalIgnoreCase).Equals(-1))
                            {
                                // Ignore extension
                                continue;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(filter))
                        {
                            if (_cmdHelper.GetFileNameWithoutExtension(fullZipToPath).IndexOf(filter.Trim(), StringComparison.OrdinalIgnoreCase).Equals(-1))
                            {
                                // Ignore filter
                                continue;
                            }
                        }

                        ++extrackCount;
                        bool canExtrack = false;

                        foreach (string item in passwordsRecheck)
                        {
                            try
                            {
                                // Set Password
                                zf.Password = item;

                                // 4K is optimum
                                byte[] buffer = new byte[4096];

                                // Unzip file in buffered chunks. This is just as fast as unpacking
                                // to a buffer the full size of the file, but does not waste memory.
                                // The "using" will close the stream even if an exception occurs.
                                using (Stream zipStream = zf.GetInputStream(zipEntry))
                                {
                                    using (Stream fsOutput = File.Create(fullZipToPath))
                                    {
                                        StreamUtils.Copy(zipStream, fsOutput, buffer);
                                    }
                                }

                                ++extrackSuccessCount;

                                canExtrack = true;

                                break;
                            }
                            catch(Exception ex)
                            {
                                // Free Code
                            }
                        }

                        if (!canExtrack)
                        {
                            ++extrackFailCount;
                        }

                    }
                    //--------------------------------
                    if (extrackSuccessCount > 0)
                    {
                        if (extrackFailCount > 0)
                        {
                            messageWarning = $"All available files are {zf.Count} files{Environment.NewLine}";
                            messageWarning += $"Files can be extracted : {extrackSuccessCount}{Environment.NewLine}";
                            messageWarning += $"Files cannot be extracted : {extrackFailCount}{Environment.NewLine}";
                            messageWarning += "Because the password in the file extraction is not valid.";
                        }
                    }
                    else
                    {
                        string exMessage = $"There are {extrackFailCount} files that could not be extracted because the password in the file extraction is not valid";
                        if (extrackCount == zf.Count)
                        {
                            throw new Exception(exMessage);
                        }
                        else
                        {
                            throw new Exception(exMessage + $" ({extrackCount} out of {zf.Count} files)");
                        }
                    }
                }
            }
        }

        public void AttachFile(string path, string directory, string password)
        {
            using (ZipFile zf = ZipReader(true))
            {
                if (!string.IsNullOrWhiteSpace(password))
                {
                    zf.Password = password;
                }
                zf.UseZip64 = UseZip64.On;
                //------------------------------------------------------------
                path = _cmdHelper.GetFullPath(path);
                path = path.Replace(@"\\", @"\");
                //------------------------------------------------------------
                Func<string, string> validationOnDirectory = (d) =>
                {
                    if (!string.IsNullOrWhiteSpace(directory))
                    {
                        directory = directory.Trim().Replace(@"\\", @"/").Replace(@"\", @"/").Replace(@"//", @"/");
                        if (!directory[directory.Length - 1].Equals('/'))
                        {
                            directory = string.Format(@"{0}/", directory);
                        }
                    }
                    return directory;
                };
                //-----------------------------------------------------------
                if (_cmdHelper.CheckExistsFile(path))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        CustomStaticDataSource dataSource = new CustomStaticDataSource();
                        dataSource.SetStream(fs);
                        string directoryPath = string.Format("{0}{1}", validationOnDirectory(directory), _cmdHelper.GetFileName(path));
                        //-------------------------------------------------------
                        zf.Add(dataSource, directoryPath);
                        zf.CommitUpdate();
                    }
                }
                else if (_cmdHelper.CheckExistsDirectory(path))
                {
                    Action<ZipFile, string, string> addFile = null;
                    addFile = (zip, pathDirectory, pathEntry) =>
                    {
                        string directoryPathFile = _cmdHelper.GetFileName(pathDirectory);
                        pathEntry = string.IsNullOrWhiteSpace(directoryPathFile) ? pathEntry : ($"{(string.IsNullOrWhiteSpace(pathEntry) ? pathEntry : $"{pathEntry}/")}{directoryPathFile}");
                        //---------------------------------------------------------
                        zf.BeginUpdate();
                        zip.AddDirectory(pathEntry);
                        zf.CommitUpdate();
                        //-------------------------------------------------------------
                        string[] files = _cmdHelper.GetFiles(pathDirectory, SearchOption.TopDirectoryOnly).ToArray();
                        foreach (string item in files)
                        {
                            using (FileStream fs = new FileStream(item, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                CustomStaticDataSource dataSource = new CustomStaticDataSource();
                                dataSource.SetStream(fs);
                                string tempEntryName = string.Format("{0}/{1}", pathEntry, _cmdHelper.GetFileName(item));
                                //---------------------------------------------------------
                                zf.BeginUpdate();
                                zip.Add(dataSource, tempEntryName);
                                zf.CommitUpdate();
                            }
                        }
                        string[] directories = _cmdHelper.GetDirectories(pathDirectory, SearchOption.TopDirectoryOnly).ToArray();
                        foreach (string item in directories)
                        {
                            addFile(zip, item, pathEntry);
                        }
                    };
                    //-------------------------------------------------------
                    addFile(zf, path, validationOnDirectory(directory));
                }
                else
                {
                    throw new FileNotFoundException("That path doesn't exist.");
                }
            }
        }

        public void DeleteFile(string target, ZipDeleteTypeEnum zipDelete)
        {
            using (ZipFile zf = ZipReader(false))
            {
                target = target?.Trim();
                if (zf.Count > 0)
                {
                    bool hasDelete = false;
                    switch (zipDelete)
                    {
                        case ZipDeleteTypeEnum.All:
                            foreach (ZipEntry zipEntry in zf)
                            {
                                zf.BeginUpdate();
                                zf.Delete(zipEntry);
                                zf.CommitUpdate();
                            }
                            break;
                        case ZipDeleteTypeEnum.EntryName:
                            if (string.IsNullOrWhiteSpace(target))
                            {
                                throw new Exception($"Target entry name can't be null or empty.");
                            }
                            foreach (ZipEntry zipEntry in zf)
                            {
                                if (zipEntry.Name.Trim().Equals(target, StringComparison.OrdinalIgnoreCase))
                                {
                                    zf.BeginUpdate();
                                    zf.Delete(zipEntry);
                                    zf.CommitUpdate();

                                    hasDelete = true;
                                    break;
                                }
                            }
                            if (!hasDelete)
                            {
                                throw new Exception($"'{target}' Could not find the item to be deleted.");
                            }
                            break;
                        case ZipDeleteTypeEnum.FilterFile:
                            if (string.IsNullOrWhiteSpace(target))
                            {
                                throw new Exception($"Target filter file can't be null or empty.");
                            }
                            foreach (ZipEntry zipEntry in zf)
                            {
                                if (!zipEntry.IsFile)
                                {
                                    // Ignore extension
                                    continue;
                                }

                                string entryFileName;
                                string[] tempWord = zipEntry.Name.Segmentation(@"/");
                                //----------------------------------------------
                                if (tempWord.Length > 0)
                                {
                                    entryFileName = tempWord[tempWord.Length - 1];
                                }
                                else
                                {
                                    entryFileName = zipEntry.Name;
                                }
                                entryFileName = _cmdHelper.GetFileNameWithoutExtension(entryFileName);
                                //------------------------------------------------
                                if (entryFileName.Trim().IndexOf(target.Trim(), StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    zf.BeginUpdate();
                                    zf.Delete(zipEntry);
                                    zf.CommitUpdate();
                                }
                            }
                            break;
                        case ZipDeleteTypeEnum.Extension:
                            if (string.IsNullOrWhiteSpace(target))
                            {
                                throw new Exception($"Target extension name can't be null or empty.");
                            }
                            foreach (ZipEntry zipEntry in zf)
                            {
                                if (zipEntry.Name.Trim().EndsWith(target, StringComparison.OrdinalIgnoreCase))
                                {
                                    zf.BeginUpdate();
                                    zf.Delete(zipEntry);
                                    zf.CommitUpdate();
                                }
                            }
                            break;
                    }
                }
            }
        }

        public void SetComment(string comment)
        {
            using (ZipFile zf = ZipReader(true))
            {
                zf.SetComment(comment);
                //---------------------------------------------------
                zf.CommitUpdate();
            }
        }

        public void SaveAs(string path)
        {
            try
            {
                _application.UnLockFile();
                //-----------------------------------------------------------------------
                path = _cmdHelper.GetFullPath(path);
                ZipHelper.CopyFile(_application.ReaderCachePath, path);
                //-----------------------------------------------------------------------
                _application.LockFile();
            }
            catch
            {
                _application.LockFile();
                //-----------------------------------------------------------------------
                throw;
            }
        }

        public void Save()
        {
            try
            {
                _application.UnLockFile();
                //-----------------------------------------------------------------------
                ZipHelper.CopyFile(_application.ReaderCachePath, _application.SourcePath);
                //-----------------------------------------------------------------------
                _application.LockFile();
            }
            catch
            {
                _application.LockFile();
                //-----------------------------------------------------------------------
                throw;
            }
        }

        public string GetComment()
        {
            using (ZipFile zf = ZipReader(false))
            {
                return zf.ZipFileComment;
            }
        }

        public IEnumerable<string> GetFiles()
        {
            using (ZipFile zf = ZipReader(false))
            {
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        // Ignore directories
                        continue;
                    }
                    //----------------------------------------------------------
                    yield return zipEntry.Name;
                }
            }
        }

        public ZipApplication GetApplication()
        {
            return _application;
        }

        public void Dispose()
        {
            _application.UnLockFile();
            //---------------------------------------------------------
            if (_cmdHelper.CheckExistsFile(_application.ReaderCachePath))
            {
                _cmdHelper.DeleteFile(_application.ReaderCachePath, true);
            }
            //---------------------------------------------------------
            if (_cmdHelper.CheckExistsFile(_application.WriterCachePath))
            {
                _cmdHelper.DeleteFile(_application.WriterCachePath, true);
            }
        }

        //-----------------------------------------------------------------------------

        public static void CopyFile(string source, string destination)
        {
            WinCmdHelper winCmdHelper = new WinCmdHelper();
            winCmdHelper.CopyFile(source, destination, true);
        }

        //-----------------------------------------------------------------------------

        private ZipFile ZipReader(bool isBeginUpdate)
        {
            ZipFile zf = new ZipFile(_application.ReaderCachePath);
            //-----------------------------------------------------------------
            if (isBeginUpdate)
            {
                zf.BeginUpdate();
            }
            //-----------------------------------------------------------------
            return zf;
        }

    }

    internal class CustomStaticDataSource : IStaticDataSource
    {
        private Stream _stream;
        // Implement method from IStaticDataSource
        public Stream GetSource()
        {
            return _stream;
        }

        // Call this to provide the memorystream
        public void SetStream(Stream inputStream)
        {
            _stream = inputStream;
            _stream.Position = 0;
        }
    }
}
