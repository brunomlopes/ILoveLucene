using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Core.Abstractions;
using System.Xml.XPath;

namespace Plugins.Calibre
{
    [Export(typeof(IItemSource))]
    public class CalibreBookSource : BaseItemSource
    {
        private static Process _currentCalibreProcess;
        private static readonly object ProcessLock = new object();

        [Import]
        public Configuration Config { get; set; }

        [Import]
        public ILog Log { get; set; }
     
        public override Task<IEnumerable<object>> GetItems()
        {
            return Task.Factory
                .StartNew(() => GetBooks());
        }

        private IEnumerable<object> GetBooks()
        {
            lock (ProcessLock)
            {
                if (CalibreDbIsRunning())
                    yield break;
            }
            var pathToCalibreDb = Path.Combine(Config.PathToCalibreInstalation, "calibredb.exe");
            if (!File.Exists(pathToCalibreDb))
            {
                Log.Debug("File {0} doesn't exist.", pathToCalibreDb);
            }

            var outputPath = Path.GetTempFileName() + ".xml";
            try
            {
                lock (ProcessLock)
                {
                    var processStartInfo = new ProcessStartInfo();
                    processStartInfo.FileName = pathToCalibreDb;
                    processStartInfo.UseShellExecute = false;
                    processStartInfo.CreateNoWindow = true;
                    processStartInfo.Arguments = string.Format("catalog \"{0}\"", outputPath);
                    processStartInfo.RedirectStandardError = true;
                    processStartInfo.RedirectStandardOutput = true;
                    if (CalibreDbIsRunning())
                        yield break;
                    _currentCalibreProcess = Process.Start(processStartInfo);
                }
                
                _currentCalibreProcess.WaitForExit();
                if (_currentCalibreProcess.ExitCode != 0)
                {
                    Log.Warn("Calibredb process exited with {0}", _currentCalibreProcess.ExitCode);
                    Log.Warn("output : " + _currentCalibreProcess.StandardOutput.ReadToEnd());
                    Log.Warn("error : " + _currentCalibreProcess.StandardError.ReadToEnd());
                    yield break;
                }

                _currentCalibreProcess = null;

                var xmlText = File.ReadAllText(outputPath);
                var xml = XDocument.Parse(xmlText);
                foreach (var record in xml.XPathSelectElements("calibredb/record"))
                {
                    Book book = null;
                    try
                    {
                        book = new Book();
                        book.Id = int.Parse(record.Element("id").Value);
                        book.Title = record.Element("title").Value;
                        book.Authors = string.Join(", ", record.XPathSelectElements("authors/author").Select(e => e.Value));
                        foreach (var format in record.XPathSelectElements("formats/format").Select(e => e.Value))
                        {
                            book.Formats.Add(format);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error parsing book record {0}", record.ToString());
                    }
                    if(book != null)
                        yield return book;
                }
            }
            finally
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
            }
        }

        private bool CalibreDbIsRunning()
        {
            var processIsLive = false;
            if(_currentCalibreProcess != null && !_currentCalibreProcess.HasExited)
            {
                if ((DateTime.Now - _currentCalibreProcess.StartTime).TotalSeconds > Config.MaximumSecondsProcessShouldTake)
                {
                    _currentCalibreProcess.Kill();
                }
                else
                {
                    Log.Info(string.Format("Waiting for process {0} to finish", _currentCalibreProcess.Id));
                    processIsLive = true;
                }
            }
            return processIsLive;
        }
    }
}
