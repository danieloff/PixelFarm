﻿//MIT, 2016-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.CpuBlit.Imaging;
using PixelFarm.CpuBlit.Rasterization;
using PixelFarm.CpuBlit.FragmentProcessing;
using PixelFarm.CpuBlit.PixelProcessing;

using BitmapBufferEx;
namespace PixelFarm.CpuBlit
{

    class MyBitmapBlender : BitmapBlenderBase
    {
        ActualBitmap actualImage;


        public MyBitmapBlender(ActualBitmap actualImage, PixelProcessing.PixelBlender32 pxBlender)
        {
            this.actualImage = actualImage;
            Attach(actualImage);
        }
        public override void ReplaceBuffer(int[] newbuffer)
        {
            ActualBitmap.ReplaceBuffer(actualImage, newbuffer);
        }

    }
    public class VectorTool : PixelFarm.Drawing.PainterExtensions.VectorTool
    {
        Stroke _stroke = new Stroke(1);
        public override void CreateStroke(VertexStore orgVxs, float strokeW, VertexStore output)
        {
            _stroke.Width = strokeW;
            _stroke.MakeVxs(orgVxs, output);

        }
    }


    public enum LineRenderingTechnique
    {
        StrokeVxsGenerator,
        OutlineAARenderer,

    }



    public class AggPainter : Painter
    {
        AggRenderSurface _aggsx; //target rendering surface

        //--------------------
        //low-level rasterizer 
        /// <summary>
        /// scanline rasterizer to bitmap
        /// </summary>
        DestBitmapRasterizer _bmpRasterizer;
        BitmapBuffer _bxt;
        //--------------------


        //low-level outline renderer
        LineRenderingTechnique _lineRenderingTech;
        Rasterization.Lines.LineProfileAnitAlias _lineProfileAA;
        Rasterization.Lines.OutlineAARasterizer _outlineRas; //low-level outline aa

        //--------------------
        //pen 
        Stroke _stroke;
        Color _strokeColor;
        //--------------------
        //brush
        Color fillColor;
        //--------------------
        //image processing,
        FilterMan filterMan = new FilterMan();

        //font
        RequestFont currentFont;
        //-------------
        //vector generators for various object
        SimpleRect _simpleRectVxsGen = new SimpleRect();
        Ellipse _ellipse = new Ellipse();
        PathWriter _lineGen = new PathWriter();

        LineDashGenerator _lineDashGen;
        int ellipseGenNSteps = 20;
        SmoothingMode _smoothingMode;

        VectorTool _vectorTool;

        Brush _curBrush;
        Pen _curPen;

        //
        PixelBlenderWithMask maskPixelBlender;
        PixelBlenderPerColorComponentWithMask maskPixelBlenderPerCompo;
        //

        bool _useDefaultBrush;

        public AggPainter(AggRenderSurface aggsx)
        {
            //painter paint to target surface
            _orientation = DrawBoardOrientation.LeftBottom;

            this._aggsx = aggsx;
         

            this._bmpRasterizer = aggsx.BitmapRasterizer;
            _bxt = new BitmapBuffer(
                aggsx.Width,
                aggsx.Height,
                ActualBitmap.GetBuffer(aggsx.DestActualImage));
            //----------------------------------------------------

            this._stroke = new Stroke(1);//default
            _vectorTool = new VectorTool();
            _useDefaultBrush = true;
        }

        public LineRenderingTechnique LineRenderingTech
        {
            get { return _lineRenderingTech; }
            set
            {

                if (value == LineRenderingTechnique.OutlineAARenderer
                     && _outlineRas == null)
                {


                    _lineProfileAA = new Rasterization.Lines.LineProfileAnitAlias(this.StrokeWidth, null);

                    var blender = new PixelBlenderBGRA();

                    var outlineRenderer = new Rasterization.Lines.OutlineRenderer(
                        new ClipProxyImage(new SubBitmapBlender(_aggsx.DestActualImage, blender)), //Need ClipProxyImage
                        blender,
                        _lineProfileAA);
                    outlineRenderer.SetClipBox(0, 0, this.Width, this.Height);

                    //TODO: impl 'Pen'

                    _outlineRas = new Rasterization.Lines.OutlineAARasterizer(outlineRenderer);
                    _outlineRas.LineJoin = Rasterization.Lines.OutlineAARasterizer.OutlineJoin.Round;
                    _outlineRas.RoundCap = true;

                }
                _lineRenderingTech = value;
            }
        }
        public override Brush CurrentBrush
        {
            get
            {
                return _curBrush;
            }
            set
            {
                _curBrush = value;
                //check brush kind
                if (value == null)
                {
                    _useDefaultBrush = true;
                    return;
                }

                _useDefaultBrush = false;

                switch (value.BrushKind)
                {
                    default: throw new NotSupportedException();
                    //
                    case BrushKind.Solid:
                        break;
                    case BrushKind.LinearGradient:
                        break;
                    case BrushKind.CircularGraident:
                        break;
                    case BrushKind.GeometryGradient:
                        break;
                    case BrushKind.Texture:
                        break;
                }
            }
        }
        public override Pen CurrentPen
        {
            get { return _curPen; }
            set
            {
                _curPen = value;
            }
        }
        public static AggPainter Create(ActualBitmap bmp, PixelProcessing.PixelBlender32 blender = null)
        {
            //helper func

            AggRenderSurface renderSx = new AggRenderSurface(bmp);
            if (blender == null)
            {
                blender = new PixelProcessing.PixelBlenderBGRA();
            }
            renderSx.PixelBlender = blender;

            return new AggPainter(renderSx);
        }


