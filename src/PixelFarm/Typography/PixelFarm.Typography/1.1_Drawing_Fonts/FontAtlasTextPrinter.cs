﻿//MIT, 2016-present, WinterDev, Sam Hocevar
using System;

using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.PixelProcessing;
using Typography.OpenFont;
using Typography.Rendering;
using Typography.TextLayout;

namespace PixelFarm.Drawing.Fonts
{

    public class FontAtlasTextPrinter : DevTextPrinterBase, ITextPrinter
    {
        PixelBlenderWithMask maskPixelBlender = new PixelBlenderWithMask();
        PixelBlenderPerColorComponentWithMask maskPixelBlenderPerCompo = new PixelBlenderPerColorComponentWithMask();

        AggPainter _maskBufferPainter;
        ActualBitmap _fontBmp;
        ActualBitmap _alphaBmp;
        float _finalTextureScale = 1;

        /// <summary>
        /// target canvas
        /// </summary>
        AggPainter _painter;
        RequestFont _font;
        //-----------------------------------------------------------  
        Typeface _currentTypeface;
        Color _fontColor;

        float _currentFontSizePxScale;
        bool _useLcdTech;

        LayoutFarm.OpenFontTextService _textServices;
        BitmapFontManager<ActualBitmap> _bmpFontMx;
        SimpleFontAtlas _fontAtlas;

        public FontAtlasTextPrinter(Painter painter)
        {
            StartDrawOnLeftTop = true;
            this._painter = (AggPainter)painter;

            this.PositionTechnique = PositionTechnique.OpenFont;

            _textServices = new LayoutFarm.OpenFontTextService();
            //2. create manager
            _bmpFontMx = new BitmapFontManager<ActualBitmap>(
                TextureKind.StencilLcdEffect,
                _textServices,
                atlas =>
                {
                    GlyphImage totalGlyphImg = atlas.TotalGlyph;
                    return new ActualBitmap(totalGlyphImg.Width, totalGlyphImg.Height, totalGlyphImg.GetImageBuffer());
                }
            );
            _bmpFontMx.SetCurrentScriptLangs(new ScriptLang[]
            {
                ScriptLangs.Latin
            });

            //3.  

            ChangeFont(new RequestFont("tahoma", 10));
            SetupMaskPixelBlender(painter.Width, painter.Height);
        }
        /// <summary>
        /// start draw on 'left-top' of a given area box
        /// </summary>
        public bool StartDrawOnLeftTop { get; set; }

        public Painter TargetCanvasPainter
        {
            get
            {
                return _painter;
            }
            set
            {
                _painter = (AggPainter)value;
            }
        }

        /// <summary>
        /// for layout that use with our  lcd subpixel rendering technique 
        /// </summary>
        public bool UseWithLcdSubPixelRenderingTechnique
        {
            get { return _useLcdTech; }
            set
            {
                _useLcdTech = value;
            }
        }
        public void ChangeFont(RequestFont font)
        {
            //call to service
            _font = font;
            _textServices.ResolveTypeface(font); //resolve for 'actual' font
            _fontAtlas = _bmpFontMx.GetFontAtlas(_font, out _fontBmp);
        }
        public RequestFont CurrentFont
        {
            get { return _font; }

        }

        public void ChangeFillColor(Color fontColor)
        {
            //change font color
            _fontColor = fontColor;
        }
        public void ChangeStrokeColor(Color strokeColor)
        {
            //TODO: ...
        }
        protected override void OnFontSizeChanged()
        {
            //update some font metrics property   
            Typeface currentTypeface = _currentTypeface;
            if (currentTypeface != null)
            {
                float pointToPixelScale = currentTypeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints);
                this.FontAscendingPx = currentTypeface.Ascender * pointToPixelScale;
                this.FontDescedingPx = currentTypeface.Descender * pointToPixelScale;
                this.FontLineGapPx = currentTypeface.LineGap * pointToPixelScale;
                this.FontLineSpacingPx = FontAscendingPx - FontDescedingPx + FontLineGapPx;
            }

        }
        public override GlyphLayout GlyphLayoutMan
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override Typeface Typeface
        {
            get
            {
                return _currentTypeface;
            }
            set
            {

                if (_currentTypeface == value) return;
                //
                _currentTypeface = value;
                OnFontSizeChanged();
            }
        }


