using System;
using System.IO;
using Core;
using Core.Abstractions;
using Plugins.Commands;
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
        
        [Fact]
        public void CanFindAction()
        {
            var act = new MockActOnFileInfo();
            var info = new FileInfoItem(new FileInfo("does.not.exist"));

            var getItems = new GetActionsForItem(new[] {act});

            var actionsForItem = getItems.ActionsForItem(info);
            Assert.NotEmpty(actionsForItem);
            Assert.Contains(act, actionsForItem);
        }
        
        [Fact]
        public void CanNotFindActionWhichDoesntActOnItem()
        {
            var act = new MockActOnFileInfo();
            var dontAct = new MockActOnFileInfoWithFilter(false);
            var info = new FileInfoItem(new FileInfo("does.not.exist"));

            var getItems = new GetActionsForItem(new IActOnItem[] {act, dontAct});

            var actionsForItem = getItems.ActionsForItem(info);
            Assert.NotEmpty(actionsForItem);
            Assert.Contains(act, actionsForItem);
            Assert.DoesNotContain(dontAct, actionsForItem);
        }
        
        [Fact]
        public void CommandCanActOnItself()
        {
            var command = new MockCommand();

            var getItems = new GetActionsForItem(new IActOnItem[] {command});

            var actionsForItem = getItems.ActionsForItem(command);
            Assert.NotEmpty(actionsForItem);
            Assert.Contains(command, actionsForItem);

            command.ActOn(command);
            Assert.True(command.Acted);
        }
    }

    class MockCommand : BaseCommand<MockCommand>
    {
        public bool Acted;

        public override void ActOn(ITypedItem<MockCommand> item)
        {
            Acted = true;
        }

        public override string Text
        {
            get { return "not relevant"; }
        }

        public override string Description
        {
            get { return "description"; }
        }

        public override MockCommand Item
        {
            get { return this; }
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


    class MockActOnFileInfoWithFilter : BaseActOnTypedItem<FileInfo>, IActOnTypedItemWithArguments<FileInfo>, ICanActOnTypedItem<FileInfo>
    {
        private readonly bool _canAct;
        public bool Acted;
        public FileInfo Info;
        public string Arguments;

        public MockActOnFileInfoWithFilter(bool canAct)
        {
            _canAct = canAct;
        }
       
        public bool CanActOn(ITypedItem<FileInfo> fileInfo)
        {
            return _canAct;
        }

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