        public override Drawing.PainterExtensions.VectorTool VectorTool
        {
            get { return _vectorTool; }
        }

        public AggRenderSurface RenderSurface
        {
            get { return this._aggsx; }
        }

        public BitmapBlenderBase DestBitmapBlender
        {
            get { return this._aggsx.DestImage; }
        }


        DrawBoardOrientation _orientation;
        public override DrawBoardOrientation Orientation
        {
            get { return _orientation; }
            set
            { _orientation = value; }
        }
        public override int Width
        {
            get
            {
                return _aggsx.Width;
            }
        }
        public override int Height
        {
            get
            {
                return _aggsx.Height;
            }
        }
        public override void Clear(Color color)
        {
            _aggsx.Clear(color);
        }
        public override float OriginX
        {
            get { return _aggsx.ScanlineRasOriginX; }
        }
        public override float OriginY
        {
            get { return _aggsx.ScanlineRasOriginY; }
        }
        public override void SetOrigin(float x, float y)
        {
            _aggsx.SetScanlineRasOrigin(x, y);

        }
        RenderQualtity _renderQuality;
        public override RenderQualtity RenderQuality
        {
            get { return _renderQuality; }
            set { _renderQuality = value; }
        }

        public override SmoothingMode SmoothingMode
        {
            get
            {
                return _smoothingMode;
            }
            set
            {
                switch (_smoothingMode = value)
                {
                    case Drawing.SmoothingMode.HighQuality:
                    case Drawing.SmoothingMode.AntiAlias:
                        //TODO: review here
                        //anti alias != lcd technique 
                        this.RenderQuality = RenderQualtity.HighQuality;
                        _aggsx.UseSubPixelLcdEffect = true;
                        break;
                    case Drawing.SmoothingMode.HighSpeed:
                    default:
                        this.RenderQuality = RenderQualtity.Fast;
                        _aggsx.UseSubPixelLcdEffect = false;
                        break;
                }
            }
        }
        public override RectInt ClipBox
        {
            get { return this._aggsx.GetClippingRect(); }
            set { this._aggsx.SetClippingRect(value); }
        }
        public override void SetClipBox(int x1, int y1, int x2, int y2)
        {
            this._aggsx.SetClippingRect(new RectInt(x1, y1, x2, y2));
        }


        public override void Draw(VertexStoreSnap vxs)
        {
            if (_lineRenderingTech == LineRenderingTechnique.StrokeVxsGenerator)
            {
                using (VxsContext.Temp(out var v1))
                {
                    _aggsx.Render(vxs, this._strokeColor);
                }
            }
            else
            {
                _outlineRas.RenderVertexSnap(vxs, this._strokeColor);
            }
        }


        /// <summary>
        /// draw line
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="color"></param>
        public override void DrawLine(double x1, double y1, double x2, double y2)
        {
            //BitmapExt
            if (this.RenderQuality == RenderQualtity.Fast)
            {
                this._bxt.DrawLine(
                    (int)Math.Round(x1),
                    (int)Math.Round(y1),
                    (int)Math.Round(x2),
                    (int)Math.Round(y2),
                    this._strokeColor.ToARGB()
                    );

                return;
            }

            //----------------------------------------------------------
            //Agg
            if (_orientation == DrawBoardOrientation.LeftBottom)
            {
                //as original
                _lineGen.Clear();
                _lineGen.MoveTo(x1, y1);
                _lineGen.LineTo(x2, y2);
            }
            else
            {
                //left-top
                int h = this.Height;

                _lineGen.Clear();
                _lineGen.MoveTo(x1, h - y1);
                _lineGen.LineTo(x2, h - y2);
            }
            //----------------------------------------------------------
            if (_lineRenderingTech == LineRenderingTechnique.StrokeVxsGenerator)
            {
                using (VxsContext.Temp(out var v1))
                {
                    _aggsx.Render(_stroke.MakeVxs(_lineGen.Vxs, v1), this._strokeColor);
                }
            }
            else
            {
                _outlineRas.RenderVertexSnap(new VertexStoreSnap(_lineGen.Vxs), this._strokeColor);
            }
        }

        double _strokeW;
        public override double StrokeWidth
        {
            get
            {
                return _strokeW;
            }
            set
            {
                if (value != _strokeW)
                {
                    //strokeW change
                    _strokeW = value;
                    if (_lineRenderingTech == LineRenderingTechnique.StrokeVxsGenerator)
                    {
                        this._stroke.Width = value;
                    }
                    else
                    {
                        _lineProfileAA.SubPixelWidth = (float)value;
                    }
                }
            }
        }
        public override void Draw(VertexStore vxs)
        {
            if (_lineDashGen == null)
            {
                //no line dash

                if (LineRenderingTech == LineRenderingTechnique.StrokeVxsGenerator)
                {
                    using (VxsContext.Temp(out var v1))
                    {
                        _aggsx.Render(_stroke.MakeVxs(vxs, v1), this._strokeColor);
                    }
                }
                else
                {
                    _outlineRas.RenderVertexSnap(new VertexStoreSnap(vxs), this._strokeColor);
                }
            }
            else
            {
                if (LineRenderingTech == LineRenderingTechnique.StrokeVxsGenerator)
                {

                    using (VxsContext.Temp(out var v1, out var v2))
                    {
                        _lineDashGen.CreateDash(vxs, v1);
                        _stroke.MakeVxs(v1, v2);
                        _aggsx.Render(v2, this._strokeColor);
                    }
                }
                else
                {
                    using (VxsContext.Temp(out var v1))
                    {
                        _lineDashGen.CreateDash(vxs, v1);
                        _outlineRas.RenderVertexSnap(new VertexStoreSnap(v1), this._strokeColor);
                    }
                }

            }
        }

