﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.Drawing.WinGdi;
namespace LayoutFarm.UI.GdiPlus
{
    class GdiPlusCanvasViewport : CanvasViewport
    {
        QuadPages quadPages = null;
        public GdiPlusCanvasViewport(RootGraphic rootgfx,
            Size viewportSize, int cachedPageNum)
            : base(rootgfx, viewportSize, cachedPageNum)
        {
            quadPages = new QuadPages(cachedPageNum, viewportSize.Width, viewportSize.Height);
            this.CalculateCanvasPages();
        }

        ~GdiPlusCanvasViewport()
        {
            if (quadPages != null)
            {
                quadPages.Dispose();
            }
        }

        //static int dbugCount = 0;
        protected override void OnClosing()
        {
            if (quadPages != null)
            {
                quadPages.Dispose();
                quadPages = null;
            }
            base.OnClosing();
        }

#if DEBUG
        //int dbugCount;
#endif
        public override void CanvasInvalidateArea(Rectangle r)
        {
            quadPages.CanvasInvalidate(r);
#if DEBUG
            //Console.WriteLine("CanvasInvalidateArea:" + (dbugCount++).ToString() + " " + r.ToString());
#endif
        }
        public override bool IsQuadPageValid
        {
            get
            {
                return this.quadPages.IsValid;
            }
        }
        protected override void ResetQuadPages(int viewportWidth, int viewportHeight)
        {
            quadPages.ResizeAllPages(viewportWidth, viewportHeight);
        }
        protected override void CalculateCanvasPages()
        {
            quadPages.CalculateCanvasPages(this.ViewportX, this.ViewportY, this.ViewportWidth, this.ViewportHeight);
            this.FullMode = true;
        }
        public void PaintMe2(IntPtr hdc, Rectangle invalidateArea)
        {
            if (this.IsClosed) { return; }
            //------------------------------------ 

            this.rootGraphics.PrepareRender();
            //---------------
            this.rootGraphics.IsInRenderPhase = true;
#if DEBUG
            this.rootGraphics.dbug_rootDrawingMsg.Clear();
            this.rootGraphics.dbug_drawLevel = 0;
#endif
            if (this.FullMode)
            {
                quadPages.RenderToOutputWindowFullMode(
                    rootGraphics.TopWindowRenderBox, hdc,
                    this.ViewportX, this.ViewportY, this.ViewportWidth, this.ViewportHeight);
            }
            else
            {
                //temp to full mode
                //quadPages.RenderToOutputWindowFullMode(rootGraphics.TopWindowRenderBox, hdc, this.ViewportX, this.ViewportY, this.ViewportWidth, this.ViewportHeight);
                quadPages.RenderToOutputWindowPartialMode2(
                   rootGraphics.TopWindowRenderBox, hdc,
                   this.ViewportX, this.ViewportY, this.ViewportWidth, this.ViewportHeight, invalidateArea);
            }
            this.rootGraphics.IsInRenderPhase = false;
#if DEBUG

            RootGraphic visualroot = RootGraphic.dbugCurrentGlobalVRoot;
            if (visualroot.dbug_RecordDrawingChain)
            {
                List<dbugLayoutMsg> outputMsgs = dbugOutputWindow.dbug_rootDocDebugMsgs;
                outputMsgs.Clear();
                outputMsgs.Add(new dbugLayoutMsg(null as RenderElement, "[" + debug_render_to_output_count + "]"));
                visualroot.dbug_DumpRootDrawingMsg(outputMsgs);
                dbugOutputWindow.dbug_InvokeVisualRootDrawMsg();
                debug_render_to_output_count++;
            }


            if (dbugHelper01.dbugVE_HighlightMe != null)
            {
                dbugOutputWindow.dbug_HighlightMeNow(dbugHelper01.dbugVE_HighlightMe.dbugGetGlobalRect());
            }
#endif
        }

