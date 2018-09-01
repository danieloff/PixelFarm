﻿//Apache2, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using System.Text;
using PixelFarm.Drawing;

using LayoutFarm.Text;
using LayoutFarm.UI;
namespace LayoutFarm.CustomWidgets
{






    public class TextBox : AbstractRectUI
    {
        TextSurfaceEventListener _textSurfaceListener;
        TextEditRenderBox _textEditRenderElement;
        bool _multiline;
        TextSpanStyle _defaultSpanStyle;
        Color _backgroundColor = Color.White;
        string _userTextContent;

        public TextBox(int width, int height, bool multiline)
            : base(width, height)
        {
            this._multiline = multiline;

        }
        public void ClearText()
        {
            if (_textEditRenderElement != null)
            {
                this._textEditRenderElement.ClearAllChildren();
            }
        }
        public Color BackgroundColor
        {
            get { return this._backgroundColor; }
            set
            {
                this._backgroundColor = value;
                if (_textEditRenderElement != null)
                {
                    _textEditRenderElement.BackgroundColor = value;
                }
            }
        }

        public TextSpanStyle DefaultSpanStyle
        {
            get { return this._defaultSpanStyle; }
            set
            {
                this._defaultSpanStyle = value;
                if (_textEditRenderElement != null)
                {
                    _textEditRenderElement.CurrentTextSpanStyle = value;

                }
            }
        }
        public ContentTextSplitter TextSplitter
        {
            get;
            set;
        }
        public int CurrentLineHeight
        {
            get
            {
                return this._textEditRenderElement.CurrentLineHeight;
            }
        }
        public Point CaretPosition
        {
            get { return this._textEditRenderElement.CurrentCaretPos; }
        }
        public int CurrentLineCharIndex
        {
            get { return this._textEditRenderElement.CurrentLineCharIndex; }
        }
        public int CurrentRunCharIndex
        {
            get { return this._textEditRenderElement.CurrentTextRunCharIndex; }
        }
        public string Text
        {
            get
            {
                if (_textEditRenderElement != null)
                {
                    StringBuilder stBuilder = new StringBuilder();
                    _textEditRenderElement.CopyContentToStringBuilder(stBuilder);
                    return stBuilder.ToString();
                }
                else
                {
                    return _userTextContent;
                }
            }
            set
            {
                if (_textEditRenderElement == null)
                {
                    this._userTextContent = value;
                    return;
                }
                //---------------                 

                this._textEditRenderElement.ClearAllChildren();
                //convert to runs
                if (value == null)
                {
                    return;
                }
                //---------------                 
                using (var reader = new System.IO.StringReader(value))
                {
                    string line = reader.ReadLine(); // line
                    int lineCount = 0;
                    while (line != null)
                    {
                        if (lineCount > 0)
                        {
                            _textEditRenderElement.SplitCurrentLineToNewLine();
                        }

                        //create textspan
                        //user can parse text line to smaller span
                        //eg. split by whitespace 

                        if (this.TextSplitter != null)
                        {
                            //parse with textsplitter 
                            //TODO: review here ***
                            //we should encapsulte the detail of this ?
                            //1.technique, 2. performance
                            //char[] buffer = value.ToCharArray();
                            char[] buffer = line.ToCharArray();
                            if (buffer.Length == 0)
                            {

                            }
                            foreach (Composers.TextSplitBound splitBound in TextSplitter.ParseWordContent(buffer, 0, buffer.Length))
                            {
                                int startIndex = splitBound.startIndex;
                                int length = splitBound.length;
                                char[] splitBuffer = new char[length];
                                Array.Copy(buffer, startIndex, splitBuffer, 0, length);

                                //TODO: review
                                //this just test ***  that text box can hold freeze text run
                                //var textspan = textEditRenderElement.CreateFreezeTextRun(splitBuffer);
                                //-----------------------------------
                                //but for general 
                                EditableRun textspan = _textEditRenderElement.CreateEditableTextRun(splitBuffer);
                                _textEditRenderElement.AddTextRun(textspan);
                            }
                        }
                        else
                        {
                            var textspan = _textEditRenderElement.CreateEditableTextRun(line);
                            _textEditRenderElement.AddTextRun(textspan);
                        }


                        lineCount++;
                        line = reader.ReadLine();
                    }
                }
                this.InvalidateGraphics();
            }
        }
        public override void Focus()
        {
            //request keyboard focus
            base.Focus();
            _textEditRenderElement.Focus();
        }
        public override void Blur()
        {
            base.Blur();
        }