        public override void DrawRect(double left, double top, double width, double height)
        {

            //BitmapExt
            if (this.RenderQuality == RenderQualtity.Fast)
            {

                if (this._orientation == DrawBoardOrientation.LeftBottom)
                {

                    this._bxt.DrawRectangle(
                        (int)Math.Round(left),
                        (int)Math.Round(top),
                        (int)Math.Round(left + width),
                        (int)Math.Round(top + height),

                    ColorInt.FromArgb(this._strokeColor.ToARGB()));
                }
                else
                {
                    //TODO: review here
                    throw new NotSupportedException();
                    //int canvasH = this.Height; 
                    ////_simpleRectVxsGen.SetRect(left + 0.5, canvasH - (bottom + 0.5 + height), right - 0.5, canvasH - (top - 0.5 + height));
                    //this._bmpBuffer.DrawRectangle(
                    //(int)Math.Round(left),
                    //(int)Math.Round(top),
                    //(int)Math.Round(left + width),
                    //(int)Math.Round(top + height),
                    //ColorInt.FromArgb(this.strokeColor.ToARGB()));
                }
                return; //exit
            }

            //----------------------------------------------------------
            //Agg
            if (this._orientation == DrawBoardOrientation.LeftBottom)
            {
                double right = left + width;
                double bottom = top + height;
                _simpleRectVxsGen.SetRect(left + 0.5, bottom + 0.5, right - 0.5, top - 0.5);
            }
            else
            {
                double right = left + width;
                double bottom = top - height;
                int canvasH = this.Height;
                //_simpleRectVxsGen.SetRect(left + 0.5, canvasH - (bottom + 0.5), right - 0.5, canvasH - (top - 0.5));
                _simpleRectVxsGen.SetRect(left + 0.5, canvasH - (bottom + 0.5 + height), right - 0.5, canvasH - (top - 0.5 + height));
            }

            if (LineRenderingTech == LineRenderingTechnique.StrokeVxsGenerator)
            {
                using (VxsContext.Temp(out var v1, out var v2))
                {
                    _aggsx.Render(_stroke.MakeVxs(_simpleRectVxsGen.MakeVxs(v1), v2), this._strokeColor);
                }
            }
            else
            {
                using (VxsContext.Temp(out var v1))
                {
                    _outlineRas.RenderVertexSnap(new VertexStoreSnap(_simpleRectVxsGen.MakeVxs(v1)), this._strokeColor);
                }
            }


        }

