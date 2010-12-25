namespace Core.Lucene
{
    public interface ILearningRepository
    {
        string LearningsFor(string sha1);
        string LearnFor(string learning, string sha1);
    }
}