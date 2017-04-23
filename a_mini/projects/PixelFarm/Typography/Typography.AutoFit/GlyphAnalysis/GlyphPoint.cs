﻿//MIT, 2016-2017, WinterDev
using System.Collections.Generic;
namespace Typography.Rendering
{

    public enum PointKind : byte
    {
        LineStart,
        LineStop,
        //
        C3Start,
        C3Control1,
        C3End,
        //
        C4Start,
        C4Control1,
        C4Control2,
        C4End,

        CurveInbetween,
    }



    public class GlyphPoint
    {
        public readonly float x;
        public readonly float y;
        public readonly PointKind kind;

        /// <summary>
        /// glyph pointnumber
        /// </summary>
        int _glyphPointNo;
        // 
        float _adjX;
        float _adjY;
        //
        public bool isPartOfHorizontalEdge;
        public bool isUpperSide;

        /// <summary>
        /// outside edge0 
        /// </summary>
        EdgeLine _e0;
        /// <summary>
        /// outside edge 1
        /// </summary>
        EdgeLine _e1;

        public float newX;
        public float newY;

#if DEBUG
        //for debug only
        public readonly int dbugId = dbugTotalId++;
        static int dbugTotalId;
        internal GlyphPart dbugOwnerPart;  //link back to owner part
        public Poly2Tri.TriangulationPoint dbugTriangulationPoint;
#endif
        public GlyphPoint(float x, float y, PointKind kind)
        {
            this.x = x;
            this.y = y;
            this.kind = kind;
        }

        internal void SetRelatedEdgeLine(EdgeLine edge)
        {
            if (_e0 == null)
            {
                _e0 = edge;
            }
            else if (_e1 == null)
            {
                _e1 = edge;
            }
            else
            {
                throw new System.NotSupportedException();
            }
            //----
#if DEBUG
            if (edge == null)
            {

            }
            if (_e0 == _e1)
            {
                throw new System.NotSupportedException();
            }
#endif
        }
        public float AdjustedY
        {
            get { return _adjY; }
            internal set
            {
                _adjY = value;
            }
        }
        public float AdjustedX
        {
            get { return _adjX; }
            internal set
            {
                _adjX = value;
            }
        }

        internal int GlyphPointNo
        {
            get { return this._glyphPointNo; }
            set { this._glyphPointNo = value; }
        }

        internal EdgeLine E0
        {
            get { return this._e0; }
        }
        internal EdgeLine E1
        {
            get { return this._e1; }
        }

        internal void ClearAdjustValues()
        {
            _adjX = _adjY = 0;
        }


        internal void AddVerticalEdge(EdgeLine v_edge)
        {
            //associated 
            if (!this.IsPartOfVerticalEdge)
            {
                this.IsPartOfVerticalEdge = true;
            }
            if (!this.IsLeftSide)
            {
                this.IsLeftSide = v_edge.IsLeftSide;
            }
        }

        internal static bool SameCoordAs(GlyphPoint a, GlyphPoint b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public bool IsLeftSide { get; private set; }
        public bool IsPartOfVerticalEdge { get; private set; }

        Dictionary<GlyphBone, bool> _assocBones;

        internal void AddAssociateBone(GlyphBone bone)
        {
            if (_assocBones == null)
            {
                _assocBones = new Dictionary<GlyphBone, bool>();
            }
            if (!_assocBones.ContainsKey(bone))
            {
                _assocBones.Add(bone, true);
            }             
        }


#if DEBUG
        public Dictionary<GlyphBone, bool> dbugGetAssocBones() { return _assocBones; }
        public override string ToString()
        {
            return this.dbugId + " :" + ((AdjustedY != 0) ? "***" : "") +
                    (x + "," + y + " " + kind.ToString());
        }
#endif 
    }


}