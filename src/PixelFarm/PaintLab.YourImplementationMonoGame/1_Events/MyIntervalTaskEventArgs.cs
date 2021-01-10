﻿//Apache2, 2014-present, WinterDev

using PixelFarm.Drawing;
namespace LayoutFarm.UI
{
    class MyIntervalTaskEventArgs : LayoutFarm.RenderBoxes.GraphicsTimerTaskEventArgs
    {
        internal void ClearForReuse()
        {
            this.NeedUpdate = 0;
            this.GraphicUpdateArea = Rectangle.Empty;
        }
    }
}