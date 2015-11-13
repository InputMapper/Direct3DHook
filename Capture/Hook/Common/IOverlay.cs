using System.Collections.Generic;

namespace Capture.Hook.Common
{
    internal interface IOverlay : IOverlayElement
    {
        List<IOverlayElement> Elements { get; set; }
    }
}