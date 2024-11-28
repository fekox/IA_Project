using System;
using System.Collections.Generic;
using IA_Library.Brain;

namespace IA_Library
{
    /// <summary>
    /// Create the genentic data of the brains.
    /// </summary>
    [Serializable]
    public class GeneticData
    {
        public float totalFitness = 0;
        public int eliteCount = 0;
        public float mutationChance = 0.0f;
        public float mutationRate = 0.0f;
        public readonly int maxStalledGenerationsUntilEvolve = 5;
        public Brain.Brain brainStructure;
        public Genome[] lastGenome = Array.Empty<Genome>();
        public int generationStalled = 0;
        public int generationCount = 0;

        /// <summary>
        /// Create a default genetic data.
        /// </summary>
        public GeneticData()
        {
            eliteCount = 5;
            mutationChance = 0.2f;
            mutationRate = 0.4f;
        }
        
        /// <summary>
        /// Create a custom genetic data.
        /// </summary>
        /// <param name="data">The data</param>
        /// <param name="offset">The star index to read the data</param>
        public GeneticData(byte[] data, ref int offset)
        {
            eliteCount = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);

            mutationChance = BitConverter.ToSingle(data, offset);
            offset += sizeof(float);

            mutationRate = BitConverter.ToSingle(data, offset);
            offset += sizeof(float);

            brainStructure = new Brain.Brain(data, ref offset);

            lastGenome = CreateGenomeArray(data, ref offset);

            generationStalled = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);

