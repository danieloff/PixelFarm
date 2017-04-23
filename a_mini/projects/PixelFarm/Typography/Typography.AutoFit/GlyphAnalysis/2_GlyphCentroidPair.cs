﻿//MIT, 2017, WinterDev
using System;
using System.Numerics;
namespace Typography.Rendering
{
    /// <summary>
    /// a link (line) that connects between centroid of 2 GlyphTriangle(p => q)
    /// </summary>
    class GlyphCentroidPair
    {

        internal readonly GlyphTriangle p, q;
        GlyphBoneJoint _boneJoint;
        internal GlyphCentroidPair(GlyphTriangle p, GlyphTriangle q)
        {

            //each triangle has 1 centroid point
            //a centrod line connects between 2 adjacent triangles via centroid 
            //
            //
            //
            //p triangle=> (x0,y0)  (centroid of p)
            //q triangle=> (x1,y1)  (centroid of q)
            //a centroid line  move from p to q  
            this.p = p;
            this.q = q;
            //1 centroid pair has 1 GlyphBoneJoint
            //-------------------------------------- 
        }
        internal bool IsAdjacentTo(GlyphCentroidPair another)
        {
            return this.p == another.p ||
                    this.p == another.q ||
                    this.q == another.p ||
                    this.q == another.q;
        }
        public void SetCentroidLine(GlyphCentroidLine centroidLine)
        {
            p.OwnerCentroidLine = centroidLine;
            q.OwnerCentroidLine = centroidLine;
        }
        public bool SpecialConnectFromLastToFirst { get; set; }
        public GlyphBoneJoint BoneJoint { get { return _boneJoint; } }
        /// <summary>
        /// add information about edges to each triangle and create BoneJoint and Tip
        /// </summary>
        internal void AnalyzeEdgesAndCreateBoneJoint()
        {

#if DEBUG
            if (p == q)
            {
                throw new NotSupportedException();
            }
#endif
            //-------------------------------------- 
            //[C]
            //Find relation between edges of 2 triangle p and q
            //....
            //pick up a edge of p and compare to all edge of q
            //do until complete
            AddEdgesInformation(q, p.e0);
            AddEdgesInformation(q, p.e1);
            AddEdgesInformation(q, p.e2);
            //
            AddEdgesInformation(p, q.e0);
            AddEdgesInformation(p, q.e1);
            AddEdgesInformation(p, q.e2);
            //after this process, a boneJoint should be created
            //------------------------------------

#if DEBUG
            //the pair must have joint
            //1.
            if (_boneJoint == null) { throw new NotSupportedException(); }
            //2. 
            if (_boneJoint._p_contact_edge == _boneJoint._q_contact_edge)
            {
                throw new NotSupportedException();
            }
#endif

            //a joint has 2 contact edge (they are mathcing edge, but not the same).
            //we add 'information' about other edge compare to the contact edges
            //---
            //both contact edge is INSIDE edge***
            //then, we mark outside edge compare to the known inside edge          
            MarkProperOppositeOutsideEdges(p, _boneJoint._p_contact_edge, true);
            MarkProperOppositeOutsideEdges(q, _boneJoint._q_contact_edge, false);
        }
        internal void UpdateTips()
        {

            //GlyphBoneJoint joint = pair.BoneJoint;
            //get another edge for endtip

            //if (IsOwnerOf(pair.p, joint.TipEdgeP))
            //{
            //    //tip edge is from p side
            //    //so another side is q.

            //    Vector2 tipPoint = joint.TipPointP;
            //    GlyphTip tip = new GlyphTip(pair, tipPoint, joint.TipEdgeP);
            //    pair.P_Tip = tip;

            //    if (twoside)
            //    {
            //        EdgeLine tipEdge = FindTip(pair, pair.q);
            //        if (tipEdge == null) throw new NotSupportedException();
            //        //-----
            //        tip = new GlyphTip(pair, tipEdge.GetMidPoint(), tipEdge);
            //        pair.Q_Tip = tip;
            //    }
            //}
            //else if (IsOwnerOf(pair.q, joint.TipEdgeP))
            //{

            //    Vector2 tipPoint = joint.TipPointP;
            //    GlyphTip tip = new GlyphTip(pair, tipPoint, joint.TipEdgeP);
            //    pair.Q_Tip = tip;


            //    //tip edge is from q side
            //    //so another side is p.

            //    if (twoside)
            //    {
            //        //find proper tip edge
            //        EdgeLine tipEdge = FindTip(pair, pair.p);
            //        if (tipEdge == null)
            //        {
            //            //some time no tip found ***
            //            return;
            //        }
            //        //-----
            //        tip = new GlyphTip(pair, tipEdge.GetMidPoint(), tipEdge);
            //        pair.P_Tip = tip;
            //    }
            //}
            //else
            //{
            //    throw new NotSupportedException();
            //}
        }


