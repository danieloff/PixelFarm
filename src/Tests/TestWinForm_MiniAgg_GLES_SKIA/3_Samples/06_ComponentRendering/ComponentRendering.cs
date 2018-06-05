﻿//BSD, 2014-2018, WinterDev
//MatterHackers

using System;
using PixelFarm.Drawing;
using PixelFarm.Agg.Imaging;
using Mini;
namespace PixelFarm.Agg
{
    [Info(OrderCode = "06")]
    //[Info("AGG has a gray-scale renderer that can use any 8-bit color channel of an RGB or RGBA frame buffer. Most likely it will be used to draw gray-scale images directly in the alpha-channel.")]
    public class ComponentRendering : DemoBase
    {
        public ComponentRendering()
        {
            this.AlphaValue = 255;
        }

        [DemoConfig(MaxValue = 255)]
        public int AlphaValue
        {
            get;
            set;
        }
        [DemoConfig]
        public bool UseBlackBlackground
        {
            get;
            set;
        }

        public override void Draw(PixelFarm.Drawing.Painter p)
        {
            //specific for agg
            if (!(p is AggPainter))
            {
                return;
            }


            AggPainter p2 = (AggPainter)p;
            AggRenderSurface asx = p2.RenderSurface;
            if (asx.DestImage != null)
            {
                IImageReaderWriter backBuffer = asx.DestImage;
                IPixelBlender currentPixelBlender = asx.PixelBlender;
                int distBetween = backBuffer.BytesBetweenPixelsInclusive;
 
                //use different pixel blender 
                var redImageBuffer = new SubImageRW(backBuffer, new PixelBlenderRed());
                var greenImageBuffer = new SubImageRW(backBuffer, new PixelBlenderRed());
                var blueImageBuffer = new SubImageRW(backBuffer, new PixelBlenderRed());
 
                ClipProxyImage clippingProxy = new ClipProxyImage(backBuffer);
                ClipProxyImage clippingProxyRed = new ClipProxyImage(redImageBuffer);
                ClipProxyImage clippingProxyGreen = new ClipProxyImage(greenImageBuffer);
                ClipProxyImage clippingProxyBlue = new ClipProxyImage(blueImageBuffer);
                //
                ScanlineRasterizer sclineRas = asx.ScanlineRasterizer;
                ScanlinePacked8 scline = asx.ScanlinePacked8;
                Drawing.Color clearColor = this.UseBlackBlackground ? Drawing.Color.FromArgb(0, 0, 0) : Drawing.Color.FromArgb(255, 255, 255);
                clippingProxy.Clear(clearColor);
                Drawing.Color fillColor = this.UseBlackBlackground ?
                    new Drawing.Color((byte)(this.AlphaValue), 255, 255, 255) :
                    new Drawing.Color((byte)(this.AlphaValue), 0, 0, 0);


                ScalineRasToDestinationBitmap sclineRasToBmp = asx.ScanlineRasToDestBitmap;
                VertexSource.Ellipse er = new PixelFarm.Agg.VertexSource.Ellipse(Width / 2 - 0.87 * 50, Height / 2 - 0.5 * 50, 100, 100, 100);
                //
                VectorToolBox.GetFreeVxs(out var v1);
                sclineRas.AddPath(er.MakeVxs(v1));
                v1.Clear();
                sclineRasToBmp.RenderWithColor(clippingProxyRed, sclineRas, scline, fillColor);
                
                ////
                VertexSource.Ellipse eg = new PixelFarm.Agg.VertexSource.Ellipse(Width / 2 + 0.87 * 50, Height / 2 - 0.5 * 50, 100, 100, 100);
                sclineRas.AddPath(eg.MakeVertexSnap(v1));
                v1.Clear();
                sclineRasToBmp.RenderWithColor(clippingProxyGreen, sclineRas, scline, fillColor);

                //
                VertexSource.Ellipse eb = new PixelFarm.Agg.VertexSource.Ellipse(Width / 2, Height / 2 + 50, 100, 100, 100);
                sclineRas.AddPath(eb.MakeVertexSnap(v1));
                v1.Clear();
                sclineRasToBmp.RenderWithColor(clippingProxyBlue, sclineRas, scline, fillColor);

                VectorToolBox.ReleaseVxs(ref v1);
            }
            //            else if (graphics2D.DestImageFloat != null)
            //            {
            //#if false
            //                IImageFloat backBuffer = graphics2D.DestImageFloat;

            //                int distBetween = backBuffer.GetFloatsBetweenPixelsInclusive();
            //                ImageBufferFloat redImageBuffer = new ImageBufferFloat();
            //                redImageBuffer.Attach(backBuffer, new blender_gray(distBetween), distBetween, 2, 8);
            //                ImageBufferFloat greenImageBuffer = new ImageBufferFloat();
            //                greenImageBuffer.Attach(backBuffer, new blender_gray(distBetween), distBetween, 1, 8);
            //                ImageBufferFloat blueImageBuffer = new ImageBufferFloat();
            //                blueImageBuffer.Attach(backBuffer, new blender_gray(distBetween), distBetween, 0, 8);

            //                ImageClippingProxy clippingProxy = new ImageClippingProxy(backBuffer);
            //                ImageClippingProxy clippingProxyRed = new ImageClippingProxy(redImageBuffer);
            //                ImageClippingProxy clippingProxyGreen = new ImageClippingProxy(greenImageBuffer);
            //                ImageClippingProxy clippingProxyBlue = new ImageClippingProxy(blueImageBuffer);

            //                ScanlineRasterizer ras = new ScanlineRasterizer();
            //                ScanlineCachePacked8 sl = new ScanlineCachePacked8();

            //                RGBA_Bytes clearColor = useBlackBackgroundCheckbox.Checked ? new RGBA_Bytes(0, 0, 0) : new RGBA_Bytes(255, 255, 255);
            //                clippingProxy.clear(clearColor);
            //                alphaSlider.View.BackGroundColor = clearColor;

            //                RGBA_Bytes FillColor = useBlackBackgroundCheckbox.Checked ? new RGBA_Bytes(255, 255, 255, (int)(alphaSlider.Value)) : new RGBA_Bytes(0, 0, 0, (int)(alphaSlider.Value));

            //                VertexSource.Ellipse er = new AGG.VertexSource.Ellipse(Width / 2 - 0.87 * 50, Height / 2 - 0.5 * 50, 100, 100, 100);
            //                ras.add_path(er);
            //                agg_renderer_scanline.Default.render_scanlines_aa_solid(clippingProxyRed, ras, sl, FillColor);

            //                VertexSource.Ellipse eg = new AGG.VertexSource.Ellipse(Width / 2 + 0.87 * 50, Height / 2 - 0.5 * 50, 100, 100, 100);
            //                ras.add_path(eg);
            //                agg_renderer_scanline.Default.render_scanlines_aa_solid(clippingProxyGreen, ras, sl, FillColor);

            //                VertexSource.Ellipse eb = new AGG.VertexSource.Ellipse(Width / 2, Height / 2 + 50, 100, 100, 100);
            //                ras.add_path(eb);
            //                agg_renderer_scanline.Default.render_scanlines_aa_solid(clippingProxyBlue, ras, sl, FillColor);
            //#endif
            //            }


        }
    }
}
