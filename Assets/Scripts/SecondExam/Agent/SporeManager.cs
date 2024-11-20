using System.Collections.Generic;
using RojoinSaveSystem;
using RojoinSaveSystem.Attributes;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Miner.SecondExam.Agent
{
    public class SporeManager : ISaveObject
    {
        SaveObjectData saveObject = new SaveObjectData();
        [SaveValue(0)] public int gridSizeX = 10;
        [SaveValue(1)] public int gridSizeY = 10;
        [SaveValue(3)] public int hervivoreCount = 30;
        [SaveValue(4)] public int carnivoreCount = 20;
        [SaveValue(5)] public int scavengerCount = 20;
        [SaveValue(7)] public int EliteCount = 4;
        [SaveValue(8)] public float MutationChance = 0.10f;
        [SaveValue(9)] public float MutationRate = 0.01f;
        public int turnCount = 100;
        private int currentTurn = 0;

        private List<Herbivore> herbis = new List<Herbivore>();
        private List<Plant> plants = new List<Plant>();
        private List<Carnivore> carnivores = new List<Carnivore>();
        private List<Scavenger> scavengers = new List<Scavenger>();

        private List<Brain> herbMainBrains = new List<Brain>();
        private List<Brain> herbEatBrains = new List<Brain>();
        private List<Brain> herbMoveBrains = new List<Brain>();
        private List<Brain> herbEscapeBrains = new List<Brain>();
        private List<Brain> carnMainBrains = new List<Brain>();
        private List<Brain> carnMoveBrains = new List<Brain>();
        private List<Brain> carnEatBrains = new List<Brain>();
        private List<Brain> scavMainBrains = new List<Brain>();
        private List<Brain> scavFlokingBrains = new List<Brain>();
        private bool isActive;
        private Dictionary<uint, Brain> entities;

        public SporeManager()
        {
            CreateAgents();
            ECSManager.Init();
            entities = new Dictionary<uint, Brain>();
            InitEntities();
        }

        public void Tick(float deltaTime)
        {
            if (!isActive)
                return;
            if (currentTurn < turnCount)
            {
                PreUpdateAgents(deltaTime);
                UpdateInputs();
                ECSManager.Tick(deltaTime);
                AfterTick(deltaTime);
                currentTurn++;
            }
            else
            {
                EpochAllBrains();
                isActive = false;
                CreateNewGeneration();
            }
        }

        private void CreateNewGeneration()
        {
            
            throw new System.NotImplementedException();
        }

        private void InitEntities()
        {
            for (int i = 0; i < hervivoreCount; i++)
            {
                CreateEntity(herbis[i].mainBrain);
                CreateEntity(herbis[i].moveBrain);
                CreateEntity(herbis[i].eatBrain);
                CreateEntity(herbis[i].escapeBrain);
            }

            for (int i = 0; i < carnivoreCount; i++)
            {
                CreateEntity(carnivores[i].mainBrain);
                CreateEntity(carnivores[i].moveBrain);
                CreateEntity(carnivores[i].eatBrain);
            }

            for (int i = 0; i < scavengerCount; i++)
            {
                CreateEntity(scavengers[i].mainBrain);
                CreateEntity(scavengers[i].flockingBrain);
            }
        }

        private void CreateAgents()
        {
            for (int i = 0; i < hervivoreCount; i++)
            {
                herbis.Add(new Herbivore(this));
                herbMainBrains.Add(herbis[i].mainBrain);
                herbEatBrains.Add(herbis[i].eatBrain);
                herbEscapeBrains.Add(herbis[i].escapeBrain);
                herbMoveBrains.Add(herbis[i].moveBrain);
            }

            for (int i = 0; i < carnivoreCount; i++)
            {
                carnivores.Add(new Carnivore(this));
                carnMainBrains.Add(carnivores[i].mainBrain);
                carnEatBrains.Add(carnivores[i].eatBrain);
                carnMoveBrains.Add(carnivores[i].moveBrain);
            }

            for (int i = 0; i < scavengerCount; i++)
            {
                scavengers.Add(new Scavenger(this));
                scavMainBrains.Add(scavengers[i].mainBrain);
                scavFlokingBrains.Add(scavengers[i].flockingBrain);
            }
        }

        private void CreateEntity(Brain brain)
        {
            uint entityID = ECSManager.CreateEntity();
            ECSManager.AddComponent<BiasComponent>(entityID, new BiasComponent(brain.bias));
            ECSManager.AddComponent<SigmoidComponent>(entityID, new SigmoidComponent(brain.p));
            ECSManager.AddComponent<InputLayerComponent>(entityID, new InputLayerComponent(brain.GetInputLayer()));
            ECSManager.AddComponent<HiddenLayerComponent>(entityID, new HiddenLayerComponent(brain.GetHiddenLayers()));
            ECSManager.AddComponent<OutputLayerComponent>(entityID, new OutputLayerComponent(brain.GetOutputLayer()));
            ECSManager.AddComponent<OutputComponent>(entityID, new OutputComponent(brain.outputs));
            ECSManager.AddComponent<InputComponent>(entityID, new InputComponent(brain.inputs));
            entities.Add(entityID, brain);
        }

        #region Epoch

        private void EpochAllBrains()
        {
            EpochHerbivore();
            EpochCarnivore();
            EpochScavenger();

            foreach (KeyValuePair<uint, Brain> entity in entities)
            {
                HiddenLayerComponent inputComponent = ECSManager.GetComponent<HiddenLayerComponent>(entity.Key);
                inputComponent.hiddenLayers = entity.Value.GetHiddenLayers();
            }
        }

        private void EpochScavenger()
        {
            List<Brain> scavMainBrain = new List<Brain>();
            List<Brain> scavFlockingBrain = new List<Brain>();
            foreach (var scav in scavengers)
            {
                if (scav.hasEaten)
                {
                    scavMainBrain.Add(scav.mainBrain);
                    scavFlockingBrain.Add(scav.flockingBrain);
                }
            }

            EpochLocal(scavMainBrain);
            EpochLocal(scavFlockingBrain);
        }

        void EpochCarnivore()
        {
            List<Brain> carnivoreMainBrain = new List<Brain>();
            List<Brain> carnivoreEatBrain = new List<Brain>();
            List<Brain> carnivoreMoveBrain = new List<Brain>();
            foreach (var carnivore in carnivores)
            {
                if (carnivore.hasEatenEnoughFood)
                {
                    carnivoreMainBrain.Add(carnivore.mainBrain);
                    carnivoreEatBrain.Add(carnivore.eatBrain);
                    carnivoreMoveBrain.Add(carnivore.moveBrain);
                }
            }

            EpochLocal(carnivoreMainBrain);
            EpochLocal(carnivoreEatBrain);
            EpochLocal(carnivoreMoveBrain);
        }

        private void EpochHerbivore()
        {
            List<Brain> herbivoresMainBrain = new List<Brain>();
            List<Brain> herbivoresEscapeBrain = new List<Brain>();
            List<Brain> herbivoresMoveBrain = new List<Brain>();
            List<Brain> herbivoresEatBrain = new List<Brain>();
            foreach (Herbivore herbivore in herbis)
            {
                if (herbivore.lives > 0 && herbivore.hasEatenFood)
                {
                    herbivoresMainBrain.Add(herbivore.mainBrain);
                    herbivoresEatBrain.Add(herbivore.eatBrain);
                    herbivoresMoveBrain.Add(herbivore.moveBrain);
                    herbivoresEscapeBrain.Add(herbivore.escapeBrain);
                }
            }

            EpochLocal(herbivoresMainBrain);
            EpochLocal(herbivoresMoveBrain);
            EpochLocal(herbivoresEatBrain);
            EpochLocal(herbivoresEscapeBrain);
        }

        private void EpochLocal(List<Brain> brains)
        {
            Genome[] newGenomes = this.Epoch(GetGenomes(brains));

            for (int i = 0; i < brains.Count; i++)
            {
                Brain brain = brains[i];
                brain.SetWeights(newGenomes[i].genome);
            }
        }

        private Genome[] GetGenomes(List<Brain> brains)
        {
            List<Genome> genomes = new List<Genome>();
            foreach (var brain in brains)
            {
                Genome genome = new Genome(brain.GetTotalWeightsCount());

                brain.SetWeights(genome.genome);
                brains.Add(brain);

                genomes.Add(genome);
            }

            return genomes.ToArray();
        }


        public Genome[] Epoch(Genome[] oldGenomes)
        {
            float totalFitness = 0;
            List<Genome> population = new List<Genome>();
            List<Genome> newPopulation = new List<Genome>();

            population.AddRange(oldGenomes);

            foreach (Genome g in population)
            {
                totalFitness += g.fitness;
            }

            int eliteCount = 5;
            for (int i = 0; i < eliteCount && newPopulation.Count < population.Count; i++)
            {
                newPopulation.Add(population[i]);
            }

            return newPopulation.ToArray();
        }

        public Genome RouletteSelection(float totalFitness, List<Genome> population)
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

        void Crossover(List<Genome> oldPopulation, List<Genome> newPopulation, float totalFitness, float mutationRate,
            float mutationChance)
        {
            Genome mom = RouletteSelection(totalFitness, oldPopulation);
            Genome dad = RouletteSelection(totalFitness, oldPopulation);

            Genome child1;
            Genome child2;

            Crossover(mom, dad, mutationChance, mutationRate, out child1, out child2);

            newPopulation.Add(child1);
            newPopulation.Add(child2);
        }

        void Crossover(Genome mom, Genome dad, float mutationChance, float mutationRate, out Genome child1,
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

                if (ShouldMutate(mutationChance))
                    child1.genome[i] += Random.Range(-mutationRate, mutationRate);

                child2.genome[i] = dad.genome[i];

                if (ShouldMutate(mutationChance))
                    child2.genome[i] += Random.Range(-mutationRate, mutationRate);
            }

            for (int i = pivot; i < mom.genome.Length; i++)
            {
                child2.genome[i] = mom.genome[i];

                if (ShouldMutate(mutationChance))
                    child2.genome[i] += Random.Range(-mutationRate, mutationRate);

                child1.genome[i] = dad.genome[i];

                if (ShouldMutate(mutationChance))
                    child1.genome[i] += Random.Range(-mutationRate, mutationRate);
            }
        }

        bool ShouldMutate(float mutationChance)
        {
            return Random.Range(0.0f, 1.0f) < mutationChance;
        }

        int HandleComparison(Genome x, Genome y)
        {
            return x.fitness > y.fitness ? 1 : x.fitness < y.fitness ? -1 : 0;
        }

        #endregion

        #region Updates

        private void PreUpdateAgents(float deltaTime)
        {
            foreach (Herbivore herbi in herbis)
            {
                herbi.PreUpdate(deltaTime);
            }

            foreach (Carnivore carn in carnivores)
            {
                carn.PreUpdate(deltaTime);
            }

            foreach (Scavenger scav in scavengers)
            {
                scav.PreUpdate(deltaTime);
            }
        }

        private void UpdateInputs()
        {
            foreach (KeyValuePair<uint, Brain> entity in entities)
            {
                InputComponent inputComponent = ECSManager.GetComponent<InputComponent>(entity.Key);
                inputComponent.inputs = entity.Value.inputs;
            }
        }

        public void AfterTick(float deltaTime = 0)
        {
            foreach (KeyValuePair<uint, Brain> entity in entities)
            {
                OutputComponent output = ECSManager.GetComponent<OutputComponent>(entity.Key);
                entity.Value.outputs = output.outputs;
            }

            foreach (Herbivore herbi in herbis)
            {
                herbi.Update(deltaTime);
            }

            foreach (Carnivore carn in carnivores)
            {
                carn.Update(deltaTime);
            }

            foreach (Scavenger scav in scavengers)
            {
                scav.Update(deltaTime);
            }
        }

        #endregion

        public int GetID()
        {
            return saveObject.id;
        }

        public ISaveObject GetObject()
        {
            return this;
        }

        public void Save()
        {
        }

        public Herbivore GetNearHerbivore(Vector2 position)
        {
            Herbivore nearest = herbis[0];
            float distance = (position.X * nearest.position.X) + (position.Y * nearest.position.Y);

            foreach (Herbivore go in herbis)
            {
                float newDist = (go.position.X * position.X) + (go.position.Y * position.Y);
                if (newDist < distance)
                {
                    nearest = go;
                    distance = newDist;
                }
            }

            return nearest;
        }

        public Plant GetNearPlant(Vector2 position)
        {
            Plant nearest = plants[0];
            float distance = (position.X * nearest.position.X) + (position.Y * nearest.position.Y);

            foreach (Plant go in plants)
            {
                if (go.isAvailable)
                {
                    float newDist = (go.position.X * position.X) + (go.position.Y * position.Y);
                    if (newDist < distance)
                    {
                        nearest = go;
                        distance = newDist;
                    }
                }
            }

            return nearest;
        }
    }
}