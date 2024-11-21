using System;
using System.Collections.Generic;
using System.Numerics;
using IA_Library;
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

    public class GeneticAlgorithmData
    {

        public float totalFitness = 0;
        public int eliteCount = 0;
        public float mutationChance = 0.0f;
        public float mutationRate = 0.0f;
        public IA_Library.Brain.Brain brainStructure;
        public readonly int maxStalledGenerationsUntilEvolve = 5;
        public int generationStalled = 0;
        public Genome[] lastGenome;
        public int generationCount = 0;
        public GeneticAlgorithmData()
        {
            eliteCount = 5;
            float mutationChance = 0.2f;
            float mutationRate = 0.4f;
        }
        public GeneticAlgorithmData(int eliteCount, float mutationChance, float mutationRate, IA_Library.Brain.Brain brain,
            int maxStalledGenerationsUntilEvolve = 5)
        {
            this.eliteCount = eliteCount;
            this.mutationChance = mutationChance;
            this.mutationRate = mutationRate;
            this.brainStructure = brain;
            this.maxStalledGenerationsUntilEvolve = maxStalledGenerationsUntilEvolve;
        }

        public GeneticAlgorithmData(GeneticAlgorithmData data)
        {
            this.eliteCount = data.eliteCount;
            this.mutationChance = data.mutationChance;
            this.mutationRate = data.mutationRate;
            this.brainStructure = data.brainStructure;
            this.maxStalledGenerationsUntilEvolve = data.maxStalledGenerationsUntilEvolve;
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

        static List<Genome> newPopulation = new List<Genome>();
        public static List<Genome> population = new List<Genome>();


        private static int newNeuronToAddQuantity;
        private static int randomLayer = 0;
        private static List<NeuronLayer> neuronLayers;

        private static readonly Random random = new Random();

        public Genome[] GetRandomGenomes(int count, int genesCount)
        {
            var genomes = new Genome[count];

            for (var i = 0; i < count; i++) 
            { 
                genomes[i] = new Genome(genesCount); 
            }

            return genomes;
        }

        public Genome[] Epoch(Genome[] oldGenomes, GeneticAlgorithmData data, bool forceEvolve = false)
        {
            float currentTotalFitness = 0;
            EvolutionType evolutionType = EvolutionType.None;

            population.Clear();
            newPopulation.Clear();

            population.AddRange(oldGenomes);
            population.Sort(HandleComparison);

            GeneticAlgorithmData backUpData = new(data);

            foreach (var g in population) 
            {
                currentTotalFitness += g.fitness; 
            }

            if (forceEvolve) 
            {
                data.generationStalled = 0;
                data.mutationChance *= 1.2f;
                data.mutationRate *= 1.2f;
                evolutionType = (EvolutionType)random.Next(1, Enum.GetValues(typeof(EvolutionType)).Length + 1);
            }

            else if (currentTotalFitness < data.totalFitness)
            {
                data.generationStalled++;

                if (data.generationStalled >= data.maxStalledGenerationsUntilEvolve)
                {
                    data.generationStalled = 0;
                    evolutionType = (EvolutionType)random.Next(1, Enum.GetValues(typeof(EvolutionType)).Length + 1);
                }
            }

            data.totalFitness = currentTotalFitness;
            CalculateNeuronsToAdd(data.brainStructure);

            SelectElite(evolutionType, data.eliteCount);

            while (newPopulation.Count < population.Count) 
            { 
                Crossover(data, evolutionType); 
            }

            data.mutationChance = backUpData.mutationChance;
            data.mutationRate = backUpData.mutationRate;
            return newPopulation.ToArray();
        }

        private static void CalculateNeuronsToAdd(IA_Library.Brain.Brain brain)
        {
            newNeuronToAddQuantity = random.Next(1, 4);
            randomLayer = random.Next(1, (brain.layers.Count + 1) - 1);
            neuronLayers = brain.layers;
        }

        static void SelectElite(EvolutionType evolutionType, int eliteCount)
        {
            for (var i = 0; i < eliteCount && newPopulation.Count < population.Count; i++) 
            {
                newPopulation.Add(population[i]);
            }
        }

        private void Crossover(GeneticAlgorithmData data, EvolutionType evolutionType)
        {
            Genome mom = RouletteSelection(data.totalFitness);
            Genome dad = RouletteSelection(data.totalFitness);

            Genome child1;
            Genome child2;

            Crossover(data, evolutionType, mom, dad, out child1, out child2);

            newPopulation.Add(child1);
            newPopulation.Add(child2);
        }

        private void Crossover(GeneticAlgorithmData data, EvolutionType evolutionType, Genome parent1, Genome parent2, out Genome child1, out Genome child2)
        {
            child1 = new Genome();
            child2 = new Genome();

            child1.genome = new float[parent1.genome.Length];
            child2.genome = new float[parent1.genome.Length];

            var selectionChance = 0.5f;

            for (var i = 0; i < parent1.genome.Length; i++)
            {
                if (random.NextDouble() < selectionChance)
                {
                    child1.genome[i] = parent1.genome[i];
                    child2.genome[i] = parent2.genome[i];
                }

                else
                {
                    child1.genome[i] = parent2.genome[i];
                    child2.genome[i] = parent1.genome[i];
                }
            }
        }

        private bool ShouldMutate(float mutationChance)
        {
            return random.NextDouble() < mutationChance;
        }

        private static int HandleComparison(Genome x, Genome y)
        {
            return x.fitness > y.fitness ? 1 : x.fitness < y.fitness ? -1 : 0;
        }

        public Genome RouletteSelection(float totalFitness)
        {
            var rnd = random.NextDouble() * totalFitness;

            float fitness = 0;

            for (var i = 0; i < population.Count; i++)
            {
                fitness += Math.Max(population[i].fitness, 0);
               
                if (fitness >= rnd) 
                {
                    return population[i];
                }
            }

            return null;
        }
    }
}