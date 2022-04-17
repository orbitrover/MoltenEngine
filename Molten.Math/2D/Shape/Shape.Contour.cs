﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public partial class ContourShape
    {
        public class Contour
        {
            public List<Edge> Edges { get; } = new List<Edge>();

            public void AddEdge(Edge edge)
            {
                Edges.Add(edge);
            }

            public void AppendLinearPoint(Vector2D p)
            {
                if (Edges.Count == 0)
                    throw new Exception("Cannot append edge point without at least 1 existing edge.");

                Edge last = Edges.Last();
                Edges.Add(new LinearEdge(last.Points[Edge.P1], p));
            }

            public void AppendQuadraticPoint(Vector2D p, Vector2D pControl)
            {
                if (Edges.Count == 0)
                    throw new Exception("Cannot append edge point without at least 1 existing edge.");

                Edge last = Edges.Last();
                Edges.Add(new QuadraticEdge(last.Points[Edge.P1], p, pControl));
            }

            public void AppendCubicPoint(Vector2D p, Vector2D pControl1, Vector2D pControl2)
            {
                if (Edges.Count == 0)
                    throw new Exception("Cannot append edge point without at least 1 existing edge.");

                Edge last = Edges.Last();
                Edges.Add(new CubicEdge(last.Points[Edge.P1], p, pControl1, pControl2));
            }

            private double Shoelace(Vector2D a, Vector2D b)
            {
                return (b.X - a.X) * (a.Y + b.Y);
            }

            public int GetWinding()
            {
                if (Edges.Count == 0)
                    return 0;

                double total = 0;
                if (Edges.Count == 1)
                {
                    Vector2D a = Edges[0].Point(0), b = Edges[0].Point(1 / 3.0), c = Edges[0].Point(2 / 3.0);
                    total += Shoelace(a, b);
                    total += Shoelace(b, c);
                    total += Shoelace(c, a);
                }
                else if (Edges.Count == 2)
                {
                    Vector2D a = Edges[0].Point(0), b = Edges[0].Point(.5), c = Edges[1].Point(0), d = Edges[1].Point(.5);
                    total += Shoelace(a, b);
                    total += Shoelace(b, c);
                    total += Shoelace(c, d);
                    total += Shoelace(d, a);
                }
                else
                {
                    Vector2D prev = Edges.Last().Point(0);
                    foreach (Edge edge in Edges)
                    {
                        Vector2D cur = edge.Point(0);
                        total += Shoelace(prev, cur);
                        prev = cur;
                    }
                }
                return MathHelperDP.Sign(total);
            }

            public bool Contains(Vector2D point, int edgeResolution = 3)
            {
                // Thanks to: https://codereview.stackexchange.com/a/108903
                int polygonLength = Edges.Count;
                int j = 0;
                bool inside = false;
                double pointX = point.X;
                double pointY = point.Y;

                // start / end point for the current polygon segment.
                double startX, startY, endX, endY;
                Vector2D endPoint = Edges[polygonLength - 1].Points[Edge.P0];
                endX = endPoint.X;
                endY = endPoint.Y;


                while (j < polygonLength)
                {
                    Edge edge = Edges[j++];

                    // Are we using a curve edge?
                    if (edge is not LinearEdge)
                    {
                        // Get points along the edge, with respect to edge resolution.
                        double distInc = 1.0 / edgeResolution;
                        for (int i = 0; i < edgeResolution; i++)
                        {
                            double dist = (distInc * i);

                            startX = endX; startY = endY;
                            endPoint = edge.PointAlongEdge(dist);
                            endX = endPoint.X; endY = endPoint.Y;

                            inside ^= (endY > pointY ^ startY > pointY) /* ? pointY inside [startY;endY] segment ? */
                                      && /* if so, test if it is under the segment */
                                      ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
                        }
                    }
                    else
                    {
                        startX = endX; startY = endY;
                        endPoint = edge.Points[Edge.P1];
                        endX = endPoint.X; endY = endPoint.Y;
                        //
                        inside ^= (endY > pointY ^ startY > pointY) /* ? pointY inside [startY;endY] segment ? */
                                  && /* if so, test if it is under the segment */
                                  ((pointX - endX) < (pointY - endY) * (startX - endX) / (startY - endY));
                    }
                }

                return inside;
            }

            /// <summary>
            /// Produces a <see cref="RectangleF"/> which contains all of the shape's points.
            /// </summary>
            public RectangleF CalculateBounds()
            {
                throw new NotImplementedException();

                /*RectangleF b = new RectangleF()
                {
                    Left = float.MaxValue,
                    Top = float.MaxValue,
                    Right = float.MinValue,
                    Bottom = float.MinValue,
                };

                foreach (TriPoint p in Points)
                {
                    if (p.X < b.Left)
                        b.Left = p.X;
                    else if (p.X > b.Right)
                        b.Right = p.Y;

                    if (p.Y < b.Top)
                        b.Top = p.Y;
                    else if (p.Y > b.Bottom)
                        b.Bottom = p.Y;
                }

                return b;*/
            }

            public List<TriPoint> GetEdgePoints(int edgeResolution = 3)
            {
                if (edgeResolution < 3)
                    throw new Exception("Edge resolution must be at least 3");

                List<TriPoint> points = new List<TriPoint>();
                foreach (Edge edge in Edges)
                {
                    // Are we using a curve edge?
                    if (edge is not LinearEdge)
                    {
                        double distInc = 1.0 / edgeResolution;
                        for (int i = points.Count > 0 ? 1 : 0; i < edgeResolution; i++)
                        {
                            double dist = (distInc * i);
                            Vector2F ep = (Vector2F)edge.PointAlongEdge(dist);
                            points.Add(new TriPoint(ep));
                        }
                    }
                    else
                    {
                        if (points.Count == 0)
                            points.Add(new TriPoint((Vector2F)edge.Points[Edge.P0]));

                        points.Add(new TriPoint((Vector2F)edge.Points[Edge.P1]));
                    }
                }

                return points;
            }
        }
    }
}