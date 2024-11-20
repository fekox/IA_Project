using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Genome
{
    public float[] genome;
    public float fitness = 0;

    public Genome(float[] genes)
    {
        this.genome = genes;
        fitness = 0;
    }

    public Genome(int genesCount)
    {
        genome = new float[genesCount];

        for (int j = 0; j < genesCount; j++)
            genome[j] = Random.Range(-1.0f, 1.0f);

        fitness = 0;
    }

    public Genome()
    {
        fitness = 0;
    }
}

[System.Serializable]
public class GeneticAlgorithm
{
    public List<Genome> population = new List<Genome>();
    List<Genome> newPopulation = new List<Genome>();
    public Brain brain;
    public bool shouldEvolve = true;
    float totalFitness;

    int eliteCount = 0;
    float mutationChance = 0.0f;
    float mutationRate = 0.0f;
    private int newNeuronToAddQuantity;
    private int randomLayer = 0;
    private int neuronPositionToAdd = 0;
    int neuronPos = 0;
    private List<NeuronLayer> neuronLayers;

    public GeneticAlgorithm(int eliteCount, float mutationChance, float mutationRate, Brain brain)
    {
        this.eliteCount = eliteCount;
        this.mutationChance = mutationChance;
        this.mutationRate = mutationRate;
        this.brain = brain;
    }

    public Genome[] GetRandomGenomes(int count, int genesCount)
    {
        Genome[] genomes = new Genome[count];

        for (int i = 0; i < count; i++)
        {
            genomes[i] = new Genome(genesCount);
        }

        return genomes;
    }


    public Genome[] Epoch(Genome[] oldGenomes)
    {
        totalFitness = 0;

        population.Clear();
        newPopulation.Clear();

        population.AddRange(oldGenomes);
        population.Sort(HandleComparison);

        foreach (Genome g in population)
        {
            totalFitness += g.fitness;
        }


        CalculateNeuronsToAdd();


        SelectElite(shouldEvolve);
        while (newPopulation.Count < population.Count)
        {
            Crossover(brain, randomLayer, shouldEvolve);
        }

        // brain.layers[randomLayer].inputsCount++;


        brain.AddNeuronLayerAtPosition(newNeuronToAddQuantity, randomLayer);


        return newPopulation.ToArray();
    }

    private void CalculateNeuronsToAdd()
    {
        newNeuronToAddQuantity = Random.Range(1, 3);
        randomLayer = Random.Range(1, brain.layers.Count - 1);
        neuronLayers = brain.layers;
        neuronPositionToAdd = Random.Range(0, neuronLayers[randomLayer].NeuronsCount);
        neuronPos = 0;

        for (int i = 0; i < neuronLayers.Count; i++)
        {
            if (i < randomLayer)
            {
                neuronPos += neuronLayers[i].NeuronsCount;
            }
            else if (i == randomLayer)
            {
                int neuronCount = 0;
                while (neuronCount < neuronPositionToAdd)
                {
                    neuronCount++;
                }

                neuronPos += neuronCount;
            }
        }
    }


    void SelectElite(bool shouldEvolve)
    {
        for (int i = 0; i < eliteCount && newPopulation.Count < population.Count; i++)
        {
            if (shouldEvolve)
            {
                EvolveChildLayer(population[i]);
            }

            newPopulation.Add(population[i]);
        }
    }

    void Crossover(Brain brain, int layer, bool shouldEvolve)
    {
        Genome mom = RouletteSelection();
        Genome dad = RouletteSelection();

        Genome child1;
        Genome child2;

        Crossover(brain, layer, shouldEvolve, mom, dad, out child1, out child2);

        newPopulation.Add(child1);
        newPopulation.Add(child2);
    }

    void Crossover(Brain brainStructure, int layer, bool shouldEvolve, Genome mom, Genome dad, out Genome child1,
        out Genome child2)
    {
        child1 = new Genome();
        child2 = new Genome();

        child1.genome = new float[mom.genome.Length];
        child2.genome = new float[mom.genome.Length];

        int pivot = Random.Range(0, mom.genome.Length);

        for (int i = 0; i < pivot; i++)
        {
            child1.genome[i] = mom.genome[i];

            if (ShouldMutate())
                child1.genome[i] += Random.Range(-mutationRate, mutationRate);

            child2.genome[i] = dad.genome[i];

            if (ShouldMutate())
                child2.genome[i] += Random.Range(-mutationRate, mutationRate);
        }


        for (int i = pivot; i < mom.genome.Length; i++)
        {
            child2.genome[i] = mom.genome[i];

            if (ShouldMutate())
                child2.genome[i] += Random.Range(-mutationRate, mutationRate);

            child1.genome[i] = dad.genome[i];

            if (ShouldMutate())
                child1.genome[i] += Random.Range(-mutationRate, mutationRate);
        }

        if (shouldEvolve)
        {
            EvolveChildLayer(child1);
            EvolveChildLayer(child2);
        }
    }

    bool ShouldMutate()
    {
        return Random.Range(0.0f, 1.0f) < mutationChance;
    }

    int HandleComparison(Genome x, Genome y)
    {
        return x.fitness > y.fitness ? 1 : x.fitness < y.fitness ? -1 : 0;
    }