        protected override bool HasReadyRenderElement
        {
            get { return this._textEditRenderElement != null; }
        }
        public override RenderElement CurrentPrimaryRenderElement
        {
            get { return this._textEditRenderElement; }
        }
        public override RenderElement GetPrimaryRenderElement(RootGraphic rootgfx)
        {
            if (_textEditRenderElement == null)
            {
                var tbox = new TextEditRenderBox(rootgfx, this.Width, this.Height, _multiline);
                tbox.SetLocation(this.Left, this.Top);
                tbox.HasSpecificWidthAndHeight = true;
                if (this._defaultSpanStyle.IsEmpty())
                {
                    this._defaultSpanStyle = new TextSpanStyle();
                    this._defaultSpanStyle.ReqFont = rootgfx.DefaultTextEditFontInfo;
                    tbox.CurrentTextSpanStyle = this._defaultSpanStyle;
                }
                else
                {
                    tbox.CurrentTextSpanStyle = this._defaultSpanStyle;
                }
                tbox.BackgroundColor = this._backgroundColor;
                tbox.SetController(this);

                if (this._textSurfaceListener != null)
                {
                    tbox.TextSurfaceListener = _textSurfaceListener;
                }
                this._textEditRenderElement = tbox;
                if (_userTextContent != null)
                {
                    this.Text = _userTextContent;
                    _userTextContent = null;//clear
                }
            }
            return _textEditRenderElement;
        }
        //----------------------------------------------------------------
        public bool IsMultilineTextBox
        {
            get { return this._multiline; }
        }
        public void FindCurrentUnderlyingWord(out int startAt, out int len)
        {
            _textEditRenderElement.FindCurrentUnderlyingWord(out startAt, out len);
        }
        public TextSurfaceEventListener TextEventListener
        {
            get { return this._textSurfaceListener; }
            set
            {
                this._textSurfaceListener = value;
                if (this._textEditRenderElement != null)
                {
                    this._textEditRenderElement.TextSurfaceListener = value;
                }
            }
        }
        public EditableRun CurrentTextSpan
        {
            get
            {
                return this._textEditRenderElement.CurrentTextRun;
            }
        }

        public void ReplaceCurrentTextRunContent(int nBackspaces, string newstr)
        {
            if (_textEditRenderElement != null)
            {
                _textEditRenderElement.ReplaceCurrentTextRunContent(nBackspaces, newstr);
            }
        }
        public void ReplaceCurrentLineTextRuns(IEnumerable<EditableRun> textRuns)
        {
            if (_textEditRenderElement != null)
            {
                _textEditRenderElement.ReplaceCurrentLineTextRuns(textRuns);
            }
        }
        public void CopyCurrentLine(StringBuilder stbuilder)
        {
            _textEditRenderElement.CopyCurrentLine(stbuilder);
        }
        //---------------------------------------------------------------- 
        protected override void OnMouseLeave(UIMouseEventArgs e)
        {
            e.MouseCursorStyle = MouseCursorStyle.Arrow;
        }
        protected override void OnDoubleClick(UIMouseEventArgs e)
        {
            _textEditRenderElement.HandleDoubleClick(e);
            e.CancelBubbling = true;
        }
        protected override void OnKeyPress(UIKeyEventArgs e)
        {
            //eg. mask text
            //we collect actual key and send the mask to to the background 
            _textEditRenderElement.HandleKeyPress(e);
            e.CancelBubbling = true;
        }
        protected override void OnKeyDown(UIKeyEventArgs e)
        {
            _textEditRenderElement.HandleKeyDown(e);
            e.CancelBubbling = true;
        }
        protected override void OnKeyUp(UIKeyEventArgs e)
        {
            _textEditRenderElement.HandleKeyUp(e);
            e.CancelBubbling = true;
        }
        protected override bool OnProcessDialogKey(UIKeyEventArgs e)
        {
            if (_textEditRenderElement.HandleProcessDialogKey(e))
            {
                e.CancelBubbling = true;
                return true;
            }
            return false;
        }
        protected override void OnMouseDown(UIMouseEventArgs e)
        {
            this.Focus();
            e.MouseCursorStyle = MouseCursorStyle.IBeam;
            e.CancelBubbling = true;
            e.CurrentContextElement = this;
            _textEditRenderElement.HandleMouseDown(e);
        }
        protected override void OnLostKeyboardFocus(UIFocusEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            _textEditRenderElement.Blur();
        }
        protected override void OnMouseMove(UIMouseEventArgs e)
        {
            if (e.IsDragging)
            {
                _textEditRenderElement.HandleDrag(e);
                e.CancelBubbling = true;
                e.MouseCursorStyle = MouseCursorStyle.IBeam;
            }
        }
        protected override void OnMouseUp(UIMouseEventArgs e)
        {
            if (e.IsDragging)
            {
                _textEditRenderElement.HandleDragEnd(e);
            }
            else
            {
                _textEditRenderElement.HandleMouseUp(e);
            }
            e.MouseCursorStyle = MouseCursorStyle.Default;
            e.CancelBubbling = true;
        }


