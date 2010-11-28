using System;
using System.IO;
using Core.Abstractions;
using Plugins.Shortcuts;
using Xunit;

namespace Tests
{
    class MockActOnFileInfo: BaseActOnTypedItem<FileInfo>
    {
        public bool Acted;
        public FileInfo Info;
        public override void ActOn(ITypedItem<FileInfo> item)
        {
            Acted = true;
            Info = item.Item;
        }

        public override string Text
        {
            get { return "not relevant"; }
        }
    }

    public class ActOnItemTests
    {
        [Fact]
        public void CanCallAct()
        {
            var act = new MockActOnFileInfo();
            var info = new FileInfoItem(new FileInfo("does.not.exist"));

            act.ActOn((IItem)info);
            Assert.True(act.Acted);
            Assert.Equal(act.Info, info.Item);
        }
    }
}