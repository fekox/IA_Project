using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using IA_Library_ECS;
using IA_Library_FSM;
using IA_Library.Brain;

namespace IA_Library
{
    public class Simulation
    {
        public GridManager gridManager;
        private int currentGeneration = 0;

        private int totalHerbivores;
        private int totalCarnivores;
        private int totalScavengers;
        private int totalElite;

        private float mutationChance;
        private float mutationRate;

        private int generationLifeTime;
        private int currentTurn = 0;

        //Agents
        public List<AgentHerbivore> Herbivore = new List<AgentHerbivore>();
        public List<AgentCarnivore> Carnivore = new List<AgentCarnivore>();
        public List<AgentScavenger> Scavenger = new List<AgentScavenger>();
        public List<AgentPlant> Plants = new List<AgentPlant>();

        //Herbivore Brains
        private List<Brain.Brain> herbivoreMainBrain = new List<Brain.Brain>();
        private List<Brain.Brain> herbivoreMoveFoodBrain = new List<Brain.Brain>();
        private List<Brain.Brain> herbivoreMoveEscapeBrain = new List<Brain.Brain>();
        private List<Brain.Brain> herbivoreEatBrain = new List<Brain.Brain>();

        //Carnivore Brains
        private List<Brain.Brain> carnivoreMainBrain = new List<Brain.Brain>();
        private List<Brain.Brain> carnivoreMoveBrain = new List<Brain.Brain>();
        private List<Brain.Brain> carnivoreEatBrain = new List<Brain.Brain>();

        //Scavenger Brains
        private List<Brain.Brain> ScavengerMainBrain = new List<Brain.Brain>();
        private List<Brain.Brain> ScavengerFlockingBrain = new List<Brain.Brain>();

        //Save Brain Data
        private List<BrainData> herbivoreData;
        private List<BrainData> carnivoreData;
        private List<BrainData> scavengerData;

        //Genetics Algorithm
        private GeneticData HeMainBrain;
        private GeneticData HeMoveFoodBrain;
        private GeneticData HeMoveEscapeBrain;
        private GeneticData HeEatBrain;
        private GeneticData CaMainBrain;
        private GeneticData CaMoveBrain;
        private GeneticData CaEatBrain;
        private GeneticData ScaMainBrain;
        private GeneticData ScaFlockingBrain;

        private bool isActive;
        private Dictionary<uint, Brain.Brain> entities;
        private Dictionary<List<Brain.Brain>, GeneticData> geneticInfo;

        public Simulation(GridManager grid,
            List<BrainData> herbivoreData, List<BrainData> carnivoreData, List<BrainData> scavengerData,
            int totalHerbivores, int totalCarnivores, int totalScavengers, int totalElite,
            float mutationChance, float mutationRate, int generationLifeTime)
        {
            //Data
            this.herbivoreData = herbivoreData;
            this.carnivoreData = carnivoreData;
            this.scavengerData = scavengerData;

            //Settings
            gridManager = grid;
            this.generationLifeTime = generationLifeTime;

            this.totalHerbivores = totalHerbivores;
            this.totalCarnivores = totalCarnivores;
            this.totalScavengers = totalScavengers;
            this.totalElite = totalElite;
            this.mutationChance = mutationChance;
            this.mutationRate = mutationRate;

            ECSManager.Init();
            CreateEntities();
            entities = new Dictionary<uint, Brain.Brain>();
            CreateECSEntities();
            CreateNewGeneration();
        }