        static void ClassifyTriangleEdges(
            GlyphTriangle triangle,
            EdgeLine knownInsideEdge,
            out EdgeLine anotherInsideEdge,
            out EdgeLine outside0,
            out EdgeLine outside1,
            out EdgeLine outside2,
            out int outsideCount)
        {
            outsideCount = 0;
            outside0 = outside1 = outside2 = anotherInsideEdge = null;

            if (triangle.e0.IsOutside)
            {
                switch (outsideCount)
                {
                    case 0: outside0 = triangle.e0; break;
                    case 1: outside1 = triangle.e0; break;
                    case 2: outside2 = triangle.e0; break;
                }
                outsideCount++;
            }
            else
            {
                //e0 is not known inside edge
                if (triangle.e0 != knownInsideEdge)
                {
                    anotherInsideEdge = triangle.e0;
                }
            }
            //
            if (triangle.e1.IsOutside)
            {
                switch (outsideCount)
                {
                    case 0: outside0 = triangle.e1; break;
                    case 1: outside1 = triangle.e1; break;
                    case 2: outside2 = triangle.e1; break;
                }
                outsideCount++;
            }
            else
            {
                if (triangle.e1 != knownInsideEdge)
                {
                    anotherInsideEdge = triangle.e1;
                }
            }
            //
            if (triangle.e2.IsOutside)
            {
                switch (outsideCount)
                {
                    case 0: outside0 = triangle.e2; break;
                    case 1: outside1 = triangle.e2; break;
                    case 2: outside2 = triangle.e2; break;
                }
                outsideCount++;
            }
            else
            {
                if (triangle.e2 != knownInsideEdge)
                {
                    anotherInsideEdge = triangle.e2;
                }
            }
        }

