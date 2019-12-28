﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using LayoutFarm.UI.InputBridge;
using PixelFarm.DrawingGL;

namespace OpenTK
{
    public sealed class GLControl : UserControl
    {
        MyNativeWindow _surfaceControl;
        Win32EventBridge _winBridge;
        public GLControl()
        {

        }
        public void SetGpuSurfaceViewportControl(MyNativeWindow gpuSurfaceControl)
        {
            _surfaceControl = gpuSurfaceControl;
        }
        protected override void WndProc(ref Message m)
        {
            _winBridge?.CustomPanelMsgHandler(m.HWnd, (uint)m.Msg, m.WParam, m.LParam);
            base.WndProc(ref m);
        }
        /// <summary>Raises the HandleCreated event.</summary>
        /// <param name="e">Not used.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            _surfaceControl.SetNativeHwnd(this.Handle, false);
            //translator
            _winBridge = new Win32EventBridge();
            _winBridge.SetMainWindowControl(_surfaceControl);
            base.OnHandleCreated(e);
        }
        public MyNativeWindow SurfaceControl => _surfaceControl;

        public void MakeCurrent()
        {
            _surfaceControl.MakeCurrent();
        }

    }
}
namespace LayoutFarm.UI
{
   
    public partial class UISurfaceViewportControl : UserControl
    {
        TopWindowBridgeWinForm _winBridge;
        RootGraphic _rootgfx;
        ITopWindowEventRoot _topWinEventRoot;
        InnerViewportKind _innerViewportKind;
        OpenGL.GpuOpenGLSurfaceView _gpuSurfaceViewUserControl;
        GLPainterContext _pcx;
        GLPainter _glPainter;
        OpenTK.GLControl _glControl;

        //PixelFarm.Drawing.Rectangle _winBoxAccumInvalidateArea;
        //bool _hasInvalidateAreaAccum;


        List<Form> _subForms = new List<Form>();

