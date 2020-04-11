﻿//MIT, 2016-present, WinterDev
using System;
using PixelFarm.Forms;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit;

using LayoutFarm;
using LayoutFarm.UI;
using PaintLab.Svg;

using LayoutFarm.CustomWidgets;
using PixelFarm.DrawingGL;

namespace TestGlfw
{
       

#if DEBUG
    class dbugMyBoxUI : UIElement
    {
        RenderElement _renderElem;
        public dbugMyBoxUI()
        {
        }

        public void SetRenderElement(RenderElement renderE)
        {
            _renderElem = renderE;
        }

        public override RenderElement CurrentPrimaryRenderElement => _renderElem;

        public override RenderElement GetPrimaryRenderElement(RootGraphic rootgfx) => _renderElem;

        public override void InvalidateGraphics() => _renderElem.InvalidateGraphics();
        protected override void OnMouseMove(UIMouseMoveEventArgs e)
        {
            if (_isMouseDown)
            {
                //dragging

                _renderElem.SetLocation(_renderElem.X + e.XDiff, _renderElem.Y + e.YDiff);
            }

            base.OnMouseMove(e);
        }
        bool _isMouseDown;
        protected override void OnMouseDown(UIMouseDownEventArgs e)
        {
            _isMouseDown = true;
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(UIMouseUpEventArgs e)
        {
            _isMouseDown = false;
            base.OnMouseUp(e);
        }

    }
    class dbugMySprite : RenderElement
    {
        VgVisualElement _renderVx;
        public dbugMySprite(RootGraphic root, int w, int h) : base(root, w, h)
        {
            _renderVx = VgVisualDocHelper.CreateVgVisualDocFromFile(@"lion.svg").VgRootElem;
        }
        protected override void RenderClientContent(DrawBoard d, UpdateArea updateArea)
        {

            using (Tools.More.BorrowVgPaintArgs(d.GetPainter(), out var paintArgs))
            {
                _renderVx.Paint(paintArgs);
            }
            //d.FillRectangle(Color.Blue, 0, 0, 50, 50);
        }
        public override void ResetRootGraphics(RootGraphic rootgfx)
        {

        }
    }
#endif
    static class MyApp3
    {

        public static int s_formW = 800;
        public static int s_formH = 600;

