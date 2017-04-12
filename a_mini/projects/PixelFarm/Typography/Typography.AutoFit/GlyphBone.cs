﻿//MIT, 2017, WinterDev
using System;
using System.Numerics;

namespace Typography.Rendering
{

    /// <summary>
    /// link between 2 GlyphBoneJoint or Joint and tipEdge
    /// </summary>
    public class GlyphBone
    {
        public readonly EdgeLine TipEdge;
        public readonly GlyphBoneJoint JointA;
        public readonly GlyphBoneJoint JointB;

        double _len;
        public GlyphBone(GlyphBoneJoint a, GlyphBoneJoint b)
        {
#if DEBUG
            if (a == b)
            {
                throw new NotSupportedException();
            }
#endif
            JointA = a;
            JointB = b;

            var bpos = b.Position;
            _len = Math.Sqrt(a.CalculateSqrDistance(bpos));
            EvaluteSlope(a.Position, bpos);
            //------  
        }

        public GlyphBone(GlyphBoneJoint a, EdgeLine tipEdge)
        {
            JointA = a;
            TipEdge = tipEdge;

            var midPoint = tipEdge.GetMidPoint();
            _len = Math.Sqrt(a.CalculateSqrDistance(midPoint));
            EvaluteSlope(a.Position, midPoint);
            //------


        }
        void EvaluteSlope(Vector2 p, Vector2 q)
        {

            double x0 = p.X;
            double y0 = p.Y;
            //q
            double x1 = q.X;
            double y1 = q.Y;

            if (x1 == x0)
            {
                this.SlopeKind = LineSlopeKind.Vertical;
                SlopeAngle = 1;
            }
            else
            {
                SlopeAngle = Math.Abs(Math.Atan2(Math.Abs(y1 - y0), Math.Abs(x1 - x0)));
                if (SlopeAngle > MyMath._85degreeToRad)
                {
                    SlopeKind = LineSlopeKind.Vertical;
                }
                else if (SlopeAngle < MyMath._03degreeToRad) //_15degreeToRad
                {
                    SlopeKind = LineSlopeKind.Horizontal;
                }
                else
                {
                    SlopeKind = LineSlopeKind.Other;
                }
            }
        }
        internal double SlopeAngle { get; set; }
        public LineSlopeKind SlopeKind { get; set; }
        internal double Length
        {
            get
            {
                return _len;
            }
        }
        public bool IsLongBone { get; internal set; }

        internal double CalculateAvgBoneWidth()
        {
            //avg bone width
            //(for this bone only) is calculated by avg of 4 ribs 
            //around 2 joints

            //this only ...
            double a_side = Math.Sqrt(JointA.CalculateSqrDistance(JointA.RibEndPointA));
            double b_side = Math.Sqrt(JointA.CalculateSqrDistance(JointA.RibEndPointB));
            return (a_side + b_side) / 2;
        }
        //--------
        public float LeftMostPoint()
        {
            if (JointB != null)
            {
                //compare joint A and B 
                if (JointA.Position.X < JointB.Position.X)
                {
                    return JointA.GetLeftMostRib();
                }
                else
                {
                    return JointB.GetLeftMostRib();
                }
            }
            else
            {
                return JointA.GetLeftMostRib();
            }
        }

#if DEBUG
        public override string ToString()
        {
            if (TipEdge != null)
            {
                return JointA.ToString() + "->" + TipEdge.GetMidPoint().ToString();
            }
            else
            {
                return JointA.ToString() + "->" + JointB.ToString();
            }
        }
#endif
    }

    public class GlyphBoneJoint
    {
        //Bone joint connects (contact) 'inside' EdgeLines
        //(_p_contact_edge, _q_contact_edge)

        public EdgeLine _p_contact_edge;
        public EdgeLine _q_contact_edge;
        GlyphCentroidLine _owner;
        public GlyphBoneJoint(GlyphCentroidLine owner,
            EdgeLine p_contact_edge,
            EdgeLine q_contact_edge)
        {
            this._p_contact_edge = p_contact_edge;
            this._q_contact_edge = q_contact_edge;
            this._owner = owner;
        }

