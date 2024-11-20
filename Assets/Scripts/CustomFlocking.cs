using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomFlocking : MonoBehaviour
{
    public Transform target;
    // public int boidCount = 50;
    public List<GameObject> agents;
    private List<IAlarmable> alarmables = new List<IAlarmable>();
    private List<BoidAgent> boids = new List<BoidAgent>();
    public float detectionRadious = 3.0f;
    public float aligmentWeight = 1;
    public float cohesionWeight = 1.5f;
    public float separationWeight = 2;
    public float speed = 2;
    

    [ContextMenu("RaiseAlarm")]
    public void RaiseAlarm()
    {
         foreach (IAlarmable agent in alarmables)
         {
             agent.InvokeAlarmOn();
         }
    } 
    [ContextMenu("StopAlarm")]
    public void StopAlarm()
    {
        foreach (IAlarmable agent in alarmables)
        {
            agent.InvokeAlarmOff();
        }
    }
    private void Start()
    {
        foreach (var agent in agents)
        {
            BoidAgent boid = agent.GetComponent<IFlock>().GetBoid();
            alarmables.Add(agent.GetComponent<IAlarmable>());
            boid.Init(Alignment, Cohesion, Separation, Direction);
            SetBoidParams(boid);
            boids.Add(boid);

        }
        
    }

    private void OnDisable()
    {
    
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            foreach (GameObject agent in agents)
            {
                BoidAgent boid = agent.GetComponent<IFlock>().GetBoid();
                SetBoidParams(boid);
            }
        }
    }

    private void SetBoidParams(BoidAgent boid)
    {
        boid.detectionRadious = detectionRadious;
        boid.aligmentWeight = aligmentWeight;
        boid.cohesionWeight = cohesionWeight;
        boid.separationWeight = separationWeight;
        boid.speed = speed;
    }

    public Vector3 Alignment(BoidAgent boid)
    {
        List<BoidAgent> insideRadiusBoids = GetBoidsInsideRadius(boid);
        Vector3 avg = Vector3.zero;
        foreach (BoidAgent b in insideRadiusBoids)
        {
            avg += b.parent.transform.right * b.speed;
        }

        avg /= insideRadiusBoids.Count;
        avg.Normalize();
        return avg;
    }

    public Vector3 Cohesion(BoidAgent boid)
    {
        List<BoidAgent> insideRadiusBoids = GetBoidsInsideRadius(boid);
        Vector3 avg = Vector3.zero;
        foreach (BoidAgent b in insideRadiusBoids)
        {
            avg += b.parent.transform.position;
        }

        avg /= insideRadiusBoids.Count;
        return (avg - boid.parent.transform.position).normalized;
    }

    public Vector3 Separation(BoidAgent boid)
    {
        List<BoidAgent> insideRadiusBoids = GetBoidsInsideRadius(boid);
        Vector3 avg = Vector3.zero;
        foreach (BoidAgent b in insideRadiusBoids)
        {
            avg += (b.parent.transform.position - boid.parent.transform.position);
        }

        avg /= insideRadiusBoids.Count;
        avg *= -1;
        avg.Normalize();
        return avg;
    }

    public Vector3 Direction(BoidAgent boid)
    {
        return (boid.objective - boid.parent.transform.position).normalized;
    }

    public List<BoidAgent> GetBoidsInsideRadius(BoidAgent boid)
    {
        List<BoidAgent> insideRadiusBoids = new List<BoidAgent>();

        foreach (BoidAgent b in boids)
        {
            float distance = Vector3.Distance(boid.parent.transform.position, b.parent.transform.position);
            if (distance < boid.detectionRadious)
            {
                insideRadiusBoids.Add(b);
            }
        }

        return insideRadiusBoids;
    }
}