        void SetupMaskPixelBlender(int width, int height)
        {
            //----------
            //same size
            _alphaBmp = new ActualBitmap(width, height);
            _maskBufferPainter = AggPainter.Create(_alphaBmp, new PixelBlenderBGRA());
            _maskBufferPainter.Clear(Color.Black);
            //------------ 
            //draw glyph bmp to _alpha bmp
            //_maskBufferPainter.DrawImage(_glyphBmp, 0, 0);
            maskPixelBlender.SetMaskBitmap(_alphaBmp);
            maskPixelBlenderPerCompo.SetMaskBitmap(_alphaBmp);
        }
        public void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] text, int startAt, int len)
        {

            //1. update some props.. 
            //2. update current type face
            UpdateGlyphLayoutSettings();
            Typeface typeface = _currentTypeface;
        }

        public override void DrawCaret(float x, float y)
        {
            //TODO: remove draw caret here, this is for debug only 

        }

        public void UpdateGlyphLayoutSettings()
        {
            if (this._font == null)
            {
                //this.ScriptLang = canvasPainter.CurrentFont.GetOpenFontScriptLang();
                ChangeFont(_painter.CurrentFont);
            }

            ////2.1              
            //_glyphMeshStore.SetHintTechnique(this.HintTechnique);
            ////2.2
            //_glyphLayout.Typeface = this.Typeface;
            //_glyphLayout.ScriptLang = this.ScriptLang;
            //_glyphLayout.PositionTechnique = this.PositionTechnique;
            //_glyphLayout.EnableLigature = this.EnableLigature;

            //_currentFontSizePxScale = Typeface.CalculateScaleToPixelFromPointSize(FontSizeInPoints);

            ////2.3
            //if (_pxScaleEngine != null)
            //{
            //    _pxScaleEngine.SetFont(this.Typeface, this.FontSizeInPoints);
            //}
            ////3. layout glyphs with selected layout technique
            ////TODO: review this again, we should use pixel?




            ////3.
            ////color...
        }

        /// <summary>
        /// draw specfic glyph with current settings, at specific position
        /// </summary>
        /// <param name="glyph"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawGlyph(Glyph glyph, double x, double y)
        {
            //TODO...
        }
        public void DrawString(RenderVxFormattedString renderVx, double x, double y)
        {
            //TODO...
        }
        public override void DrawFromGlyphPlans(PxScaledGlyphPlanList glyphPlanList, int startAt, int len, float x, float y)
        {
            //TODO...
        }
        public override void DrawFromGlyphPlans(UnscaledGlyphPlanList glyphPlanList, int startAt, int len, float x, float y)
        {
            //TODO...

        }
        public void DrawString(char[] text, int startAt, int len, double x, double y)
        {
            InternalDrawString(text, startAt, len, (float)x, (float)y);
        }
        public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {
            InternalDrawString(textBuffer, startAt, len, x, y);
        }

        void InternalDrawString(char[] buffer, int startAt, int len, float x, float y)
        {


            int j = buffer.Length;
            //create temp buffer span that describe the part of a whole char buffer
            TextBufferSpan textBufferSpan = new TextBufferSpan(buffer, startAt, len);
            //ask text service to parse user input char buffer and create a glyph-plan-sequence (list of glyph-plan) 
            //with specific request font
            GlyphPlanSequence glyphPlanSeq = _textServices.CreateGlyphPlanSeq(ref textBufferSpan, _font);

            float scale = _fontAtlas.TargetTextureScale;
            int recommendLineSpacing = _fontAtlas.OriginalRecommendLineSpacing;
            //--------------------------
            //TODO:
            //if (x,y) is left top
            //we need to adjust y again
            y -= ((_fontAtlas.OriginalRecommendLineSpacing) * scale);

            // 
            float scaleFromTexture = _finalTextureScale;
            TextureKind textureKind = _fontAtlas.TextureKind;

            float gx = 0;
            float gy = 0;
            int baseY = (int)Math.Round(y);
            int n = glyphPlanSeq.len;
            int endBefore = glyphPlanSeq.startAt + n;
            //------------------------------------- 

            float acc_x = 0;
            float acc_y = 0;
            UnscaledGlyphPlanList glyphPlanList = GlyphPlanSequence.UnsafeGetInteralGlyphPlanList(glyphPlanSeq);

            int lineHeight = (int)_font.LineSpacingInPx;//temp

            PixelBlender32 prevPxBlender = _painter.DestBitmapBlender.OutputPixelBlender; //save
            _painter.DestBitmapBlender.OutputPixelBlender = maskPixelBlenderPerCompo; //change to new blender  

            for (int i = glyphPlanSeq.startAt; i < endBefore; ++i)
            {
                UnscaledGlyphPlan glyph = glyphPlanList[i];
                TextureFontGlyphData glyphData;
                if (!_fontAtlas.TryGetGlyphDataByGlyphIndex(glyph.glyphIndex, out glyphData))
                {
                    //if no glyph data, we should render a missing glyph ***
                    continue;
                }
                //--------------------------------------
                //TODO: review precise height in float
                //-------------------------------------- 
                int srcX, srcY, srcW, srcH;
                glyphData.GetGlyphRect(out srcX, out srcY, out srcW, out srcH);

                float ngx = acc_x + (float)Math.Round(glyph.OffsetX * scale);
                float ngy = acc_y + (float)Math.Round(glyph.OffsetY * scale);
                //NOTE:
                // -glyphData.TextureXOffset => restore to original pos
                // -glyphData.TextureYOffset => restore to original pos 
                //--------------------------
                gx = (float)(x + (ngx - glyphData.TextureXOffset) * scaleFromTexture); //ideal x
                gy = (float)(y + (ngy - glyphData.TextureYOffset - srcH + lineHeight) * scaleFromTexture);

                acc_x += (float)Math.Round(glyph.AdvanceX * scale);
                gy = (float)Math.Floor(gy) + lineHeight;

                //clear with solid black color 
                //_maskBufferPainter.Clear(Color.Black);
                //clear mask buffer at specific pos
                _maskBufferPainter.FillRect(gx - 1, gy - 1, srcW + 2, srcH + 2, Color.Black);
                //draw 'stencil' glyph on mask-buffer                
                _maskBufferPainter.DrawImage(_fontBmp, gx, gy, srcX, _fontBmp.Height - (srcY), srcW, srcH);

                //select component to render this need to render 3 times for lcd technique
                //1. B
                maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.B;
                maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.B;
                _painter.FillRect(gx + 1, gy, srcW, srcH);
                //2. G
                maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.G;
                maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.G;
                _painter.FillRect(gx + 1, gy, srcW, srcH);
                //3. R
                maskPixelBlenderPerCompo.SelectedMaskComponent = PixelBlenderColorComponent.R;
                maskPixelBlenderPerCompo.EnableOutputColorComponent = EnableOutputColorComponent.R;
                _painter.FillRect(gx + 1, gy, srcW, srcH);
            }

            //
            _painter.DestBitmapBlender.OutputPixelBlender = prevPxBlender;//restore back
        }
    }
}