        public override void DrawEllipse(double left, double top, double width, double height)
        {

            double ox = (left + width / 2);
            double oy = (top + height / 2);
            if (this._orientation == DrawBoardOrientation.LeftTop)
            {
                //modified
                oy = this.Height - oy;
            }

            //---------------------------------------------------------- 
            //BitmapExt
            if (this._renderQuality == RenderQualtity.Fast)
            {

                this._bxt.DrawEllipseCentered(
                   (int)Math.Round(ox), (int)Math.Round(oy),
                   (int)Math.Round(width / 2),
                   (int)Math.Round(height / 2),
                   this._strokeColor.ToARGB());

                return;
            }

            //Agg
            //---------------------------------------------------------- 
            _ellipse.Reset(ox,
                         oy,
                         width / 2,
                         height / 2,
                         ellipseGenNSteps);

            if (LineRenderingTech == LineRenderingTechnique.StrokeVxsGenerator)
            {
                using (VxsContext.Temp(out var v1, out var v2))
                {
                    _aggsx.Render(_stroke.MakeVxs(_ellipse.MakeVxs(v1), v2), this._strokeColor);
                }
            }
            else
            {
                using (VxsContext.Temp(out var v1))
                {
                    _outlineRas.RenderVertexSnap(new VertexStoreSnap(_ellipse.MakeVxs(v1)), this._strokeColor);
                }
            }
        }
        public override void FillEllipse(double left, double top, double width, double height)
        {
            double ox = (left + width / 2);
            double oy = (top + height / 2);
            if (this._orientation == DrawBoardOrientation.LeftTop)
            {
                //modified
                oy = this.Height - oy;
            }
            //---------------------------------------------------------- 
            //BitmapExt
            if (this._renderQuality == RenderQualtity.Fast)
            {
                this._bxt.FillEllipseCentered(
                   (int)Math.Round(ox), (int)Math.Round(oy),
                   (int)Math.Round(width / 2),
                   (int)Math.Round(height / 2),
                   this.fillColor.ToARGB());
                return;
            }


            //Agg
            //---------------------------------------------------------- 
            _ellipse.Reset(ox,
                          oy,
                          width / 2,
                          height / 2,
                          ellipseGenNSteps);


            using (VxsContext.Temp(out var v1))
            {
                _aggsx.Render(_ellipse.MakeVxs(v1), this.fillColor);
            }
        }
        public override void FillRect(double left, double top, double width, double height)
        {


            //---------------------------------------------------------- 
            //BitmapExt
            if (_useDefaultBrush && this._renderQuality == RenderQualtity.Fast)
            {
                this._bxt.FillRectangle(
                      (int)Math.Round(left),
                      (int)Math.Round(top),
                      (int)Math.Round(left + width),
                      (int)Math.Round(top + height),
                      ColorInt.FromArgb(this.fillColor.ToARGB()));
                return;
            }

            //Agg 
            //---------------------------------------------------------- 
            if (this._orientation == DrawBoardOrientation.LeftBottom)
            {
                double right = left + width;
                double bottom = top - height;
                if (right < left || top < bottom)
                {
#if DEBUG
                    throw new ArgumentException();
#else
                return;
#endif
                }

                _simpleRectVxsGen.SetRect(left + 0.5, (bottom + 0.5) + height, right - 0.5, (top - 0.5) + height);
            }
            else
            {

                double right = left + width;
                double bottom = top - height;
                if (right < left || top < bottom)
                {
#if DEBUG
                    throw new ArgumentException();
#else
                return;
#endif
                }

                int canvasH = this.Height;
                _simpleRectVxsGen.SetRect(left + 0.5, canvasH - (bottom + 0.5 + height), right - 0.5, canvasH - (top - 0.5 + height));
            }

            using (VxsContext.Temp(out var v1))
            {
                //---------------------------------------------------------- 
                if (!_useDefaultBrush)
                {
                    Brush br = _curBrush;
                    switch (br.BrushKind)
                    {
                        case BrushKind.LinearGradient:
                            {
                                //fill linear gradient brush
                                //....

                                //check resolved object for br 
                                //if not then create a new one
                                //------------------------------------------- 
                                //original agg's gradient fill 

                                _aggGradientBrush.ResolveBrush((LinearGradientBrush)br);
                                _aggGradientBrush.SetOffset((float)-left, (float)-top);
                                Fill(_simpleRectVxsGen.MakeVxs(v1), _aggGradientBrush);
                            }
                            break;
                        case BrushKind.CircularGraident:
                            {
                                _circularGradBrush.ResolveBrush((CircularGradientBrush)br);
                                _circularGradBrush.SetOffset((float)-left, (float)-top);
                                Fill(_simpleRectVxsGen.MakeVxs(v1), _circularGradBrush);
                            }
                            break;
                        default:
                            {
                                _aggsx.Render(_simpleRectVxsGen.MakeVertexSnap(v1), this.fillColor);
                            }
                            break;
                    }
                }
                else
                {
                    _aggsx.Render(_simpleRectVxsGen.MakeVertexSnap(v1), this.fillColor);
                }
            }
        }
        AggLinearGradientBrush _aggGradientBrush = new AggLinearGradientBrush();
        AggCircularGradientBrush _circularGradBrush = new AggCircularGradientBrush();



        public override RequestFont CurrentFont
        {
            get
            {
                return this.currentFont;
            }
            set
            {
                this.currentFont = value;
                //this request font must resolve to actual font
                //within canvas *** 
                //TODO: review drawing string  with agg here 
                if (_textPrinter != null && value != null)
                {
                    _textPrinter.ChangeFont(value);
                }
            }
        }

        public override void DrawString(
           string text,
           double x,
           double y)
        {
            //TODO: review drawing string  with agg here   
            if (_textPrinter != null)
            {
                if (this._orientation == DrawBoardOrientation.LeftBottom)
                {
                    _textPrinter.DrawString(text, x, y);
                }
                else
                {
                    //from current point size 
                    //we need line height of current font size
                    //then we will start on 'base line'

                    _textPrinter.DrawString(text, x, this.Height - y);
                }

            }
        }
        public override void DrawString(RenderVxFormattedString renderVx, double x, double y)
        {
            //draw string from render vx
            if (_textPrinter != null)
            {
                _textPrinter.DrawString(renderVx, x, y);
            }
        }
        public override RenderVxFormattedString CreateRenderVx(string textspan)
        {

            var renderVxFmtStr = new AggRenderVxFormattedString(textspan);
            if (_textPrinter != null)
            {
                char[] buffer = textspan.ToCharArray();
                _textPrinter.PrepareStringForRenderVx(renderVxFmtStr, buffer, 0, buffer.Length);

            }
            return renderVxFmtStr;
        }

        ITextPrinter _textPrinter;
        public ITextPrinter TextPrinter
        {
            get
            {
                return _textPrinter;
            }
            set
            {
                _textPrinter = value;
                if (_textPrinter != null)
                {
                    _textPrinter.ChangeFont(this.currentFont);
                }
            }
        }


