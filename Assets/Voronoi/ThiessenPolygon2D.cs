using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class ThiessenPolygon2D<SegmentType, Coord> : PoligonsVoronoi<SegmentVec2, Vector2>
    where SegmentType : Segment<Vector2>, new() where Coord : IEquatable<Vector2>
{
    public ThiessenPolygon2D(Vector2 item, List<Vector2> allIntersections, float mediatrix = defaultMediatrix) : base(
        item, allIntersections, mediatrix)
    {
        INVALID_VALUE = new Vector2(-1, -1);
    }

    public Color colorGizmos;

    protected override bool IsInvalid(Vector2 intersection)
    {
        return (intersection.Equals(INVALID_VALUE) || intersection.Equals(Vector2.positiveInfinity) ||
                intersection.Equals(Vector2.negativeInfinity) || float.IsNaN(intersection.x) ||
                float.IsNaN(intersection.y));
    }

    public override void AddSegmentsWithLimits(List<SegmentLimit> limits)
    {
        foreach (SegmentLimit limit in limits)
        {
            Vector2 origin = itemSector;
            Vector2 final = limit.GetOpositePosition(origin);

            SegmentVec2 segment = new SegmentVec2();
            segment.AddNewSegment(origin, final, 0.5f);
            this.limits.Add(segment);
            segments.Add(segment);
        }
    }

    public override void DrawPoly()
    {
#if UNITY_EDITOR

        Vector3[] points = new Vector3[intersections.Count + 1];

        for (int i = 0; i < intersections.Count; i++)
        {
            points[i] = intersections[i];
        }

        points[intersections.Count] = points[0];
        Handles.color = colorGizmos;
        Handles.DrawAAConvexPolygon(points);

        Handles.color = Color.black;
        Handles.DrawPolyLine(points);

#endif
    }

    public override bool IsInside(Vector2 point)
    {
        int length = intersections.Count;

        if (length < 3)
        {
            return false;
        }

        Vector2 extreme = new Vector2(1000000000000, point.y);

        int count = 0;
        for (int i = 0; i < length; i++)
        {
            int next = (i + 1) % length;
            SegmentType intersectionChecker = new SegmentType();
            intersectionChecker.AddNewSegment(Vector2.zero, Vector2.zero, 0);
            Vector2 intersection =
                intersectionChecker.Intersection(intersections[i], intersections[next], point, extreme);
            if (!intersection.Equals(INVALID_VALUE))
                if (IsPointInSegment(intersection, intersections[i], intersections[next]))
                    if (IsPointInSegment(intersection, point, extreme))
                        count++;
        }

        return (count % 2 == 1);
    }

    public override float GetDistance(Vector2 centerCircle, Vector2 segment)
    {
        return Mathf.Sqrt(Mathf.Pow(Mathf.Abs(centerCircle.x - segment.x), 2) +
                          Mathf.Pow(Mathf.Abs(centerCircle.y - segment.y), 2));
    }

    protected override void SortPointsPolygon(bool value)
    {
        Vector2 centroid = new Vector2(intersections.Average(p => p.x), intersections.Average(p => p.y));

        intersections = intersections.OrderBy(p => Mathf.Atan2(p.y - centroid.y, p.x - centroid.x)).ToList();
    }

    protected override void SortPointsPolygon()
    {
        intersections.RemoveAll(p => p.x > 15);
        intersections.RemoveAll(p => p.y > 15);
        intersections.RemoveAll(p => p.y < -15);
        intersections.RemoveAll(p => p.y < -15);
        Vector2 centroid = new Vector2(intersections.Average(p => p.x), intersections.Average(p => p.y));
        intersections = intersections.OrderBy(p => Mathf.Atan2(p.y - centroid.y, p.x - centroid.x)).ToList();

        return;

        indexIntersections.Clear();


        float minX = intersections[0].x;
        float maxX = intersections[0].x;
        float minY = intersections[0].y;
        float maxY = intersections[0].y;
        for (int i = 0; i < intersections.Count; i++)
        {
            if (intersections[i].x < minX) minX = intersections[i].x;
            if (intersections[i].x > maxX) maxX = intersections[i].x;
            if (intersections[i].y < minY) minY = intersections[i].y;
            if (intersections[i].y > maxY) maxY = intersections[i].y;
        }

        Vector2 center = new Vector2(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);

        Dictionary<Vector2, float> angles = new Dictionary<Vector2, float>();
        foreach (var coord in intersections)
        {
            angles[coord] = (float)Math.Acos((coord.x - center.x) /
                                             Math.Sqrt(Math.Pow(coord.x - center.x, 2f) +
                                                       Math.Pow(coord.y - center.y, 2f)));

            // If the Y coordinate of the intersection is greater than the Y coordinate of the center,
            // adjust the angle to ensure it is in the correct range (0 to 2π radians)
            if (coord.y > center.y)
                angles[coord] = (float)(Math.PI + Math.PI - angles[coord]);
        }


        for (int i = 0; i < intersections.Count - 1; i++)
        {
            for (int j = 0; j < intersections.Count - 1; j++)
            {
                if (angles[intersections[j]] > angles[intersections[j + 1]])
                {
                    var temp = intersections[j];
                    intersections[j] = intersections[j + 1];
                    intersections[j + 1] = temp;
                }
            }
        }


        UpdateIntersectionList();
    }

    public override bool IsPointInSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        return (point.x <= Mathf.Max(start.x, end.x) &&
                point.x >= Mathf.Min(start.x, end.x) &&
                point.y <= Mathf.Max(start.y, end.y) &&
                point.y >= Mathf.Min(start.y, end.y));
    }
}