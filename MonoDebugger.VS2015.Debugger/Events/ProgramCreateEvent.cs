﻿using Microsoft.VisualStudio.Debugger.Interop;

namespace MonoDebugger.VS2015.Debugger.Events
{
    internal class ProgramCreateEvent : AsynchronousEvent, IDebugProgramCreateEvent2
    {
        public const string IID = "96CD11EE-ECD4-4E89-957E-B5D496FC4139";
    }
}