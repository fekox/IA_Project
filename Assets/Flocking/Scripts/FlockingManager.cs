using System.Collections.Generic;
using UnityEngine;

public class FlockingManager : MonoBehaviour
{
    public Transform target;
    public int boidCount = 50;
    public Boid boidPrefab;
    private List<Boid> boids = new List<Boid>();

    private void Start()
    {
        for (int i = 0; i < boidCount; i++)
        {
            GameObject boidGO = Instantiate(boidPrefab.gameObject, new Vector3(Random.Range(-10, 10), Random.Range(-10, 10)), Quaternion.identity);
            Boid boid = boidGO.GetComponent<Boid>();
            boid.Init(Alignment, Cohesion, Separation, Direction);
            boids.Add(boid);
        }
    }

    public Vector3 Alignment(Boid boid)
    {
        List<Boid> insideRadiusBoids = GetBoidsInsideRadius(boid);
        Vector3 avg = Vector3.zero;
        foreach (Boid b in insideRadiusBoids)
        {
            avg += b.transform.forward * b.speed;
        }
        avg /= insideRadiusBoids.Count;
        avg.Normalize();
        return avg;
    }

    public Vector3 Cohesion(Boid boid)
    {
        List<Boid> insideRadiusBoids = GetBoidsInsideRadius(boid);
        Vector3 avg = Vector3.zero;
        foreach (Boid b in insideRadiusBoids)
        {
            avg += b.transform.position;
        }
        avg /= insideRadiusBoids.Count;
        return (avg - boid.transform.position).normalized;
    }

    public Vector3 Separation(Boid boid)
    {
        List<Boid> insideRadiusBoids = GetBoidsInsideRadius(boid);
        Vector3 avg = Vector3.zero;
        foreach (Boid b in insideRadiusBoids)
        {
            avg += (b.transform.position - boid.transform.position);
        }
        avg /= insideRadiusBoids.Count;
        avg *= -1;
        avg.Normalize();
        return avg;
    }

    public Vector3 Direction(Boid boid)
    {
        return (target.position - boid.transform.position).normalized;
    }

    public List<Boid> GetBoidsInsideRadius(Boid boid)
    {
        List<Boid> insideRadiusBoids = new List<Boid>();

        foreach (Boid b in boids)
        {
            float distance = Vector3.Distance(boid.transform.position, b.transform.position);
            if (distance < boid.detectionRadious)
            {
                insideRadiusBoids.Add(b);
            }
        }

        return insideRadiusBoids;
    }
}