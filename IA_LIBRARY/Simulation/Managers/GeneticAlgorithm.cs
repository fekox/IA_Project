using System;
using System.Collections.Generic;
using IA_Library.Brain;

namespace IA_Library
{
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

            Random rand = new Random();

            for (int j = 0; j < genesCount; j++)
                genome[j] = (float)(rand.NextDouble() * 2 - 1);

            fitness = 0;
        }

        public Genome()
        {
            fitness = 0;
        }
    }

    public class GeneticData
    {
        public float totalFitness = 0;
        public float mutationChance = 0.0f;
        public float mutationRate = 0.0f;
        public int totalElites = 0;
        public Brain.Brain brainStructure;
        public readonly int stuckGenerationTimes = 6;
        public int stuckGenerationCounter = 0;

        public GeneticData()
        {
            totalElites = 5;
            mutationChance = 0.4f;
            mutationRate = 0.4f;
        }

        public GeneticData(int totalElites, float mutationChance, float mutationRate, Brain.Brain brain,
            int stuckGenerationTimes = 6)
        {
            this.totalElites = totalElites;
            this.mutationChance = mutationChance;
            this.mutationRate = mutationRate;
            this.brainStructure = brain;
            this.stuckGenerationTimes = stuckGenerationTimes;
        }

        public GeneticData(GeneticData data)
        {
            totalElites = data.totalElites;
            mutationChance = data.mutationChance;
            mutationRate = data.mutationRate;
            brainStructure = data.brainStructure;
            stuckGenerationTimes = data.stuckGenerationTimes;
        }
    }

    public class GeneticAlgorithm
    {
        enum EvolutionType
        {
            None = 0,
            AddNeurons,
            AddLayer
        }

        public static List<Genome> population = new List<Genome>();
        static List<Genome> newPopulation = new List<Genome>();

        private static int newNeuronToAddQuantity;
        private static int randomLayer = 0;
        private static List<NeuronLayer> neuronLayers;

        static Random random = new Random();

        public static Genome[] GetRandomGenomes(int count, int genesCount)
        {
            Genome[] genomes = new Genome[count];

            for (int i = 0; i < count; i++)
            {
                genomes[i] = new Genome(genesCount);
            }

            return genomes;
        }

        public static Genome[] Epoch(Genome[] oldGenomes, GeneticData data, bool forceEvolve = false)
        {
            float currentTotalFitness = 0;
            EvolutionType evolutionType = EvolutionType.None;

            population.Clear();
            newPopulation.Clear();

            population.AddRange(oldGenomes);
            population.Sort(HandleComparison);

            GeneticData backUpData = new GeneticData(data);
            
            foreach (Genome g in population)
            {
                currentTotalFitness += g.fitness;
            }

            if (forceEvolve)
            {
                data.stuckGenerationCounter = 0;
                data.mutationChance *= 1.2f;
                data.mutationRate *= 1.2f;
                evolutionType = (EvolutionType)random.Next(1, Enum.GetValues(typeof(EvolutionType)).Length);
            }

            else if (currentTotalFitness < data.totalFitness)
            {
                data.stuckGenerationCounter++;

                if (data.stuckGenerationCounter >= data.stuckGenerationTimes)
                {
                    data.stuckGenerationCounter = 0;
                    evolutionType = (EvolutionType)random.Next(1, Enum.GetValues(typeof(EvolutionType)).Length);
                }
            }

            SelectElite(evolutionType, data.totalElites);

            while (newPopulation.Count < population.Count)
            {
                Crossover(data, evolutionType);
            }

            switch (evolutionType)
            {
                case EvolutionType.None:
                    break;

                case EvolutionType.AddNeurons:
                    data.brainStructure.AddNeuronAtLayer(newNeuronToAddQuantity, randomLayer);
                    break;

                case EvolutionType.AddLayer:
                    data.brainStructure.AddNeuronLayerAtPosition(newNeuronToAddQuantity, randomLayer);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(evolutionType), evolutionType, null);
            }

            data.mutationChance = backUpData.mutationChance;
            data.mutationRate = backUpData.mutationRate;

            return newPopulation.ToArray();
        }

        static void SelectElite(EvolutionType evolutionType, int eliteCount)
        {
            for (int i = 0; i < eliteCount && newPopulation.Count < population.Count; i++)
            {
                switch (evolutionType)
                {
                    case EvolutionType.None:
                        break;

                    case EvolutionType.AddNeurons:
                        EvolveChildNeurons(population[i]);
                        break;

                    case EvolutionType.AddLayer:
                        EvolveChildLayer(population[i]);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(evolutionType), evolutionType, null);
                }

                newPopulation.Add(population[i]);
            }
        }

        public static void CalculateNeuronsToAdd(Brain.Brain brain)
        {
            newNeuronToAddQuantity = random.Next(1, 3);
            randomLayer = random.Next(1, brain.layers.Count - 1);
            neuronLayers = brain.layers;
        }

        static void Crossover(GeneticData data, EvolutionType evolutionType)
        {
            Genome mom = RouletteSelection(data.totalFitness);
            Genome dad = RouletteSelection(data.totalFitness);

            Genome child1;
            Genome child2;

            Crossover(data, evolutionType, mom, dad, out child1, out child2);

            newPopulation.Add(child1);
            newPopulation.Add(child2);
        }

        static void Crossover(GeneticData data, EvolutionType evolutionType, Genome mom, Genome dad,
            out Genome child1,
            out Genome child2)
        {
            child1 = new Genome();
            child2 = new Genome();

            child1.genome = new float[mom.genome.Length];
            child2.genome = new float[mom.genome.Length];

            int pivot = random.Next(0, mom.genome.Length);

            for (int i = 0; i < pivot; i++)
            {
                child1.genome[i] = mom.genome[i];

                if (ShouldMutate(data.mutationChance))
                    child1.genome[i] += GetRandomMutation(data.mutationRate);

                child2.genome[i] = dad.genome[i];

                if (ShouldMutate(data.mutationChance))
                    child2.genome[i] += GetRandomMutation(data.mutationRate);
            }


            for (int i = pivot; i < mom.genome.Length; i++)
            {
                child2.genome[i] = mom.genome[i];

                if (ShouldMutate(data.mutationChance))
                    child2.genome[i] += GetRandomMutation(data.mutationRate);

                child1.genome[i] = dad.genome[i];

                if (ShouldMutate(data.mutationChance))
                    child1.genome[i] += GetRandomMutation(data.mutationRate);
            }

            switch (evolutionType)
            {
                case EvolutionType.None:
                    break;

                case EvolutionType.AddNeurons:
                    EvolveChildNeurons(child1);
                    EvolveChildNeurons(child2);
                    break;

                case EvolutionType.AddLayer:
                    EvolveChildLayer(child1);
                    EvolveChildLayer(child2);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(evolutionType), evolutionType, null);
            }

            float GetRandomMutation(float mutationRate)
            {
                return (float)(random.NextDouble() * 2 * mutationRate - mutationRate);
            }
        }

        public static Genome RouletteSelection(float totalFitness)
        {
            float rnd = (float)(random.NextDouble() * Math.Max(totalFitness, 0));

            float fitness = 0;

            for (int i = 0; i < population.Count; i++)
            {
                fitness += Math.Max(population[i].fitness, 0);
                
                if (fitness >= rnd)
                    return population[i];
            }

            return null;
        }

        static bool ShouldMutate(float mutationChance)
        {
            return random.NextDouble() < mutationChance;
        }

        static int HandleComparison(Genome x, Genome y)
        {
            return x.fitness > y.fitness ? 1 : x.fitness < y.fitness ? -1 : 0;
        }

        static void EvolveChildNeurons(Genome child)
        {
            int previousLayerOutputs = neuronLayers[randomLayer].OutputsCount;
            int nextLayerOutputs = neuronLayers[randomLayer + 1].OutputsCount;

            int newNeuronCount = child.genome.Length
                                 + newNeuronToAddQuantity * neuronLayers[randomLayer].InputsCount +
                                 nextLayerOutputs * newNeuronToAddQuantity;

            float[] newWeight = new float[newNeuronCount];

            int count = 0;
            int originalWeightsCount = 0;


            for (int i = 0; i < randomLayer; i++)
            {
                for (int w = 0; w < neuronLayers[i].totalWeights; w++)
                {
                    CopyExistingWeights(ref count, ref originalWeightsCount);
                }
            }


            for (int i = 0; i < neuronLayers[randomLayer].InputsCount; i++)
            {
                for (int j = 0; j < previousLayerOutputs + newNeuronToAddQuantity; j++)
                {
                    if (j < previousLayerOutputs)
                    {
                        CopyExistingWeights(ref count, ref originalWeightsCount);
                    }

                    else
                    {
                        CreateNewWeights(ref count);
                    }
                }
            }

            for (int i = 0; i < previousLayerOutputs + newNeuronToAddQuantity; i++)
            {
                for (int j = 0; j < nextLayerOutputs; j++)
                {
                    if (i < previousLayerOutputs)
                    {
                        CopyExistingWeights(ref count, ref originalWeightsCount);
                    }

                    else
                    {
                        CreateNewWeights(ref count);
                    }
                }
            }

            while (count < newNeuronCount)
            {
                CopyExistingWeights(ref count, ref originalWeightsCount);
            }

            child.genome = newWeight;

            return;


            void CopyExistingWeights(ref int count, ref int originalWeightsCount)
            {
                newWeight[count] = child.genome[originalWeightsCount];
                
                originalWeightsCount++;
                count++;
            }

            void CreateNewWeights(ref int count)
            {
                newWeight[count] = (float)(random.NextDouble() * 2 - 1);
                count++;
            }
        }

        static void EvolveChildLayer(Genome child)
        {
            int count = 0;
            int originalWeightsCount = 0;

            int previousLayerInputs = neuronLayers[randomLayer].OutputsCount;

            int nextLayerInputs = neuronLayers[randomLayer + 1].OutputsCount;

            int oldConections = ((previousLayerInputs) * nextLayerInputs);

            int newTotalWeight = child.genome.Length - oldConections +
                                 (previousLayerInputs * newNeuronToAddQuantity) +
                                 (newNeuronToAddQuantity) * nextLayerInputs;

            float[] newWeight = new float[newTotalWeight];


            int weightsBeforeInsertion = 0;

            for (int layerIndex = 0; layerIndex < randomLayer; layerIndex++)
            {
                weightsBeforeInsertion += neuronLayers[layerIndex].totalWeights;
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

            return;


            void CopyExistingWeights(ref int count, ref int originalWeightsCount)
            {
                newWeight[count] = child.genome[originalWeightsCount];
                originalWeightsCount++;
                count++;
            }

            void CreateNewWeights(ref int count)
            {
                newWeight[count] = (float)(random.NextDouble() * 2 - 1);
                count++;
            }
        }
    }
}