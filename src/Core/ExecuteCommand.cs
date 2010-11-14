using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Core.Abstractions;

namespace Core
{
    class ExecuteCommand : IExecuteCommand
    {
        public void Execute(string command)
        {
            var split = command.Split(new char[]{' '}, 1, StringSplitOptions.RemoveEmptyEntries);
            var psi = new ProcessStartInfo(split[0], string.Join(" ", split.Skip(1).ToArray()));

            // TODO: check return values and status and whatnot
            Process.Start(psi);
        }
    }
}