        private void CreateEntities()
        {
            for (int i = 0; i < totalHerbivores * 2; i++)
            {
                Plants.Add(new AgentPlant(this, gridManager));
            }

            for (int i = 0; i < totalHerbivores; i++)
            {
                Herbivore.Add(new AgentHerbivore(this, gridManager, herbivoreData[0].CreateBrain(),
                    herbivoreData[1].CreateBrain(), herbivoreData[2].CreateBrain(), herbivoreData[3].CreateBrain()));

                herbivoreMainBrain.Add(Herbivore[i].mainBrain);
                herbivoreMoveFoodBrain.Add(Herbivore[i].moveToFoodBrain);
                herbivoreMoveEscapeBrain.Add(Herbivore[i].moveToEscapeBrain);
                herbivoreEatBrain.Add(Herbivore[i].eatBrain);
            }

            for (int i = 0; i < totalCarnivores; i++)
            {
                Carnivore.Add(new AgentCarnivore(this, gridManager, carnivoreData[0].CreateBrain(),
                    carnivoreData[1].CreateBrain(), carnivoreData[2].CreateBrain()));
                carnivoreMainBrain.Add(Carnivore[i].mainBrain);
                carnivoreMoveBrain.Add(Carnivore[i].moveToFoodBrain);
                carnivoreEatBrain.Add(Carnivore[i].eatBrain);
            }

            for (int i = 0; i < totalScavengers; i++)
            {
                Scavenger.Add(new AgentScavenger(this, gridManager, scavengerData[0].CreateBrain(),
                    scavengerData[1].CreateBrain()));
                ScavengerMainBrain.Add(Scavenger[i].mainBrain);
                ScavengerFlockingBrain.Add(Scavenger[i].flockingBrain);
            }

            HeMainBrain = new GeneticData(totalElite, mutationChance, mutationRate, herbivoreMainBrain[0]);
            HeEatBrain = new GeneticData(totalElite, mutationChance, mutationRate, herbivoreEatBrain[0]);
            HeMoveFoodBrain = new GeneticData(totalElite, mutationChance, mutationRate, herbivoreMoveFoodBrain[0]);
            HeMoveEscapeBrain = new GeneticData(totalElite, mutationChance, mutationRate, herbivoreMoveEscapeBrain[0]);
            CaMainBrain = new GeneticData(totalElite, mutationChance, mutationRate, carnivoreMainBrain[0]);
            CaEatBrain = new GeneticData(totalElite, mutationChance, mutationRate, carnivoreEatBrain[0]);
            CaMoveBrain = new GeneticData(totalElite, mutationChance, mutationRate, carnivoreMoveBrain[0]);
            ScaMainBrain = new GeneticData(totalElite, mutationChance, mutationRate, ScavengerMainBrain[0]);
            ScaFlockingBrain = new GeneticData(totalElite, mutationChance, mutationRate, ScavengerFlockingBrain[0]);
        }

        private void CreateECSEntities()
        {
            for (int i = 0; i < totalHerbivores; i++)
            {
                CreateEntity(Herbivore[i].mainBrain);
                //CreateEntity(Herbivore[i].moveToFoodBrain);
                //CreateEntity(Herbivore[i].moveToEscapeBrain);
                //CreateEntity(Herbivore[i].eatBrain);
            }

            for (int i = 0; i < totalCarnivores; i++)
            {
                // CreateEntity(Carnivore[i].mainBrain);
                //(Carnivore[i].moveToFoodBrain);
                //CreateEntity(Carnivore[i].eatBrain);
            }

            for (int i = 0; i < totalScavengers; i++)
            {
                //CreateEntity(Scavenger[i].mainBrain);
                //CreateEntity(Scavenger[i].flockingBrain);
            }
        }

        private void CreateEntity(Brain.Brain brain)
        {
            uint entityID = ECSManager.CreateEntity();
            ECSManager.AddComponent<InputLayerComponent>(entityID, new InputLayerComponent(brain.GetInputLayer()));
            ECSManager.AddComponent<HiddenLayerComponent>(entityID, new HiddenLayerComponent(brain.GetHiddenLayers()));
            ECSManager.AddComponent<OutputLayerComponent>(entityID, new OutputLayerComponent(brain.GetOutputLayer()));

            ECSManager.AddComponent<OutputComponent>(entityID, new OutputComponent(brain.outputs));
            ECSManager.AddComponent<InputComponent>(entityID, new InputComponent(brain.inputs, brain.InputsCount));

            ECSManager.AddComponent<BiasComponent>(entityID, new BiasComponent(brain.bias));
            ECSManager.AddComponent<SigmoidComponent>(entityID, new SigmoidComponent(brain.p));
            entities.Add(entityID, brain);
        }

        public void UpdateSimulation(float deltaTime)
        {
            if (currentTurn < generationLifeTime)
            {
                SettingBrain(deltaTime);
                UpdateInputs();
                ECSManager.Tick(deltaTime);
                UpdateOutputs();
                currentTurn++;
            }
            else
            {
                Epoch();
                CreateNewGeneration();
            }
        }

        private void Epoch()
        {
            EpochHerbivore();
            EpochCarnivore();
            EpochScavenger();

            foreach (KeyValuePair<uint, Brain.Brain> entity in entities)
            {
                HiddenLayerComponent inputComponent = ECSManager.GetComponent<HiddenLayerComponent>(entity.Key);
                inputComponent.hiddenLayers = entity.Value.GetHiddenLayers();
                OutputLayerComponent outputComponent = ECSManager.GetComponent<OutputLayerComponent>(entity.Key);
                outputComponent.layer = entity.Value.GetOutputLayer();
            }
        }

