using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.Text;

namespace Utils.Common
{
    public class ZipApplication
    {
        private bool _isCreate;
        private ZipFile _readerLock;
        private readonly string _sourcePath;
        private readonly string _readerCachePath;
        private readonly string _writerCachePath;
        private readonly int _level;

        private ZipApplication(string sourceFilePath, string cacheFolderPath, int level)
        {
            _level = level;
            string getDateTime = DateTime.Now.ToString("yyyyMMddhhMMssfff");
            if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
            {
                _isCreate = true;
            }
            else
            {
                _sourcePath = Path.GetFullPath(sourceFilePath);
            }
            _readerCachePath = Path.Combine(cacheFolderPath, $"ZipReaderCache{getDateTime}.zip");
            _writerCachePath = Path.Combine(cacheFolderPath, $"ZipWriterCache{getDateTime}.zip");
            //----------------------------------------------
            LockFile();
            InitializeCache();
        }

        private ZipApplication() : this(null, "", 3) { }

        public string SourcePath { get { return _sourcePath; } }

        public string ReaderCachePath { get { return _readerCachePath; } }

        public string WriterCachePath { get { return _writerCachePath; } }

        public static ZipApplication CreateApplication(string sourcePath, string cacheTempPath, int level = 3)
        {
            return new ZipApplication(sourcePath, cacheTempPath, level);
        }

        public void UnLockFile()
        {
            if (_readerLock != null)
            {
                _readerLock.Close();
                _readerLock = null;
            }
        }

        public void LockFile()
        {
            if (!_isCreate)
            {
                _readerLock = new ZipFile(_sourcePath);
            }
        }

        private void InitializeCache()
        {
            if (!_isCreate)
            {
                File.Copy(_sourcePath, _readerCachePath);
            }
            else
            {
                CreateZip();
            }
        }

        private void CreateZip()
        {
            using (FileStream fsOut = File.Create(_readerCachePath))
            using (var zipStream = new ZipOutputStream(fsOut))
            {
                zipStream.UseZip64 = UseZip64.Dynamic;
                //0-9, 9 being the highest level of compression
                zipStream.SetLevel(_level);
            }
        }

        /// <summary>
        /// Return custom value to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            //----------------------------------------------------
            stringBuilder.AppendLine($"Zip Path : {_sourcePath}");
            //----------------------------------------------------
            return stringBuilder.ToString();
        }
    }
}