        List<int> _reusablePolygonList = new List<int>();
        /// <summary>
        /// fill with BitmapBufferExtension lib
        /// </summary>
        void FillWithBxt(VertexStoreSnap snap)
        {
            //transate the vxs/snap to command
            double x = 0;
            double y = 0;
            double offsetOrgX = this.OriginX;
            double offsetOrgY = this.OriginY;

            VertexSnapIter snapIter = snap.GetVertexSnapIter();
            VertexCmd cmd;

            int latestMoveToX = 0, latestMoveToY = 0;
            int latestX = 0, latestY = 0;


            bool closed = false;

            _reusablePolygonList.Clear();

            while ((cmd = snapIter.GetNextVertex(out x, out y)) != VertexCmd.NoMore)
            {
                x += offsetOrgX;
                y += offsetOrgY;

                switch (cmd)
                {
                    case VertexCmd.MoveTo:
                        {
                            if (_reusablePolygonList.Count > 0)
                            {
                                //no drawline
                                _reusablePolygonList.Clear();
                            }

                            closed = false;
                            _reusablePolygonList.Add(latestMoveToX = latestX = (int)Math.Round(x));
                            _reusablePolygonList.Add(latestMoveToY = latestY = (int)Math.Round(y));

                        }
                        break;
                    case VertexCmd.LineTo:
                    case VertexCmd.P2c:
                    case VertexCmd.P3c:
                        {
                            //collect to the polygon
                            _reusablePolygonList.Add(latestX = (int)Math.Round(x));
                            _reusablePolygonList.Add(latestY = (int)Math.Round(y));
                        }
                        break;
                    case VertexCmd.Close:
                    case VertexCmd.CloseAndEndFigure:
                        {
                            if (_reusablePolygonList.Count > 0)
                            {
                                //flush by draw line
                                _reusablePolygonList.Add(latestX = latestMoveToX);
                                _reusablePolygonList.Add(latestY = latestMoveToY);

                                _bxt.FillPolygon(_reusablePolygonList.ToArray(),
                                    this.fillColor.ToARGB());
                            }

                            _reusablePolygonList.Clear();
                            closed = true;
                        }
                        break;
                    default:
                        break;
                }
            }
            //---------------
            if (!closed && (_reusablePolygonList.Count > 0) &&
               (latestX == latestMoveToX) && (latestY == latestMoveToY))
            {

                //flush by draw line
                _reusablePolygonList.Add(latestMoveToX);
                _reusablePolygonList.Add(latestMoveToY);

                _bxt.FillPolygon(
                    _reusablePolygonList.ToArray(),
                    this.fillColor.ToARGB());
            }
        }
        /// <summary>
        /// fill vertex store, we do NOT store snap
        /// </summary>
        /// <param name="vxs"></param>
        /// <param name="c"></param>
        public override void Fill(VertexStoreSnap snap)
        {

            //BitmapExt
            if (this._renderQuality == RenderQualtity.Fast)
            {
                FillWithBxt(snap);
                return;
            }

            _aggsx.Render(snap, fillColor);

            //_sclineRas.Reset();
            //_sclineRas.AddPath(snap);
            //_bmpRasterizer.RenderWithColor(this._aggsx.DestImage, _sclineRas, _scline, fillColor);
        }
        /// <summary>
        /// fill vxs, we do NOT store vxs
        /// </summary>
        /// <param name="vxs"></param>
        public override void Fill(VertexStore vxs)
        {
            //
            if (_useDefaultBrush && this._renderQuality == RenderQualtity.Fast)
            {
                FillWithBxt(new VertexStoreSnap(vxs));
                return;
            }
            if (!_useDefaultBrush)
            {
                Brush br = _curBrush;
                switch (br.BrushKind)
                {
                    case BrushKind.LinearGradient:
                        {
                            //fill linear gradient brush
                            //....

                            //check resolved object for br 
                            //if not then create a new one
                            //------------------------------------------- 
                            //original agg's gradient fill 

                            _aggGradientBrush.ResolveBrush((LinearGradientBrush)br);
                            _aggGradientBrush.SetOffset(0, 0);
                            Fill(vxs, _aggGradientBrush);
                        }
                        break;
                    case BrushKind.CircularGraident:
                        {
                            _circularGradBrush.ResolveBrush((CircularGradientBrush)br);
                            _circularGradBrush.SetOffset(0, 0);
                            Fill(vxs, _circularGradBrush);
                        }
                        break;
                    default:
                        {
                            //_sclineRas.AddPath(vxs);
                            //_bmpRasterizer.RenderWithColor(this._aggsx.DestImage, _sclineRas, _scline, fillColor);

                            _aggsx.Render(vxs, fillColor);
                        }
                        break;
                }
            }
            else
            {
                _aggsx.Render(vxs, fillColor);

                //_sclineRas.AddPath(vxs);
                //_bmpRasterizer.RenderWithColor(this._aggsx.DestImage, _sclineRas, _scline, fillColor);
            }


        }
        public override bool UseSubPixelLcdEffect
        {
            get
            {
                return _aggsx.UseSubPixelLcdEffect;
            }
            set
            {
                _aggsx.UseSubPixelLcdEffect = value;
            }
        }
        public override Color FillColor
        {
            get { return fillColor; }
            set
            {
                this.fillColor = value;
            }
        }
        public override Color StrokeColor
        {
            get { return _strokeColor; }
            set { this._strokeColor = value; }
        }

