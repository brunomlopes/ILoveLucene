using System;
using Core.Extensions;

namespace Core.API
{
    public class DocumentId
    {
        public string Id { get; private set; }
        public string SourceId { get; private set; }
        private readonly string _converterId;
        private readonly string _learningId;

        public DocumentId(string converterId, string itemId, string sourceId, string learningId)
        {
            Id = itemId;
            SourceId = sourceId;
            _converterId = converterId;
            _learningId = learningId;
        }

        public string GetId()
        {
            return GetSha1();
        }

        public string GetLearningId()
        {
            return string.IsNullOrWhiteSpace(_learningId) ? GetSha1() : _learningId;
        }

        public override string ToString()
        {
            return String.Format("<Command Converter:'{0}' Id:'{1}' Source:'{2}' Learning Id:'{3}'>", _converterId, Id, SourceId, GetLearningId());
        }

        private string GetSha1()
        {
            return Hash.HashStrings(_converterId, Id, SourceId);
        }
    }
}