        public UISurfaceViewportControl()
        {
            InitializeComponent();
            //this.panel1.Visible = false; 
        }
        public void Close()
        {
            //1. clear all subForms
            if (_rootgfx != null)
            {
                _rootgfx.CloseWinRoot();
                _rootgfx = null;
            }

            if (_winBridge != null)
            {
                _winBridge.Close();
                _winBridge = null;
            }

#if DEBUG
            System.GC.Collect();
#endif
        }
        //
        public InnerViewportKind InnerViewportKind => _innerViewportKind;
        //
        public UIPlatform Platform => UIPlatformWinForm.GetDefault();
        //
        public RootGraphic RootGfx => _rootgfx;
        //
#if DEBUG
        static int s_dbugCount;
#endif
        //protected override void OnGotFocus(EventArgs e)
        //{
        //    s_dbugCount++;
        //System.Diagnostics.Debug.WriteLine("focus" + s_dbugCount.ToString());
        //    base.OnGotFocus(e);
        //}
        protected override void OnVisibleChanged(EventArgs e)
        {
            //s_dbugCount++;
            //System.Diagnostics.Debug.WriteLine("focus" + s_dbugCount.ToString());
            _rootgfx.InvalidateRectArea(new PixelFarm.Drawing.Rectangle(0, 0, _rootgfx.Width, _rootgfx.Height));
            _rootgfx.FlushAccumGraphics();
            //#if DEBUG
            //            s_dbugCount++;
            //System.Diagnostics.Debug.WriteLine("vis" + s_dbugCount.ToString());
            //#endif
            base.OnVisibleChanged(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            //on Windows request paint at specific area
            if (_rootgfx != null)
            {
                if (e.ClipRectangle.Width + e.ClipRectangle.Height == 0)
                {
                    //entire window
                    //_rootgfx.InvalidateRectArea(new PixelFarm.Drawing.Rectangle(0, 0, _rootgfx.Width, _rootgfx.Height));
                }
                else
                {
                    _rootgfx.InvalidateRectArea(
                        new PixelFarm.Drawing.Rectangle(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width, e.ClipRectangle.Height));
                    _rootgfx.FlushAccumGraphics();
                }

            }

            //#if DEBUG
            //            s_dbugCount++;
            //System.Diagnostics.Debug.WriteLine("p" + s_dbugCount.ToString() + e.ClipRectangle);
            //#endif
            base.OnPaint(e);
        }

        public OpenTK.GLControl GetOpenTKControl() => _glControl;
        public GLPainter GetGLPainter() => _glPainter;
        public GLPainterContext GetGLRenderSurface() => _pcx;

        PixelFarm.Drawing.DrawBoard CreateSoftwareDrawBoard(int width, int height, InnerViewportKind innerViewportKind)
        {

            PixelFarm.Drawing.WinGdi.GdiPlusRenderSurface gdiRenderSurface = new PixelFarm.Drawing.WinGdi.GdiPlusRenderSurface(width, height);
            var drawBoard = new PixelFarm.Drawing.WinGdi.GdiPlusDrawBoard(gdiRenderSurface);

            return drawBoard;
        }
        public void InitRootGraphics(
            RootGraphic rootgfx,
            ITopWindowEventRoot topWinEventRoot,
            InnerViewportKind innerViewportKind)
        {

            //create a proper bridge****
            _rootgfx = rootgfx;
            _topWinEventRoot = topWinEventRoot;
            _innerViewportKind = innerViewportKind;
            switch (innerViewportKind)
            {


                case InnerViewportKind.GdiPlusOnGLES:
                case InnerViewportKind.AggOnGLES:
                case InnerViewportKind.GLES:
                    {

                        //temp not suppport 

                        var bridge = new OpenGL.MyTopWindowBridgeOpenGL(rootgfx, topWinEventRoot); 
                        var view = new OpenTK.GLControl();
                        _glControl = view;
                        view.Size = new System.Drawing.Size(rootgfx.Width, rootgfx.Height);

                        var gpuSurfaceView = new OpenGL.GpuOpenGLSurfaceView();
                        gpuSurfaceView.SetTopWinBridge(bridge);
                        gpuSurfaceView.SetSize(rootgfx.Width, rootgfx.Height);
                        gpuSurfaceView.SetControl(view);

                        _gpuSurfaceViewUserControl = gpuSurfaceView;
                        this.Controls.Add(view);
                        //--------------------------------------- 
                        gpuSurfaceView.Bind(bridge);
                        _winBridge = bridge;
                        //---------------------------------------  
                        IntPtr hh1 = view.Handle; //force real window handle creation
                        try
                        {
                            gpuSurfaceView.MakeCurrent();
                        }
                        catch (Exception ex)
                        {

                        }
                        int max = Math.Max(view.Width, view.Height);

                        _pcx = GLPainterContext.Create(max, max, view.Width, view.Height, true);

                        _glPainter = new GLPainter();
                        _glPainter.BindToPainterContext(_pcx);
                        _glPainter.TextPrinter = new GLBitmapGlyphTextPrinter(_glPainter, PixelFarm.Drawing.GLES2.GLES2Platform.TextService);

                        //canvasPainter.SmoothingMode = PixelFarm.Drawing.SmoothingMode.HighQuality;
                        //----------------------
                        //1. win gdi based
                        //var printer = new WinGdiFontPrinter(canvas2d, view.Width, view.Height);
                        //canvasPainter.TextPrinter = printer;
                        //----------------------
                        //2. raw vxs
                        //var printer = new PixelFarm.Drawing.Fonts.VxsTextPrinter(canvasPainter);
                        //canvasPainter.TextPrinter = printer;
                        //----------------------
                        //3. agg texture based font texture 
                        //_glPainter.TextPrinter = new CpuBlitTextSpanPrinter2(_glPainter, 400, 50, PixelFarm.Drawing.GLES2.GLES2Platform.TextService);

                        //3  
                        var myGLCanvas1 = new PixelFarm.Drawing.GLES2.MyGLDrawBoard(_glPainter);
                        //{
                        //in mixed mode
                        //GDI+ on GLES, Agg on GLES we provide a software rendering layer too
                        PixelFarm.Drawing.DrawBoard cpuDrawBoard = null;// CreateSoftwareDrawBoard(view.Width, view.Height, innerViewportKind);
                        myGLCanvas1.SetCpuBlitDrawBoardCreator(() => cpuDrawBoard ?? (cpuDrawBoard = CreateSoftwareDrawBoard(view.Width, view.Height, innerViewportKind)));
                        //}

                        bridge.SetCanvas(myGLCanvas1);

                    }
                    break;


                case InnerViewportKind.PureAgg:
                    {
                        var bridge = new GdiPlus.MyTopWindowBridgeAgg(rootgfx, topWinEventRoot); //bridge to agg                          
                        var view = new OpenTK.GLControl();
                        view.Size = new System.Drawing.Size(rootgfx.Width, rootgfx.Height);
                        _glControl = view;

                        var gpuSurfaceView = new OpenGL.GpuOpenGLSurfaceView();
                        gpuSurfaceView.SetTopWinBridge(bridge);
                        gpuSurfaceView.SetSize(rootgfx.Width, rootgfx.Height);
                        gpuSurfaceView.SetControl(view);
                        this.Controls.Add(view);

                        gpuSurfaceView.Bind(bridge); 
                        
                        _winBridge = bridge;
                    }
                    break;
                case InnerViewportKind.GdiPlus:
                default:
                    {
                        var bridge = new GdiPlus.MyTopWindowBridgeGdiPlus(rootgfx, topWinEventRoot); //bridge with GDI+
                        var view = new OpenTK.GLControl();
                        view.Size = new System.Drawing.Size(rootgfx.Width, rootgfx.Height);
                        _glControl = view;


                        var gpuSurfaceView = new OpenGL.GpuOpenGLSurfaceView();
                        gpuSurfaceView.SetTopWinBridge(bridge);
                        gpuSurfaceView.SetSize(rootgfx.Width, rootgfx.Height);
                        gpuSurfaceView.SetControl(view);
                        this.Controls.Add(view);

                        gpuSurfaceView.Bind(bridge);

                        _winBridge = bridge;
                    }
                    break;
#if __SKIA__
                    //case InnerViewportKind.Skia:
                    //    {
                    //        //skiasharp ***

                    //        var bridge = new Skia.MyTopWindowBridgeSkia(rootgfx, topWinEventRoot);
                    //        var view = new CpuSurfaceView();
                    //        view.Dock = DockStyle.Fill;
                    //        this.Controls.Add(view);
                    //        //--------------------------------------- 
                    //        view.Bind(bridge);
                    //        _winBridge = bridge;

                    //    }
                    //    break;
#endif
            }
        }
        void InitializeComponent()
        {
            //this.panel1 = new System.Windows.Forms.Panel();
            //this.SuspendLayout();
            //// 
            //// panel1
            //// 
            //this.panel1.BackColor = System.Drawing.Color.Gray;
            //this.panel1.Location = new System.Drawing.Point(4, 4);
            //this.panel1.Name = "panel1";
            //this.panel1.Size = new System.Drawing.Size(851, 753);
            //this.panel1.TabIndex = 0;
            // 
            // UISurfaceViewportControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            //this.Controls.Add(this.panel1);
#if DEBUG
            this.Name = "UISurfaceViewportControl";
#endif
            this.Size = new System.Drawing.Size(863, 760);
            this.ResumeLayout(false);

        }
        protected override void OnLoad(EventArgs e)
        {
            _winBridge.OnHostControlLoaded();
        }
        public void PaintMe()
        {
            _winBridge.PaintToOutputWindow();
        }
        public void PaintToPixelBuffer(IntPtr outputPixelBuffer)
        {
            _winBridge.CopyOutputPixelBuffer(0, 0, this.Width, this.Height, outputPixelBuffer);
        }

#if DEBUG
        public void dbugPaintMeFullMode()
        {
            _winBridge.dbugPaintToOutputWindowFullMode();
        }
        public IdbugOutputWindow IdebugOutputWin => _winBridge;

#endif
        public void TopDownRecalculateContent()
        {
            _rootgfx.TopDownRecalculateContent();
        }
        //public void AddChild(RenderElement vi)
        //{
        //    _rootgfx.AddChild(vi);
        //}


        //public void AddChild(RenderElement renderElem, object owner)
        //{
        //    //temp disable this feature

        //    if (renderElem is RenderBoxBase &&
        //        owner is ITopWindowBox topWinBox)
        //    {
        //        if (topWinBox.PlatformWinBox == null)
        //        {

        //            FormPopupShadow popupShadow1 = new FormPopupShadow();
        //            popupShadow1.Visible = false;
        //            IntPtr handle1 = popupShadow1.Handle; //***


        //            //create platform winbox 
        //            var newForm = new AbstractCompletionWindow();
        //            newForm.LinkedParentForm = this.FindForm();
        //            newForm.LinkedParentControl = this;
        //            newForm.PopupShadow = popupShadow1;

        //            //TODO: 
        //            //1. review here=> 300,200
        //            //2. how to choose InnerViewportKind 

        //            UISurfaceViewportControl newSurfaceViewport = this.CreateNewOne(300, 200, InnerViewportKind.GLES);
        //            newSurfaceViewport.Location = new System.Drawing.Point(0, 0);
        //            newForm.Controls.Add(newSurfaceViewport);
        //            renderElem.ResetRootGraphics(newSurfaceViewport.RootGfx);
        //            renderElem.SetLocation(0, 0);
        //            newSurfaceViewport.RootGfx.AddChild(renderElem);

        //            //-----------------------------------------------------                        
        //            IntPtr tmpHandle = newForm.Handle;//force newform to create window handle

        //            //-----------------------------------------------------              

        //            var platformWinBox = new PlatformWinBoxForm(newForm);
        //            topWinBox.PlatformWinBox = platformWinBox;
        //            platformWinBox.UseRelativeLocationToParent = true;
        //            platformWinBox.PreviewVisibilityChanged += PlatformWinBox_PreviewVisibilityChanged;
        //            platformWinBox.PreviewBoundChanged += PlatformWinBox_PreviewBoundChanged;
        //            platformWinBox.BoundsChanged += PlatformWinBox_BoundsChanged;
        //            platformWinBox.VisibityChanged += PlatformWinBox_VisibityChanged;
        //            _subForms.Add(newForm);
        //        }
        //    }
        //    else
        //    {
        //        _rootgfx.AddChild(renderElem);
        //    }

        //    // 
        //    //if (renderElem is RenderBoxBase)
        //    //{
        //    //    if (owner is ITopWindowBox)
        //    //    {
        //    //        var topWinBox = owner as ITopWindowBox;
        //    //        if (topWinBox.PlatformWinBox == null)
        //    //        {

        //    //            FormPopupShadow popupShadow1 = new FormPopupShadow();
        //    //            popupShadow1.Visible = false;
        //    //            IntPtr handle1 = popupShadow1.Handle; //***


        //    //            //create platform winbox 
        //    //            var newForm = new AbstractCompletionWindow();
        //    //            newForm.LinkedParentForm = this.FindForm();
        //    //            newForm.LinkedParentControl = this;
        //    //            newForm.PopupShadow = popupShadow1;

        //    //            //TODO: 
        //    //            //1. review here=> 300,200
        //    //            //2. how to choose InnerViewportKind 

        //    //            UISurfaceViewportControl newSurfaceViewport = this.CreateNewOne(300, 200, InnerViewportKind.GLES);
        //    //            newSurfaceViewport.Location = new System.Drawing.Point(0, 0);
        //    //            newForm.Controls.Add(newSurfaceViewport);
        //    //            renderElem.ResetRootGraphics(newSurfaceViewport.RootGfx);
        //    //            renderElem.SetLocation(0, 0);
        //    //            newSurfaceViewport.AddChild(renderElem);
        //    //            //-----------------------------------------------------                        
        //    //            IntPtr tmpHandle = newForm.Handle;//force newform to create window handle

        //    //            //-----------------------------------------------------              

        //    //            var platformWinBox = new PlatformWinBoxForm(newForm);
        //    //            topWinBox.PlatformWinBox = platformWinBox;
        //    //            platformWinBox.UseRelativeLocationToParent = true;
        //    //            platformWinBox.PreviewVisibilityChanged += PlatformWinBox_PreviewVisibilityChanged;
        //    //            platformWinBox.PreviewBoundChanged += PlatformWinBox_PreviewBoundChanged;
        //    //            platformWinBox.BoundsChanged += PlatformWinBox_BoundsChanged;
        //    //            platformWinBox.VisibityChanged += PlatformWinBox_VisibityChanged;
        //    //            _subForms.Add(newForm);
        //    //        }
        //    //    }
        //    //    else
        //    //    {
        //    //        _rootgfx.AddChild(renderElem);
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    _rootgfx.AddChild(renderElem);
        //    //}
        //}





        /// <summary>
        /// create new UIViewport based on this control's current platform
        /// </summary>
        /// <returns></returns>
        public UISurfaceViewportControl CreateNewOne(int w, int h, InnerViewportKind innerViewportKind)
        {
            //each viewport has its own root graphics 

            UISurfaceViewportControl newViewportControl = new UISurfaceViewportControl();
            newViewportControl.Size = new System.Drawing.Size(w, h);
            RootGraphic newRootGraphic = _rootgfx.CreateNewOne(w, h);
            ITopWindowEventRoot topEventRoot = null;
            if (newRootGraphic is ITopWindowEventRootProvider)
            {
                topEventRoot = ((ITopWindowEventRootProvider)newRootGraphic).EventRoot;
            }
            newViewportControl.InitRootGraphics(
                newRootGraphic,//new root
                topEventRoot,
                innerViewportKind);
            return newViewportControl;
        }
    }


}