        /// <summary>
        /// we do NOT store vxs
        /// </summary>
        /// <param name="vxs"></param>
        /// <param name="spanGen"></param>
        public void Fill(VertexStore vxs, ISpanGenerator spanGen)
        {
            _aggsx.Render(vxs, spanGen);

            //this._sclineRas.AddPath(vxs);
            //_bmpRasterizer.RenderWithSpan(this._aggsx.DestImage, _sclineRas, _scline, spanGen);
        }
        void DrawBitmap(ActualBitmap actualBmp, double left, double top)
        {
            //check image caching system 
            if (this._renderQuality == RenderQualtity.Fast)
            {
                BitmapBuffer srcBmp = new BitmapBuffer(actualBmp.Width, actualBmp.Height, ActualBitmap.GetBuffer(actualBmp));
                try
                {
                    this._bxt.CopyBlit((int)left, (int)top, srcBmp);
                }
                catch (Exception ex)
                {

                }

                return;
            }

            //save, restore later... 
            bool useSubPix = UseSubPixelLcdEffect;
            //before render an image we turn off vxs subpixel rendering
            this.UseSubPixelLcdEffect = false;
            _aggsx.UseSubPixelLcdEffect = false;

            if (this._orientation == DrawBoardOrientation.LeftTop)
            {
                //place left upper corner at specific x y                    
                this._aggsx.Render(actualBmp, left, this.Height - (top + actualBmp.Height));
            }
            else
            {
                //left-bottom as original
                //place left-lower of the img at specific (x,y)
                this._aggsx.Render(actualBmp, left, top);
            }

            //restore...
            this.UseSubPixelLcdEffect = useSubPix;
            _aggsx.UseSubPixelLcdEffect = useSubPix;
        }
        void DrawBitmap(ActualBitmap actualBmp, double left, double top, int srcX, int srcY, int srcW, int srcH)
        {
            //check image caching system 
            if (this._renderQuality == RenderQualtity.Fast)
            {
                BitmapBuffer srcBmp = new BitmapBuffer(actualBmp.Width, actualBmp.Height, ActualBitmap.GetBuffer(actualBmp));
                try
                {
                    var src = new BitmapBufferEx.RectD(srcX, srcY, srcW, srcH);
                    var dest = new BitmapBufferEx.RectD(left, top, srcW, srcH);
                    BitmapBuffer bmpBuffer = new BitmapBuffer(actualBmp.Width, actualBmp.Height, ActualBitmap.GetBuffer(actualBmp));
                    this._bxt.CopyBlit(dest, bmpBuffer, src);
                }
                catch (Exception ex)
                {

                }

                return;
            }

            //save, restore later... 
            bool useSubPix = UseSubPixelLcdEffect;
            //before render an image we turn off vxs subpixel rendering
            this.UseSubPixelLcdEffect = false; 

            if (this._orientation == DrawBoardOrientation.LeftTop)
            {
                //place left upper corner at specific x y                    
                this._aggsx.Render(actualBmp, left, this.Height - (top + actualBmp.Height), srcX, srcY, srcW, srcH);
            }
            else
            {
                //left-bottom as original
                //place left-lower of the img at specific (x,y)
                this._aggsx.Render(actualBmp, left, top, srcX, srcY, srcW, srcH);
            }

            //restore...
            this.UseSubPixelLcdEffect = useSubPix; 
        }
        public override void DrawImage(Image actualImage, double left, double top, int srcX, int srcY, int srcW, int srcH)
        {
            ActualBitmap actualBmp = actualImage as ActualBitmap;
            if (actualBmp == null)
            {
                //test with other bitmap 
                return;
            }
            else
            {
                DrawBitmap(actualBmp, left, top, srcX, srcY, srcW, srcH);
            }
        }
        public override void DrawImage(Image img, double left, double top)
        {
            ActualBitmap actualBmp = img as ActualBitmap;
            if (actualBmp == null)
            {
                //test with other bitmap 
                return;
            }
            else
            {
                DrawBitmap(actualBmp, left, top);
            }
        }
        public override void DrawImage(Image img, params AffinePlan[] affinePlans)
        {
            ActualBitmap actualImg = img as ActualBitmap;
            if (actualImg == null)
            {
                //? TODO
                return;
            }

            if (this._renderQuality == RenderQualtity.Fast)
            {
                //todo, review here again
                BitmapBuffer srcBmp = new BitmapBuffer(img.Width, img.Height, ActualBitmap.GetBuffer(actualImg));
                if (affinePlans != null)
                {
                    this._bxt.BlitRender(srcBmp, false, 1, new BitmapBufferEx.MatrixTransform(affinePlans));
                }
                else
                {
                    this._bxt.BlitRender(srcBmp, false, 1, null);
                }
                return;
            }

            bool useSubPix = UseSubPixelLcdEffect; //save, restore later... 
                                                   //before render an image we turn off vxs subpixel rendering
            this.UseSubPixelLcdEffect = false;
            
            this._aggsx.Render(actualImg, affinePlans);
            //restore...
            this.UseSubPixelLcdEffect = useSubPix;
           
        }
        public override void ApplyFilter(ImageFilter imgFilter)
        {
            ////----------------------
            ///// <summary>
            ///// do filter at specific area
            ///// </summary>
            ///// <param name="filter"></param>
            ///// <param name="area"></param>
            //public override void DoFilterBlurStack(RectInt area, int r)
            //{
            //    ChildImage img = new ChildImage(this._aggsx.DestImage, _aggsx.PixelBlender,
            //        area.Left, area.Bottom, area.Right, area.Top);
            //    filterMan.DoStackBlur(img, r);
            //}
            //public override void DoFilterBlurRecursive(RectInt area, int r)
            //{
            //    ChildImage img = new ChildImage(this._aggsx.DestImage, _aggsx.PixelBlender,
            //        area.Left, area.Bottom, area.Right, area.Top);
            //    filterMan.DoRecursiveBlur(img, r);
            //}
            //public override void DoFilter(RectInt area, int r)
            //{
            //    ChildImage img = new ChildImage(this._aggsx.DestImage, _aggsx.PixelBlender,
            //      area.Left, area.Top, area.Right, area.Bottom);
            //    filterMan.DoSharpen(img, r);
            //}
            //TODO: implement this
            //resolve internal img filter
            //switch (imgFilter.Name)
            //{ 
            //} 
        }
        public override RenderVx CreateRenderVx(VertexStoreSnap snap)
        {
            return new AggRenderVx(snap);
        }
        public override void DrawRenderVx(RenderVx renderVx)
        {
            AggRenderVx aggRenderVx = (AggRenderVx)renderVx;
            Draw(aggRenderVx.snap);
        }
        public override void FillRenderVx(Brush brush, RenderVx renderVx)
        {
            AggRenderVx aggRenderVx = (AggRenderVx)renderVx;
            //fill with brush 
            if (brush is SolidBrush)
            {
                SolidBrush solidBrush = (SolidBrush)brush;
                var prevColor = this.fillColor;
                this.fillColor = solidBrush.Color;
                Fill(aggRenderVx.snap);
                this.fillColor = prevColor;
            }
            else
            {
                Fill(aggRenderVx.snap);
            }
        }
        public override void FillRenderVx(RenderVx renderVx)
        {
            AggRenderVx aggRenderVx = (AggRenderVx)renderVx;
            Fill(aggRenderVx.snap);
        }
        public LineJoin LineJoin
        {
            get { return _stroke.LineJoin; }
            set
            {
                _stroke.LineJoin = value;
            }
        }
        public LineCap LineCap
        {
            get { return _stroke.LineCap; }
            set
            {
                _stroke.LineCap = value;
            }
        }
        public LineDashGenerator LineDashGen
        {
            get { return this._lineDashGen; }
            set { this._lineDashGen = value; }
        }



