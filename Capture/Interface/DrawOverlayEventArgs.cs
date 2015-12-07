using System;
using System.Collections.Generic;
using Direct3DHookLib.Hook.Common;

namespace Direct3DHookLib.Interface
{
    [Serializable]
    public class DrawOverlayEventArgs : MarshalByRefObject
    {
        public List<IOverlayElement> OverlayElements { get; set; }

        public bool IsUpdatePending { get; set; }

        public DrawOverlayEventArgs()
        {
            OverlayElements = new List<IOverlayElement>();
        }
    }
}
