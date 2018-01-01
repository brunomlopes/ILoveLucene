using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Core.API;
using Xunit;
using Xunit.Extensions;

namespace Tests
{
    public class MakeSureIDoNotScrewUpHashingTests
    {
        [Fact] 
        public void DidI()
        {
            var correctHash = GetSha1("converter", "id", "sourceid");
            var newHash = new DocumentId("converter", "id", "sourceid", "learningid").GetId();
            Assert.Equal(correctHash, newHash);
        }

        private string GetSha1(string converterId, string Id, string SourceId)
        {
            var sha1 = SHA1.Create();
            sha1.Initialize();

            return
                BitConverter.ToString(
                    sha1.ComputeHash(Encoding.UTF8.GetBytes(converterId)
                                         .Concat(Encoding.UTF8.GetBytes(Id))
                                         .Concat(Encoding.UTF8.GetBytes(SourceId))
                                         .ToArray()))
                    .Replace("-", "");
        }
    }
}