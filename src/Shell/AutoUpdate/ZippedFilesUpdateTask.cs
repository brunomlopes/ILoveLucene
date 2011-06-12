using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Conditions;
using NAppUpdate.Framework.Sources;
using NAppUpdate.Framework.Tasks;
using SevenZip;

namespace ILoveLucene.AutoUpdate
{
    public class ZippedFilesUpdateTask : IUpdateTask
    {
        string _tempFile = null;
        string _tempDecompressedDir = null;
        Dictionary<string,string> _coldUpdates;
        List<string> _directoriesToCreate;
        private string _appPath; 

        public ZippedFilesUpdateTask()
        {
            Attributes = new Dictionary<string, string>();
            _coldUpdates = new Dictionary<string, string>();
            _directoriesToCreate = new List<string>();
            UpdateConditions = new NAppUpdate.Framework.Conditions.BooleanCondition();
            // TODO: replace this with something from the updatemanager
            _appPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); 
        }

        public bool Prepare(IUpdateSource source)
        {
            var fileName = Attributes["remotePath"];
            
            try
            {
                string tempFileLocal = Path.Combine(UpdateManager.Instance.TempFolder, Guid.NewGuid().ToString());
                if (!source.GetData(fileName, string.Empty /* this is not used*/, ref tempFileLocal))
                    return false;

                _tempFile = tempFileLocal;
            }
            catch (Exception ex)
            {
                throw new UpdateProcessFailedException("Couldn't get Data from source", ex);
            }

            _tempDecompressedDir = Path.Combine(UpdateManager.Instance.TempFolder, Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDecompressedDir);

            SevenZipBase.SetLibraryPath(Path.Combine(Path.GetDirectoryName(typeof(SevenZipExtractor).Assembly.Location), "7z.dll"));

            using (var tmp = new SevenZipExtractor(_tempFile))
            {
                tmp.ExtractArchive(_tempDecompressedDir);

                for (int i = 0; i < tmp.ArchiveFileData.Count; i++)
                {
                    var archiveFileInfo = tmp.ArchiveFileData[i];

                    if (archiveFileInfo.IsDirectory)
                    {
                        string appDirectoryName = Path.Combine(_appPath, archiveFileInfo.FileName);
                        if(!Directory.Exists(appDirectoryName))
                        {
                            _directoriesToCreate.Add(appDirectoryName);
                        }
                    }
                    else
                    {
                        _coldUpdates[archiveFileInfo.FileName] = Path.Combine(_tempDecompressedDir,
                                                                              archiveFileInfo.FileName);
                    }
                }
            }
            File.Delete(_tempFile);
            return true;
        }

        public bool Execute()
        {
            foreach (var dir in _directoriesToCreate)
            {
                Directory.CreateDirectory(dir);
            }
            return true;
        }

        public IEnumerator<KeyValuePair<string, object>> GetColdUpdates()
        {
            foreach (var coldUpdate in _coldUpdates)
            {
                yield return new KeyValuePair<string, object>(coldUpdate.Key, coldUpdate.Value);
            }
        }

        public bool Rollback()
        {
            throw new NotImplementedException("No rollback implemented yet. So sorry :(");
        }

        public IDictionary<string, string> Attributes { get; private set; }
        public string Description { get; set; }
        public BooleanCondition UpdateConditions { get; set; }
    }
}