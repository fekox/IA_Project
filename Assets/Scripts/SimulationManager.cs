using System;
using IA_Library;
using IA_Library_FSM;
using UnityEngine;
using UnityEngine.Serialization;

public class SimulationManager : MonoBehaviour
{
    private Simulation simulation;
    public Material gridMaterial;
    public Material plantMaterial;
    public Material herbivoreMaterial;
    public Material carnivoreMaterial;
    public Material scavengerMaterial;
    public Mesh CubeMesh;
    private Mesh cellMesh;

    private void OnEnable()
    {
        GridManager NewGrid = new GridManager(20, 20, 2);

        cellMesh = CreateCellMesh();

        simulation = new Simulation(NewGrid, 10, 10, 10, 5, 10, 10, 10);
    }

    private void Update()
    {
    }

    private void DrawEntities()
    {
        foreach (AgentPlant agent in simulation.Plants)
        {
            DrawSquare(new Vector3(agent.position.Y, agent.position.Y, 0), plantMaterial, 1);
        }

        foreach (AgentHerbivore agent in simulation.Herbivore)
        {
            DrawSquare(new Vector3(agent.position.X, agent.position.Y, 1), herbivoreMaterial, 1);
        }

        foreach (AgentCarnivore agent in simulation.Carnivore)
        {
            DrawSquare(new Vector3(agent.position.X, agent.position.Y, 2), carnivoreMaterial, 1);
        }

        foreach (AgentScavenger agent in simulation.Scavenger)
        {
            DrawSquare(new Vector3(agent.position.X, agent.position.Y, 3), scavengerMaterial, agent.radius * 2);
        }
    }

    Mesh CreateCellMesh()
    {
        // Crear un Mesh simple de tipo "Quad" (rectángulo)
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        int[] triangles = new int[6];

        // Definir las posiciones de los vértices de un cuadrado
        vertices[0] = new Vector3(0, 0, 0);  // Vértice inferior izquierdo
        vertices[1] = new Vector3(1, 0, 0);  // Vértice inferior derecho
        vertices[2] = new Vector3(1, 1, 0);  // Vértice superior derecho
        vertices[3] = new Vector3(0, 1, 0);  // Vértice superior izquierdo

        // Definir los triángulos (para los dos triángulos que forman el cuadrado)
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        return mesh;
    }

    private void DrawGrid()
    {
        for (int x = 0; x < simulation.gridManager.size.X; x++)
        {
            for (int z = 0; z < simulation.gridManager.size.Y; z++)
            {
                Vector3 position = new Vector3(x * simulation.gridManager.cellSize, 0, z * simulation.gridManager.cellSize);
                Graphics.DrawMesh(cellMesh, position, Quaternion.identity, gridMaterial, 0);
            }
        }
    }

    private void DrawSquare(Vector3 position, Material color, float squareSize)
    {
        color.SetPass(0);
        Matrix4x4 matrix =
            Matrix4x4.TRS(position, Quaternion.identity, new Vector3(squareSize, squareSize, squareSize));
        Graphics.DrawMeshNow(CubeMesh, matrix);
    }

    private void OnRenderObject()
    {
        DrawGrid();
        DrawEntities();
    }
}