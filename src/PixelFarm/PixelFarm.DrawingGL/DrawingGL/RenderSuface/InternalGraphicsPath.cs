﻿//MIT, 2016-present, WinterDev

using System.Collections.Generic;
using PixelFarm.CpuBlit.VertexProcessing;

namespace PixelFarm.DrawingGL
{

    class VBOStream : System.IDisposable
    {
        VertexBufferObject _vbo;
        List<float> _mergedInputXYs = new List<float>();

        public VBOSegment CreateSegment(float[] input, int vertexCount, int vertexSize)
        {
            int actualLen = _mergedInputXYs.Count;
            int mod = actualLen % vertexSize;
            if (mod > 0)
            {
                //padding to specific offset
                //** we need padding here
                //eg. previous shape uses 2 floats per vertex
                //and this shape uses 4 floats per vertext
                //we must calculate a proper start index (array offset)
                //for this object.
                //****

                for (int i = mod; i > 0; --i)
                {
                    _mergedInputXYs.Add(0);
                }
                actualLen += mod; //change actual len after add pad data
            }

            _mergedInputXYs.AddRange(input);
            return new VBOSegment() { startAt = (actualLen / vertexSize), vertexCount = vertexCount };
        }
        public void BuildBuffer(bool clearInputXYs = true)
        {
            if (_vbo != null)
            {
                //must clear this first
                throw new System.Exception();
            }
            //----------------
            _vbo = new VertexBufferObject();
            _vbo.CreateBuffers(_mergedInputXYs.ToArray(), null);
            if (clearInputXYs)
            {
                _mergedInputXYs.Clear();
                _mergedInputXYs = null;
            }
            //clear _mergedInputXYs 
        }
        public void Bind()
        {
            _vbo.Bind();
        }
        public void Unbind()
        {
            _vbo.UnBind();
        }
        public void Dispose()
        {
            if (_vbo != null)
            {
                _vbo.Dispose();
                _vbo = null;
            }
        }
    }

    class VBOSegment
    {
        public int startAt;
        public int vertexCount;
    }

    public class TextureRenderVx : PixelFarm.Drawing.RenderVx
    {
        //msdf texture-based render vx
        GLBitmap _glBmp;
        ExtMsdfGen.SpriteTextureMapData<PixelFarm.CpuBlit.MemBitmap> _spriteMapTextureData;
        internal TextureRenderVx(ExtMsdfGen.SpriteTextureMapData<PixelFarm.CpuBlit.MemBitmap> spriteTextureMapData)
        {
            _spriteMapTextureData = spriteTextureMapData;
        }
        internal ExtMsdfGen.SpriteTextureMapData<PixelFarm.CpuBlit.MemBitmap> SpriteMap
        {
            get => _spriteMapTextureData;
            set => _spriteMapTextureData = value;
        }
        internal GLBitmap GetBmp()
        {
            if (_glBmp == null)
            {
                if (_spriteMapTextureData != null)
                {
                    _glBmp = new GLBitmap(_spriteMapTextureData.Source);
                    return _glBmp;
                }
            }
            return _glBmp;
        }
    }

    /// <summary>
    /// a wrapper of internal private class
    /// </summary>
    public class PathRenderVx : PixelFarm.Drawing.RenderVx
    {
        //since Figure is private=> we use this class to expose to public 
        readonly Figure _figure;
        readonly MultiFigures _figures;

        internal VBOSegment _tessAreaVboSeg;
        internal VBOSegment _smoothBorderVboSeg;
        internal VBOStream _tessVBOStream;
        internal bool _isTessVBOStreamOwner;
        internal bool _enableVBO;
#if DEBUG
        static int s_dbugTotalId;
        public readonly int dbugId = s_dbugTotalId++;
#endif

        internal PathRenderVx(MultiFigures figures)
        {
            _figure = null;
            _figures = figures;
            _enableVBO = true;
        }
        internal PathRenderVx(Figure fig)
        {
            _figures = null;
            _figure = fig;
            _enableVBO = true;
        }

