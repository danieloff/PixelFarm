//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
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

namespace MatterHackers.Agg.Lines
{

    //============================================span_interpolator_persp_lerp

    public sealed class InterpolatorPerspectiveLerp : ISpanInterpolator
    {
        Transform.Perspective m_trans_dir;
        Transform.Perspective m_trans_inv;
        LineInterpolatorDDA2 m_coord_x;
        LineInterpolatorDDA2 m_coord_y;
        LineInterpolatorDDA2 m_scale_x;
        LineInterpolatorDDA2 m_scale_y;

        const int SUBPIXEL_SHIFT = 8;
        const int SUBPIXEL_SCALE = 1 << SUBPIXEL_SHIFT;

        //--------------------------------------------------------------------
        public InterpolatorPerspectiveLerp()
        {
            m_trans_dir = new Transform.Perspective();
            m_trans_inv = new Transform.Perspective();
        }

        //--------------------------------------------------------------------
        // Arbitrary quadrangle transformations
        public InterpolatorPerspectiveLerp(double[] src, double[] dst)
            : this()
        {
            quad_to_quad(src, dst);
        }

        //--------------------------------------------------------------------
        // Direct transformations 
        public InterpolatorPerspectiveLerp(double x1, double y1,
                                     double x2, double y2,
                                     double[] quad)
            : this()
        {
            rect_to_quad(x1, y1, x2, y2, quad);
        }

        //--------------------------------------------------------------------
        // Reverse transformations 
        public InterpolatorPerspectiveLerp(double[] quad,
                                     double x1, double y1,
                                     double x2, double y2)
            : this()
        {
            quad_to_rect(quad, x1, y1, x2, y2);
        }

        //--------------------------------------------------------------------
        // Set the transformations using two arbitrary quadrangles.
        public void quad_to_quad(double[] src, double[] dst)
        {
            m_trans_dir.quad_to_quad(src, dst);
            m_trans_inv.quad_to_quad(dst, src);
        }

        //--------------------------------------------------------------------
        // Set the direct transformations, i.e., rectangle -> quadrangle
        public void rect_to_quad(double x1, double y1, double x2, double y2, double[] quad)
        {
            double[] src = new double[8];
            src[0] = src[6] = x1;
            src[2] = src[4] = x2;
            src[1] = src[3] = y1;
            src[5] = src[7] = y2;
            quad_to_quad(src, quad);
        }


        //--------------------------------------------------------------------
        // Set the reverse transformations, i.e., quadrangle -> rectangle
        public void quad_to_rect(double[] quad,
                          double x1, double y1, double x2, double y2)
        {
            double[] dst = new double[8];
            dst[0] = dst[6] = x1;
            dst[2] = dst[4] = x2;
            dst[1] = dst[3] = y1;
            dst[5] = dst[7] = y2;
            quad_to_quad(quad, dst);
        }

        //--------------------------------------------------------------------
        // Check if the equations were solved successfully
        public bool is_valid() { return m_trans_dir.is_valid(); }