        static MyRootGraphic s_myRootGfx;
        static GraphicsViewRoot s_viewroot;
        static void Init(GlFwForm form)
        {
            form.MakeCurrent();

            OpenTK.Platform.Factory.GetCustomPlatformFactory = () => OpenTK.Platform.Egl.EglAngle.NewFactory();
            OpenTK.Toolkit.Init(new OpenTK.ToolkitOptions {
                Backend = OpenTK.PlatformBackend.PreferNative,
            });
            OpenTK.Graphics.PlatformAddressPortal.GetAddressDelegate = OpenTK.Platform.Utilities.CreateGetAddress();

            GLESInit.InitGLES();

            string icu_datadir = "brkitr"; //see brkitr folder, we link data from Typography project and copy to output if newer
            if (!System.IO.Directory.Exists(icu_datadir))
            {
                throw new System.NotSupportedException("dic");
            }
            var dicProvider = new Typography.TextBreak.IcuSimpleTextFileDictionaryProvider() { DataDir = icu_datadir };
            Typography.TextBreak.CustomBreakerBuilder.Setup(dicProvider);

            PixelFarm.CpuBlit.MemBitmapExtensions.DefaultMemBitmapIO = new PixelFarm.Drawing.WinGdi.GdiBitmapIO();

            PixelFarm.Platforms.StorageService.RegisterProvider(new YourImplementation.LocalFileStorageProvider(""));


            //---------------------------------------------------------------------------
            //Typography TextService
            OpenFontTextService textService = new OpenFontTextService();
            textService.LoadFontsFromFolder("Fonts");
            GlobalRootGraphic.TextService = textService;
            //---------------------------------------------------------------------------

            s_myRootGfx = new MyRootGraphic(s_formW, s_formH, textService);
            //---------------------------------------------------------------------------


            s_viewroot = new GraphicsViewRoot(s_formW, s_formH);
            MyGlfwTopWindowBridge bridge1 = new MyGlfwTopWindowBridge(s_myRootGfx, s_myRootGfx.TopWinEventPortal);
            ((MyGlfwTopWindowBridge.GlfwEventBridge)(form.WindowEventListener)).SetWindowBridge(bridge1);


            var glfwWindowWrapper = new GlfwWindowWrapper(form);
            bridge1.BindWindowControl(glfwWindowWrapper);

            s_viewroot.InitRootGraphics(s_myRootGfx,
                  s_myRootGfx.TopWinEventPortal,
                  InnerViewportKind.GLES,
                  glfwWindowWrapper,
                  bridge1);



            //------------------------------------------------------------------------
            //optional:
            if (s_viewroot.GetGLPainter() is GLPainter glPainter)
            {
                glPainter.SmoothingMode = SmoothingMode.AntiAlias;
            }


            //------------------------------------------------------------------------
            //optional:
            //if we don't set this, it will error on read-write image
            var pars = new PixelFarm.Platforms.ImageIOSetupParameters();
            pars.SaveToPng = (IntPtr imgBuffer, int stride, int width, int height, string filename) =>
            {
                using (System.Drawing.Bitmap newBmp = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    PixelFarm.CpuBlit.BitmapHelper.CopyToGdiPlusBitmapSameSize(imgBuffer, newBmp);
                    //save
                    newBmp.Save(filename);
                }
            };
            pars.ReadFromMemStream = (System.IO.MemoryStream ms, string kind) =>
            {
                //read  
                //TODO: review here again
                using (System.Drawing.Bitmap gdiBmp = new System.Drawing.Bitmap(ms))
                {
                    PixelFarm.CpuBlit.MemBitmap memBmp = new PixelFarm.CpuBlit.MemBitmap(gdiBmp.Width, gdiBmp.Height);
                    //#if DEBUG
                    //                        memBmp._dbugNote = "img;
                    //#endif

                    PixelFarm.CpuBlit.BitmapHelper.CopyFromGdiPlusBitmapSameSizeTo32BitsBuffer(gdiBmp, memBmp);
                    return memBmp;
                }

            };
            PixelFarm.Platforms.ImageIOPortal.Setup(pars);
            //------------------------------------------------------------------------
        }
#if DEBUG
        public static void dbugStart_Basic()
        {
            //demonstrate basic setup
            var bridge = new MyGlfwTopWindowBridge.GlfwEventBridge();
            var form = new GlFwForm(s_formW, s_formH, "GLES_GLFW", bridge);
            Init(form);
            //------ 

            //this is an app detail
            Box bgBox = new Box(s_formW, s_formH);
            bgBox.BackColor = Color.White;
            s_myRootGfx.AddChild(bgBox.GetPrimaryRenderElement(s_myRootGfx));

            //----------------------
            dbugMySprite sprite = new dbugMySprite(s_myRootGfx, 200, 300);
            dbugMyBoxUI boxUI = new dbugMyBoxUI();
            boxUI.SetRenderElement(sprite);
            sprite.SetController(boxUI);

            bgBox.Add(boxUI);
            //---------  
        }
#endif
        public static void Start()
        {
            var bridge = new MyGlfwTopWindowBridge.GlfwEventBridge();
            var form = new GlFwForm(s_formW, s_formH, "GLES_GLFW", bridge);
            //
            Init(form);
            //------

            AppHost appHost = new AppHost();
            AppHostConfig config = new AppHostConfig();
            config.RootGfx = s_myRootGfx;
            config.ScreenW = s_formW;
            config.ScreenH = s_formH;
            appHost.Setup(config);
            //------
            Box bgBox = new Box(s_formW, s_formH);
            bgBox.BackColor = Color.White;
            s_myRootGfx.AddChild(bgBox.GetPrimaryRenderElement(s_myRootGfx));
            //------ 


            //appHost.StartApp(new Demo_BoxEvents3());
            appHost.StartApp(new Demo_ScrollView());
            //appHost.StartApp(new Demo_MultipleLabels());
            //appHost.StartApp(new Demo_MultipleLabels2());
            //---------  

        }
    }
}