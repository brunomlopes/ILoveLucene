using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Ionic.Zip;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Conditions;
using NAppUpdate.Framework.Sources;
using NAppUpdate.Framework.Tasks;

namespace ILoveLucene.AutoUpdate
{
    public class ZippedFilesUpdateTask : IUpdateTask
    {
        string _tempFile = null;
        string _tempDecompressedDir = null;
        Dictionary<string,string> _coldUpdates;
        Dictionary<string,string> _warmUpdates;
        List<string> _directoriesToCreate;
        private string _appPath; 

        public ZippedFilesUpdateTask()
        {
            Attributes = new Dictionary<string, string>();
            _coldUpdates = new Dictionary<string, string>();
            _warmUpdates = new Dictionary<string, string>();
            _directoriesToCreate = new List<string>();
            UpdateConditions = new BooleanCondition();
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

            using (ZipFile zip = ZipFile.Read(_tempFile))
            {
                zip.ExtractAll(_tempDecompressedDir, ExtractExistingFileAction.OverwriteSilently);

                foreach (ZipEntry e in zip)
                {
                    if (e.IsDirectory)
                    {
                        string appDirectoryName = Path.Combine(_appPath, e.FileName);
                        if (!Directory.Exists(appDirectoryName))
                        {
                            _directoriesToCreate.Add(appDirectoryName);
                        }
                    }
                    else if(Attributes.ContainsKey("warm-update") && Attributes["warm-update"] == "true")
                    {
                        _warmUpdates[e.FileName] = Path.Combine(_tempDecompressedDir, e.FileName);
                    }
                    else
                    {
                        _coldUpdates[e.FileName] = Path.Combine(_tempDecompressedDir, e.FileName);
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

            foreach (var warmUpdate in _warmUpdates)
            {
                var destinationFile = Path.Combine(_appPath, warmUpdate.Key);

                try
                {
                    if (File.Exists(destinationFile))
                        File.Delete(destinationFile);
                    File.Move(warmUpdate.Value, destinationFile);
                }
                catch (Exception ex)
                {
                    throw new UpdateProcessFailedException(
                        string.Format("Couldn't move hot-swap file {0} into position {1}", warmUpdate.Value,
                                      destinationFile), ex);
                }
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