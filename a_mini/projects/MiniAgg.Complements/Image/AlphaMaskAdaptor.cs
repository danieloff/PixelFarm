//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------
using System;
using MatterHackers.Agg;

namespace MatterHackers.Agg.Image
{
    //==================================================pixfmt_amask_adaptor
    public sealed class AlphaMaskAdaptor : ImageProxy
    {
        IAlphaMask m_mask;
        ArrayPOD<byte> m_spans;

        enum span_extra_tail_e { span_extra_tail = 256 };

        const byte cover_full = 255;

        void realloc_span(int len)
        {
            if (len > m_spans.Size())
            {
                m_spans.Resize(len + (int)span_extra_tail_e.span_extra_tail);
            }
        }
         

        void init_span(int len, byte cover)
        {
            realloc_span(len);
            agg_basics.memset(m_spans.Array, 0, cover, len);
        }

        void init_span(int len, byte[] covers, int coversIndex)
        {
            realloc_span(len);
            byte[] array = m_spans.Array;
            for (int i = 0; i < (int)len; i++)
            {
                array[i] = covers[coversIndex + i];
            }
        }

        public AlphaMaskAdaptor(IImageBuffer image, IAlphaMask mask)
            : base(image)
        {
            linkedImage = image;
            m_mask = mask;
            m_spans = new ArrayPOD<byte>(255);
        }

        public void AttachImage(IImageBuffer image)
        {
            linkedImage = image;
        }
        public void attach_alpha_mask(IAlphaMask mask)
        {
            m_mask = mask;
        }

        public void copy_pixel(int x, int y, RGBA_Bytes c)
        {
            linkedImage.BlendPixel(x, y, c, m_mask.pixel(x, y));
        }

        public override void copy_hline(int x, int y, int len, RGBA_Bytes c)
        {
            throw new NotImplementedException();
            /*
                        realloc_span((int)len);
                        unsafe
                        {
                            fixed (byte* pBuffer = m_span.Array)
                            {
                                m_mask.fill_hspan(x, y, pBuffer, (int)len);
                                m_LinkedImage.blend_solid_hspan(x, y, len, c, pBuffer);
                            }
                        }
             */
        }

        public override void blend_hline(int x1, int y, int x2, RGBA_Bytes c, byte cover)
        {
            int len = x2 - x1 + 1;
            if (cover == cover_full)
            {
                realloc_span(len);
                m_mask.combine_hspanFullCover(x1, y, m_spans.Array, 0, (int)len);
                linkedImage.blend_solid_hspan(x1, y, (int)len, c, m_spans.Array, 0);
            }
            else
            {
                init_span(len, cover);
                m_mask.combine_hspan(x1, y, m_spans.Array, 0, (int)len);
                linkedImage.blend_solid_hspan(x1, y, (int)len, c, m_spans.Array, 0);
            }
        }

        public override void copy_vline(int x, int y, int len, RGBA_Bytes c)
        {
            throw new NotImplementedException(); /*
            realloc_span((int)len);
            unsafe
            {
                fixed (byte* pBuffer = m_span.Array)
                {
                    m_mask.fill_vspan(x, y, pBuffer, (int)len);
                    m_LinkedImage.blend_solid_vspan(x, y, len, c, pBuffer);
                }
            }
                                                  */
        }

        public override void blend_vline(int x, int y1, int y2, RGBA_Bytes c, byte cover)
        {
            throw new NotImplementedException(); /*
            int len = y2 - y1 + 1;
            init_span(len, cover);
            unsafe
            {
                fixed (byte* pBuffer = m_span.Array)
                {
                    m_mask.combine_vspan(x, y1, pBuffer, len);
                    throw new System.NotImplementedException("blend_solid_vspan does not take a y2 yet");
                    //m_pixf.blend_solid_vspan(x, y1, y2, c, pBuffer);
                }
            }
                                                  */
        }

        public override void blend_solid_hspan(int x, int y, int len, RGBA_Bytes color, byte[] covers, int coversIndex)
        {
            byte[] buffer = m_spans.Array;
            m_mask.combine_hspan(x, y, covers, coversIndex, len);
            linkedImage.blend_solid_hspan(x, y, len, color, covers, coversIndex);
        }

        public override void blend_solid_vspan(int x, int y, int len, RGBA_Bytes c, byte[] covers, int coversIndex)
        {
            throw new System.NotImplementedException();
#if false
            init_span((int)len, covers);
            unsafe
            {
                fixed (byte* pBuffer = m_span.Array)
                {
                    m_mask.combine_vspan(x, y, pBuffer, (int)len);
                    m_LinkedImage.blend_solid_vspan(x, y, len, c, pBuffer);
                }
            }
#endif
        }

        public override void copy_color_hspan(int x, int y, int len, RGBA_Bytes[] colors, int colorsIndex)
        {
            throw new System.NotImplementedException();
#if false
            realloc_span((int)len);
            unsafe
            {
                fixed (byte* pBuffer = m_span.GetArray())
                {
                    m_mask.fill_hspan(x, y, pBuffer, (int)len);
                    m_pixf.blend_color_hspan(x, y, len, colors, pBuffer, cover_full);
                }
            }
#endif
        }

        public override void copy_color_vspan(int x, int y, int len, RGBA_Bytes[] colors, int colorsIndex)
        {
            throw new System.NotImplementedException();
#if false
            realloc_span((int)len);
            unsafe
            {
                fixed (byte* pBuffer = m_span.GetArray())
                {
                    m_mask.fill_vspan(x, y, pBuffer, (int)len);
                    m_pixf.blend_color_vspan(x, y, len, colors, pBuffer, cover_full);
                }
            }
#endif
        }

        public override void blend_color_hspan(int x, int y, int len, RGBA_Bytes[] colors, int colorsIndex, byte[] covers, int coversIndex, bool firstCoverForAll)
        {
            throw new System.NotImplementedException();
#if false
            unsafe
            {
                fixed (byte* pBuffer = m_span.GetArray())
                {
                    if (covers != null)
                    {
                        init_span((int)len, covers);
                        m_mask.combine_hspan(x, y, pBuffer, (int)len);
                    }
                    else
                    {
                        realloc_span((int)len);
                        m_mask.fill_hspan(x, y, pBuffer, (int)len);
                    }
                    m_pixf.blend_color_hspan(x, y, len, colors, pBuffer, cover);
                }
            }
#endif
        }

        public override void blend_color_vspan(int x, int y, int len, RGBA_Bytes[] colors, int colorsIndex, byte[] covers, int coversIndex, bool firstCoverForAll)
        {
            throw new System.NotImplementedException();
#if false
            unsafe
            {
                fixed (byte* pBuffer = m_span.GetArray())
                {
                    if (covers != null)
                    {
                        init_span((int)len, covers);
                        m_mask.combine_vspan(x, y, pBuffer, (int)len);
                    }
                    else
                    {
                        realloc_span((int)len);
                        m_mask.fill_vspan(x, y, pBuffer, (int)len);
                    }
                    m_pixf.blend_color_vspan(x, y, len, colors, pBuffer, cover);
                }
            }
#endif
        }
    };
}
