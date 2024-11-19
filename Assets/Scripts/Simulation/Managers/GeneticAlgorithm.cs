using System;
using System.Collections.Generic;

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
    
    public class GeneticAlgorithm
    {
        private readonly List<Genome> newPopulation = new List<Genome>();
        private readonly List<Genome> population = new List<Genome>();

        private readonly int eliteCount;
        private readonly float mutationChance;
        private readonly float mutationRate;

        private float totalFitness;

        private static readonly Random random = new Random();

        public GeneticAlgorithm(int eliteCount, float mutationChance, float mutationRate)
        {
            this.eliteCount = eliteCount;
            this.mutationChance = mutationChance;
            this.mutationRate = mutationRate;
        }

        public Genome[] GetRandomGenomes(int count, int genesCount)
        {
            var genomes = new Genome[count];

            for (var i = 0; i < count; i++) genomes[i] = new Genome(genesCount);

            return genomes;
        }

        public Genome[] Epoch(Genome[] oldGenomes)
        {
            totalFitness = 0;

            population.Clear();
            newPopulation.Clear();

            population.AddRange(oldGenomes);
            population.Sort(HandleComparison);

            foreach (var g in population) totalFitness += g.fitness;

            SelectElite();

            while (newPopulation.Count < population.Count) Crossover();

            return newPopulation.ToArray();
        }

        private void SelectElite()
        {
            for (var i = 0; i < eliteCount && newPopulation.Count < population.Count; i++)
                newPopulation.Add(population[i]);
        }

        private void Crossover()
        {
            var mom = RouletteSelection();
            var dad = RouletteSelection();

            Genome child1;
            Genome child2;

            Crossover(mom, dad, out child1, out child2);

            newPopulation.Add(child1);
            newPopulation.Add(child2);
        }

        private void Crossover(Genome parent1, Genome parent2, out Genome child1, out Genome child2)
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

        private bool ShouldMutate()
        {
            return random.NextDouble() < mutationChance;
        }

        private static int HandleComparison(Genome x, Genome y)
        {
            return x.fitness > y.fitness ? 1 : x.fitness < y.fitness ? -1 : 0;
        }

        public Genome RouletteSelection()
        {
            var rnd = random.NextDouble() * Math.Max(totalFitness, 0);

            float fitness = 0;

            for (var i = 0; i < population.Count; i++)
            {
                fitness += Math.Max(population[i].fitness, 0);
                if (fitness >= rnd)
                    return population[i];
            }

            return null;
        }
    }
}