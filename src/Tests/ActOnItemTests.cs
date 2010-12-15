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
            Assert.Equal(act.Info, info.TypedItem);
        }
        
        [Fact]
        public void CanCallActWithArguments()
        {
            var act = new MockActOnFileInfo();
            var info = new FileInfoItem(new FileInfo("does.not.exist"));

            act.ActOn((IItem)info, "argument");
            Assert.True(act.Acted);
            Assert.Equal(act.Info, info.TypedItem);
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
        public void ExecuteCanActOnCommand()
        {
            var command = new MockCommand();
            var action = new ExecuteCommand();
            var getItems = new GetActionsForItem(new IActOnItem[] {action});

            var actionsForItem = getItems.ActionsForItem(new CommandItem(command));
            Assert.NotEmpty(actionsForItem);
            Assert.Contains(action, actionsForItem);

            action.ActOn(command);
            Assert.True(command.Acted);
        }

        [Fact]
        public void DefaultTextIsCamelCaseOfClassNameWithSpaces()
        {
            var command = new MockActWithoutText();
            Assert.Equal("Mock Act Without Text", command.Text);
        }
    }

    class MockCommand : ICommand
    {
        public bool Acted;

        public void Act()
        {
            Acted = true;
        }

        public string Text
        {
            get { return "not relevant"; }
        }

        public string Description
        {
            get { return "description"; }
        }
    }

    class MockActWithoutText : BaseActOnTypedItem<string>
    {
        public override void ActOn(string item)
        {
            // NO-OP
        }
    }

    class MockActOnFileInfo: BaseActOnTypedItem<FileInfo>, IActOnTypedItemWithArguments<FileInfo>
    {
        public bool Acted;
        public FileInfo Info;
        public string Arguments;

        public override void ActOn(FileInfo item)
        {
            Acted = true;
            Info = item;
        }

        public void ActOn(FileInfo item, string arguments)
        {
            Acted = true;
            Info = item;
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
       
        public bool CanActOn(FileInfo fileInfo)
        {
            return _canAct;
        }

        public override void ActOn(FileInfo item)
        {
            Acted = true;
            Info = item;
        }

        public void ActOn(FileInfo item, string arguments)
        {
            Acted = true;
            Info = item;
            Arguments = arguments;
        }

        public override string Text
        {
            get { return "not relevant"; }
        }
    }
}