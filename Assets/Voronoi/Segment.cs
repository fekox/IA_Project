using System;
using System.Collections.Generic;


public abstract class Segment<Coord> : IEquatable<Segment<Coord>>
{
    public static int amountSegments = 0;
    public int id = 0;
    public bool isLimit;

     public Coord origin;
     public Coord final;

     public Coord direction;
     public Coord mediatrix;
     public float distance;
      public float persentageOfDistance;

    public List<Coord> intersection = new List<Coord>();

    public Segment(Coord newOrigin, Coord newFinal)
    {
        
    }public Segment()
    {
        
    }
    public Coord Direction => direction;
    public Coord Mediatrix => mediatrix;
    public Coord Origin => origin;
    public Coord Final => final;
    public float Distance => distance;

    public abstract void GetTwoPoints(out Coord p1, out Coord p2);
    
    public abstract Coord Intersection(Coord ap1, Coord ap2, Coord bp1, Coord bp2);

    public abstract void AddNewSegment(Coord newOrigin, Coord newFinal, float persentageOfDistance);

    public bool Equals(Segment<Coord> other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return id == other.id && isLimit == other.isLimit && EqualityComparer<Coord>.Default.Equals(origin, other.origin) && EqualityComparer<Coord>.Default.Equals(final, other.final) && EqualityComparer<Coord>.Default.Equals(direction, other.direction) && EqualityComparer<Coord>.Default.Equals(mediatrix, other.mediatrix) && distance.Equals(other.distance) && persentageOfDistance.Equals(other.persentageOfDistance) && Equals(intersection, other.intersection);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Segment<Coord>)obj);
    }

    public override int GetHashCode()
    {
        HashCode hashCode = new HashCode();
        hashCode.Add(id);
        hashCode.Add(isLimit);
        hashCode.Add(origin);
        hashCode.Add(final);
        hashCode.Add(direction);
        hashCode.Add(mediatrix);
        hashCode.Add(distance);
        hashCode.Add(persentageOfDistance);
        hashCode.Add(intersection);
        return hashCode.ToHashCode();
    }
}