﻿//MIT, 2017-present, WinterDev
namespace LayoutFarm.UI
{
    public enum InnerViewportKind
    {
        PureAgg,
        AggOnGLES,
        GdiPlus,
        GdiPlusOnGLES, //temporary! 
        Skia,
        GLES,
        MonoGame
    }
}