        public override void Walk(UIVisitor visitor)
        {
            visitor.BeginElement(this, "textbox");
            this.Describe(visitor);
            visitor.TextNode(this.Text);
            visitor.EndElement();
        }
    }


    public class MaskTextBox : AbstractRectUI
    {
        TextSurfaceEventListener _textSurfaceListener;
        TextEditRenderBox _textEditRenderElement;

        bool _multiline;
        TextSpanStyle _defaultSpanStyle;
        Color _backgroundColor = Color.White;


        List<char> _actualUserInputText = new List<char>();

        public MaskTextBox(int width, int height, bool multiline)
            : base(width, height)
        {
            //
            this._multiline = false;
        }
        public void ClearText()
        {
            if (_textEditRenderElement != null)
            {
                this._textEditRenderElement.ClearAllChildren();
            }
            _actualUserInputText.Clear();
        }
        public Color BackgroundColor
        {
            get { return this._backgroundColor; }
            set
            {
                this._backgroundColor = value;
                if (_textEditRenderElement != null)
                {
                    _textEditRenderElement.BackgroundColor = value;
                }
            }
        }

        public TextSpanStyle DefaultSpanStyle
        {
            get { return this._defaultSpanStyle; }
            set
            {
                this._defaultSpanStyle = value;
                if (_textEditRenderElement != null)
                {
                    _textEditRenderElement.CurrentTextSpanStyle = value;

                }
            }
        }
        public ContentTextSplitter TextSplitter
        {
            get;
            set;
        }
        public int CurrentLineHeight
        {
            get
            {
                return this._textEditRenderElement.CurrentLineHeight;
            }
        }
        public Point CaretPosition
        {
            get { return this._textEditRenderElement.CurrentCaretPos; }
        }
        public int CurrentLineCharIndex
        {
            get { return this._textEditRenderElement.CurrentLineCharIndex; }
        }
        public int CurrentRunCharIndex
        {
            get { return this._textEditRenderElement.CurrentTextRunCharIndex; }
        }
        public string Text
        {
            get
            {
                StringBuilder stBuilder = new StringBuilder();
                stBuilder.Append(this._actualUserInputText.ToArray());
                return stBuilder.ToString();
            }
        }
        public override void Focus()
        {
            //request keyboard focus
            base.Focus();
            _textEditRenderElement?.Focus();
        }
        public override void Blur()
        {
            base.Blur();
            _textEditRenderElement?.Blur();
        }

        protected override bool HasReadyRenderElement
        {
            get { return this._textEditRenderElement != null; }
        }
        public override RenderElement CurrentPrimaryRenderElement
        {
            get { return this._textEditRenderElement; }
        }
        public override RenderElement GetPrimaryRenderElement(RootGraphic rootgfx)
        {
            if (_textEditRenderElement == null)
            {
                var tbox = new TextEditRenderBox(rootgfx, this.Width, this.Height, _multiline);
                tbox.SetLocation(this.Left, this.Top);
                tbox.HasSpecificWidthAndHeight = true;
                if (this._defaultSpanStyle.IsEmpty())
                {
                    this._defaultSpanStyle = new TextSpanStyle();
                    this._defaultSpanStyle.ReqFont = rootgfx.DefaultTextEditFontInfo;
                    tbox.CurrentTextSpanStyle = this._defaultSpanStyle;
                }
                else
                {
                    tbox.CurrentTextSpanStyle = this._defaultSpanStyle;
                }
                tbox.BackgroundColor = this._backgroundColor;
                tbox.SetController(this);

                if (this._textSurfaceListener != null)
                {
                    tbox.TextSurfaceListener = _textSurfaceListener;
                }
                this._textEditRenderElement = tbox;
            }
            return _textEditRenderElement;
        }
        //----------------------------------------------------------------
        public bool IsMultilineTextBox
        {
            get { return this._multiline; }
        }
        public void FindCurrentUnderlyingWord(out int startAt, out int len)
        {
            _textEditRenderElement.FindCurrentUnderlyingWord(out startAt, out len);
        }
        public TextSurfaceEventListener TextEventListener
        {
            get { return this._textSurfaceListener; }
            set
            {
                this._textSurfaceListener = value;
                if (this._textEditRenderElement != null)
                {
                    this._textEditRenderElement.TextSurfaceListener = value;
                }
            }
        }