        static double CalculateCentroidPairSlope(GlyphCentroidPair centroidPair, out LineSlopeKind centroidLineSlope)
        {
            //calculate centroid pair slope 
            //p
            double x0 = centroidPair.p.CentroidX;
            double y0 = centroidPair.p.CentroidY;
            //q
            double x1 = centroidPair.q.CentroidX;
            double y1 = centroidPair.q.CentroidY;

            centroidLineSlope = LineSlopeKind.Other;
            double slopeAngleNoDirection = Math.Abs(Math.Atan2(Math.Abs(y1 - y0), Math.Abs(x1 - x0)));
            //we don't care direction of vector 
            if (x1 == x0)
            {
                centroidLineSlope = LineSlopeKind.Vertical;
            }
            else
            {

                if (slopeAngleNoDirection > MyMath._85degreeToRad)
                {
                    //assume
                    centroidLineSlope = LineSlopeKind.Vertical;
                }
                else if (slopeAngleNoDirection < MyMath._03degreeToRad) //_15degreeToRad
                {
                    //assume
                    centroidLineSlope = LineSlopeKind.Horizontal;
                }
                else
                {
                    centroidLineSlope = LineSlopeKind.Other;
                }
            }
            return slopeAngleNoDirection;
        }
        static void SelectMostProperTipEdge(
            GlyphCentroidPair centroidPair,
            EdgeLine outside0,
            EdgeLine outside1,
            out EdgeLine tipEdge,
            out EdgeLine notTipEdge)
        {
            LineSlopeKind slopeKind;
            //slop angle in rad
            double slopeAngle = CalculateCentroidPairSlope(centroidPair, out slopeKind);
            double diff0 = Math.Abs(outside0.GetSlopeAngleNoDirection() - slopeAngle);
            double diff1 = Math.Abs(outside1.GetSlopeAngleNoDirection() - slopeAngle);
            if (diff0 > diff1)
            {
                tipEdge = outside0;
                notTipEdge = outside1;
            }
            else
            {
                tipEdge = outside1;
                notTipEdge = outside0;
            }



            ////---------------------
            //switch (slopeKind)
            //{
            //    default: throw new NotSupportedException();
            //    case LineSlopeKind.Horizontal:
            //        if (outside0.SlopeKind == LineSlopeKind.Vertical)
            //        {
            //            tipEdge = outside0;
            //            notTipEdge = outside1;
            //        }
            //        else
            //        {
            //            tipEdge = outside1;
            //            notTipEdge = outside0;
            //        }
            //        break;
            //    case LineSlopeKind.Vertical:
            //        if (outside0.SlopeKind == LineSlopeKind.Horizontal)
            //        {
            //            tipEdge = outside0;
            //            notTipEdge = outside1;
            //        }
            //        else
            //        {
            //            tipEdge = outside1;
            //            notTipEdge = outside0;
            //        }
            //        break;
            //    case LineSlopeKind.Other:
            //        //select 1
            //        //choose the horizontal one 
            //        if (outside0.SlopeKind == LineSlopeKind.Horizontal)
            //        {
            //            tipEdge = outside0;
            //            notTipEdge = outside1;
            //        }
            //        else
            //        {
            //            tipEdge = outside1;
            //            notTipEdge = outside0;
            //        }
            //        break;
            //}
        }
        /// <summary>
        /// add information about each edge of a triangle, compare to the contactEdge of a ownerEdgeJoint
        /// </summary>
        /// <param name="triangle"></param>
        /// <param name="boneJoint"></param>
        /// <param name="contactEdge"></param>
        void MarkProperOppositeOutsideEdges(
            GlyphTriangle triangle,
            EdgeLine contactEdge,
            bool is_p_side)
        {

            int outsideCount;
            EdgeLine outside0, outside1, outside2, anotherInsideEdge;
            ClassifyTriangleEdges(
                triangle,
                contactEdge,
                out anotherInsideEdge,
                out outside0,
                out outside1,
                out outside2,
                out outsideCount);
            //-------------------------------------------------------------------------------------------------------
            //1. check each edge of triangle 
            //if an edge is outside  edge (since it is outside edge, it is not the contactEdge) 
            //count represent OUTSIDE edge count, compare to the contactEdge
            //if count ==0 ,=> no outside edge found
            //count==1, => only 1 outside edge
            //count==2, => this is tip of the centroid branch, *** 
            //a contact edge has 2 glyph points
            //a GlyphPoint from p triangle=> GlyphPoint_P
            //and
            //a GlyphPoint from q triangle=> GlyphPoint_Q

            switch (outsideCount)
            {
                default: throw new NotSupportedException();
                case 0: break;
                case 3: throw new NotImplementedException();//TODO: implement this 
                case 1:
                    {

                        ////----------------------------------------------------------------------------
                        ////primary ribs
                        ////find shortest part from boneJoint to  edge or to corner.
                        ////draw perpendicular line to outside edge
                        ////and to the  corner of current edge.
                        //GlyphPoint p_ = contactEdge.GlyphPoint_P;
                        //GlyphPoint q_ = contactEdge.GlyphPoint_Q;

                        ////TODO: review 
                        //switch (GetOnCurvePoints(p_, q_))
                        //{
                        //    default: throw new NotSupportedException();
                        //    case 2:

                        //        //both connect with ON-curve point 
                        //        //select p?

                        //        ownerEdgeJoint.AddRibEndAt(contactEdge, new Vector2(p_.x, p_.y));
                        //        break;
                        //    case 0:
                        //        //select p 

                        //        ownerEdgeJoint.AddRibEndAt(contactEdge, new Vector2(p_.x, p_.y));
                        //        break;
                        //    //break;
                        //    case 1:
                        //        //select q  
                        //        ownerEdgeJoint.AddRibEndAt(contactEdge, new Vector2(q_.x, q_.y));
                        //        break;
                        //    //break;
                        //    case -1:
                        //        //both p and q are curve in between
                        //        break;
                        //}

                    }
                    break;
                case 2:
                    {
                        //tip end 
                        //find which edge should be 'tip edge'                         
                        //in this version we compare each edge slope to centroid line slope.
                        //the most diff angle should be opposite edge (to the centroid) => tip edge
                        //-------------------------------------------------------------------------

                        EdgeLine tipEdge, notTipEdge;
                        SelectMostProperTipEdge(this,
                            outside0,
                            outside1,
                            out tipEdge,
                            out notTipEdge);
                        //for TipEdge
                        if (is_p_side)
                        {
                            _boneJoint.SetTipEdge_P(tipEdge);
                        }
                        else
                        {
                            //q side
                            _boneJoint.SetTipEdge_Q(tipEdge);
                        }
                        //for notTipEdge 
                        //Vector2 perpend_B;
                        //if (MyMath.FindPerpendicularCutPoint(notTipEdge, ownerEdgeJoint.Position, out perpend_B))
                        //{
                        //    ownerEdgeJoint.AddRibEndAt(notTipEdge, perpend_B);
                        //}
                    }
                    break;
            }
        }
        /// <summary>
        /// add information on each edge of the given triangle compare the given EdgeLine
        /// </summary>
        /// <param name="edgeLine"></param>
        /// <param name="anotherTriangle"></param>
        void AddEdgesInformation(GlyphTriangle anotherTriangle, EdgeLine edgeLine)
        {
            if (edgeLine.IsOutside)
            {
                //if edgeLine is outside edge,
                //mark the relation of this to anotherTriangle.
                MarkMatchingOutsideEdge(edgeLine, anotherTriangle);
            }
            else
            {
                //TODO: review here
                //if edge is inside =>
                //we will evaluate if _boneJoint== null

                if (_boneJoint == null)
                {
                    if (MarkMatchingInsideEdge(edgeLine, anotherTriangle))
                    {

                        _boneJoint = new GlyphBoneJoint(
                            this,
                            edgeLine,
                            edgeLine.contactToEdge);
                    }
                }
            }
        }


