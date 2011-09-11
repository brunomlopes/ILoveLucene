using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Core.Lucene
{
    public class FileSystemLearningRepository : ILearningRepository
    {
        public FileSystemLearningRepository(DirectoryInfo input)
        {
            _rootDirectory = input;
            if(!input.Exists)
            {
                input.Create();
                input.Refresh();
            }

            var dirs = input.EnumerateDirectories("??");
            _learnings = dirs
                .SelectMany(d =>d.EnumerateFiles().Select(f => new {f.Name, Text = File.ReadAllText(f.FullName).Trim().Split('\n').ToList()}))
                .ToDictionary(t => t.Name, t => t.Text);
        }

        public IEnumerable<string> LearningsFor(string sha1)
        {
            if (_learnings.ContainsKey(sha1))
            {
                return _learnings[sha1];
            }
            return new string[]{};
        }

        public IEnumerable<string> LearnFor(string learning, string sha1)
        {
            if(!_learnings.ContainsKey(sha1))
                _learnings[sha1] = new List<string>() { learning };
            else
                _learnings[sha1].Add(learning);
            WriteLearning(sha1, _learnings[sha1]);
            return _learnings[sha1];
        }

        public void SaveAll()
        {
            foreach (var learning in _learnings)
            {
                var sha1 = learning.Key;
                var learningValue = learning.Value;
                WriteLearning(sha1, learningValue);
            }
        }

        private void WriteLearning(string sha1, IList<string> learningValue)
        {
            var path = SubPathFor(sha1);
            if(!Directory.Exists(path))
                _rootDirectory.CreateSubdirectory(path);

            var fullPath = Path.Combine(_rootDirectory.FullName, path, sha1);
            var b = new StringBuilder();
            foreach (string learning in learningValue)
            {
                b.AppendFormat("{0}\n", learning);
            }
            File.WriteAllText(fullPath, b.ToString());
        }

        private string SubPathFor(string sha1)
        {
            return sha1.Substring(0, 1);
        }

        private readonly Dictionary<string, List<string>> _learnings;

        private readonly DirectoryInfo _rootDirectory;
    }
}