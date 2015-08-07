﻿namespace Rubberduck.UI.Command
{
    public class RunCodeInspectionsCommand : ICommand
    {
        public void Execute()
        {
            throw new System.NotImplementedException();
        }
    }

    public class RunCodeInspectionsCommandMenuItem : CommandMenuItemBase
    {
        public RunCodeInspectionsCommandMenuItem(ICommand command) 
            : base(command)
        {
        }

        public override string Key { get { return RubberduckUI.RubberduckMenu_CodeInspections; } }
        public override int DisplayOrder { get { return (int)RubberduckMenuItemDisplayOrder.CodeInspections; } }
    }
}