using System;
using System.Collections.Generic;
using UnityEngine;

public class Voronoi<NodeType, Coordinate> where NodeType : INode<Coordinate>
    where Coordinate : IEquatable<Vector2>
{
    private List<Node<Vector2>> voronoiCenters;
    public Dictionary<Node<Vector2>, List<Segments<Vector2>>> voronoiPolygons;
    private int gridWidth, gridHeight;
    // private IGraph<NodeType, Coordinate> graph;

    private Coordinate min;
    private Coordinate max;
    private float padding = 0.5f;
    public static readonly Vector2 INVALID_VALUE = new(Single.MaxValue, Single.MinValue);

    public Voronoi(List<Node<Vector2>> centers, int width, int height, IGraph<NodeType, Coordinate> graph)
    {
        voronoiCenters = centers;
        voronoiPolygons = new Dictionary<Node<Vector2>, List<Segments<Vector2>>>();
        gridWidth = width;
        gridHeight = height;
        // this.graph = graph;
    }

    //Node scale * quatity + (sepation* cuantity -1)
    public void GenerateVoronoi()
    {
        foreach (Node<Vector2> center in voronoiCenters)
        {
            List<Segments<Vector2>> voronoiPoints = new List<Segments<Vector2>>();

            Point<Vector2> upLeft = new(new Vector2(-padding, gridHeight + padding));
            Point<Vector2> upRight = new(new Vector2(gridWidth + padding, gridHeight + padding));
            Point<Vector2> downRight = new(new Vector2(gridWidth + padding, -padding));
            Point<Vector2> downLeft = new(new Vector2(-padding, -padding));

            Segments<Vector2> upSegments = new(upLeft, upRight);
            Segments<Vector2> rightSegments = new(upRight, downRight);
            Segments<Vector2> downSegments = new(downRight, downLeft);
            Segments<Vector2> leftSegments = new(downLeft, upLeft);

            upLeft.AddSegments(upSegments, leftSegments);
            upRight.AddSegments(rightSegments, upSegments);
            downRight.AddSegments(downSegments, rightSegments);
            downLeft.AddSegments(leftSegments, downSegments);

            voronoiPoints.Add(upSegments);
            voronoiPoints.Add(rightSegments);
            voronoiPoints.Add(downSegments);
            voronoiPoints.Add(leftSegments);

            voronoiPolygons.Add(center, voronoiPoints);
            voronoiPolygons[center] = voronoiPoints;
        }

        for (int index = 0; index < voronoiCenters.Count; index++)
        {
            for (int j = 0; j < voronoiCenters.Count; j++)
            {
                if (index == j)
                {
                    continue;
                }

                //For the weighted i need to move the point according to the wight of the polygons 
                Vector2 director = GetDirectorVector(index, j, out Vector2 midPoint);

                CalculatePolygon(voronoiCenters[index], midPoint, director);
            }
        }
    }

    private Vector2 GetDirectorVector(int index, int j, out Vector2 center)
    {
        Vector2 A = voronoiCenters[index].GetCoordinate();
        Vector2 B = voronoiCenters[j].GetCoordinate();

        Vector2 direction = B - A;
        Vector2 director = new Vector2(-direction.y, direction.x).normalized;

        float distance = Vector2.Distance(A, B);
        distance = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(A.x - B.x), 2) +
                              Mathf.Pow(Mathf.Abs(A.y - B.y), 2));
        center = (A + B) * 0.5f;

        director *= (distance * 0.5f);
        return director;
    }

    public bool LineVectorCollision(Segments<Vector2> seg, Vector2 dir, out Vector2 intersection)
    {
        // Vector2 startPoint = new Vector2(-1, -1);
        Vector2 segDir = seg.end.coord - seg.init.coord;


        float denom = dir.y * segDir.x - dir.x * segDir.y;

        if (Mathf.Abs(denom) < Mathf.Epsilon)
        {
            intersection = INVALID_VALUE;
            return false;
        }


        float uA = ((dir.x * (seg.init.coord.y)) - (dir.y * (seg.init.coord.x))) / denom;
        float uB = ((segDir.x * (seg.init.coord.y)) - (segDir.y * (seg.init.coord.x))) / denom;


        if (uA >= 0 && uA <= 1)
        {
            intersection = new Vector2(
                seg.init.coord.x + uA * segDir.x,
                seg.init.coord.y + uA * segDir.y);
            intersection = seg.init.coord + dir.normalized * uA;
            return true;
        }


        intersection = INVALID_VALUE;
        return false;
    }

    public bool LineVectorCollision(Vector2 rayOrigin, Vector2 rayDir, Segments<Vector2> seg, out Vector2 intersection)
    {
        Vector2 segDir = seg.end.coord - seg.init.coord;

        float denom = rayDir.x * segDir.y - rayDir.y * segDir.x;

        // If the determinant is near zero, the lines are parallel and don't intersect
        if (Mathf.Abs(denom) < Mathf.Epsilon)
        {
            intersection = INVALID_VALUE;
            return false;
        }

        // Parametric solution to find the intersection point
        float t = ((seg.init.coord.x - rayOrigin.x) * segDir.y - (seg.init.coord.y - rayOrigin.y) * segDir.x) / denom;
        float u = ((seg.init.coord.x - rayOrigin.x) * rayDir.y - (seg.init.coord.y - rayOrigin.y) * rayDir.x) / denom;

        // Check if the intersection is within the bounds of the segment (u between 0 and 1) and if t >= 0 (intersection in front of the ray)
        if (u >= 0 && u <= 1 && t >= 0)
        {
            // Calculate the intersection point
            intersection = seg.init.coord + u * segDir;
            return true;
        }

        intersection = INVALID_VALUE;
        return false;
    }

    void CalculatePolygon(Node<Vector2> center, Vector2 origin, Vector2 dir)
    {
        List<Vector2> interceptionPoints = new List<Vector2>();
        List<Point<Vector2>> pointsA = new List<Point<Vector2>>();
        List<Point<Vector2>> pointsB = new List<Point<Vector2>>();
        List<Segments<Vector2>> polygonA = new(voronoiPolygons[center]);
        List<Segments<Vector2>> polygonB = new(voronoiPolygons[center]);

        var t = 10000;
        for (int i = 0; i < voronoiPolygons[center].Count; i++)
        {
            Segments<Vector2> segments = voronoiPolygons[center][i];
           


            Vector2 inter = INVALID_VALUE;

            if (LineVectorCollision(origin, dir * t, segments, out Vector2 inter1) && inter1 != INVALID_VALUE)
            {
                inter = inter1;
            }
            else if (LineVectorCollision(origin, dir * -t, segments, out Vector2 inter2) && inter2 != INVALID_VALUE)
            {
                inter = inter2;
            }

            if (inter != INVALID_VALUE)
            {
                Segments<Vector2> aInter = new Segments<Vector2>(segments.init, new Point<Vector2>(inter));
                segments.init.Segments[0] = aInter;
                aInter.end.Segments.Add(aInter);
                Segments<Vector2> bInter = new Segments<Vector2>(new Point<Vector2>(inter), segments.end);
                segments.end.Segments[0] = bInter;
                bInter.init.Segments.Add(bInter);
                polygonA[i] = aInter;
                polygonB[i] = bInter;
                interceptionPoints.Add(inter);
                pointsA.Add(aInter.end);
                pointsB.Add(bInter.init);
            }
        }

        if (interceptionPoints.Count == 2)
        {
            Segments<Vector2> aInter = new Segments<Vector2>(pointsA[0], pointsA[1]);
            pointsA[0].Segments.Add(aInter);
            pointsA[1].Segments.Add(aInter);
            Segments<Vector2> bInter = new Segments<Vector2>(pointsB[0], pointsB[1]);
            pointsB[0].Segments.Add(aInter);
            pointsB[1].Segments.Add(aInter);
            polygonA.Add(aInter);
            polygonB.Add(bInter);
            if (IsPointInPolygon(center.GetCoordinate(), polygonA))
            {
                voronoiPolygons[center] = polygonA;
            }
            else if (IsPointInPolygon(center.GetCoordinate(), polygonB))
            {
                voronoiPolygons[center] = polygonB;
            }
        }
    }


    public bool IsPointInPolygon(Vector2 point, List<Segments<Vector2>> polygonSegments)
    {
        int intersectionCount = 0;
        Vector2 dir = new Vector2(1, 0);

        foreach (var segment in polygonSegments)
        {
            Vector2 intersection;
            if (LineVectorCollision(point, dir, segment, out intersection) && intersection != INVALID_VALUE)
            {
                if (IsPointOnSegment(segment, intersection) && intersection.x > point.x)
                {
                    intersectionCount++;
                }
            }
        }

        return (intersectionCount % 2 == 1);
    }

    private bool IsPointOnSegment(Segments<Vector2> segments, Vector2 point)
    {
        return Mathf.Min(segments.init.coord.x, segments.end.coord.x) <= point.x
               && point.x <= Mathf.Max(segments.init.coord.x, segments.end.coord.x)
               && Mathf.Min(segments.init.coord.y, segments.end.coord.y) <= point.y
               && point.y <= Mathf.Max(segments.init.coord.y, segments.end.coord.y);
    }
}

public class Cell<NodeType, Coordinate> where NodeType : INode<Coordinate> where Coordinate : IEquatable<Coordinate>
{
    public NodeType site;
    public List<Point<Coordinate>> vertices;

    public Cell(NodeType site)
    {
        this.site = site;
    }
}

public class Segments<Coordinate>
{
    public Point<Coordinate> init;
    public Point<Coordinate> end;

    public Segments(Coordinate init, Coordinate end)
    {
        this.init = new Point<Coordinate>(init);
        this.end = new Point<Coordinate>(end);
    }

    public Segments(Point<Coordinate> init, Point<Coordinate> end)
    {
        this.init = init;
        this.end = end;
    }
}

public class Point<Coordinate>
{
    public Coordinate coord;
    public List<Segments<Coordinate>> Segments = new List<Segments<Coordinate>>();

    public Point(Coordinate coord)
    {
        this.coord = coord;
    }

    public void AddSegments(Segments<Coordinate> segment1, Segments<Coordinate> segment2)
    {
        Segments.Add(segment1);
        Segments.Add(segment2);
    }
}