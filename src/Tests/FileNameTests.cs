using System.Text.RegularExpressions;
using Xunit;

namespace Tests
{
    public class FileNameTests
    {
        private Regex regexp;

        [Fact]
        public void RegexpReplaceWorks()
        {
            Assert.Equal("_text_ here", regexp.Replace("<text> here", "_"));
            Assert.Equal("_text_ here", regexp.Replace(":text> here", "_"));
            Assert.Equal("_text_ here", regexp.Replace("\"text\\ here", "_"));
            Assert.Equal("_text_ here", regexp.Replace("?text* here", "_"));
            Assert.Equal("_text_ here", regexp.Replace("|text> here", "_"));
        }

        public FileNameTests()
        {
            regexp = new System.Text.RegularExpressions.Regex(@"[<>:""/\\|?*]");
        }
    }
}