        //---------------------------------------------------------------
        TargetBufferName _targetBufferName;
        bool _enableBuiltInMaskComposite;
        ActualBitmap _alphaBitmap;
        AggPainter _alphaMaskPainter; //TODO: review here

        void SetupMaskPixelBlender()
        {
            if (_alphaBitmap != null) return;
            //----------
            //same size                  

            _alphaBitmap = new ActualBitmap(_aggsx.Width, _aggsx.Height);
            _alphaMaskPainter = AggPainter.Create(_alphaBitmap, new PixelBlenderBGRA());
            _alphaMaskPainter.Clear(Color.Black);

            maskPixelBlender = new PixelBlenderWithMask();
            maskPixelBlenderPerCompo = new PixelBlenderPerColorComponentWithMask();

            maskPixelBlender.SetMaskBitmap(_alphaBitmap); //same alpha bitmap
            maskPixelBlenderPerCompo.SetMaskBitmap(_alphaBitmap); //same alpha bitmap
        }
        public TargetBufferName TargetBufferName
        {
            get { return _targetBufferName; }
            set
            {
                //change or not
                if (_targetBufferName != value)
                {
                    switch (value)
                    {
                        case TargetBufferName.Color:
                            //default 

                            break;
                        case TargetBufferName.AlphaMask:

                            break;
                    }
                    _targetBufferName = value;

                }

            }
        }
        public bool EnableBuiltInMaskComposite
        {
            get { return _enableBuiltInMaskComposite; }
            set
            {
                _enableBuiltInMaskComposite = value;

            }
        }
    }


    public enum TargetBufferName
    {
        Color,
        AlphaMask
    }






    class ReusableRotationTransformer : VertexProcessing.ICoordTransformer
    {

        double _angle;
        Affine affine;
        public ReusableRotationTransformer()
        {
            affine = Affine.IdentityMatrix;
        }
        public double Angle
        {
            get
            {
                return _angle;
            }
            set
            {
                if (value != _angle)
                {
                    affine = Affine.NewRotation(value);
                }
                _angle = value;
            }
        }
        public void Transform(ref double x, ref double y)
        {
            affine.Transform(ref x, ref y);
        }
    }


    struct GradientSpanPart
    {
        public GradientSpanGen _spanGenGr;
        public LinearGradientColorsProvider _linearGradientColorProvider;
        public SpanInterpolatorLinear _linerInterpolator;
        public ReusableRotationTransformer _reusableRotationTransformer;

        public void SetData(IGradientValueCalculator gvc, LinearGradientPair pair)
        {

            _linerInterpolator = new SpanInterpolatorLinear();
            _linearGradientColorProvider = new LinearGradientColorsProvider();
            _spanGenGr = new GradientSpanGen();
            //TODO:
            //user can use other coord transformer 
            _linerInterpolator.Transformer =
                _reusableRotationTransformer = new ReusableRotationTransformer();
            _reusableRotationTransformer.Angle = pair.Angle;
            _linearGradientColorProvider.SetColors(pair.c1, pair.c2, pair.steps);
            _spanGenGr.Reset(_linerInterpolator,
                gvc,
                _linearGradientColorProvider,
               pair._distance);

            _spanGenGr.SetStartPoint(pair.x1, pair.y1);

        }

