﻿//MIT, 2016-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.CpuBlit.FragmentProcessing;

namespace PixelFarm.CpuBlit
{
    class LinearGradientPair
    {
        public float _dx1;
        public float _dx2;
        float _stripWidth;
        public Color _c1;
        public Color _c2;
        public LinearGradientPair(float dx1, float dx2, Color c1, Color c2)
        {
            _dx1 = dx1;
            _dx2 = dx2;
            _c1 = c1;
            _c2 = c2;
            _stripWidth = _dx2 - _dx1;
        }
        public float StripWidth => _stripWidth;
        public Color GetColor(float x)
        {
            return _c1.CreateGradient(_c2, (x - _dx1) / _stripWidth);
        }
    }

    class LinearGradientSpanGen : ISpanGenerator
    {
        LinearGradientPair[] _pairList;
        Color _beginColor;
        Color _endColor;
        float _beginX;
        float _beginY;
        float _endX;
        float _endY;
        Affine _transformBackToHorizontal;
        float _totalLen;

        public LinearGradientSpanGen() { }
        public void Prepare()
        {

        }
        public void ResolveBrush(LinearGradientBrush linearGrBrush)
        {

            PointF p1 = linearGrBrush.StartPoint;
            PointF p2 = linearGrBrush.EndPoint;
            //assume horizontal line

            _beginX = p1.X;
            _beginY = p1.Y;
            _endX = p2.X;
            _endY = p2.Y;

            //--------------
            //find transformation matrix
            double angle = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            _transformBackToHorizontal = Affine.NewRotation(angle).CreateInvert();
            _totalLen = (float)Math.Sqrt((_endX - _beginX) * (_endX - _beginX) + (_endY - _beginY) * (_endY - _beginY));
            double tmpX = _beginX;
            double tmpY = _beginY;
            _transformBackToHorizontal.Transform(ref tmpX, ref tmpY);
            _beginX = (float)tmpX;
            _beginY = (float)tmpY;
            //--------------
            tmpX = _endX;
            tmpY = _endY;
            _transformBackToHorizontal.Transform(ref tmpX, ref tmpY);
            _endX = (float)tmpX;
            _endY = (float)tmpY;
            //--------------

            ColorStop[] colorStops = linearGrBrush.ColorStops;

            int pairCount = colorStops.Length - 1;
            _pairList = new LinearGradientPair[pairCount];

            ColorStop c0 = ColorStop.Empty;
            ColorStop c1 = ColorStop.Empty;
            for (int i = 0; i < pairCount; ++i)
            {
                c0 = colorStops[i];
                c1 = colorStops[i + 1];
                if (i == 0)
                {
                    _beginColor = c0.Color;
                }

                var pairN = new LinearGradientPair(
                   _beginX + c0.Offset * _totalLen, //to actual pixel
                   _beginX + c1.Offset * _totalLen,//to actual pixel
                    c0.Color,
                    c1.Color);
                _pairList[i] = pairN;
            }
            _endColor = c1.Color;
        }
        public void SetOffset(float x, float y)
        {

        }
        Color GetColorAt(int x, int y)
        {
            double new_x = x;
            double new_y = y;
            _transformBackToHorizontal.Transform(ref new_x, ref new_y);

            if (new_x <= _beginX)
            {
                return _beginColor;
            }
            else if (new_x >= _endX)
            {
                return _endColor;
            }
            //-----------------
            //find proper range
            for (int i = 0; i < _pairList.Length; ++i)
            {
                LinearGradientPair p = _pairList[i];
                if (new_x >= p._dx1 && new_x < p._dx2)
                {
                    return p.GetColor((float)new_x);
                }
            }
            return _endColor;
        }
        public void GenerateColors(Color[] outputColors, int startIndex, int x, int y, int spanLen)
        {
            //start at current span generator 
            for (int cur_x = x; cur_x < x + spanLen; ++cur_x)
            {
                outputColors[startIndex] = GetColorAt(cur_x, y);
                startIndex++;
            }
        }
    }



    class CircularGradientSpanGen : ISpanGenerator
    {
        LinearGradientPair[] _pairList;
        int _center_x = 0;
        int _center_y = 0;
        Color _endColor;