        private void EpochHerbivore()
        {
            int count = 0;
            foreach (AgentHerbivore current in Herbivore)
            {
                if (current.lives > 0 && current.hasEaten)
                {
                    count++;
                }
                else
                {
                    current.mainBrain.Set0Fitness();
                    current.eatBrain.Set0Fitness();
                    current.moveToFoodBrain.Set0Fitness();
                    current.moveToEscapeBrain.Set0Fitness();
                }
            }

            bool isGenerationDead = (count <= 1);

            EpochLocal(herbivoreMainBrain, isGenerationDead, HeMainBrain);
            EpochLocal(herbivoreMoveFoodBrain, isGenerationDead, HeMoveFoodBrain);
            EpochLocal(herbivoreMoveEscapeBrain, isGenerationDead, HeMoveEscapeBrain);
            EpochLocal(herbivoreEatBrain, isGenerationDead, HeEatBrain);
        }

        private void EpochCarnivore()
        {
            int count = 0;
            foreach (AgentCarnivore current in Carnivore)
            {
                if (current.hasEaten)
                {
                    count++;
                }
                else
                {
                    current.mainBrain.Set0Fitness();
                    current.eatBrain.Set0Fitness();
                    current.moveToFoodBrain.Set0Fitness();
                }
            }

            bool isGenerationDead = count <= 1;

            EpochLocal(carnivoreMainBrain, isGenerationDead, CaMainBrain);
            EpochLocal(carnivoreMoveBrain, isGenerationDead, CaMoveBrain);
            EpochLocal(carnivoreEatBrain, isGenerationDead, CaEatBrain);
        }

        private void EpochScavenger()
        {
            int count = 0;
            foreach (AgentHerbivore current in Herbivore)
            {
                if (current.hasEaten)
                {
                    count++;
                }
            }

            bool isGenerationDead = count <= 1;

            EpochLocal(ScavengerMainBrain, isGenerationDead, ScaMainBrain);
            EpochLocal(ScavengerFlockingBrain, isGenerationDead, ScaFlockingBrain);
        }

        private void EpochLocal(List<Brain.Brain> brains, bool force, GeneticData data)
        {
            Genome[] newGenomes = GeneticAlgorithm.Epoch(GetGenomes(brains), geneticInfo[brains], force);
            data.lastGenome = newGenomes;

            for (int i = 0; i < brains.Count; i++)
            {
                brains[i] = new Brain.Brain(data.brainStructure);
                brains[i].SetWeights(newGenomes[i].genome);
            }
        }

        private static Genome[] GetGenomes(List<Brain.Brain> brains)
        {
            List<Genome> genomes = new List<Genome>();
            foreach (var brain in brains)
            {
                Genome genome = new Genome(brain.GetTotalWeightsCount());
                genomes.Add(genome);
            }

            return genomes.ToArray();
        }

        private void CreateNewGeneration()
        {
            currentGeneration++;
            foreach (AgentHerbivore agent in Herbivore)
            {
                agent.Reset();
            }

            foreach (AgentCarnivore agent in Carnivore)
            {
                agent.Reset();
            }

            foreach (AgentScavenger agent in Scavenger)
            {
                agent.Reset();
            }

            foreach (AgentPlant agent in Plants)
            {
                agent.Reset();
            }

            currentTurn = 0;
        }

        public void SettingBrain(float deltaTime)
        {
            foreach (AgentHerbivore current in Herbivore)
            {
                current.SettingBrainUpdate(deltaTime);
            }

            foreach (AgentCarnivore current in Carnivore)
            {
                current.SettingBrainUpdate(deltaTime);
            }

            foreach (AgentScavenger current in Scavenger)
            {
                current.SettingBrainUpdate(deltaTime);
            }
        }

        private void UpdateInputs()
        {
            foreach (KeyValuePair<uint, Brain.Brain> entity in entities)
            {
                InputComponent inputComponent = ECSManager.GetComponent<InputComponent>(entity.Key);
                inputComponent.inputs = entity.Value.inputs;
            }
        }

