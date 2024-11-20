﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class PopulationManager : MonoBehaviour
{
    public GameObject TankPrefab;
    public GameObject MinePrefab;


    public Vector3 SceneHalfExtents = new Vector3(20.0f, 0.0f, 20.0f);

    public int PopulationCount = 40;
    public int MinesCount = 50;
    public float GenerationDuration = 20.0f;
    public int IterationCount = 1;

    public int EliteCount = 4;
    public float MutationChance = 0.10f;
    public float MutationRate = 0.01f;

    public int InputsCount = 4;
    public int HiddenLayers = 1;
    public int OutputsCount = 2;
    public int NeuronsCountPerHL = 7;
    public float Bias = 1f;
    public float P = 0.5f;


    public GeneticAlgorithm genAlg;

    List<Tank> populationGOs = new List<Tank>();
    List<Genome> population = new List<Genome>();
    List<Brain> brains = new List<Brain>();
    List<IMinable> mines = new List<IMinable>();
    List<IMinable> goodMines = new List<IMinable>();
    List<IMinable> badMines = new List<IMinable>();

    float accumTime = 0;
    bool isRunning = false;

    public int generation { get; private set; }

    public float bestFitness { get; private set; }

    public float avgFitness { get; private set; }

    public float worstFitness { get; private set; }

    private float getBestFitness()
    {
        float fitness = 0;
        foreach (Genome g in population)
        {
            if (fitness < g.fitness)
                fitness = g.fitness;
        }

        return fitness;
    }

    private float getAvgFitness()
    {
        float fitness = 0;
        foreach (Genome g in population)
        {
            fitness += g.fitness;
        }

        return fitness / population.Count;
    }

    private float getWorstFitness()
    {
        float fitness = float.MaxValue;
        foreach (Genome g in population)
        {
            if (fitness > g.fitness)
                fitness = g.fitness;
        }

        return fitness;
    }

    static PopulationManager instance = null;

    public static PopulationManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<PopulationManager>();

            return instance;
        }
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
    }

    public void StartSimulation()
    {
        // Create and confiugre the Genetic Algorithm

    GenerateInitialPopulation();
        CreateMines();
        genAlg = new GeneticAlgorithm(EliteCount, MutationChance, MutationRate, brains[0]);

        isRunning = true;
    }

    public void PauseSimulation()
    {
        isRunning = !isRunning;
    }

    public void StopSimulation()
    {
        isRunning = false;

        generation = 0;

        // Destroy previous tanks (if there are any)
        DestroyTanks();

        // Destroy all mines
        DestroyMines();
    }

    // Generate the random initial population
    void GenerateInitialPopulation()
    {
        generation = 0;

        // Destroy previous tanks (if there are any)
        DestroyTanks();

        for (int i = 0; i < PopulationCount; i++)
        {
            Brain brain = CreateBrain();

            Genome genome = new Genome(brain.GetTotalWeightsCount());

            brain.SetWeights(genome.genome);
            brains.Add(brain);

            population.Add(genome);
            populationGOs.Add(CreateTank(genome, brain));
        }

        accumTime = 0.0f;
    }

    // Creates a new NeuralNetwork
    Brain CreateBrain()
    {
        Brain brain = new Brain();

        // Add first neuron layer that has as many neurons as inputs
        brain.AddFirstNeuronLayer(InputsCount, Bias, P);

        for (int i = 0; i < HiddenLayers; i++)
        {
            // Add each hidden layer with custom neurons count
            brain.AddNeuronLayer(NeuronsCountPerHL, Bias, P);
        }

        // Add the output layer with as many neurons as outputs
        brain.AddNeuronLayer(OutputsCount, Bias, P);

        return brain;
    }

    // Evolve!!!
    void Epoch()
    {
        // Increment generation counter
        generation++;

        // Calculate best, average and worst fitness
        bestFitness = getBestFitness();
        avgFitness = getAvgFitness();
        worstFitness = getWorstFitness();

        //Todo: Make a way to get this the genAlg
        string genonmes = JsonUtility.ToJson(genAlg,true);
        System.IO.File.WriteAllText(Application.dataPath+ "/Saves/Genomes.json", genonmes);
        // Evolve each genome and create a new array of genomes
        Genome[] newGenomes = genAlg.Epoch(population.ToArray());

        // Clear current population
        population.Clear();

        // Add new population
        population.AddRange(newGenomes);

        // Set the new genomes as each NeuralNetwork weights
        for (int i = 0; i < PopulationCount; i++)
        {
            Brain brain = brains[i];

            brain.CopyStructureFrom(genAlg.brain);
            brain.SetWeights(newGenomes[i].genome);

            populationGOs[i].SetBrain(newGenomes[i], brain);
            populationGOs[i].transform.position = GetRandomPos();
            populationGOs[i].transform.rotation = GetRandomRot();
        }
        genAlg.brain = brains[0];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isRunning)
            return;

        float dt = Time.fixedDeltaTime;

        for (int i = 0; i < Mathf.Clamp((float)(IterationCount / 100.0f) * 50, 1, 50); i++)
        {
            foreach (Tank t in populationGOs)
            {
                // Get the nearest mine
                IMinable mine = GetNearestMine(t.transform.position);

                // Set the nearest mine to current tank
                t.SetNearestMine(mine);

                mine = GetNearestGoodMine(t.transform.position);

                // Set the nearest mine to current tank
                t.SetGoodNearestMine(mine);

                mine = GetNearestBadMine(t.transform.position);

                // Set the nearest mine to current tank
                t.SetBadNearestMine(mine);

                // Think!! 
                t.Think(dt);

                // Just adjust tank position when reaching world extents
                Vector3 pos = t.transform.position;
                if (pos.x > SceneHalfExtents.x)
                    pos.x -= SceneHalfExtents.x * 2;
                else if (pos.x < -SceneHalfExtents.x)
                    pos.x += SceneHalfExtents.x * 2;

                if (pos.z > SceneHalfExtents.z)
                    pos.z -= SceneHalfExtents.z * 2;
                else if (pos.z < -SceneHalfExtents.z)
                    pos.z += SceneHalfExtents.z * 2;

                // Set tank position
                t.transform.position = pos;
            }

            // Check the time to evolve
            accumTime += dt;
            if (accumTime >= GenerationDuration)
            {
                accumTime -= GenerationDuration;
                Epoch();
                break;
            }
        }
    }

    #region Helpers

    Tank CreateTank(Genome genome, Brain brain)
    {
        Vector3 position = GetRandomPos();
        GameObject go = Instantiate<GameObject>(TankPrefab, position, GetRandomRot());
        Tank t = go.GetComponent<Tank>();
        t.SetBrain(genome, brain);
        return t;
    }

    void DestroyMines()
    {
        foreach (IMinable go in mines)
            Destroy(go.GetGameObject());

        mines.Clear();
        goodMines.Clear();
        badMines.Clear();
    }

    void DestroyTanks()
    {
        foreach (Tank go in populationGOs)
            Destroy(go.gameObject);

        populationGOs.Clear();
        population.Clear();
        brains.Clear();
    }

    void CreateMines()
    {
        // Destroy previous created mines
        DestroyMines();

        for (int i = 0; i < MinesCount; i++)
        {
            Vector3 position = GetRandomPos();
            GameObject go = Instantiate(MinePrefab, position, Quaternion.identity);

            bool good = Random.Range(-1.0f, 1.0f) >= 0;
            IMinable currentMine = go.GetComponent<IMinable>();
            SetMineGood(good, currentMine);

            mines.Add(currentMine);
        }
    }

    void SetMineGood(bool good, IMinable go)
    {
        if (good)
        {
            //go.GetComponent<Renderer>().material.color = Color.green;
            go.SetMine(true);
            goodMines.Add(go);
        }
        else
        {
            //go.GetComponent<Renderer>().material.color = Color.red;
            go.SetMine(false);
            badMines.Add(go);
        }
    }

    public void RelocateMine(IMinable mine)
    {
        if (goodMines.Contains(mine))
            goodMines.Remove(mine);
        else
            badMines.Remove(mine);

        bool good = Random.Range(-1.0f, 1.0f) >= 0;

        SetMineGood(good, mine);

        mine.SetPosition(GetRandomPos());
    }


    Vector3 GetRandomPos()
    {
        return new Vector3(Random.value * SceneHalfExtents.x * 2.0f - SceneHalfExtents.x, 0.0f,
            Random.value * SceneHalfExtents.z * 2.0f - SceneHalfExtents.z);
    }

    Quaternion GetRandomRot()
    {
        return Quaternion.AngleAxis(Random.value * 360.0f, Vector3.up);
    }

    IMinable GetNearestMine(Vector3 pos)
    {
        IMinable nearest = mines[0];
        float distance = (pos - nearest.GetPosition()).sqrMagnitude;

        foreach (IMinable go in mines)
        {
            float newDist = (go.GetPosition() - pos).sqrMagnitude;
            if (newDist < distance)
            {
                nearest = go;
                distance = newDist;
            }
        }

        return nearest;
    }

    IMinable GetNearestGoodMine(Vector3 pos)
    {
        IMinable nearest = mines[0];
        float distance = (pos - nearest.GetPosition()).sqrMagnitude;

        foreach (IMinable go in goodMines)
        {
            float newDist = (go.GetPosition() - pos).sqrMagnitude;
            if (newDist < distance)
            {
                nearest = go;
                distance = newDist;
            }
        }

        return nearest;
    }

    IMinable GetNearestBadMine(Vector3 pos)
    {
        IMinable nearest = mines[0];
        float distance = (pos - nearest.GetPosition()).sqrMagnitude;

        foreach (IMinable go in badMines)
        {
            float newDist = (go.GetPosition() - pos).sqrMagnitude;
            if (newDist < distance)
            {
                nearest = go;
                distance = newDist;
            }
        }

        return nearest;
    }

    #endregion
}