        /// <summary>
        /// get position of this bone joint (mid point of the edge)
        /// </summary>
        /// <returns></returns>
        public Vector2 Position
        {
            get
            {
                //mid point of the edge line
                return _p_contact_edge.GetMidPoint();
            }
        }
        public GlyphCentroidLine OwnerCentroidLine
        {
            get { return _owner; }
        }
        public float GetLeftMostRib()
        {
            float a_x = this.RibEndPointA.X;
            if (this._selectedEdgeB != null)
            {
                float b_x = this.RibEndPointB.X;
                if (a_x < b_x)
                {
                    return a_x;
                }
                else
                {
                    return b_x;
                }
            }
            else
            {
                return a_x;
            }
        }
        /// <summary>
        /// calculate distance^2 from contact point to specific point v
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public double CalculateSqrDistance(Vector2 v)
        {

            Vector2 contactPoint = this.Position;
            float xdiff = contactPoint.X - v.X;
            float ydiff = contactPoint.Y - v.Y;

            return (xdiff * xdiff) + (ydiff * ydiff);
        }


        short _ribCount;

        Vector2 _ribEndPoint_A, _ribEndPoint_B;
        /// <summary>
        /// tip point (mid of tip edge)
        /// </summary>
        Vector2 _tipPoint;

        EdgeLine _selectedEdgeA, _selectedEdgeB, _selectedTipEdge;

       
        bool _dbugSwap;

        public void AddRibEndAt(EdgeLine edgeLine, Vector2 vec)
        {
            switch (_ribCount)
            {
                //not more than 2
                default: throw new NotSupportedException();
                case 0:
                    _selectedEdgeA = edgeLine;
                    _ribEndPoint_A = vec;
                    break;
                case 1:
                    _selectedEdgeB = edgeLine;
                    _ribEndPoint_B = vec;

                    //swap edge if need
                    //if (_ribEndPoint_A.X > _ribEndPoint_B.X)
                    //{
                    //    EdgeLine tmpA = _selectedEdgeA;
                    //    _selectedEdgeA = _selectedEdgeB;
                    //    _selectedEdgeB = tmpA;
                    //    Vector2 tmpAEndPoint = _ribEndPoint_A;
                    //    _ribEndPoint_A = _ribEndPoint_B;
                    //    _ribEndPoint_B = tmpAEndPoint;
                    //    _dbugSwap = true;
                    //}
                    break;
            }


            _ribCount++;
        }
        public void SetTipEdge(EdgeLine tipEdge)
        {
            this._selectedTipEdge = tipEdge;
            this._tipPoint = tipEdge.GetMidPoint();
        }

        public short SelectedEdgePointCount { get { return _ribCount; } }
        public Vector2 RibEndPointA { get { return _ribEndPoint_A; } }
        public Vector2 RibEndPointB { get { return _ribEndPoint_B; } }
        public Vector2 TipPoint { get { return _tipPoint; } }
        public EdgeLine RibEndEdgeA { get { return _selectedEdgeA; } }
        public EdgeLine RibEndEdgeB { get { return _selectedEdgeB; } }
        public EdgeLine TipEdge { get { return _selectedTipEdge; } }
        //public double RibA_ArcTan()
        //{
        //    Vector2 jointPos = this.Position;
        //    return Math.Atan2(_ribEndPoint_A.Y - jointPos.Y,
        //        _ribEndPoint_A.X - jointPos.X);
        //}
        //public double RibB_ArcTan()
        //{
        //    Vector2 jointPos = this.Position;
        //    return Math.Atan2(_ribEndPoint_B.Y - jointPos.Y,
        //        _ribEndPoint_B.X - jointPos.X);
        //}
        //public double Tip_ArcTan()
        //{
        //    Vector2 jointPos = this.Position;
        //    return Math.Atan2(_tipPoint.Y - jointPos.Y,
        //        _tipPoint.X - jointPos.X);
        //}


#if DEBUG
        public override string ToString()
        {
            return this.Position.ToString();
        }
#endif

    }



}