            generationCount = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
        }

        /// <summary>
        /// Serialize the data.
        /// </summary>
        /// <returns>The array of bytes</returns>
        public byte[] Serialize()
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(eliteCount));
            bytes.AddRange(BitConverter.GetBytes(mutationChance));
            bytes.AddRange(BitConverter.GetBytes(mutationRate));
            bytes.AddRange(brainStructure.Serialize());
            bytes.AddRange(SerializeGenomeArray(lastGenome));
            bytes.AddRange(BitConverter.GetBytes(generationStalled));
            bytes.AddRange(BitConverter.GetBytes(generationCount));

            return bytes.ToArray();
        }

        /// <summary>
        /// Create the genomes array.
        /// </summary>
        /// <param name="data">The data</param>
        /// <param name="currentOffset">The current index to read the data</param>
        /// <returns>The array of genomes</returns>
        private Genome[] CreateGenomeArray(byte[] data, ref int currentOffset)
        {
            int arrayLength = BitConverter.ToInt32(data, currentOffset);
            currentOffset += sizeof(int);
            Genome[] genomes = new Genome[arrayLength];

            for (int i = 0; i < arrayLength; i++)
            {
                genomes[i] = new Genome(data, ref currentOffset);
            }

            return genomes;
        }

        /// <summary>
        /// Serialize the genomes array.
        /// </summary>
        /// <param name="genomes">The genomes array</param>
        /// <returns>The array of bytes</returns>
        private byte[] SerializeGenomeArray(Genome[] genomes)
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(genomes.Length));

            foreach (var genome in genomes)
            {
                bytes.AddRange(genome.Serialize());
            }

            return bytes.ToArray();
        }

        /// <summary>
        /// Create the genetic data.
        /// </summary>
        /// <param name="eliteCount">The count of elites</param>
        /// <param name="mutationChance">The mutation chance</param>
        /// <param name="mutationRate">The mutation rate</param>
        /// <param name="brain">The brain</param>
        /// <param name="maxStalledGenerationsUntilEvolve">Limit generation to evolve</param>
        public GeneticData(int eliteCount, float mutationChance, float mutationRate, Brain.Brain brain,
            int maxStalledGenerationsUntilEvolve = 5)
        {
            this.eliteCount = eliteCount;
            this.mutationChance = mutationChance;
            this.mutationRate = mutationRate;
            brainStructure = brain;
            this.maxStalledGenerationsUntilEvolve = maxStalledGenerationsUntilEvolve;
        }

        /// <summary>
        /// Sets the genetic data.
        /// </summary>
        /// <param name="data"></param>
        public GeneticData(GeneticData data)
        {
            eliteCount = data.eliteCount;
            mutationChance = data.mutationChance;
            mutationRate = data.mutationRate;
            brainStructure = data.brainStructure;
            maxStalledGenerationsUntilEvolve = data.maxStalledGenerationsUntilEvolve;
        }

        public void Save()
        {
        }

        public void Load()
        {
        }
    }

    /// <summary>
    /// Manage and create the genetic algotithm.
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Get a random array of genomes
        /// </summary>
        /// <param name="count">Size of the array</param>
        /// <param name="genesCount">Genes counter</param>
        /// <returns>The genomes array</returns>
        public static Genome[] GetRandomGenomes(int count, int genesCount)
        {
            Genome[] genomes = new Genome[count];

            for (int i = 0; i < count; i++)
            {
                genomes[i] = new Genome(genesCount);
            }

            return genomes;
        }

        /// <summary>
        /// Returns a float random range.
        /// </summary>
        /// <param name="min">The min for the range</param>
        /// <param name="max">The max for the range</param>
        /// <returns>The float random range</returns>
        public static float RandomRangeFloat(float min, float max)
        {
            return (float)(random.NextDouble() * (max - min) + min);
        }

        /// <summary>
        /// Evolves the agents.
        /// </summary>
        /// <param name="oldGenomes">Old genomes</param>
        /// <param name="data">Data</param>
        /// <param name="forceEvolve">Bolean to force evolve or not</param>
        /// <returns>The evolved genome array</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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
                data.generationStalled = 0;
                data.mutationChance *= 2.8f;
                data.mutationRate *= 2.8f;
                evolutionType = (EvolutionType)random.Next(1, Enum.GetValues(typeof(EvolutionType)).Length);
            }

            else if (currentTotalFitness < data.totalFitness)
            {
                data.generationStalled++;

                if (data.generationStalled >= data.maxStalledGenerationsUntilEvolve)
                {
                    data.generationStalled = 0;
                    evolutionType = (EvolutionType)random.Next(1, Enum.GetValues(typeof(EvolutionType)).Length);
                }
            }

            data.totalFitness = currentTotalFitness;
            CalculateNeuronsToAdd(data.brainStructure);
            SelectElite(evolutionType, data.eliteCount);
            
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
            data.lastGenome = newPopulation.ToArray();
            
            return data.lastGenome;
        }

        /// <summary>
        /// Calculate the neurons layers to add in the brain.
        /// </summary>
        /// <param name="brain">The entity brain</param>
        private static void CalculateNeuronsToAdd(Brain.Brain brain)
        {
            newNeuronToAddQuantity = random.Next(1, 3);
            randomLayer = random.Next(1, brain.layers.Count - 1);
            neuronLayers = brain.layers;
        }

        /// <summary>
        /// Select the elite.
        /// </summary>
        /// <param name="evolutionType">Type of evolution</param>
        /// <param name="eliteCount">Elite counter</param>
        static void SelectElite(EvolutionType evolutionType, int eliteCount)
        {
            for (int i = 0; i < eliteCount && newPopulation.Count < population.Count; i++)
            {
                newPopulation.Add(population[i]);
            }
        }

        /// <summary>
        /// Crossover the genomes of the parents.
        /// </summary>
        /// <param name="data">Data of the entity</param>
        /// <param name="evolutionType">Type of evolution</param>
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

        /// <summary>
        /// Custom crossover for the genomes of the parents.
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="evolutionType">Evolution Type</param>
        /// <param name="mom">The genome of the mom</param>
        /// <param name="dad">The genome of the dad</param>
        /// <param name="child1">Fist child</param>
        /// <param name="child2">Second child</param>
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
                    child1.genome[i] += RandomRangeFloat(-data.mutationRate, data.mutationRate);

                child2.genome[i] = dad.genome[i];

                if (ShouldMutate(data.mutationChance))
                    child2.genome[i] += RandomRangeFloat(-data.mutationRate, data.mutationRate);
            }


            for (int i = pivot; i < mom.genome.Length; i++)
            {
                child2.genome[i] = mom.genome[i];

                if (ShouldMutate(data.mutationChance))
                    child2.genome[i] += RandomRangeFloat(-data.mutationRate, data.mutationRate);

                child1.genome[i] = dad.genome[i];

                if (ShouldMutate(data.mutationChance))
                    child1.genome[i] += RandomRangeFloat(-data.mutationRate, data.mutationRate);
            }
        }

        /// <summary>
        /// Check if the entity should mutate or not.
        /// </summary>
        /// <param name="mutationChance">Mutation chance</param>
        /// <returns>If the entity should mutate or not</returns>
        static bool ShouldMutate(float mutationChance)
        {
            return RandomRangeFloat(0.0f, 1.0f) < mutationChance;
        }

        /// <summary>
        /// Compare the genomes and return the one with the most fitness
        /// </summary>
        /// <param name="x">First genome</param>
        /// <param name="y">Second genome</param>
        /// <returns>The genome with the most fitnnes</returns>
        static int HandleComparison(Genome x, Genome y)
        {
            return x.fitness > y.fitness ? 1 : x.fitness < y.fitness ? -1 : 0;
        }

        /// <summary>
        /// Evolve the neurons of the children.
        /// </summary>
        /// <param name="child">Genome of the child</param>
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
                newWeight[count] = RandomRangeFloat(-1.0f, 1.0f);
                count++;
            }
        }

        /// <summary>
        /// Evolve the neuron layer of the child.
        /// </summary>
        /// <param name="child"></param>
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
                weightsBeforeInsertion += neuronLayers[layerIndex].GetWeightCount();
            }

            while (count < weightsBeforeInsertion)
            {
                CopyExistingWeights(ref count, ref originalWeightsCount);
            }

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
                newWeight[count] = RandomRangeFloat(-1.0f, 1.0f);
                count++;
            }
        }

        /// <summary>
        /// Select the population with the most fitness.
        /// </summary>
        /// <param name="totalFitness">Total fitnes</param>
        /// <returns>The population</returns>
        public static Genome RouletteSelection(float totalFitness)
        {
            float rnd = RandomRangeFloat(0, MathF.Max(totalFitness, 0));

            float fitness = 0;

            for (int i = 0; i < population.Count; i++)
            {
                fitness += MathF.Max(population[i].fitness, 0);

                if (fitness >= rnd)
                    return population[i];
            }

            return null;
        }
    }
}