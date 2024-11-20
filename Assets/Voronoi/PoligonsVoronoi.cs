using System;
using System.Collections.Generic;
using System;


[System.Serializable]
public abstract class PoligonsVoronoi<SegmentType, Coord> where Coord : IEquatable<Coord>
    where SegmentType : Segment<Coord>, new()
{
    public bool drawPoli;
    public float weight = 0;
    public float relationOfMediatrix = 0;
    public Coord itemSector;
    public List<SegmentType> segments = new List<SegmentType>();
    public List<SegmentType> limits = new List<SegmentType>();
    public List<Coord> intersections = new List<Coord>();
    public List<int> indexIntersections = new List<int>();
    public const float defaultMediatrix = 0.5f;
    protected List<Coord> allIntersections;
    public static Coord INVALID_VALUE;


    public void SortSegment()
    {
        segments.Sort((p1, p2) => p1.Distance.CompareTo(p2.Distance));
        //
    }


    public PoligonsVoronoi(Coord item, List<Coord> allIntersections, float relationOfMediatrix = defaultMediatrix)
    {
        itemSector = item;
        this.allIntersections = allIntersections;
        this.relationOfMediatrix = defaultMediatrix;
    }

    public void AddSegment(SegmentType refSegment)
    {
        SegmentType segment = new SegmentType();
        segment.AddNewSegment(refSegment.Origin, refSegment.Final, refSegment.persentageOfDistance);
        segments.Add(segment);
    }

    public void SetIntersections()
    {
        intersections.Clear();
        weight = 0;

        SortSegment();

        for (int i = 0; i < segments.Count; i++)
        {
            for (int j = 0; j < segments.Count; j++)
            {
                if (i == j)
                    continue;
                if (segments[i].id == segments[j].id)
                    continue;

                segments[i].GetTwoPoints(out Coord p1, out Coord p2);
                segments[j].GetTwoPoints(out Coord p3, out Coord p4);

                Coord intersection = segments[i].Intersection(p1, p2, p3, p4);
                if (IsInvalid(intersection)) continue;

                //Todo:Check for wrong point
                if (intersections.Contains(intersection))
                    continue;

                float maxDistance = GetDistance(intersection, segments[i].Origin);

                bool hasOtherPoint = false;
                for (int k = 0; k < segments.Count; k++)
                {
                    if (k == i || k == j)
                        continue;
                    if (HasOtherPointInCircle(intersection, segments[k], maxDistance))
                    {
                        hasOtherPoint = true;
                        break;
                    }
                }

                if (!hasOtherPoint)
                {
                    intersections.Add(intersection);
                    segments[i].intersection.Add(intersection);
                    segments[j].intersection.Add(intersection);
                }
            }
        }

        RemoveUnusedSegments();
        SortPointsPolygon();
    }

    protected abstract bool IsInvalid(Coord intersection);

    public abstract void AddSegmentsWithLimits(List<SegmentLimit> limits);

    private bool HasOtherPointInCircle(Coord centerCircle, SegmentType segment, float maxDistance)
    {
        float distance = GetDistance(centerCircle, segment.Final);
        return distance < maxDistance;
    }

    protected void RemoveUnusedSegments()
    {
        List<SegmentType> segmentsUnused = new List<SegmentType>();
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i].intersection.Count != 2)
                segmentsUnused.Add(segments[i]);
        }

        for (int i = 0; i < segmentsUnused.Count; i++)
        {
            segments.Remove(segmentsUnused[i]);
        }
    }

    public bool hasSameSegment(PoligonsVoronoi<SegmentType, Coord> otherPoly)
    {
        for (int index = 0; index < intersections.Count; index++)
        {
            for (int j = 0; j < otherPoly.intersections.Count; j++)
            {
                if (otherPoly.intersections.Count > 0)
                {
                    if (otherPoly.intersections[j].Equals(intersections[index]))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    protected abstract void SortPointsPolygon(bool value);

    protected virtual void SortPointsPolygon()
    {
        intersections.Clear();


        Coord lastIntersection = segments[0].intersection[0];
        intersections.Add(lastIntersection);

        for (int i = 0; i < segments.Count; i++)
        {
            for (int j = 0; j < segments.Count; j++)
            {
                if (i == j) continue;

                Coord firstIntersection = segments[j].intersection[0];
                Coord secondIntersection = segments[j].intersection[1];


                if (lastIntersection.Equals(firstIntersection) && !intersections.Contains(secondIntersection))
                {
                    intersections.Add(secondIntersection);
                    lastIntersection = secondIntersection;
                    break;
                }

                if (lastIntersection.Equals(secondIntersection) && !intersections.Contains(firstIntersection))
                {
                    intersections.Add(firstIntersection);
                    lastIntersection = firstIntersection;
                    break;
                }
            }
        }


        Coord firstIntersectionInLastSegment = segments[^1].intersection[0];
        if (!intersections.Contains(firstIntersectionInLastSegment))
            intersections.Add(firstIntersectionInLastSegment);

        Coord secondIntersectionInLastSegment = segments[^1].intersection[1];
        if (!intersections.Contains(secondIntersectionInLastSegment))
            intersections.Add(secondIntersectionInLastSegment);


        indexIntersections.Clear();
        for (int i = 0; i < intersections.Count; i++)
        {
            Coord intersection = intersections[i];
            if (!allIntersections.Contains(intersection))
            {
                allIntersections.Add(intersection);
                indexIntersections.Add(allIntersections.Count - 1);
            }
            else
            {
                indexIntersections.Add(allIntersections.IndexOf(intersection));
            }
        }

        UpdateIntersectionList();
    }

    protected void UpdateIntersectionList()
    {
        // intersections.Clear();
        //
        // for (int i = 0; i < indexIntersections.Count; i++)
        // {
        //     intersections.Add(allIntersections[indexIntersections[i]]);
        // }
    }

    public abstract void DrawPoly();

    public abstract bool IsInside(Coord point);

    public abstract float GetDistance(Coord centerCircle, Coord segment);

    public abstract bool IsPointInSegment(Coord point, Coord start, Coord end);
}