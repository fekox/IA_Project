using System;
using UnityEngine;

public class SegmentVec2 : Segment<Vector2> 
{
    public SegmentVec2(Vector2 newOrigin, Vector2 newFinal,float relationOfMediatrix) : base(newOrigin,newFinal)
    {
        AddNewSegment(newOrigin, newFinal,relationOfMediatrix);
    }    
    public SegmentVec2() : base()
    {

    }

    public override void GetTwoPoints(out Vector2 p1, out Vector2 p2)
    {
        p1 = mediatrix;
        p2 = mediatrix + direction * 40000;
    }

    public override Vector2 Intersection(Vector2 ap1, Vector2 ap2, Vector2 bp1, Vector2 bp2)
    {
        float denominator = ((ap1.x - ap2.x) * (bp1.y - bp2.y) - (ap1.y - ap2.y) * (bp1.x - bp2.x));

        if (denominator == 0)
            return new Vector2(-1, -1);; 

        float numeradorX = ((ap1.x * ap2.y - ap1.y * ap2.x) * (bp1.x - bp2.x) - (ap1.x - ap2.x) * (bp1.x * bp2.y - bp1.y * bp2.x));
        float numeradorY = ((ap1.x * ap2.y - ap1.y * ap2.x) * (bp1.y - bp2.y) - (ap1.y - ap2.y) * (bp1.x * bp2.y - bp1.y * bp2.x));

        Vector2 intersection = new Vector2(numeradorX / denominator, numeradorY / denominator);
        return intersection;
    }

    public override void AddNewSegment(Vector2 newOrigin, Vector2 newFinal, float persentageOfDistance)
    {
        id = amountSegments;
        amountSegments++;
        origin = newOrigin;
        final = newFinal;
        this.persentageOfDistance = persentageOfDistance;
        distance =  Mathf.Sqrt(Mathf.Pow(Mathf.Abs(origin.x - final.x), 2) +
                               Mathf.Pow(Mathf.Abs(origin.y - final.y), 2));

        float a = 1;
        mediatrix = (origin + final) * persentageOfDistance;
        //mediatrix = origin * (a - persentageOfDistance)+ final * persentageOfDistance;
       mediatrix = Vector2.Lerp(origin, final, persentageOfDistance);

        direction = (final - origin).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        direction = perpendicular; 
    }
}