        //----------------------------------------------------------------
        public void begin(double x, double y, int len)
        {
            // Calculate transformed coordinates at x1,y1 
            double xt = x;
            double yt = y;
            m_trans_dir.Transform(ref xt, ref yt);
            int x1 = AggBasics.iround(xt * SUBPIXEL_SCALE);
            int y1 = AggBasics.iround(yt * SUBPIXEL_SCALE);

            double dx;
            double dy;
            double delta = 1 / (double)SUBPIXEL_SCALE;

            // Calculate scale by X at x1,y1
            dx = xt + delta;
            dy = yt;
            m_trans_inv.Transform(ref dx, ref dy);
            dx -= x;
            dy -= y;
            int sx1 = (int)AggBasics.uround(SUBPIXEL_SCALE / Math.Sqrt(dx * dx + dy * dy)) >> SUBPIXEL_SHIFT;

            // Calculate scale by Y at x1,y1
            dx = xt;
            dy = yt + delta;
            m_trans_inv.Transform(ref dx, ref dy);
            dx -= x;
            dy -= y;
            int sy1 = (int)AggBasics.uround(SUBPIXEL_SCALE / Math.Sqrt(dx * dx + dy * dy)) >> SUBPIXEL_SHIFT;

            // Calculate transformed coordinates at x2,y2 
            x += len;
            xt = x;
            yt = y;
            m_trans_dir.Transform(ref xt, ref yt);
            int x2 = AggBasics.iround(xt * SUBPIXEL_SCALE);
            int y2 = AggBasics.iround(yt * SUBPIXEL_SCALE);

            // Calculate scale by X at x2,y2
            dx = xt + delta;
            dy = yt;
            m_trans_inv.Transform(ref dx, ref dy);
            dx -= x;
            dy -= y;
            int sx2 = (int)AggBasics.uround(SUBPIXEL_SCALE / Math.Sqrt(dx * dx + dy * dy)) >> SUBPIXEL_SHIFT;

            // Calculate scale by Y at x2,y2
            dx = xt;
            dy = yt + delta;
            m_trans_inv.Transform(ref dx, ref dy);
            dx -= x;
            dy -= y;
            int sy2 = (int)AggBasics.uround(SUBPIXEL_SCALE / Math.Sqrt(dx * dx + dy * dy)) >> SUBPIXEL_SHIFT;

            // Initialize the interpolators
            m_coord_x = new LineInterpolatorDDA2(x1, x2, (int)len);
            m_coord_y = new LineInterpolatorDDA2(y1, y2, (int)len);
            m_scale_x = new LineInterpolatorDDA2(sx1, sx2, (int)len);
            m_scale_y = new LineInterpolatorDDA2(sy1, sy2, (int)len);
        }


        //----------------------------------------------------------------
        public void resynchronize(double xe, double ye, int len)
        {
            // Assume x1,y1 are equal to the ones at the previous end point 
            int x1 = m_coord_x.y();
            int y1 = m_coord_y.y();
            int sx1 = m_scale_x.y();
            int sy1 = m_scale_y.y();

            // Calculate transformed coordinates at x2,y2 
            double xt = xe;
            double yt = ye;
            m_trans_dir.Transform(ref xt, ref yt);
            int x2 = AggBasics.iround(xt * SUBPIXEL_SCALE);
            int y2 = AggBasics.iround(yt * SUBPIXEL_SCALE);

            double delta = 1 / (double)SUBPIXEL_SCALE;
            double dx;
            double dy;

            // Calculate scale by X at x2,y2
            dx = xt + delta;
            dy = yt;
            m_trans_inv.Transform(ref dx, ref dy);
            dx -= xe;
            dy -= ye;
            int sx2 = (int)AggBasics.uround(SUBPIXEL_SCALE / Math.Sqrt(dx * dx + dy * dy)) >> SUBPIXEL_SHIFT;

            // Calculate scale by Y at x2,y2
            dx = xt;
            dy = yt + delta;
            m_trans_inv.Transform(ref dx, ref dy);
            dx -= xe;
            dy -= ye;
            int sy2 = (int)AggBasics.uround(SUBPIXEL_SCALE / Math.Sqrt(dx * dx + dy * dy)) >> SUBPIXEL_SHIFT;

            // Initialize the interpolators
            m_coord_x = new LineInterpolatorDDA2(x1, x2, (int)len);
            m_coord_y = new LineInterpolatorDDA2(y1, y2, (int)len);
            m_scale_x = new LineInterpolatorDDA2(sx1, sx2, (int)len);
            m_scale_y = new LineInterpolatorDDA2(sy1, sy2, (int)len);
        }

        public Transform.ITransform transformer()
        {
            throw new System.NotImplementedException();
        }

        public void transformer(Transform.ITransform trans)
        {
            throw new System.NotImplementedException();
        }

        //----------------------------------------------------------------
        public void Next()
        {
            m_coord_x.Next();
            m_coord_y.Next();
            m_scale_x.Next();
            m_scale_y.Next();
        }

        //----------------------------------------------------------------
        public void coordinates(out int x, out int y)
        {
            x = m_coord_x.y();
            y = m_coord_y.y();
        }

        //----------------------------------------------------------------
        public void local_scale(out int x, out int y)
        {
            x = m_scale_x.y();
            y = m_scale_y.y();
        }

        //----------------------------------------------------------------
        public void transform(ref double x, ref double y)
        {
            m_trans_dir.Transform(ref x, ref y);
        }
    } 
}