        /// <summary>
        ///helper method for debug,  read px,py, qx,qy, 
        /// </summary>
        /// <param name="px"></param>
        /// <param name="py"></param>
        /// <param name="qx"></param>
        /// <param name="qy"></param>
        public void GetLineCoords(out double px, out double py, out double qx, out double qy)
        {
            px = p.CentroidX;
            py = p.CentroidY;
            //
            qx = q.CentroidX;
            qy = q.CentroidY;
        }
        /// <summary>
        /// from a knownInsideEdge, find a matching-inside-edge on another triangle.
        /// </summary>
        /// <param name="knownInsideEdge"></param>
        /// <param name="another"></param>
        /// <returns></returns>
        static bool MarkMatchingInsideEdge(EdgeLine knownInsideEdge, GlyphTriangle another)
        {

            //evalute side-by-side
            //
            //if the two contact together
            //it must have only 1  contact edge.
            //so ... find side-by-side

            if (MarkMatchingInsideEdge(knownInsideEdge, another.e0) ||
                MarkMatchingInsideEdge(knownInsideEdge, another.e1) ||
                MarkMatchingInsideEdge(knownInsideEdge, another.e2))
            {
                //found!
                return true;
            }
            return false;
        }
        /// <summary>
        /// check if 
        /// </summary>
        /// <param name="knownInsideEdge"></param>
        /// <param name="anotherEdge"></param>
        /// <returns></returns>
        static bool MarkMatchingInsideEdge(EdgeLine knownInsideEdge, EdgeLine anotherEdge)
        {
            //another edge must be inside edge too, then check if the two is matching or not
            if (anotherEdge.IsInside && IsMatchingEdge(knownInsideEdge, anotherEdge))
            {   //if yes                
                //mark contact toEdge
                knownInsideEdge.contactToEdge = anotherEdge;
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// check if the 2 triangle is matching or not
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static bool IsMatchingEdge(EdgeLine a, EdgeLine b)
        {
            //x-axis
            if ((a.x0 == b.x0 && a.x1 == b.x1) ||
                (a.x0 == b.x1 && a.x1 == b.x0))
            {
                //pass x-axis
                //
                //y_axis
                if ((a.y0 == b.y0 && a.y1 == b.y1) ||
                    (a.y0 == b.y1 && a.y1 == b.y0))
                {
                    return true;
                }
            }
            //otherwise...
            return false;
        }
        /// <summary>
        /// analyze relation between each edge of q and knownOutsideEdge.
        /// </summary>
        /// <param name="knownOutsideEdge"></param>
        /// <param name="q"></param>
        static void MarkMatchingOutsideEdge(EdgeLine knownOutsideEdge, GlyphTriangle q)
        {

            EdgeLine matchingEdgeLine;
            int matchingEdgeSideNo;
            if (FindMatchingOuterSide(knownOutsideEdge, q, out matchingEdgeLine, out matchingEdgeSideNo))
            {
                //assign matching edge line   
                //mid point of each edge
                //p-triangle's edge midX,midY

                var pe = knownOutsideEdge.GetMidPoint();
                double pe_midX = pe.X, pe_midY = pe.Y;

                //q-triangle's edge midX,midY
                var qe = matchingEdgeLine.GetMidPoint();
                double qe_midX = qe.X, qe_midY = qe.Y;


                if (knownOutsideEdge.SlopeKind == LineSlopeKind.Vertical)
                {
                    //TODO: review same side edge (Fan shape)
                    if (pe_midX < qe_midX)
                    {
                        knownOutsideEdge.IsLeftSide = true;
                        if (matchingEdgeLine.IsOutside && matchingEdgeLine.SlopeKind == LineSlopeKind.Vertical)
                        {
                            // knownOutsideEdge.AddMatchingOutsideEdge(matchingEdgeLine);
                        }
                    }
                    else
                    {
                        //matchingEdgeLine.IsLeftSide = true;
                        if (matchingEdgeLine.IsOutside && matchingEdgeLine.SlopeKind == LineSlopeKind.Vertical)
                        {
                            // knownOutsideEdge.AddMatchingOutsideEdge(matchingEdgeLine);
                        }
                    }
                }
                else if (knownOutsideEdge.SlopeKind == LineSlopeKind.Horizontal)
                {
                    //TODO: review same side edge (Fan shape)

                    if (pe_midY > qe_midY)
                    {
                        //p side is upper , q side is lower
                        if (knownOutsideEdge.SlopeKind == LineSlopeKind.Horizontal)
                        {
                            knownOutsideEdge.IsUpper = true;
                            if (matchingEdgeLine.IsOutside && matchingEdgeLine.SlopeKind == LineSlopeKind.Horizontal)
                            {
                                // knownOutsideEdge.AddMatchingOutsideEdge(matchingEdgeLine);
                            }
                        }
                    }
                    else
                    {
                        if (matchingEdgeLine.SlopeKind == LineSlopeKind.Horizontal)
                        {
                            // matchingEdgeLine.IsUpper = true;
                            if (matchingEdgeLine.IsOutside && matchingEdgeLine.SlopeKind == LineSlopeKind.Horizontal)
                            {
                                //  knownOutsideEdge.AddMatchingOutsideEdge(matchingEdgeLine);
                            }
                        }
                    }
                }
            }
        }
        static bool FindMatchingOuterSide(EdgeLine compareEdge,
            GlyphTriangle another,
            out EdgeLine result,
            out int edgeIndex)
        {
            //compare by radian of edge line
            double compareSlope = Math.Abs(compareEdge.SlopeAngleNoDirection);
            double diff0 = double.MaxValue;
            double diff1 = double.MaxValue;
            double diff2 = double.MaxValue;

            diff0 = Math.Abs(Math.Abs(another.e0.SlopeAngleNoDirection) - compareSlope);

            diff1 = Math.Abs(Math.Abs(another.e1.SlopeAngleNoDirection) - compareSlope);

            diff2 = Math.Abs(Math.Abs(another.e2.SlopeAngleNoDirection) - compareSlope);

            //find min
            int minDiffSide = FindMinByIndex(diff0, diff1, diff2);
            if (minDiffSide > -1)
            {
                edgeIndex = minDiffSide;
                switch (minDiffSide)
                {
                    default: throw new NotSupportedException();
                    case 0:
                        result = another.e0;
                        break;
                    case 1:
                        result = another.e1;
                        break;
                    case 2:
                        result = another.e2;
                        break;
                }
                return true;
            }
            else
            {
                edgeIndex = -1;
                result = null;
                return false;
            }
        }
        /// <summary>
        /// compare d0, d1, d2 return min value by index 0 or 1 or 2
        /// </summary>
        /// <param name="d0"></param>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        static int FindMinByIndex(double d0, double d1, double d2)
        {
            unsafe
            {
                double* tmpArr = stackalloc double[3];
                tmpArr[0] = d0;
                tmpArr[1] = d1;
                tmpArr[2] = d2;

                int minAt = -1;
                double currentMin = double.MaxValue;
                for (int i = 0; i < 3; ++i)
                {
                    double d = tmpArr[i];
                    if (d < currentMin)
                    {
                        currentMin = d;
                        minAt = i;
                    }
                }
                return minAt;
            }
        }

        public override string ToString()
        {
            return p + " -> " + q;
        }
    }
}