        //---------------------------------------------------------------- 
        protected override void OnMouseLeave(UIMouseEventArgs e)
        {
            e.MouseCursorStyle = MouseCursorStyle.Arrow;
        }
        protected override void OnDoubleClick(UIMouseEventArgs e)
        {
            _textEditRenderElement.HandleDoubleClick(e);
            e.CancelBubbling = true;
        }
        protected override void OnKeyPress(UIKeyEventArgs e)
        {
            //eg. mask text
            //we collect actual key and send the mask to to the background 
            _actualUserInputText.Add(e.KeyChar);
            e.SetKeyChar('*');
            //
            _textEditRenderElement.HandleKeyPress(e);
            e.CancelBubbling = true;
        }
        protected override void OnKeyDown(UIKeyEventArgs e)
        {

            switch (e.KeyCode)
            {
                case UIKeys.Home:
                    {
                        //this.DoHome(e.Shift);
                    }
                    break;
                case UIKeys.End:
                    {
                        //this.DoEnd(e.Shift);
                    }
                    break;
                case UIKeys.Back:
                    {
                        //    if (_internalTextLayerController.SelectionRange != null)
                        //    {
                        //        InvalidateGraphicLocalArea(this, GetSelectionUpdateArea());
                        //    }
                        //    else
                        //    {
                        //        InvalidateGraphicOfCurrentLineArea();
                        //    }
                        //    if (textSurfaceEventListener == null)
                        //    {
                        //        _internalTextLayerController.DoBackspace();
                        //    }
                        //    else
                        //    {
                        //        if (!TextSurfaceEventListener.NotifyPreviewBackSpace(textSurfaceEventListener) &&
                        //            _internalTextLayerController.DoBackspace())
                        //        {
                        //            TextSurfaceEventListener.NotifyCharactersRemoved(textSurfaceEventListener,
                        //                new TextDomEventArgs(_internalTextLayerController._updateJustCurrentLine));
                        //        }
                        //    }

                        //    EnsureCaretVisible();
                    }
                    break;
                case UIKeys.Delete:
                    {
                        //if (_internalTextLayerController.SelectionRange != null)
                        //{
                        //    InvalidateGraphicLocalArea(this, GetSelectionUpdateArea());
                        //}
                        //else
                        //{
                        //    InvalidateGraphicOfCurrentLineArea();
                        //}
                        //if (textSurfaceEventListener == null)
                        //{
                        //    _internalTextLayerController.DoDelete();
                        //}
                        //else
                        //{
                        //    VisualSelectionRangeSnapShot delpart = _internalTextLayerController.DoDelete();
                        //    TextSurfaceEventListener.NotifyCharactersRemoved(textSurfaceEventListener,
                        //        new TextDomEventArgs(_internalTextLayerController._updateJustCurrentLine));
                        //}

                        //EnsureCaretVisible();
                    }
                    break;
                default:
                    {
                        //if (textSurfaceEventListener != null)
                        //{
                        //    UIKeys keycode = e.KeyCode;
                        //    if (keycode >= UIKeys.F1 && keycode <= UIKeys.F12)
                        //    {
                        //        InvalidateGraphicLocalArea(this, GetSelectionUpdateArea());
                        //        TextSurfaceEventListener.NotifyFunctionKeyDown(textSurfaceEventListener, keycode);
                        //        EnsureCaretVisible();
                        //    }
                        //}

                    }
                    break;
            }

            _textEditRenderElement.HandleKeyDown(e);
            e.CancelBubbling = true;
        }
        protected override void OnKeyUp(UIKeyEventArgs e)
        {
            _textEditRenderElement.HandleKeyUp(e);
            e.CancelBubbling = true;
        }
        protected override bool OnProcessDialogKey(UIKeyEventArgs e)
        {
            if (_textEditRenderElement.HandleProcessDialogKey(e))
            {
                e.CancelBubbling = true;
                return true;
            }
            return false;
        }
        protected override void OnMouseDown(UIMouseEventArgs e)
        {
            this.Focus();
            e.MouseCursorStyle = MouseCursorStyle.IBeam;
            e.CancelBubbling = true;
            e.CurrentContextElement = this;
            _textEditRenderElement.HandleMouseDown(e);
        }
        protected override void OnLostKeyboardFocus(UIFocusEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            _textEditRenderElement.Blur();
        }
        protected override void OnMouseMove(UIMouseEventArgs e)
        {
            if (e.IsDragging)
            {
                _textEditRenderElement.HandleDrag(e);
                e.CancelBubbling = true;
                e.MouseCursorStyle = MouseCursorStyle.IBeam;
            }
        }
        protected override void OnMouseUp(UIMouseEventArgs e)
        {
            if (e.IsDragging)
            {
                _textEditRenderElement.HandleDragEnd(e);
            }
            else
            {
                _textEditRenderElement.HandleMouseUp(e);
            }
            e.MouseCursorStyle = MouseCursorStyle.Default;
            e.CancelBubbling = true;
        }


        public override void Walk(UIVisitor visitor)
        {
            visitor.BeginElement(this, "textbox_password");
            this.Describe(visitor);
            visitor.TextNode(this.Text);
            visitor.EndElement();
        }
    }
}