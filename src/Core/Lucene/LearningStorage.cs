using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core.Lucene
{
    public class LearningStorage : ILearningStorage
    {
        public LearningStorage(DirectoryInfo input)
        {
            _rootDirectory = input;
            if(!input.Exists)
            {
                input.Create();
                input.Refresh();
            }

            var dirs = input.EnumerateDirectories("??");
            _learnings = dirs
                .SelectMany(d =>d.EnumerateFiles().Select(f => new {f.Name, Text = File.ReadAllText(f.FullName).Trim()}))
                .ToDictionary(t => t.Name, t => t.Text);
        }

        public string LearningsFor(string sha1)
        {
            if (_learnings.ContainsKey(sha1))
            {
                return _learnings[sha1];
            }
            return String.Empty;
        }

        public string LearnFor(string learning, string sha1)
        {
            if(!_learnings.ContainsKey(sha1))
                _learnings[sha1] = learning;
            else
                _learnings[sha1] += " " + learning;
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

        private void WriteLearning(string sha1, string learningValue)
        {
            var path = SubPathFor(sha1);
            if(!Directory.Exists(path))
                _rootDirectory.CreateSubdirectory(path);

            var fullPath = Path.Combine(_rootDirectory.FullName, path, sha1);
            File.WriteAllText(fullPath, learningValue);
        }

        private string SubPathFor(string sha1)
        {
            return sha1.Substring(0, 1);
        }

        private readonly Dictionary<string, string> _learnings;

        private readonly DirectoryInfo _rootDirectory;
    }
}