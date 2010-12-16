namespace Core.Lucene
{
    public interface ILearningStorage
    {
        string LearningsFor(string sha1);
        string LearnFor(string learning, string sha1);
    }
}