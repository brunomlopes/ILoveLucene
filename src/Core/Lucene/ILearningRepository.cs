using System.Collections.Generic;

namespace Core.Lucene
{
    public interface ILearningRepository
    {
        IEnumerable<string> LearningsFor(string sha1);
        IEnumerable<string> LearnFor(string learning, string sha1);
    }
}