    void EvolveChild(Genome child)
    {
        int newNeuronCount = child.genome.Length + newNeuronToAddQuantity * neuronLayers[randomLayer].InputsCount +
                             neuronLayers[randomLayer + 1].InputsCount * newNeuronToAddQuantity;
        float[] newWeight = new float[newNeuronCount];


        //Neurona

        int count = 0;
        int originalWeightsCount = 0;
        int originalNeuronsCount = 0;

        int previousLayerInputs = neuronLayers[randomLayer].inputsCount;
        int afterLayerInputs = neuronLayers[randomLayer + 1].inputsCount;
        int afterLayerCounter = 0;
        bool hasCreatedNewConections = false;

        while (count < newNeuronCount)
        {
            if (count < neuronPos)
            {
                CopyExistingWeights(ref count, ref originalWeightsCount);
            }
            else if (count >= neuronPos && count < neuronPos + newNeuronToAddQuantity)
            {
                CreateNewWeights(ref count);
            }
            else if (!hasCreatedNewConections)
            {
                HandleNewConnections(ref count, ref originalWeightsCount, ref originalNeuronsCount,
                    ref afterLayerCounter, ref hasCreatedNewConections, previousLayerInputs, afterLayerInputs,
                    newNeuronToAddQuantity);
            }
            else
            {
                CopyExistingWeights(ref count, ref originalWeightsCount);
            }

            child.genome = newWeight;
        }


        //float[]weight = new float[weightCount+ neuronq * neurons[random].InputsCount + neurons[random+1].InputsCount ];


        //Layer
        void CopyExistingWeights(ref int count, ref int originalWeightsCount)
        {
            newWeight[count] = child.genome[originalWeightsCount];
            originalWeightsCount++;
            count++;
        }

        void CreateNewWeights(ref int count)
        {
            newWeight[count] = Random.Range(-1.0f, 1.0f);
            count++;
        }

        void HandleNewConnections(ref int count, ref int originalWeightsCount, ref int originalNeuronsCount,
            ref int afterLayerCounter, ref bool hasCreatedNewConections, int previousLayerInputs, int afterLayerInputs,
            int newNeuronToAddQuantity
        )
        {
            if (afterLayerCounter < afterLayerInputs)
            {
                if (originalNeuronsCount < previousLayerInputs)
                {
                    newWeight[count] = child.genome[originalWeightsCount];
                    originalWeightsCount++;
                    originalNeuronsCount++;
                }
                else if (originalNeuronsCount < previousLayerInputs + newNeuronToAddQuantity)
                {
                    newWeight[count] = Random.Range(-1.0f, 1.0f);
                    originalNeuronsCount = 0;
                    afterLayerCounter++;
                }
            }
            else
            {
                hasCreatedNewConections = true;
            }

            count++;
        }
    }

    void EvolveChildLayer(Genome child)
    {
        //Neurona

        int count = 0;
        int originalWeightsCount = 0;


        int previousLayerInputs = neuronLayers[randomLayer].OutputsCount;
        int nextLayerInputs = neuronLayers[randomLayer + 1].OutputsCount;

        int oldConections = ((previousLayerInputs + 1) * nextLayerInputs);
        int newTotalWeight = child.genome.Length - oldConections + ((previousLayerInputs + 1) * newNeuronToAddQuantity) + (newNeuronToAddQuantity+1) * nextLayerInputs;
        
        
        Debug.Log($"The weight of the new array is {newTotalWeight}");
        float[] newWeight = new float[newTotalWeight];


        int weightsBeforeInsertion = 0;

        for (int layerIndex = 0; layerIndex < randomLayer; layerIndex++)
        {
            weightsBeforeInsertion += neuronLayers[layerIndex].inputsCount * neuronLayers[layerIndex + 1].inputsCount;
        }


        while (count < weightsBeforeInsertion)
        {
            CopyExistingWeights(ref count, ref originalWeightsCount);
        }

        int previousLayerInputCounter = 0;


      
        for (int i = 0; i < previousLayerInputs; i++)
        {
            for (int j = 0; j < newNeuronToAddQuantity; j++)
            {
                CreateNewWeights(ref count);
            }
        }

        
        for (int i = 0; i < newNeuronToAddQuantity; i++)
        {
            for (int j = 0; j < nextLayerInputs; j++)
            {
                CreateNewWeights(ref count);
            }
        }

        while (count < newTotalWeight)
        {
            CopyExistingWeights(ref count, ref originalWeightsCount);
        }


        child.genome = newWeight;

        //Layer
        void CopyExistingWeights(ref int count, ref int originalWeightsCount)
        {
            newWeight[count] = child.genome[originalWeightsCount];
            originalWeightsCount++;
            count++;
        }

        void CreateNewWeights(ref int count)
        {
            newWeight[count] = Random.Range(-1.0f, 1.0f);
            count++;
        }
    }

    public Genome RouletteSelection()
    {
        float rnd = Random.Range(0, Mathf.Max(totalFitness, 0));

        float fitness = 0;

        for (int i = 0; i < population.Count; i++)
        {
            fitness += Mathf.Max(population[i].fitness, 0);
            if (fitness >= rnd)
                return population[i];
        }

        return null;
    }
}