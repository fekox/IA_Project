using System;
using System.Collections.Generic;
using IA_Library;
using IA_Library_FSM;
using IA_Library.Brain;
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

    private BrainData herbivoreMainBrain;
    private BrainData herbivoreMoveEatBrain;
    private BrainData herbivoreMoveEscapeBrain;
    private BrainData herbivoreEatBrain;
    private BrainData carnivoreMainBrain;
    private BrainData carnivoreMoveEatBrain;
    private BrainData carnivoreEatBrain;
    private BrainData scavengerMainBrain;
    private BrainData scavengerFlockingBrain;

    private float Bias = 0.5f;
    private float P = 0.5f;

    private void OnEnable()
    {
        GridManager NewGrid = new GridManager(20, 20, 2);

        cellMesh = CreateCellMesh();

        herbivoreMainBrain = new BrainData(11, new int[] { 7, 5, 3 }, 3, Bias, P);
        herbivoreMoveEatBrain = new BrainData(4, new int[] { 5, 4 }, 4, Bias, P);
        herbivoreMoveEscapeBrain = new BrainData(5, new int[] { 3 }, 1, Bias, P);
        herbivoreEatBrain = new BrainData(8, new int[] { 5, 3 }, 4, Bias, P);

        carnivoreMainBrain = new BrainData(5, new int[] { 3, 2 }, 2, Bias, P);
        carnivoreMoveEatBrain = new BrainData(4, new int[] { 3, 2 }, 2, Bias, P);
        carnivoreEatBrain = new BrainData(5, new int[] { 2, 2 }, 1, Bias, P);

        scavengerMainBrain = new BrainData(5, new int[] { 3, 5 }, 3, Bias, P);
        scavengerFlockingBrain = new BrainData(8, new int[] { 5, 5, 5 }, 4, Bias, P);

        List<BrainData> herbivoreData = new List<BrainData>
            { herbivoreMainBrain, herbivoreMoveEatBrain, herbivoreMoveEscapeBrain, herbivoreEatBrain };
        List<BrainData> carnivoreData = new List<BrainData>
            { carnivoreMainBrain, carnivoreMoveEatBrain, carnivoreEatBrain };
        List<BrainData> scavengerData = new List<BrainData> { scavengerMainBrain, scavengerFlockingBrain };

        simulation = new Simulation(NewGrid, herbivoreData, carnivoreData, scavengerData, 10, 10, 10, 5, 10, 10, 10);
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
        vertices[0] = new Vector3(0, 0, 0); // Vértice inferior izquierdo
        vertices[1] = new Vector3(1, 0, 0); // Vértice inferior derecho
        vertices[2] = new Vector3(1, 1, 0); // Vértice superior derecho
        vertices[3] = new Vector3(0, 1, 0); // Vértice superior izquierdo

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
                Vector3 position = new Vector3(x * simulation.gridManager.cellSize, 0,
                    z * simulation.gridManager.cellSize);
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