        internal static PathRenderVx Create(FigureContainer figContainer)
        {
            return (figContainer.IsSingleFigure) ?
                new PathRenderVx(figContainer._figure) :
                new PathRenderVx(figContainer._multiFig);
        }
        public override void Dispose()
        {
            if (_isTessVBOStreamOwner && _tessVBOStream != null)
            {
                _tessVBOStream.Dispose();
                _tessVBOStream = null;
            }
            base.Dispose();
        }
        internal void CreateAreaTessVBOSegment(VBOStream ownerVBOStream,
            TessTool tess,
            Tesselate.Tesselator.WindingRuleType windingRuleType)
        {
            //
            float[] tessArea = GetAreaTess(tess, windingRuleType);
            _tessAreaVboSeg = ownerVBOStream.CreateSegment(tessArea, TessAreaVertexCount, 2);
            //
        }

        internal void CreateSmoothBorderTessSegment(VBOStream ownerVBOStream,
         SmoothBorderBuilder smoothBorderBuilder)
        {
            //
            float[] smoothBorderTess = GetSmoothBorders(smoothBorderBuilder);
            _smoothBorderVboSeg = ownerVBOStream.CreateSegment(smoothBorderTess, BorderTriangleStripCount, 4);
            //
        }
        internal int FigCount => (_figure != null) ? 1 : _figures.FigureCount;

        internal Figure GetFig(int index)
        {
            if (index == 0)
            {
                return _figure ?? _figures[0];
            }
            else
            {
                return _figures[index];
            }
        }
        internal float[] GetAreaTess(TessTool tess, Tesselate.Tesselator.WindingRuleType windingRuleType)
        {
            return (_figure != null) ?
                        _figure.GetAreaTess(tess, windingRuleType, TessTriangleTechnique.DrawArray) :
                        _figures.GetAreaTess(tess, windingRuleType, TessTriangleTechnique.DrawArray);
        }

        //
        public int TessAreaVertexCount => (_figure != null) ?
                                           _figure.TessAreaVertexCount :
                                           _figures.TessAreaVertexCount;
        //
        //----------------------------------------------------
        //
        internal float[] GetSmoothBorders(SmoothBorderBuilder smoothBorderBuilder)
        {
            return (_figure != null) ?
                    _figure.GetSmoothBorders(smoothBorderBuilder) :
                    _figures.GetSmoothBorders(smoothBorderBuilder);
        }
        //
        //
        internal int BorderTriangleStripCount => (_figure != null) ?
                                                  _figure.BorderTriangleStripCount :
                                                  _figures.BorderTriangleStripCount;


    }

    public class GLRenderVxFormattedString : PixelFarm.Drawing.RenderVxFormattedString
    {
        DrawingGL.VertexBufferObject _vbo;
        internal GLRenderVxFormattedString()
        {
        }

        public float[] VertexCoords { get; set; }
        public ushort[] IndexArray { get; set; }
        public int IndexArrayCount { get; set; }
        public float Width { get; set; }
        public float SpanHeight { get; set; }

        public ushort WordPlateId { get; set; }
        public ushort WordPlateLeft { get; set; }
        public ushort WordPlateTop { get; set; }
        public bool UseWithWordPlate { get; set; }
        internal bool PreparingWordTicket { get; set; }

        public void ClearWordPlateId()
        {
            WordPlateId = 0;
        }
        public DrawingGL.VertexBufferObject GetVbo()
        {
            if (_vbo != null)
            {
                return _vbo;
            }
            _vbo = new VertexBufferObject();
            _vbo.CreateBuffers(this.VertexCoords, this.IndexArray);
            return _vbo;
        }
        public void DisposeVbo()
        {
            //dispose only VBO
            //and we can create the vbo again
            //from VertexCoord and IndexArray 

            if (_vbo != null)
            {
                _vbo.Dispose();
                _vbo = null;
            }
        }
        public override void Dispose()
        {
            DisposeVbo();
            base.Dispose();
        }

#if DEBUG
        public string dbugText;
        public override string ToString()
        {
            if (dbugText != null)
            {
                return dbugText;
            }
            return base.ToString();
        }
#endif

    }
}