        public void SetOffset(float x, float y)
        {
            _spanGenGr.SetOffset(x, y);
        }
    }

    class AggLinearGradientBrush : ISpanGenerator
    {
        static IGradientValueCalculator _gvcX = new GvcX();
        static IGradientValueCalculator _gvcY = new GvcY();



        GradientSpanPart _grSpanGenPart;
        List<GradientSpanPart> _moreSpanGenertors;


        bool isInit;
        public void Prepare()
        {

        }


        public void ResolveBrush(LinearGradientBrush linearGrBrush)
        {
            //for gradient :
            int pairCount = linearGrBrush.PairCount;

            //resolve linear gradient to agg object  
            if (!isInit)
            {
                //temp fix  
                isInit = true;
            }
            if (_moreSpanGenertors == null)
            {
                _moreSpanGenertors = new List<GradientSpanPart>();
            }
            else
            {
                _moreSpanGenertors.Clear();
            }
            //
            //more than 1 pair   
            int partNo = 0;
            int partCount = linearGrBrush.PairCount;

            foreach (LinearGradientPair pair in linearGrBrush.GetColorPairIter())
            {
                IGradientValueCalculator gvc = null;
                switch (pair.Direction)
                {
                    case LinearGradientPair.GradientDirection.Vertical:
                        gvc = _gvcY;
                        break;
                    case LinearGradientPair.GradientDirection.Horizontal:
                        gvc = _gvcX;
                        break;
                    default:
                        //temp, 
                        //TODO: review here
                        gvc = _gvcX;
                        break;
                }

                _grSpanGenPart = new GradientSpanPart();
                _grSpanGenPart.SetData(gvc, pair);
                _grSpanGenPart._spanGenGr.PartNo = partNo;
                _grSpanGenPart._spanGenGr.IsLastPart = (partNo == partCount - 1);
                _moreSpanGenertors.Add(_grSpanGenPart);
                partNo++;
            }

            _grSpanGenPart = _moreSpanGenertors[0];

#if !COSMOS
            for (int i = 0; i < partCount - 1; ++i)
            {
                GradientSpanPart part = _moreSpanGenertors[i];
                part._spanGenGr.RequestGradientPart += (fromPartNo) =>
                {
                    if (fromPartNo < partCount)
                    {
                        return _moreSpanGenertors[fromPartNo]._spanGenGr;
                    }
                    else
                    {
                        return null;
                    }
                };
            }
#endif
        }


        public void SetOffset(float x, float y)
        {
            //apply offset to all span generator
            int j = _moreSpanGenertors.Count;
            for (int i = 0; i < j; ++i)
            {
                _moreSpanGenertors[i].SetOffset(x, y);
            }
        }
        public void GenerateColors(Color[] outputColors, int startIndex, int x, int y, int spanLen)
        {

            //start at current span generator 
            _grSpanGenPart._spanGenGr.GenerateColors(outputColors, startIndex, x, y, spanLen);
        }

    }

    class AggCircularGradientBrush : ISpanGenerator
    {

        static IGradientValueCalculator _gvcCircular = new GvcRadial();

        GradientSpanPart _grSpanGenPart;
        List<GradientSpanPart> _moreSpanGenertors;


        bool isInit;
        public void Prepare()
        {

        }


        public void ResolveBrush(CircularGradientBrush linearGrBrush)
        {
            //for gradient :
            int pairCount = linearGrBrush.PairCount;

            //resolve linear gradient to agg object  
            if (!isInit)
            {
                //temp fix   
                isInit = true;
            }
            if (_moreSpanGenertors == null)
            {
                _moreSpanGenertors = new List<GradientSpanPart>();
            }
            else
            {
                _moreSpanGenertors.Clear();
            }
            //
            //more than 1 pair   
            int partNo = 0;
            int partCount = linearGrBrush.PairCount;
            foreach (LinearGradientPair pair in linearGrBrush.GetColorPairIter())
            {
                _grSpanGenPart = new GradientSpanPart();
                _grSpanGenPart.SetData(_gvcCircular, pair);
                _grSpanGenPart._spanGenGr.PartNo = partNo;
                _grSpanGenPart._spanGenGr.IsLastPart = (partNo == partCount - 1);
                _moreSpanGenertors.Add(_grSpanGenPart);
                partNo++;
            }

            _grSpanGenPart = _moreSpanGenertors[0];

#if !COSMOS

            for (int i = 0; i < partCount - 1; ++i)
            {
                GradientSpanPart part = _moreSpanGenertors[i];
                part._spanGenGr.RequestGradientPart += (fromPartNo) =>
                {
                    if (fromPartNo != partCount - 1)
                    {
                        return _moreSpanGenertors[fromPartNo + 1]._spanGenGr;
                    }
                    else
                    {
                        return null;
                    }
                };
            }
#endif
        }



        public void SetOffset(float x, float y)
        {
            //apply offset to all span generator
            int j = _moreSpanGenertors.Count;
            for (int i = 0; i < j; ++i)
            {
                _moreSpanGenertors[i].SetOffset(x, y);
            }
        }
        public void GenerateColors(Color[] outputColors, int startIndex, int x, int y, int spanLen)
        {

            //start at current span generator 
            _grSpanGenPart._spanGenGr.GenerateColors(outputColors, startIndex, x, y, spanLen);
        }

    }

}