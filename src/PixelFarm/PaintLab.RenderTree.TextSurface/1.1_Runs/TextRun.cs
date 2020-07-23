﻿//Apache2, 2014-present, WinterDev

using System;
using PixelFarm.Drawing;

using Typography.Text;
using Typography.TextLayout;

namespace LayoutFarm.TextEditing
{

    struct TextRunModifier
    {
        //each editable run has it own (dynamic) char buffer  
        readonly CharSource _charSource;
        public TextRunModifier(CharSource charSource)
        {
            _charSource = charSource;
        }
        public TextRunModifier(TextLineBox textlineBox)
        {
            _charSource = textlineBox.CharSource;
        }
        internal void InsertAfter(TextRun run, int index, char c)
        {
            CharSpan _mybuffer = run._mybuffer;
            int oldLexLength = _mybuffer.Count;
            //
            if (index > -1 && index < _mybuffer.Count - 1)
            {
                //split to 2 parts
                //left-new-right

                int startAt = _charSource.LatestLen;
                _charSource.CopyAndAppend(_mybuffer.beginAt, index + 1);
                _charSource.Append(c);
                _charSource.CopyAndAppend(_mybuffer.beginAt + index + 1, oldLexLength - index - 1);

                run.SetNewContent(new CharSpan(_charSource, startAt, _charSource.LatestLen - startAt));
                run.InvalidateOwnerLineCharCount();
                run.UpdateRunWidth();
            }
            else if (index == -1)
            {
                int startAt = _charSource.LatestLen;
                //after -1 => first part
                _charSource.Append(c);
                //other part
                _charSource.CopyAndAppend(_mybuffer.beginAt, oldLexLength);

                run.SetNewContent(new CharSpan(_charSource, startAt, _charSource.LatestLen - startAt));
                run.InvalidateOwnerLineCharCount();
                run.UpdateRunWidth();
            }
            else if (index == oldLexLength - 1)
            {
                int startAt = _charSource.LatestLen;
                if (_mybuffer.beginAt + oldLexLength == startAt)
                {
                    //append last
                    //check a special case
                    _charSource.Append(c);
                    run.SetNewContent(new CharSpan(_charSource, _mybuffer.beginAt, _mybuffer.len + 1));
                }
                else
                {
                    _charSource.CopyAndAppend(_mybuffer.beginAt, oldLexLength);
                    _charSource.Append(c);
                    run.SetNewContent(new CharSpan(_charSource, startAt, _charSource.LatestLen - startAt));
                }
                run.InvalidateOwnerLineCharCount();
                run.UpdateRunWidth();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        internal CharSpan Remove(TextRun run, int startIndex, bool withFreeRun)
        {
            CharSpan _mybuffer = run._mybuffer;
            if (startIndex > -1 && _mybuffer.len > 0)
            {
                int oldLexLength = _mybuffer.Count;

                //copy of right part
                CharSpan rightpart = new CharSpan(_charSource, _mybuffer.beginAt + startIndex, oldLexLength - startIndex);


                //new span for left part
                //it is ok the use the same rgn
                CharSpan leftpart = new CharSpan(_charSource, _mybuffer.beginAt, startIndex);

                run.SetNewContent(leftpart);//replace 
                run.InvalidateOwnerLineCharCount();
                run.UpdateRunWidth();

                //char[] newBuff = new char[oldLexLength - length];
                //if (withFreeRun)
                //{
                //    freeRun = MakeCopy(startIndex, length);
                //}
                //if (startIndex > 0)
                //{
                //    Array.Copy(_mybuffer, 0, newBuff, 0, startIndex);
                //}

                //Array.Copy(_mybuffer, startIndex + length, newBuff, startIndex, oldLexLength - startIndex - length);
                //SetNewContent(newBuff);
                //InvalidateOwnerLineCharCount();
                //UpdateRunWidth();

                return rightpart;
            }
            return new CharSpan();//empty 
        }
        internal CharSpan Remove(TextRun run, int startIndex, int len, bool withFreeRun)
        {
            CharSpan _mybuffer = run._mybuffer;
            if (startIndex > -1 && _mybuffer.len > 0)
            {
                int oldLexLength = _mybuffer.Count;

                //copy of right part
                int right_startAt = startIndex + len;

                CharSpan rightpart = new CharSpan(_charSource, _mybuffer.beginAt + right_startAt, oldLexLength - right_startAt);

                //new span for left part
                //it is ok the use the same rgn
                CharSpan leftpart = new CharSpan(_charSource, _mybuffer.beginAt, startIndex);


                //then merge 2 part togather
                if (rightpart.len > 0 && leftpart.len > 0)
                {
                    int begin = _charSource.LatestLen;
                    _charSource.Append(leftpart);
                    _charSource.Append(rightpart);

                    run.SetNewContent(new CharSpan(_charSource, begin, _charSource.LatestLen - begin));
                }
                else if (rightpart.len > 0)
                {
                    //only right part
                    run.SetNewContent(rightpart);//replace 
                }
                else if (leftpart.len > 0)
                {
                    //only left part
                    run.SetNewContent(leftpart);//replace 
                }
                else
                {
                    //empty span
                    run.SetNewContent(new CharSpan(_charSource, 0, 0));
                }

                run.InvalidateOwnerLineCharCount();
                run.UpdateRunWidth();
                return rightpart;
            }
            return new CharSpan();//empty 
        }
    }


    public class TextRun : Run, IDisposable
    {

        internal CharSpan _mybuffer;
        int[] _charAdvances = null;
        RenderVxFormattedString _renderVxFormattedString; //re-creatable from original content

        bool _content_Parsed;//

        internal TextRun(RunStyle runstyle, CharSpan copyBuffer)
            : base(runstyle)
        {
            //we need font info (in style) for evaluating the size fo this span
            //without font info we can't measure the size of this span 

            _mybuffer = copyBuffer;
            UpdateRunWidth();
        }

        public bool DelayFormattedString { get; set; }
        public void Dispose()
        {
            if (_renderVxFormattedString != null)
            {
                _renderVxFormattedString.Dispose();
                _renderVxFormattedString = null;
            }
            _content_Parsed = false;
        }
        internal void SetNewContent(CharSpan newSpan)
        {
            _mybuffer = newSpan;
            _content_Parsed = false;
            _renderVxFormattedString?.Dispose();
            _renderVxFormattedString = null;

        }

        public override void WriteTo(TextCopyBuffer output)
        {
            _mybuffer.WriteTo(output);
        }
        public override void WriteTo(TextCopyBuffer output, int start, int len)
        {
            if (len > 0)
            {
                _mybuffer.WriteTo(output, start, len);
            }
        }
        public override void WriteTo(TextCopyBuffer output, int start)
        {
            int len = _mybuffer.Count - start;
            if (len > 0)
            {
                _mybuffer.WriteTo(output, start, len);
            }
        }

        CopyRun MakeCopy(int sourceIndex, int length)
        {
            if (length > 0)
            {
                //CopyRun newTextRun = null;
                char[] newContent = new char[length];
                _mybuffer.Copy(newContent, sourceIndex, length);
                return new CopyRun(newContent);
            }
            else
            {
                throw new Exception("string must be null or zero length");
            }
        }

        public override int GetRunWidth(int charOffset)
        {
            if (!_content_Parsed)
            {
                UpdateRunWidth();
            }

            //the content is parsed ***
            if (_mybuffer.Count == charOffset)
            {
                return this.Width;
            }
            else
            {
                //ca
                int total = 0;
                if (_charAdvances != null)
                {
                    for (int i = 0; i < charOffset; ++i)
                    {
                        total += _charAdvances[i];
                    }
                }
                return total;
            }
        }
        public override int GetRunWidth(int startAtCharOffset, int count)
        {
            if (!_content_Parsed)
            {
                UpdateRunWidth();
            }
            int total = 0;
            if (_charAdvances != null)
            {
                for (int i = startAtCharOffset; i < count; ++i)
                {
                    total += _charAdvances[i];
                }
            }
            return total;
        }

        public override string GetText() => _mybuffer.GetString();

        public override void UpdateRunWidth()
        {
            //1 check if we need to recalculate all?
            //TODO: review here, 
            //1. if mybuffer lenght is not changed,we don't need to alloc new array?

            if (_content_Parsed) { return; }

            if (_charAdvances == null || _charAdvances.Length != _mybuffer.Count)
            {
                //sometime we change only font style
                //so the buffer char_count is not changed
                _charAdvances = new int[_mybuffer.Count];
            }

            if (_renderVxFormattedString != null)
            {
                _renderVxFormattedString.Dispose();
                _renderVxFormattedString = null;
            }


            var measureResult = new TextSpanMeasureResult();
            measureResult.outputXAdvances = _charAdvances;

            GlobalTextService.TxtClient.CalculateUserCharGlyphAdvancePos(
               _mybuffer.GetTextBufferSpan(),
               RunStyle.ReqFont,
               ref measureResult);

            _content_Parsed = true;

            SetSize(measureResult.outputTotalW, measureResult.lineHeight);

            InvalidateGraphics();
        }


        protected void AdjustClientBounds(ref Rectangle bounds)
        {
            if (this.OwnerLine != null)
            {
                bounds.Offset(0, this.OwnerLine.Top);
            }
        }
        public override char GetChar(int index) => _mybuffer.GetUtf16Char(index);

        public override int CharacterCount => _mybuffer.Count;

        //public override void SetStyle(RunStyle runstyle)
        //{
        //    //TODO: review this again
        //    //update style may affect the 'visual' layout of the span
        //    //the span may expand large or shrink down
        //    //so we invalidate graphics area pre and post

        //    //TODO: review here***
        //    this.InvalidateGraphics();
        //    base.SetStyle(runstyle);
        //    this.InvalidateGraphics();
        //    UpdateRunWidth();
        //}


        public override CopyRun Copy(int startIndex, int length) => (startIndex > -1 && length > 0) ? MakeCopy(startIndex, length) : null;
        public override CopyRun Copy(int startIndex)
        {
            int length = _mybuffer.len - startIndex;
            if (startIndex > -1 && length > 0)
            {
                return MakeCopy(startIndex, length);
            }
            else
            {
                return null;
            }
        }
        const int SAME_FONT_SAME_TEXT_COLOR = 0;
        const int SAME_FONT_DIFF_TEXT_COLOR = 1;
        const int DIFF_FONT_SAME_TEXT_COLOR = 2;
        const int DIFF_FONT_DIFF_TEXT_COLOR = 3;

        static int EvaluateFontAndTextColor(DrawBoard d, RunStyle runstyle)
        {
            RequestFont font = runstyle.ReqFont;
            Color color = runstyle.FontColor;
            RequestFont currentTextFont = d.CurrentFont;
            Color currentTextColor = d.CurrentTextColor;
            if (font != null && font != currentTextFont)
            {
                if (currentTextColor != color)
                {
                    return DIFF_FONT_DIFF_TEXT_COLOR;
                }
                else
                {
                    return DIFF_FONT_SAME_TEXT_COLOR;
                }
            }
            else
            {
                if (currentTextColor != color)
                {
                    return SAME_FONT_DIFF_TEXT_COLOR;
                }
                else
                {
                    return SAME_FONT_SAME_TEXT_COLOR;
                }
            }
        }

        internal static RenderElement s_currentRenderE;

        void DrawText(DrawBoard d)
        {
            if (_renderVxFormattedString == null)
            {
                _renderVxFormattedString = d.CreateFormattedString(
                    _mybuffer.UnsafeInternalCharArr,
                    _mybuffer.beginAt,
                    _mybuffer.Count,
                    DelayFormattedString);
            }

            switch (_renderVxFormattedString.State)
            {
                case RenderVxFormattedString.VxState.Ready:
                    d.DrawRenderVx(_renderVxFormattedString, 0, 0);
                    break;
                case RenderVxFormattedString.VxState.NoStrip:
                    {
                        //put this to the update queue system
                        //(TODO: add extension method for this)
                        GlobalRootGraphic.CurrentRootGfx.EnqueueRenderRequest(
                            new RenderBoxes.RenderElementRequest(
                                s_currentRenderE,
                                RenderBoxes.RequestCommand.ProcessFormattedString,
                                _renderVxFormattedString));
                    }
                    break;
            }
        }
        public override void Draw(DrawBoard d, UpdateArea updateArea)
        {
            int bWidth = this.Width;
            int bHeight = this.Height;

#if DEBUG
            //canvas.dbug_DrawCrossRect(Color.Red, new Rectangle(0, 0, bWidth, bHeight));
            //canvas.DrawRectangle(Color.Red, 0, 0, bWidth, bHeight);
#endif

            RunStyle style = this.RunStyle;//must 

            //set style to the canvas  
            switch (EvaluateFontAndTextColor(d, style))
            {
                case DIFF_FONT_DIFF_TEXT_COLOR:
                    {
                        RequestFont prevFont = d.CurrentFont;
                        Color prevColor = d.CurrentTextColor;
                        d.CurrentFont = style.ReqFont;
                        d.CurrentTextColor = style.FontColor;

                        DrawText(d);

                        d.CurrentFont = prevFont;
                        d.CurrentTextColor = prevColor;
                    }
                    break;
                case DIFF_FONT_SAME_TEXT_COLOR:
                    {
                        RequestFont prevFont = d.CurrentFont;
                        d.CurrentFont = style.ReqFont;

                        DrawText(d);

                        d.CurrentFont = prevFont;
                    }
                    break;

                case SAME_FONT_DIFF_TEXT_COLOR:
                    {
                        Color prevColor = d.CurrentTextColor;
                        d.CurrentTextColor = style.FontColor;
                        DrawText(d);
                        d.CurrentTextColor = prevColor;
                    }
                    break;
                default:
                    {
                        DrawText(d);
                    }
                    break;
            }

        }

        /// <summary>
        /// find charactor from pixel offset (pixel offset starts at the begining of this run)
        /// </summary>
        /// <param name="pixelOffset"></param>
        /// <returns></returns>
        public override CharLocation GetCharacterFromPixelOffset(int pixelOffset)
        {
            if (pixelOffset < Width)
            {
                int j = _charAdvances.Length;
                int accWidth = 0; //accummulate width
                for (int i = 0; i < j; i++)
                {
                    int charW = _charAdvances[i];
                    if (charW == 0)
                    {
                        continue;
                    }
                    if (accWidth + charW > pixelOffset)
                    {
                        //stop
                        //then decide that if width > char/w => please consider stop at next char

                        if (pixelOffset - accWidth > (charW / 2))
                        {
                            //this run is no select 
                            //but we must check if 
                            if ((i < j - 1) && (_charAdvances[i + 1] == 0))
                            {
                                //temp fix for surrogate pair
                                return new CharLocation(accWidth + charW, i + 2);
                            }

                            return new CharLocation(accWidth + charW, i + 1);
                        }
                        else
                        {
                            return new CharLocation(accWidth, i);
                        }
                    }
                    else
                    {
                        accWidth += charW;
                    }
                }

                //end of this run
                return new CharLocation(accWidth, j);
            }
            else
            {
                //pixelOffset>=width
                //not in this run, may be next run
                return new CharLocation(0, 1);
            }
        }

        public override CopyRun LeftCopy(int index) => (index > 0) ? MakeCopy(0, index) : null;




        internal override bool IsInsertable => true;

        //
#if DEBUG
        public override string ToString() => _mybuffer.GetString();
#endif

    }
}
