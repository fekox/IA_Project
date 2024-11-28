using System.Collections.Generic;
using IA_Library;
using IA_Library_FSM;
using IA_Library.Brain;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimulationManager : MonoBehaviour
{
    private Simulation simulation;

    [SerializeField] private UpdateGenerationText updateGenerationText;

    [Header("Current generation")]
    public int currentGeneration = 1;

    [Header("Save system")]
    public string fileToLoad;

    public string filePath = "/Saves/";
    public string fileExtension = "generation";

    [Header("Grid settings")]
    public Vector2Int gridSize;
    public float cellSize;

    [Header("Entities settings")]
    [SerializeField] private int totalHerbivores;
    [SerializeField] private int totalCarnivores;
    [SerializeField] private int totalScavengers;
    [SerializeField] private int totalPlants;

    [Header("Simulation settings")]
    [SerializeField] private int mutationChance;
    [SerializeField] private int mutationRate;
    [SerializeField] private int totalElites;
    [SerializeField] private int generationTime;

    [Header("Bias")]
    public float herbivoreBias = 0.5f;
    public float carnivoreBias = 0.5f;
    public float scavengerBias = 0.5f;

    [Header("P")]
    public float herbivoreP = 0.5f;
    public float carnivoreP = 0.5f;
    public float scavengerP = 0.5f;

    [Header("Materials")]
    public Material plantMaterial;
    public Material herbivoreMaterial;
    public Material deadHerbivoreMaterial;
    public Material carnivoreMaterial;
    public Material scavengerMaterial;
    public Material gridMaterial;

    [Header("Meshes")]
    public Mesh herbivoreMesh;
    public Mesh carnivoreMesh;
    public Mesh scavengerMesh;
    public Mesh plantMesh;
    public Mesh gridMesh;

    private BrainData herbivoreMainBrain;
    private BrainData herbivoreMoveEatBrain;
    private BrainData herbivoreMoveEscapeBrain;
    private BrainData herbivoreEatBrain;
    private BrainData carnivoreMainBrain;
    private BrainData carnivoreMoveEatBrain;
    private BrainData carnivoreEatBrain;
    private BrainData scavengerMainBrain;
    private BrainData scavengerFlockingBrain;

    private GridManager NewGrid;

    #region Inputs Fields

    [Header("Save System UI")]
    [SerializeField] private TMP_InputField fileToLoadIF;

    [Header("Grid Settings UI")]
    [SerializeField] private TMP_InputField gridXIF;
    [SerializeField] private TMP_InputField gridYIF;
    [SerializeField] private TMP_InputField cellSizeIF;

    [Header("Entities Settings UI")]
    [SerializeField] private TMP_InputField maxHerbivoresIF;
    [SerializeField] private TMP_InputField maxCarnivoresIF;
    [SerializeField] private TMP_InputField maxScavengersIF;
    [SerializeField] private TMP_InputField maxPlantsIF;

    [Header("Simulation Settings UI")]
    [SerializeField] private TMP_InputField mutationChanceIF;
    [SerializeField] private TMP_InputField mutationRateIF;
    [SerializeField] private TMP_InputField totalElitesIF;
    [SerializeField] private TMP_InputField generationTimeIF;
    [SerializeField] private TMP_InputField biasIF;
    [SerializeField] private TMP_InputField PIF;

    #endregion

    private void OnEnable()
    {
        NewGrid = new GridManager(gridSize.x, gridSize.y, cellSize);

        herbivoreMainBrain = new BrainData(11, new int[] { 9, 7, 5, 3 }, 3, herbivoreBias, herbivoreP);
        herbivoreMoveEatBrain = new BrainData(4, new int[] { 8, 8, 6, 4 }, 4, herbivoreBias, herbivoreP);
        herbivoreMoveEscapeBrain = new BrainData(8, new int[] { 8, 6, 4, 4 }, 4, herbivoreBias, herbivoreP);
        herbivoreEatBrain = new BrainData(5, new int[] { 3, 3, 2 }, 1, herbivoreBias, herbivoreP);

        carnivoreMainBrain = new BrainData(5, new int[] { 3, 2 }, 2, carnivoreBias, carnivoreP);
        carnivoreMoveEatBrain = new BrainData(4, new int[] { 6, 4, 4 }, 2, carnivoreBias, carnivoreP);
        carnivoreEatBrain = new BrainData(5, new int[] { 6, 4, 2 }, 1, carnivoreBias, carnivoreP);

        scavengerMainBrain = new BrainData(5, new int[] { 8, 6, 4, 6 }, 2, scavengerBias, scavengerP);
        scavengerFlockingBrain = new BrainData(8, new int[] { 10, 8, 6, 6 }, 6, scavengerBias, scavengerP);

        List<BrainData> herbivoreData = new List<BrainData>
            { herbivoreMainBrain, herbivoreMoveEatBrain, herbivoreMoveEscapeBrain, herbivoreEatBrain };

        List<BrainData> carnivoreData = new List<BrainData>
            { carnivoreMainBrain, carnivoreMoveEatBrain, carnivoreEatBrain };

        List<BrainData> scavengerData = new List<BrainData> { scavengerMainBrain, scavengerFlockingBrain };

        simulation = new Simulation(NewGrid, herbivoreData, carnivoreData, scavengerData, totalHerbivores,
            totalCarnivores, totalScavengers, totalPlants, totalElites, mutationChance, mutationRate, generationTime)
        {
            filepath = Application.dataPath + filePath,
            fileExtension = fileExtension,
            fileToLoad = fileToLoad
        };

        simulation.OnFitnessCalculated += LogFitness;
    }


    private void OnDisable()
    {
        simulation.OnFitnessCalculated -= LogFitness;
    }

    private void Update()
    {
        currentGeneration = simulation.UpdateSimulation(Time.deltaTime);
        updateGenerationText.UpdateCurrentGeneration(currentGeneration);
    }

    private void LogFitness(int nH, float FH, int nC, float FC, int nS, float FS)
    {
        // Debug.Log("--- Average Fitness ---");
        // Debug.Log("Herbivore - Alive = " + nH + " / fitness = " + FH);
        // Debug.Log("Carnivore - Alive = " + nC + " / fitness = " + FC);
        // Debug.Log("Scavenger - Alive = " + nS + " / fitness = " + FS);
    }

    private void DrawEntities()
    {
        foreach (AgentHerbivore agent in simulation.Herbivore)
        {
            if (agent.lives <= 0)
            {
                DrawMesh(herbivoreMesh, new Vector3(agent.position.X, agent.position.Y, 0), deadHerbivoreMaterial, 1);
            }

            else
            {
                DrawMesh(herbivoreMesh, new Vector3(agent.position.X, agent.position.Y, 0), herbivoreMaterial, 1);
            }
        }

        foreach (AgentCarnivore agent in simulation.Carnivore)
        {
            DrawMesh(carnivoreMesh, new Vector3(agent.position.X, agent.position.Y, 0), carnivoreMaterial, 1);
        }

        foreach (AgentScavenger agent in simulation.Scavenger)
        {
            DrawMesh(scavengerMesh, new Vector3(agent.position.X, agent.position.Y, 0), scavengerMaterial, 1);
        }

        foreach (AgentPlant agent in simulation.Plants)
        {
            DrawMesh(plantMesh, new Vector3(agent.position.X, agent.position.Y, 0), plantMaterial, 1);
        }
    }

    public void DrawGrid()
    {
        for (int x = 0; x < simulation.gridManager.size.X; x++)
        {
            for (int y = 0; y < simulation.gridManager.size.Y; y++)
            {
                DrawMesh(gridMesh, new Vector3(x, y, 0), gridMaterial, 0.3f);
            }
        }
    }

    private void DrawMesh(Mesh meshType, Vector3 position, Material color, float squareSize)
    {
        color.SetPass(0);

        Matrix4x4 matrix =
            Matrix4x4.TRS(position, Quaternion.identity, new Vector3(squareSize, squareSize, squareSize));

        Graphics.DrawMeshNow(meshType, matrix);
    }

    private void OnRenderObject()
    {
        DrawGrid();
        DrawEntities();
    }

    [ContextMenu("Load Save")]
    private void Load()
    {
        simulation.fileToLoad = Application.dataPath + filePath + fileToLoad + "." + fileExtension;
        simulation.filepath = Application.dataPath + filePath;
        simulation.fileExtension = fileExtension;
        simulation.Load();
    }

    #region Setters
    public void SetFileToLoad()
    {
        fileToLoad = fileToLoadIF.text;
    }

    public void SetGirdXSize()
    {
        if (int.TryParse(gridXIF.text, out int value))
        {
            gridSize.x = value;
        }
    }
    public void SetGirdYSize()
    {
        if (int.TryParse(gridYIF.text, out int value))
        {
            gridSize.y = value;
        }
    }

    public void SetCellSize()
    {
        if (float.TryParse(cellSizeIF.text, out float value))
        {
            cellSize = value;
        }
    }

    public void SetMaxHerbivores()
    {
        if (int.TryParse(maxHerbivoresIF.text, out int value))
        {
            totalHerbivores = value;
        }
    }

    public void SetMaxCarnivores()
    {
        if (int.TryParse(maxCarnivoresIF.text, out int value))
        {
            totalCarnivores = value;
        }
    }

    public void SetMaxScavengers()
    {
        if (int.TryParse(maxScavengersIF.text, out int value))
        {
            totalScavengers = value;
        }
    }

    public void SetMaxPlants()
    {
        if (int.TryParse(maxPlantsIF.text, out int value))
        {
            totalPlants = value;
        }
    }

    public void SetMutationChance()
    {
        if (int.TryParse(mutationChanceIF.text, out int value))
        {
            mutationChance = value;
        }
    }

    public void SetMutationRate()
    {
        if (int.TryParse(mutationRateIF.text, out int value))
        {
            mutationRate = value;
        }
    }

    public void SetTotalElites()
    {
        if (int.TryParse(totalElitesIF.text, out int value))
        {
            totalElites = value;
        }
    }

    public void SetGenerationTime()
    {
        if (int.TryParse(generationTimeIF.text, out int value))
        {
            generationTime = value;
        }
    }

    public void SetBias()
    {
        if (float.TryParse(biasIF.text, out float value))
        {
            herbivoreBias = value;
            carnivoreBias = value;
            scavengerBias = value;
        }
    }

    public void SetP()
    {
        if (float.TryParse(PIF.text, out float value))
        {
            herbivoreP = value;
            carnivoreP = value;
            scavengerP = value;
        }
    }
    #endregion
}