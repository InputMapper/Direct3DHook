﻿using System;

namespace Direct3DHookLib.Interface
{
    [Serializable]
    public class ScreenshotReceivedEventArgs : MarshalByRefObject
    {
        public ScreenshotReceivedEventArgs(int processId, Screenshot screenshot)
        {
            ProcessId = processId;
            Screenshot = screenshot;
        }

        public int ProcessId { get; set; }
        public Screenshot Screenshot { get; set; }
    }
}