        static float[] s_simpleDistanceTable = new float[1024 * 1024];
        static CircularGradientSpanGen()
        {
            int index = 0;
            for (int y = 0; y < 1024; ++y)
            {
                for (int x = 0; x < 1024; x++)
                {
                    s_simpleDistanceTable[index++] = (float)Math.Sqrt(x * x + y * y);
                }
            }
        }
        static float CalculateDistance(int dx, int dy)
        {
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;
            if (dx < 1024 && dy < 1024)
            {
                return s_simpleDistanceTable[dx + dy * 1024];
            }
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
        public void Prepare()
        {

        }
        public void ResolveBrush(CircularGradientBrush linearGrBrush)
        {
            //for gradient :

            PointF p1 = linearGrBrush.StartPoint;
            PointF p2 = linearGrBrush.EndPoint;

            _center_x = (int)Math.Round(p1.X);
            _center_y = (int)Math.Round(p1.Y);

            float r = (float)Math.Sqrt((p2.X - _center_x) * (p2.X - _center_x) + (p2.Y - _center_y) * (p2.Y - _center_y));

            ColorStop[] colorStops = linearGrBrush.ColorStops;

            int pairCount = colorStops.Length - 1;
            _pairList = new LinearGradientPair[pairCount];

            ColorStop c0 = ColorStop.Empty;
            ColorStop c1 = ColorStop.Empty;
            for (int i = 0; i < pairCount; ++i)
            {
                c0 = colorStops[i];
                c1 = colorStops[i + 1];

                var pairN = new LinearGradientPair(
                    c0.Offset * r, //to actual pixel
                    c1.Offset * r,//to actual pixel
                    c0.Color,
                    c1.Color);
                _pairList[i] = pairN;
            }
            _endColor = c1.Color;
        }

        /// <summary>
        /// set origin 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetOrigin(float x, float y)
        {

        }
        LinearGradientPair GetProperColorRange(float distance)
        {
            for (int i = 0; i < _pairList.Length; ++i)
            {
                LinearGradientPair p = _pairList[i];
                if (distance >= p._dx1 && distance < p._dx2)
                {
                    return p;
                }
            }
            return null;
        }
        public void GenerateColors(Color[] outputColors, int startIndex, int x, int y, int spanLen)
        {
            //start at current span generator 
            for (int cur_x = x; cur_x < x + spanLen; ++cur_x)
            {
                float r = CalculateDistance((cur_x - _center_x), (y - _center_y));
                //float r = (float)Math.Sqrt((cur_x - _center_x) * (cur_x - _center_x) + (y - _center_y) * (y - _center_y));

                LinearGradientPair p = GetProperColorRange(r);
                if (p != null)
                {
                    outputColors[startIndex] = p.GetColor(r);
                }
                else
                {
                    outputColors[startIndex] = _endColor;
                }

                startIndex++;
            }
        }
    }

    public static class GradientSpanGenExtensions
    {
        public static void GenerateSampleGradientLine(CircularGradientBrush circularGraident, out Color[] output)
        {
            CircularGradientSpanGen spanGen = new CircularGradientSpanGen();
            spanGen.ResolveBrush(circularGraident);
            int len = (int)Math.Round(circularGraident.Length);
            output = new Color[len];
            spanGen.GenerateColors(output, 0, (int)circularGraident.StartPoint.X, (int)circularGraident.StartPoint.Y, len);
        }
    }

    class PolygonGradientBrush
    {

        float[] _xyCoords;
        Color[] _colors;

        internal ushort[] _vertIndices;
        internal float[] _outputCoords;
        internal int _vertexCount;

        List<VertexStore> _cacheVxsList = new List<VertexStore>();
        List<GouraudVerticeBuilder.CoordAndColor> _cacheColorAndVertexList = new List<GouraudVerticeBuilder.CoordAndColor>();

        public PolygonGradientBrush()
        {
            this.DilationValue = 0.175f;
            this.LinearGamma = 0.809f;
        }
        public float[] GetXYCoords() => _xyCoords;
        public Color[] GetColors() => _colors;
        public void BuildFrom(Drawing.PolygonGradientBrush polygonGrBrush)
        {
            List<Drawing.PolygonGradientBrush.ColorVertex2d> inputVertexList = polygonGrBrush.Vertices;

            int coordCount = inputVertexList.Count;
            _xyCoords = new float[coordCount * 2];
            _colors = new Color[coordCount];

            for (int i = 0; i < coordCount; ++i)
            {
                Drawing.PolygonGradientBrush.ColorVertex2d v = inputVertexList[i];
                _xyCoords[i << 1] = v.X;
                _xyCoords[(i << 1) + 1] = v.Y;
                _colors[i] = v.C;
            }
        }
        public float DilationValue { get; set; }
        public float LinearGamma { get; set; }

        public VertexStore CurrentVxs { get; set; }
        public int CachePartCount => _cacheVxsList.Count;


        public void BuildCacheVertices(GouraudVerticeBuilder grBuilder)
        {
            _cacheVxsList.Clear(); //clear prev data
            _cacheColorAndVertexList.Clear(); //clear prev data

            grBuilder.DilationValue = this.DilationValue;
            using (VxsTemp.Borrow(out var tmpVxs))
            {
                for (int i = 0; i < _vertexCount;)
                {
                    ushort v0 = _vertIndices[i];
                    ushort v1 = _vertIndices[i + 1];
                    ushort v2 = _vertIndices[i + 2];

                    grBuilder.SetColor(_colors[v0], _colors[v1], _colors[v2]);
                    grBuilder.SetTriangle(
                        _outputCoords[v0 << 1], _outputCoords[(v0 << 1) + 1],
                        _outputCoords[v1 << 1], _outputCoords[(v1 << 1) + 1],
                        _outputCoords[v2 << 1], _outputCoords[(v2 << 1) + 1]);

                    //get result from _gouraudSpanBuilder
                    grBuilder.MakeVxs(tmpVxs);

                    grBuilder.GetArrangedVertices(
                        out GouraudVerticeBuilder.CoordAndColor c0,
                        out GouraudVerticeBuilder.CoordAndColor c1,
                        out GouraudVerticeBuilder.CoordAndColor c2);

                    _cacheColorAndVertexList.Add(c0);
                    _cacheColorAndVertexList.Add(c1);
                    _cacheColorAndVertexList.Add(c2);

                    _cacheVxsList.Add(tmpVxs.CreateTrim());

                    i += 3;
                    tmpVxs.Clear(); //clear before reuse *** in next round
                }
            }
        }


        internal void SetSpanGenWithCurrentValues(int partNo, RGBAGouraudSpanGen spanGen)
        {
            CurrentVxs = _cacheVxsList[partNo];

            spanGen.SetColorAndCoords(
                _cacheColorAndVertexList[partNo * 3],
                _cacheColorAndVertexList[(partNo * 3) + 1],
                _cacheColorAndVertexList[(partNo * 3) + 2]);
        }
    }
}