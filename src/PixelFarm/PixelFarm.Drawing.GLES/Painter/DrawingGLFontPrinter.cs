﻿//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;
//
using PixelFarm.CpuBlit;
using PixelFarm.Drawing;
using PixelFarm.Drawing.Fonts;
//
using Typography.TextLayout;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;


namespace PixelFarm.DrawingGL
{



    public class AggTextSpanPrinter : ITextPrinter
    {
        ActualBitmap _actualImage;

        AggPainter _aggPainter;
        VxsTextPrinter _vxsTextPrinter;
        int bmpWidth;
        int bmpHeight;
        GLRenderSurface _glsx;
        GLPainter canvasPainter;
        LayoutFarm.OpenFontTextService _openFontTextServices;
        public AggTextSpanPrinter(GLPainter canvasPainter, int w, int h)
        {
            //this class print long text into agg canvas
            //then copy pixel buffer from aff canvas to gl-bmp
            //then draw the  gl-bmp into target gl canvas


            //TODO: review here
            this.canvasPainter = canvasPainter;
            this._glsx = canvasPainter.Canvas;
            bmpWidth = w;
            bmpHeight = h;

            _actualImage = new ActualBitmap(bmpWidth, bmpHeight);
            _aggPainter = AggPainter.Create(_actualImage);
            _aggPainter.FillColor = Color.Black;
            _aggPainter.StrokeColor = Color.Black;

            //set default1
            _aggPainter.CurrentFont = canvasPainter.CurrentFont;

            _openFontTextServices = new LayoutFarm.OpenFontTextService();
            _vxsTextPrinter = new VxsTextPrinter(_aggPainter, _openFontTextServices);
            _aggPainter.TextPrinter = _vxsTextPrinter;
        }
        public bool StartDrawOnLeftTop { get; set; }
        public Typography.Contours.HintTechnique HintTechnique
        {
            get { return _vxsTextPrinter.HintTechnique; }
            set { _vxsTextPrinter.HintTechnique = value; }
        }
        public bool UseSubPixelRendering
        {
            get { return _aggPainter.UseSubPixelLcdEffect; }
            set
            {
                _aggPainter.UseSubPixelLcdEffect = value;
            }
        }
        public void ChangeFont(RequestFont font)
        {

            _aggPainter.CurrentFont = font;
        }
        public void ChangeFillColor(Color fillColor)
        {
            //we use agg canvas to draw a font glyph
            //so we must set fill color for this
            _aggPainter.FillColor = fillColor;
        }
        public void ChangeStrokeColor(Color strokeColor)
        {
            //we use agg canvas to draw a font glyph
            //so we must set fill color for this
            _aggPainter.StrokeColor = strokeColor;
        }
        public void DrawString(char[] text, int startAt, int len, double x, double y)
        {


            if (this.UseSubPixelRendering)
            {
                //1. clear prev drawing result
                _aggPainter.Clear(Drawing.Color.FromArgb(0, 0, 0, 0));
                //aggPainter.Clear(Drawing.Color.White);
                //aggPainter.Clear(Drawing.Color.FromArgb(0, 0, 0, 0));
                //2. print text span into Agg Canvas
                _vxsTextPrinter.DrawString(text, startAt, len, 0, 0);
                //3.copy to gl bitmap
                //byte[] buffer = PixelFarm.Agg.ActualImage.GetBuffer(_actualImage);
                //------------------------------------------------------
                GLBitmap glBmp = new GLBitmap(new LazyActualBitmapBufferProvider(_actualImage));
                glBmp.IsYFlipped = false;
                //TODO: review font height
                if (StartDrawOnLeftTop)
                {
                    y -= _vxsTextPrinter.FontLineSpacingPx;
                }
                _glsx.DrawGlyphImageWithSubPixelRenderingTechnique(glBmp, (float)x, (float)y);
                glBmp.Dispose();
            }
            else
            {

                //1. clear prev drawing result
                _aggPainter.Clear(Drawing.Color.White);
                _aggPainter.StrokeColor = Color.Black;

                //2. print text span into Agg Canvas
                _vxsTextPrinter.StartDrawOnLeftTop = false;

                float dyOffset = _vxsTextPrinter.FontDescedingPx;
                _vxsTextPrinter.DrawString(text, startAt, len, 0, -dyOffset);
                //------------------------------------------------------
                //debug save image from agg's buffer
#if DEBUG
                //actualImage.dbugSaveToPngFile("d:\\WImageTest\\aa1.png");
#endif
                //------------------------------------------------------

                //3.copy to gl bitmap
                //byte[] buffer = PixelFarm.Agg.ActualImage.GetBuffer(_actualImage);
                //------------------------------------------------------
                //debug save image from agg's buffer 

                //------------------------------------------------------
                //GLBitmap glBmp = new GLBitmap(bmpWidth, bmpHeight, buffer, true);
                GLBitmap glBmp = new GLBitmap(new LazyActualBitmapBufferProvider(_actualImage));
                glBmp.IsYFlipped = false;
                //TODO: review font height 
                //if (StartDrawOnLeftTop)
                //{
                y += _vxsTextPrinter.FontLineSpacingPx;
                //}
                _glsx.DrawGlyphImage(glBmp, (float)x, (float)y + dyOffset);
                glBmp.Dispose();
            }
        }
        public void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] text, int start, int len)
        {
            throw new NotImplementedException();
        }
        public void DrawString(RenderVxFormattedString renderVx, double x, double y)
        {
            throw new NotImplementedException();
        }
    }

    public enum GlyphTexturePrinterDrawingTechnique
    {
        Copy,
        Stencil,
        LcdSubPixelRendering,
        Msdf
    }

    public class GLBitmapGlyphTextPrinter : ITextPrinter, IDisposable
    {
        MySimpleGLBitmapFontManager _myGLBitmapFontMx;
        SimpleFontAtlas _fontAtlas;
        GLRenderSurface _glsx;
        GLPainter _painter;
        GLBitmap _glBmp;
        RequestFont _font;
        LayoutFarm.OpenFontTextService _textServices;
        float _px_scale = 1;

        List<float> _vboBufferList = new List<float>();
        List<ushort> _indexList = new List<ushort>();

#if DEBUG
        public static GlyphTexturePrinterDrawingTechnique s_dbugDrawTechnique = GlyphTexturePrinterDrawingTechnique.Copy;
        public static bool s_useVBO = true;
#endif
        /// <summary>
        /// use vertex buffer object
        /// </summary>

        public GLBitmapGlyphTextPrinter(GLPainter painter, LayoutFarm.OpenFontTextService textServices)
        {
            //create text printer for use with canvas painter           
            _painter = painter;
            _glsx = painter.Canvas;
            _textServices = textServices;

            //_currentTextureKind = TextureKind.Msdf; 
            //_currentTextureKind = TextureKind.StencilGreyScale;

            _myGLBitmapFontMx = new MySimpleGLBitmapFontManager(TextureKind.StencilLcdEffect, textServices);
            //change this to fit yours.
            _myGLBitmapFontMx.SetCurrentScriptLangs(
                new ScriptLang[]
                {
                    ScriptLangs.Latin,
                    ScriptLangs.Thai //eg. Thai, for test with complex script, you can change to your own
                });

            //test textures...

            //GlyphPosPixelSnapX = GlyphPosPixelSnapKind.Integer;
            //GlyphPosPixelSnapY = GlyphPosPixelSnapKind.Integer;
            //**
            ChangeFont(painter.CurrentFont);
            //
            DrawingTechnique = GlyphTexturePrinterDrawingTechnique.Copy; //default 
            UseVBO = true;
        }

        public bool UseVBO { get; set; }
        public GlyphTexturePrinterDrawingTechnique DrawingTechnique { get; set; }
        public void ChangeFillColor(Color color)
        {
            //called by owner painter  
            _painter.FontFillColor = color;
        }
        public void ChangeStrokeColor(Color strokeColor)
        {
            //TODO: implementation here
        }
        public bool StartDrawOnLeftTop { get; set; }


        public void ChangeFont(RequestFont font)
        {
            if (this._font == font)
            {
                return;
            }
            //font has been changed, 
            //resolve for the new one 
            //check if we have this texture-font atlas in our MySimpleGLBitmapFontManager 
            //if not-> request the MySimpleGLBitmapFontManager to create a newone 
            _fontAtlas = _myGLBitmapFontMx.GetFontAtlas(font, out _glBmp);
            _font = font;

            Typeface typeface = _textServices.ResolveTypeface(font);
            _px_scale = typeface.CalculateScaleToPixelFromPointSize(font.SizeInPoints);
        }
        public void Dispose()
        {
            _myGLBitmapFontMx.Clear();
            _myGLBitmapFontMx = null;

            if (_glBmp != null)
            {
                _glBmp.Dispose();
                _glBmp = null;
            }
        }


        public void DrawString(char[] buffer, int startAt, int len, double left, double top)
        {


            _glsx.FontFillColor = _painter.FontFillColor;
            int j = buffer.Length;

            //create temp buffer span that describe the part of a whole char buffer
            TextBufferSpan textBufferSpan = new TextBufferSpan(buffer, startAt, len);

            //ask text service to parse user input char buffer and create a glyph-plan-sequence (list of glyph-plan) 
            //with specific request font
            GlyphPlanSequence glyphPlanSeq = _textServices.CreateGlyphPlanSeq(ref textBufferSpan, _font);
            float scale = _px_scale;

            //--------------------------
            //TODO:
            //if (x,y) is left top
            //we need to adjust y again            

            float bottom = (float)top + _font.LineSpacingInPixels; //  recommendLineSpacing

            //EnsureLoadGLBmp();
            // 
            float scaleFromTexture = 1;
            TextureKind textureKind = _fontAtlas.TextureKind;

            float g_left = 0;
            float g_top = 0;
            int baseY = (int)Math.Round(bottom);

            //int n = glyphPlanSeq.len;
            //int endBefore = glyphPlanSeq.startAt + n;

            //-------------------------------------
            _glsx.LoadTexture1(_glBmp);
            //-------------------------------------

            _vboBufferList.Clear(); //clear before use
            _indexList.Clear(); //clear before use


            float acc_x = 0; //local accumulate x
            float acc_y = 0; //local accumulate y 

#if DEBUG
            _glsx.DrawImage(_glBmp, 0, 0);
            _painter.StrokeColor = Color.Magenta;
            _painter.DrawLine(left, bottom, left + 200, bottom);
            _painter.StrokeColor = Color.Blue;

            DrawingTechnique = s_dbugDrawTechnique;//for debug only
            UseVBO = s_useVBO;//for debug only
#endif


            int seqLen = glyphPlanSeq.Count;
            for (int i = 0; i < seqLen; ++i)
            {
                UnscaledGlyphPlan glyph = glyphPlanSeq[i];
                Typography.Rendering.TextureGlyphMapData glyphData;
                if (!_fontAtlas.TryGetGlyphMapData(glyph.glyphIndex, out glyphData))
                {
                    //if no glyph data, we should render a missing glyph ***
                    continue;
                }
                //--------------------------------------
                //TODO: review precise height in float
                //-------------------------------------- 


                //paint src rect
                //temp fix, glyph texture img is not flipped
                //but the associate info is flipped => so
                //we need remap exact Y from the image 

                Rectangle srcRect =
                      new Rectangle(glyphData.Left,
                         _glBmp.Height - (glyphData.Top + glyphData.Height),
                          glyphData.Width,
                          glyphData.Height);

                //offset length from 'base-line'
                float x_offset = acc_x + (float)Math.Round(glyph.OffsetX * scale) - glyphData.TextureXOffset;
                float y_offset = acc_y + (float)Math.Round(glyph.OffsetY * scale) - glyphData.TextureYOffset + srcRect.Height; //***

                //NOTE:
                // -glyphData.TextureXOffset => restore to original pos
                // -glyphData.TextureYOffset => restore to original pos 
                //--------------------------              

                g_left = (float)(left + x_offset);
                g_top = (float)(baseY - y_offset); //***

                acc_x += (float)Math.Round(glyph.AdvanceX * scale);

                //g_x = (float)Math.Round(g_x); //***
                g_top = (float)Math.Floor(g_top);//adjust to integer num ***

#if DEBUG

                //draw yellow-rect-marker on original texture

                _painter.DrawRectangle(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, Color.Yellow);

                //draw debug-rect box at target glyph position
                _painter.DrawRectangle(g_left, g_top, srcRect.Width, srcRect.Height, Color.Black);

                _painter.StrokeColor = Color.Red;
                //draw debug-line of top pos with blue color
                _painter.DrawLine(left, g_top, left + 200, g_top);
                _painter.StrokeColor = Color.Blue;
#endif 
                if (textureKind == TextureKind.Msdf)
                {
                    _glsx.DrawSubImageWithMsdf(_glBmp,
                        ref srcRect,
                        g_left,
                        g_top,
                        scaleFromTexture);
                }
                else
                {
                    switch (DrawingTechnique)
                    {


                        case GlyphTexturePrinterDrawingTechnique.Stencil:
                            {
                                //stencil gray scale with fill-color
                                _glsx.DrawGlyphImageWithStecil(_glBmp,
                                    ref srcRect,
                                    g_left,
                                    g_top,
                                    scaleFromTexture);
                            }
                            break;
                        case GlyphTexturePrinterDrawingTechnique.Copy:
                            {
                                _glsx.DrawSubImage(_glBmp,
                                    ref srcRect,
                                    g_left,
                                    g_top,
                                    1);
                            }
                            break;
                        case GlyphTexturePrinterDrawingTechnique.LcdSubPixelRendering:
                            if (UseVBO)
                            {
                                _glsx.WriteVboToList(
                                  _vboBufferList,
                                  _indexList,
                                  ref srcRect,
                                  g_left,
                                  g_top,
                                  1);
                            }
                            else
                            {
                                _glsx.DrawGlyphImageWithSubPixelRenderingTechnique2(
                                 ref srcRect,
                                    g_left,
                                    g_top,
                                    1);
                            }
                            break;
                    }
                }
            }
            //-------------------------------------------
            //
            if (DrawingTechnique == GlyphTexturePrinterDrawingTechnique.LcdSubPixelRendering && UseVBO)
            {
                _glsx.DrawGlyphImageWithSubPixelRenderingTechnique3(_vboBufferList.ToArray(), _indexList.ToArray());
            }
        }


        public void DrawString(RenderVxFormattedString renderVx, double x, double y)
        {
            _glsx.LoadTexture1(_glBmp);

            _glsx.FontFillColor = _painter.FontFillColor;
            DrawingGL.GLRenderVxFormattedString renderVxString1 = (DrawingGL.GLRenderVxFormattedString)renderVx;
            DrawingGL.VertexBufferObject2 vbo = renderVxString1.GetVbo();

            vbo.Bind();
            _glsx.DrawGlyphImageWithSubPixelRenderingTechnique4(renderVxString1.VertexCount, (float)x, (float)y);
            vbo.UnBind();

        }

        public void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] buffer, int startAt, int len)
        {


            int j = buffer.Length;

            //create temp buffer span that describe the part of a whole char buffer
            TextBufferSpan textBufferSpan = new TextBufferSpan(buffer, startAt, len);

            //ask text service to parse user input char buffer and create a glyph-plan-sequence (list of glyph-plan) 
            //with specific request font
            GlyphPlanSequence glyphPlanSeq = _textServices.CreateGlyphPlanSeq(ref textBufferSpan, _font);

            float scale = _px_scale;
            int recommendLineSpacing = (int)_font.LineSpacingInPixels;

            //--------------------------
            //TODO:
            //if (x,y) is left top
            //we need to adjust y again
            float x = 0;
            float y = 0;

            y -= ((recommendLineSpacing) * scale);
            renderVx.RecommmendLineSpacing = (int)(recommendLineSpacing * scale);

            // 
            float scaleFromTexture = 1;
            TextureKind textureKind = _fontAtlas.TextureKind;

            //--------------------------

            //TODO: review render steps 
            //NOTE:
            // -glyphData.TextureXOffset => restore to original pos
            // -glyphData.TextureYOffset => restore to original pos
            // ideal_x = (float)(x + (glyph.x * scale - glyphData.TextureXOffset) * scaleFromTexture);
            // ideal_y = (float)(y + (glyph.y * scale - glyphData.TextureYOffset + srcRect.Height) * scaleFromTexture);
            //--------------------------

            float g_x = 0;
            float g_y = 0;
            int baseY = (int)Math.Round(y);


            //***
            _glsx.SetAssociatedTextureInfo(_glBmp);
            //
            _vboBufferList.Clear();
            _indexList.Clear();

            float acc_x = 0;
            float acc_y = 0;

            int seqLen = glyphPlanSeq.Count;
            for (int i = 0; i < seqLen; ++i)
            {
                //from unscaled glyph plan
                //we must scale it to pixel size with px_scale

                UnscaledGlyphPlan glyph = glyphPlanSeq[i];

                Typography.Rendering.TextureGlyphMapData glyphData;
                if (!_fontAtlas.TryGetGlyphMapData(glyph.glyphIndex, out glyphData))
                {
                    //if no glyph data, we should render a missing glyph ***
                    continue;
                }

                //--------------------------------------
                //TODO: review precise height in float
                //-------------------------------------- 
                PixelFarm.Drawing.Rectangle srcRect =
                          new Rectangle(glyphData.Left,
                          glyphData.Top,
                          glyphData.Width,
                          glyphData.Height);

                float ngx = acc_x + (float)Math.Round(glyph.OffsetX * scale);
                float ngy = acc_y + (float)Math.Round(glyph.OffsetY * scale);

                //NOTE:
                // -glyphData.TextureXOffset => restore to original pos
                // -glyphData.TextureYOffset => restore to original pos 
                //--------------------------
                g_x = (float)(x + (ngx - glyphData.TextureXOffset) * scaleFromTexture); //ideal x
                g_y = (float)(y + (ngy - glyphData.TextureYOffset + srcRect.Height) * scaleFromTexture);


                acc_x += (float)Math.Round(glyph.AdvanceX * scale);
                g_y = (float)Math.Floor(g_y);



                switch (textureKind)
                {
                    case TextureKind.Msdf:

                        _glsx.DrawSubImageWithMsdf(_glBmp,
                            ref srcRect,
                            g_x,
                            g_y,
                            scaleFromTexture);

                        break;
                    case TextureKind.StencilGreyScale:

                        //stencil gray scale with fill-color
                        _glsx.DrawGlyphImageWithStecil(_glBmp,
                         ref srcRect,
                            g_x,
                            g_y,
                            scaleFromTexture);

                        break;
                    case TextureKind.Bitmap:
                        _glsx.DrawSubImage(_glBmp,
                         ref srcRect,
                            g_x,
                            g_y,
                            scaleFromTexture);
                        break;
                    case TextureKind.StencilLcdEffect:
                        _glsx.WriteVboToList(
                          _vboBufferList,
                          _indexList,
                          ref srcRect,
                          g_x,
                          g_y,
                          scaleFromTexture);
                        break;
                }
            }
            //--------- 

            DrawingGL.GLRenderVxFormattedString renderVxFormattedString = (DrawingGL.GLRenderVxFormattedString)renderVx;
            renderVxFormattedString.IndexArray = _indexList.ToArray();
            renderVxFormattedString.VertexCoords = _vboBufferList.ToArray();
            renderVxFormattedString.VertexCount = _indexList.Count;
        }
    }

}


