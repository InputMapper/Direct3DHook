using System.Collections.Generic;

namespace Direct3DHookLib.Hook.Common
{
    internal interface IOverlay : IOverlayElement
    {
        List<IOverlayElement> Elements { get; set; }
    }
}