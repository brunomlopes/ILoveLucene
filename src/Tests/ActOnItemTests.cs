using System;
using System.IO;
using Core.Abstractions;
using Plugins.Shortcuts;
using Xunit;

namespace Tests
{
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
        
        [Fact]
        public void CanCallActWithArguments()
        {
            var act = new MockActOnFileInfo();
            var info = new FileInfoItem(new FileInfo("does.not.exist"));

            act.ActOn((IItem)info, "argument");
            Assert.True(act.Acted);
            Assert.Equal(act.Info, info.Item);
            Assert.Equal("argument", act.Arguments);
        }
    }

    class MockActOnFileInfo: BaseActOnTypedItem<FileInfo>, IActOnTypedItemWithArguments<FileInfo> 
    {
        public bool Acted;
        public FileInfo Info;
        public string Arguments;

        public override void ActOn(ITypedItem<FileInfo> item)
        {
            Acted = true;
            Info = item.Item;
        }

        public void ActOn(ITypedItem<FileInfo> item, string arguments)
        {
            Acted = true;
            Info = item.Item;
            Arguments = arguments;
        }

        public override string Text
        {
            get { return "not relevant"; }
        }
    }
}