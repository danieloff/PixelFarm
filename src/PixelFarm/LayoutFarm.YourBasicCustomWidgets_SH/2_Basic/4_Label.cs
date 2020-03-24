﻿//Apache2, 2014-present, WinterDev

using PixelFarm.Drawing;
using LayoutFarm.UI;
namespace LayoutFarm.CustomWidgets
{

    public class Label : AbstractRectUI
    {
        string _text;
        Color _textColor; //text color
        Color _backColor;//actual filling color

        Color _textBackgroundColorHint; //background color hint, 
        //some time the label background is transparent
        //but its host has solid color, so this value will hint

        RequestFont _font;
        CustomTextRun _myTextRun;
        DrawTextTechnique _drawTextTechnique;

        //
        public Label(int w, int h)
            : base(w, h)
        {
            _textColor = PixelFarm.Drawing.Color.Black; //default?, use Theme?
            DrawTextTechnique = DrawTextTechnique.Stencil;//default
        }

#if DEBUG
        public bool dbugBreakOnRenderElement;
#endif
        public override RenderElement GetPrimaryRenderElement(RootGraphic rootgfx)
        {
            if (_myTextRun == null)
            {
                var t_run = new CustomTextRun(rootgfx, this.Width, this.Height);
                t_run.DrawTextTechnique = _drawTextTechnique;

                t_run.TextColor = _textColor;
                t_run.BackColor = _backColor;
                t_run.Text = this.Text;
                t_run.PaddingLeft = this.PaddingLeft;
                t_run.PaddingTop = this.PaddingTop;
                t_run.SetVisible(this.Visible);
                t_run.SetLocation(this.Left, this.Top);
                t_run.SetController(this);
                t_run.TransparentForMouseEvents = this.TransparentForMouseEvents;

                //
                if (_font != null)
                {
                    t_run.RequestFont = _font;
                }
                _myTextRun = t_run;
            }
            //-----------
            return _myTextRun;
        }
        protected override void InvalidatePadding(PaddingName paddingName, byte newValue)
        {
            if (_myTextRun == null) return;
            //
            switch (paddingName)
            {
                case PaddingName.Left:
                    _myTextRun.PaddingLeft = newValue;
                    break;
                case PaddingName.Top:
                    _myTextRun.PaddingTop = newValue;
                    break;
                case PaddingName.Right:
                    _myTextRun.PaddingRight = newValue;
                    break;
                case PaddingName.Bottom:
                    _myTextRun.PaddingBottom = newValue;
                    break;
                case PaddingName.AllSide:
                    _myTextRun.SetPaddings(this.PaddingLeft, this.PaddingTop, this.PaddingRight, this.PaddingBottom);
                    break;
                case PaddingName.AllSideSameValue:
                    _myTextRun.SetPaddings(newValue);
                    break;
            }
        }
        protected override void InvalidateMargin(MarginName marginName, short newValue)
        {
            //TODO:...
        }

        public override void SetFont(RequestFont font)
        {
            if (_myTextRun != null)
            {
                _myTextRun.RequestFont = font;
            }
            else
            {
                _font = font;
            }
        }
        //
        public override RenderElement CurrentPrimaryRenderElement => _myTextRun;
        //
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                if (_myTextRun != null)
                {
                    _myTextRun.Text = value;
                    _myTextRun.InvalidateGraphics();
                }
            }
        }
        public DrawTextTechnique DrawTextTechnique
        {
            get => _drawTextTechnique;
            set
            {
                _drawTextTechnique = value;
                if (_myTextRun != null)
                {
                    _myTextRun.DrawTextTechnique = value;
                }
            }
        }



        /// <summary>
        /// text color
        /// </summary>
        public Color TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                if (_myTextRun != null && _myTextRun.TextColor != value)
                {
                    _myTextRun.TextColor = value;
                    _myTextRun.InvalidateGraphics();
                }
            }
        }
        public Color BackColor
        {
            get => _backColor;
            set
            {
                _backColor = value;
                if (_myTextRun != null && _myTextRun.BackColor != value)
                {
                    _myTextRun.BackColor = value;
                    _myTextRun.InvalidateGraphics();
                }
            }
        }
        //
        public override int InnerWidth => this.Width;
        public override int InnerHeight => this.Height;

    }
}