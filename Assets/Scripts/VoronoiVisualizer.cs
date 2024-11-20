using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class VoronoiVisualizer : MonoBehaviour
{
    public Voronoi<Node<Vector2>, Vector2> voronoi; // Reference to your Voronoi instance
    public Color polygonColor = Color.green;
    public Color intersectionColor = Color.red;
    public float pointSize = 0.1f;
    public GrapfView Grapf;
    List<Node<Vector2>> centers;
    public Vector2 scale = new Vector2();
    public GameObject a;

    private IEnumerator Start()
    {

        yield return null;
        yield return null;
        yield return null;
        yield return null;
        centers = Grapf.graph.mines;

        voronoi = new Voronoi<Node<Vector2>, Vector2>(centers, (int)scale.x, (int)scale.y, null);
        voronoi.GenerateVoronoi();
    }

    private void Update()
    {
        if (voronoi != null)
        {
            var test = new Vector2(a.transform.position.x, a.transform.position.y);

            foreach (var VARIABLE in voronoi.voronoiPolygons)
            {
                if ( voronoi.IsPointInPolygon(test,VARIABLE.Value))
                {
                    Debug.Log($"Inside influence of {VARIABLE.Key.GetCoordinate().ToString()}");
                }
               
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (voronoi == null)
            return;

        // Iterate through the Voronoi polygons
        foreach (var polygon in voronoi.voronoiPolygons)
        {
            List<Segments<Vector2>> segments = polygon.Value;

            // Draw each segment of the polygon
            foreach (Segments<Vector2> segment in segments)
            {
                Vector2 start = segment.init.coord;
                Vector2 end = segment.end.coord;

                // Draw the polygon edges in the scene view
                Gizmos.color = polygonColor;
                Gizmos.DrawLine(start, end);
            }

            // Draw intersection points
             List<Vector2> intersectionPoints = CalculateAllIntersections(voronoi.voronoiPolygons);
            
             //foreach (Vector2 intersection in intersectionPoints)
             //{
             //    // Draw intersection points as small spheres in the scene view
             //    Gizmos.color = intersectionColor;
             //    Gizmos.DrawSphere(intersection, pointSize);
             //}
        }
    }

    // This function calculates and returns all intersection points from the Voronoi polygons
    private List<Vector2> CalculateAllIntersections(Dictionary<Node<Vector2>, List<Segments<Vector2>>> voronoiPolygons)
    {
        List<Vector2> intersections = new List<Vector2>();

        foreach (var polygon in voronoiPolygons)
        {
            List<Segments<Vector2>> segments = polygon.Value;
            foreach (Segments<Vector2> segment in segments)
            {
                Vector2 dir = new Vector2(1, 0); // Ray direction (you can adjust this)
                Vector2 intersection;
                if (voronoi.LineVectorCollision(segment, dir, out intersection) &&
                    intersection != Voronoi<Node<Vector2>, Vector2>.INVALID_VALUE)
                {
                    intersections.Add(intersection);
                }
            }
        }

        return intersections;
    }
}