        public void PaintMe(IntPtr hdc)
        {
            //paint the content to target hdc

            if (this.IsClosed) { return; }
            //------------------------------------ 

            this.rootGraphics.PrepareRender();
            //---------------
            this.rootGraphics.IsInRenderPhase = true;
#if DEBUG
            this.rootGraphics.dbug_rootDrawingMsg.Clear();
            this.rootGraphics.dbug_drawLevel = 0;
#endif
            if (this.FullMode)
            {
                quadPages.RenderToOutputWindowFullMode(
                    rootGraphics.TopWindowRenderBox, hdc,
                    this.ViewportX, this.ViewportY, this.ViewportWidth, this.ViewportHeight);
            }
            else
            {
                //temp to full mode
                //quadPages.RenderToOutputWindowFullMode(rootGraphics.TopWindowRenderBox, hdc, this.ViewportX, this.ViewportY, this.ViewportWidth, this.ViewportHeight);
                quadPages.RenderToOutputWindowPartialMode(
                   rootGraphics.TopWindowRenderBox, hdc,
                   this.ViewportX, this.ViewportY, this.ViewportWidth, this.ViewportHeight);
            }
            this.rootGraphics.IsInRenderPhase = false;
#if DEBUG

            RootGraphic visualroot = RootGraphic.dbugCurrentGlobalVRoot;
            if (visualroot.dbug_RecordDrawingChain)
            {
                List<dbugLayoutMsg> outputMsgs = dbugOutputWindow.dbug_rootDocDebugMsgs;
                outputMsgs.Clear();
                outputMsgs.Add(new dbugLayoutMsg(null as RenderElement, "[" + debug_render_to_output_count + "]"));
                visualroot.dbug_DumpRootDrawingMsg(outputMsgs);
                dbugOutputWindow.dbug_InvokeVisualRootDrawMsg();
                debug_render_to_output_count++;
            }


            if (dbugHelper01.dbugVE_HighlightMe != null)
            {
                dbugOutputWindow.dbug_HighlightMeNow(dbugHelper01.dbugVE_HighlightMe.dbugGetGlobalRect());
            }
#endif
        }


        public void PaintMe(GdiPlusDrawBoard mycanvas)
        {
            if (this.IsClosed) { return; }
            //------------------------------------ 

            this.rootGraphics.PrepareRender();
            //---------------
            this.rootGraphics.IsInRenderPhase = true;
#if DEBUG
            this.rootGraphics.dbug_rootDrawingMsg.Clear();
            this.rootGraphics.dbug_drawLevel = 0;
#endif

            mycanvas.OffsetCanvasOrigin(-mycanvas.Left, -mycanvas.Top);
            Rectangle rect = mycanvas.Rect;
            this.rootGraphics.TopWindowRenderBox.DrawToThisCanvas(mycanvas, rect);
#if DEBUG
            this.rootGraphics.TopWindowRenderBox.dbugShowRenderPart(mycanvas, rect);
#endif

            mycanvas.IsContentReady = true;
            mycanvas.OffsetCanvasOrigin(mycanvas.Left, mycanvas.Top);
            //if (this.FullMode)
            //{
            //    quadPages.RenderToOutputWindowFullMode(rootGraphics.TopWindowRenderBox, hdc,
            //        this.ViewportX, this.ViewportY, this.ViewportWidth, this.ViewportHeight);
            //}
            //else
            //{
            //    //temp to full mode
            //    //quadPages.RenderToOutputWindowFullMode(rootGraphics.TopWindowRenderBox, hdc, this.ViewportX, this.ViewportY, this.ViewportWidth, this.ViewportHeight);
            //    quadPages.RenderToOutputWindowPartialMode(rootGraphics.TopWindowRenderBox, hdc,
            //        this.ViewportX, this.ViewportY, this.ViewportWidth, this.ViewportHeight);

            //}
            this.rootGraphics.IsInRenderPhase = false;
#if DEBUG

            RootGraphic visualroot = RootGraphic.dbugCurrentGlobalVRoot;
            if (visualroot.dbug_RecordDrawingChain)
            {
                List<dbugLayoutMsg> outputMsgs = dbugOutputWindow.dbug_rootDocDebugMsgs;
                outputMsgs.Clear();
                outputMsgs.Add(new dbugLayoutMsg(null as RenderElement, "[" + debug_render_to_output_count + "]"));
                visualroot.dbug_DumpRootDrawingMsg(outputMsgs);
                dbugOutputWindow.dbug_InvokeVisualRootDrawMsg();
                debug_render_to_output_count++;
            }


            if (dbugHelper01.dbugVE_HighlightMe != null)
            {
                dbugOutputWindow.dbug_HighlightMeNow(dbugHelper01.dbugVE_HighlightMe.dbugGetGlobalRect());
            }
#endif
        }
    }
}