        public void UpdateOutputs(float deltaTime = 0)
        {
            foreach (KeyValuePair<uint, Brain.Brain> entity in entities)
            {
                OutputComponent output = ECSManager.GetComponent<OutputComponent>(entity.Key);
                entity.Value.outputs = output.output;
            }

            foreach (AgentHerbivore current in Herbivore)
            {
                current.Update(deltaTime);
            }

            foreach (AgentCarnivore current in Carnivore)
            {
                current.Update(deltaTime);
            }

            foreach (AgentScavenger current in Scavenger)
            {
                current.Update(deltaTime);
            }
        }

        public AgentPlant GetNearestPlantAgents(Vector2 position)
        {
            AgentPlant nearestPoint = Plants[0];
            float minDistanceSquared = (Plants[0].position.X - position.X) * (Plants[0].position.X - position.X) +
                                       (Plants[0].position.Y - position.Y) * (Plants[0].position.Y - position.Y);

            foreach (AgentPlant point in Plants)
            {
                float distanceSquared = (point.position.X - position.X) * (point.position.X - position.X) +
                                        (point.position.Y - position.Y) * (point.position.Y - position.Y);
                if (distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    nearestPoint = point;
                }
            }

            return nearestPoint;
        }

        public Vector2 GetNearestPlantPosition(Vector2 position)
        {
            AgentPlant nearestPoint = Plants[0];
            float minDistanceSquared = (Plants[0].position.X - position.X) * (Plants[0].position.X - position.X) +
                                       (Plants[0].position.Y - position.Y) * (Plants[0].position.Y - position.Y);

            foreach (AgentPlant point in Plants)
            {
                float distanceSquared = (point.position.X - position.X) * (point.position.X - position.X) +
                                        (point.position.Y - position.Y) * (point.position.Y - position.Y);
                if (distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    nearestPoint = point;
                }
            }

            return nearestPoint.position;
        }

        public List<Vector2> GetNearestCarnivoresPositions(Vector2 position, int count)
        {
            var sortedCarnivores = Carnivore
                .OrderBy(h => (h.position.X - position.X) * (h.position.X - position.X) +
                              (h.position.Y - position.Y) * (h.position.Y - position.Y))
                .Select(h => h.position)
                .Take(Math.Min(count, Carnivore.Count))
                .ToList();

            return sortedCarnivores;
        }

        public AgentHerbivore GetNearestHerbivoreAgent(Vector2 position)
        {
            AgentHerbivore nearestPoint = Herbivore[0];
            float minDistanceSquared = (Herbivore[0].position.X - position.X) * (Herbivore[0].position.X - position.X) +
                                       (Herbivore[0].position.Y - position.Y) * (Herbivore[0].position.Y - position.Y);

            foreach (var point in Herbivore)
            {
                float distanceSquared = (point.position.X - position.X) * (point.position.X - position.X) +
                                        (point.position.Y - position.Y) * (point.position.Y - position.Y);
                if (distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    nearestPoint = point;
                }
            }

            return nearestPoint;
        }

        public Vector2 GetNearestHerbivorePosition(Vector2 position)
        {
            AgentHerbivore nearestPoint = Herbivore[0];
            float minDistanceSquared = (Herbivore[0].position.X - position.X) * (Herbivore[0].position.X - position.X) +
                                       (Herbivore[0].position.Y - position.Y) * (Herbivore[0].position.Y - position.Y);

            foreach (var point in Herbivore)
            {
                float distanceSquared = (point.position.X - position.X) * (point.position.X - position.X) +
                                        (point.position.Y - position.Y) * (point.position.Y - position.Y);
                if (distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    nearestPoint = point;
                }
            }

            return nearestPoint.position;
        }

        public Vector2 GetNearestDeadHerbivorePosition(Vector2 position)
        {
            AgentHerbivore nearestPoint = null;
            float minDistanceSquared = float.MaxValue;

            foreach (var point in Herbivore)
            {
                if (!point.CanBeEaten())
                    continue;

                float distanceSquared = (point.position.X - position.X) * (point.position.X - position.X) +
                                        (point.position.Y - position.Y) * (point.position.Y - position.Y);

                if (distanceSquared < minDistanceSquared)
                {
                    minDistanceSquared = distanceSquared;
                    nearestPoint = point;
                }
            }

            return nearestPoint.position;
        }

        public List<AgentScavenger> GetNearestScavengers(Vector2 position, int count)
        {
            List<AgentScavenger> sortedScavengers = Scavenger
                .OrderBy(h => (h.position.X - position.X) * (h.position.X - position.X) +
                              (h.position.Y - position.Y) * (h.position.Y - position.Y))
                .Take(Math.Min(count, Carnivore.Count))
                .ToList();

            return sortedScavengers;
        }
    }
}