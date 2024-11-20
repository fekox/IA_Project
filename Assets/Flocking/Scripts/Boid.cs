using System;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public float speed = 2.5f;
    public float detectionRadious = 3.0f;
    public float aligmentWeight = 1;
    public float cohesionWeight = 1.5f;
    public float separationWeight = 2;
    private Func<Boid, Vector3> Alignment;
    private Func<Boid, Vector3> Cohesion;
    private Func<Boid, Vector3> Separation;
    private Func<Boid, Vector3> Direction;

    public void Init(Func<Boid, Vector3> Alignment,
        Func<Boid, Vector3> Cohesion,
        Func<Boid, Vector3> Separation,
        Func<Boid, Vector3> Direction)
    {
        this.Alignment = Alignment;
        this.Cohesion = Cohesion;
        this.Separation = Separation;
        this.Direction = Direction;
    }

    // private void Update()
    // {
    //    // transform.LookAt(ACS());
    //     transform.forward = Vector3.Lerp(transform.forward, ACS(), 1);
    //     transform.position += transform.forward * speed * Time.deltaTime;
    // }

    public Vector3 ACS()
    {
        Vector3 ACS = Alignment(this) * aligmentWeight + Cohesion(this) * cohesionWeight +
                      Separation(this) * separationWeight + Direction(this);
        return ACS.normalized;
    }
}
[Serializable]
public class BoidAgent
{
    public float speed = 2.5f;
    public float detectionRadious = 3.0f;
    public float aligmentWeight = 1;
    public float cohesionWeight = 1.5f;
    public float separationWeight = 2;
    public Transform parent;
    public Vector3 objective;
    public Vector3 position;
    private Func<BoidAgent, Vector3> Alignment;
    private Func<BoidAgent, Vector3> Cohesion;
    private Func<BoidAgent, Vector3> Separation;
    private Func<BoidAgent, Vector3> Direction;

    private Vector3 aligment;
    private Vector3 cohesion;
    private Vector3 separation;
    private Vector3 direction;
    public void Init(Func<BoidAgent, Vector3> Alignment,
        Func<BoidAgent, Vector3> Cohesion,
        Func<BoidAgent, Vector3> Separation,
        Func<BoidAgent, Vector3> Direction)
    {
        this.Alignment = Alignment;
        this.Cohesion = Cohesion;
        this.Separation = Separation;
        this.Direction = Direction;
    }
    public void SetACS(Vector3 aligment,Vector3 cohesion,Vector3 separation,Vector3 direction)
    {
        this.aligment = aligment;
        this.cohesion = cohesion;
        this.separation = separation;
        this.direction = direction;
    }

    public Vector3 ACS()
    {
        Vector3 ACS = aligment * aligmentWeight + cohesion * cohesionWeight +
                      separation * separationWeight + direction;

        return new Vector2(ACS.x, ACS.y).normalized;

    }
}