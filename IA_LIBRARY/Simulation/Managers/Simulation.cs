using System;
using System.Collections.Generic;
using System.Linq;
using IA_Library_ECS;
using IA_Library_FSM;
using IA_Library.Brain;
using Random = System.Random;
using Vector2 = System.Numerics.Vector2;

namespace IA_Library
{
    /// <summary>
    /// Init and Update the simulation, create entities and brains for the entities.
    /// </summary>
    [Serializable]
    public class Simulation
    {
        public GridManager gridManager;
        private int currentGeneration = 0;

        private int totalHerbivores;
        private int totalCarnivores;
        private int totalScavengers;
        private int totalPlants;
        private int totalElite;

        private float mutationChance;
        private float mutationRate;

        private int generationLifeTime;
        private int currentTurn = 0;

        public float averageHerbivoreFitness = 0;
        public float averageCarnivoreFitness = 0;
        public float averageScavengerFitness = 0;
        public int finalHerbivoreAlive = 0;
        public int finalCarnivoreAlive = 0;
        public int finalScavengerAlive = 0;
        public event Action<int, float, int, float, int, float> OnFitnessCalculated;

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
        private List<Brain.Brain> scavengerMainBrain = new List<Brain.Brain>();
        private List<Brain.Brain> scavengerFlockingBrain = new List<Brain.Brain>();

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

        public string fileToLoad;
        public string fileExtension;
        public string filepath;

        public List<GeneticData> data = new List<GeneticData>();
        Random random = new Random();

        public bool isActive;
        private Dictionary<uint, Brain.Brain> entities;
        private SaveDataSystem manager = new SaveDataSystem();

        /// <summary>
        /// Inint the simulation
        /// </summary>
        /// <param name="grid">The map</param>
        /// <param name="herbivoreData">Herbivore brain data</param>
        /// <param name="carnivoreData">Carnivore brain data</param>
        /// <param name="scavengerData">Scavenger brain data</param>
        /// <param name="totalHerbivores">Max herbivores</param>
        /// <param name="totalCarnivores">Max carnivores</param>
        /// <param name="totalScavengers">Max scavengers</param>
        /// <param name="totalPlants">Max plants</param>
        /// <param name="totalElite">Max elites</param>
        /// <param name="mutationChance">The mutation chance</param>
        /// <param name="mutationRate">The mutation rate</param>
        /// <param name="generationLifeTime">Time of the generation</param>
        public Simulation(GridManager grid,
            List<BrainData> herbivoreData, List<BrainData> carnivoreData, List<BrainData> scavengerData,
            int totalHerbivores, int totalCarnivores, int totalScavengers, int totalPlants, int totalElite,
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
            this.totalPlants = totalPlants;

            this.totalElite = totalElite;
            this.mutationChance = mutationChance;
            this.mutationRate = mutationRate;

            ECSManager.Init();
            CreateEntities();
            entities = new Dictionary<uint, Brain.Brain>();
            CreateECSEntities();
            CreateNewGeneration();
        }

        /// <summary>
        /// Create the entities, create the genetic data, add the entities in a respective list and add the data in to a manager.
        /// </summary>
        private void CreateEntities()
        {
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
                
                scavengerMainBrain.Add(Scavenger[i].mainBrain);
                scavengerFlockingBrain.Add(Scavenger[i].flockingBrain);
            }

            for (int i = 0; i < totalPlants; i++)
            {
                Plants.Add(new AgentPlant(this, gridManager));
            }

            HeMainBrain = new GeneticData(totalElite, mutationChance, mutationRate, herbivoreMainBrain[0]);
            HeEatBrain = new GeneticData(totalElite, mutationChance, mutationRate, herbivoreEatBrain[0]);
            HeMoveFoodBrain = new GeneticData(totalElite, mutationChance, mutationRate, herbivoreMoveFoodBrain[0]);
            HeMoveEscapeBrain = new GeneticData(totalElite, mutationChance, mutationRate, herbivoreMoveEscapeBrain[0]);
            CaMainBrain = new GeneticData(totalElite, mutationChance, mutationRate, carnivoreMainBrain[0]);
            CaEatBrain = new GeneticData(totalElite, mutationChance, mutationRate, carnivoreEatBrain[0]);
            CaMoveBrain = new GeneticData(totalElite, mutationChance, mutationRate, carnivoreMoveBrain[0]);
            ScaMainBrain = new GeneticData(totalElite, mutationChance, mutationRate, scavengerMainBrain[0]);
            ScaFlockingBrain = new GeneticData(totalElite, mutationChance, mutationRate, scavengerFlockingBrain[0]);
            
            data.Add(HeMainBrain);
            data.Add(HeMoveEscapeBrain);
            data.Add(HeMoveFoodBrain);
            data.Add(HeEatBrain);
            
            data.Add(CaMainBrain);
            data.Add(CaMoveBrain);
            data.Add(CaEatBrain);
            
            data.Add(ScaMainBrain);
            data.Add(ScaFlockingBrain);

            foreach (GeneticData geneticData in data)
            {
                manager.AddDataset(geneticData);
            }
        }

        /// <summary>
        /// Crete the ESC for the respective entity and respective brain.
        /// </summary>
        private void CreateECSEntities()
        {
            for (int i = 0; i < totalHerbivores; i++)
            {
                CreateEntity(Herbivore[i].mainBrain);
                CreateEntity(Herbivore[i].moveToFoodBrain);
                CreateEntity(Herbivore[i].moveToEscapeBrain);
                CreateEntity(Herbivore[i].eatBrain);
            }

            for (int i = 0; i < totalCarnivores; i++)
            {
                CreateEntity(Carnivore[i].mainBrain);
                CreateEntity(Carnivore[i].moveToFoodBrain);
                CreateEntity(Carnivore[i].eatBrain);
            }

            for (int i = 0; i < totalScavengers; i++)
            {
                CreateEntity(Scavenger[i].mainBrain);
                CreateEntity(Scavenger[i].flockingBrain);
            }
        }

        /// <summary>
        /// Add the components of the entity in the respective brain.
        /// </summary>
        /// <param name="brain">Entity brain</param>
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

        /// <summary>
        /// Update the simulation, sets the brains, updates the inputs, update the ECS manager, make the epoch and create a new generation.
        /// </summary>
        /// <param name="deltaTime">Delta time</param>
        /// <returns>The current generation</returns>
        public int UpdateSimulation(float deltaTime)
        {
            if (currentTurn < generationLifeTime)
            {
                SettingBrain(deltaTime);
                UpdateInputs();
                ECSManager.Tick(deltaTime);
                UpdateOutputs(deltaTime);
                currentTurn++;
            }

            else
            {
                Epoch();
                CreateNewGeneration();
            }

            return currentGeneration;
        }

        /// <summary>
        /// Make the epoch for the entities and create a file to save the data.
        /// </summary>
        private void Epoch()
        {
            foreach (GeneticData geneticData in data)
            {
                geneticData.generationCount = currentGeneration;
            }
            
            EpochHerbivore();
            EpochCarnivore();
            EpochScavenger();

            OnFitnessCalculated?.Invoke(finalHerbivoreAlive, averageHerbivoreFitness,
                finalCarnivoreAlive, averageCarnivoreFitness,
                finalScavengerAlive, averageScavengerFitness);

            string file = $"{filepath}{currentGeneration}.{fileExtension}";
            manager.SaveAll(file);
            
            foreach (KeyValuePair<uint, Brain.Brain> entity in entities)
            {
                HiddenLayerComponent inputComponent = ECSManager.GetComponent<HiddenLayerComponent>(entity.Key);
                inputComponent.hiddenLayers = entity.Value.GetHiddenLayers();
                OutputLayerComponent outputComponent = ECSManager.GetComponent<OutputLayerComponent>(entity.Key);
                outputComponent.layer = entity.Value.GetOutputLayer();
            }

            averageHerbivoreFitness = 0;
            averageCarnivoreFitness = 0;
            averageScavengerFitness = 0;
        }

        /// <summary>
        /// Epoch for the herbivore.
        /// </summary>
        private void EpochHerbivore()
        {
            int count = 0;
            float totalFitness = 0f;

            foreach (AgentHerbivore current in Herbivore)
            {
                current.ApplyFitness();

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

                totalFitness += current.mainBrain.fitness;
            }

            finalHerbivoreAlive = count;

            bool isGenerationDead = (count <= 1);

            averageHerbivoreFitness = count < 1 ? 0f : totalFitness / Herbivore.Count;

            EpochSingle(herbivoreMainBrain, isGenerationDead, HeMainBrain);
            EpochSingle(herbivoreMoveFoodBrain, isGenerationDead, HeMoveFoodBrain);
            EpochSingle(herbivoreMoveEscapeBrain, isGenerationDead, HeMoveEscapeBrain);
            EpochSingle(herbivoreEatBrain, isGenerationDead, HeEatBrain);
        }

        /// <summary>
        /// Epoch for the carnivore.
        /// </summary>
        private void EpochCarnivore()
        {
            int count = 0;
            float totalFitness = 0f;

            foreach (AgentCarnivore current in Carnivore)
            {
                current.ApplyFitness();

                if (current.hasEaten)
                {
                    count++;
                    totalFitness += current.mainBrain.fitness;
                }
            
                else
                {
                    current.mainBrain.Set0Fitness();
                    current.eatBrain.Set0Fitness();
                    current.moveToFoodBrain.Set0Fitness();
                }
            }

            finalCarnivoreAlive = count;
            bool isGenerationDead = (count <= 1);
            averageCarnivoreFitness = isGenerationDead ? 0f : totalFitness;

            EpochSingle(carnivoreMainBrain, isGenerationDead, CaMainBrain);
            EpochSingle(carnivoreMoveBrain, isGenerationDead, CaMoveBrain);
            EpochSingle(carnivoreEatBrain, isGenerationDead, CaEatBrain);
        }

        /// <summary>
        /// Epoch for the scavenger.
        /// </summary>
        private void EpochScavenger()
        {
            int count = 0;
            float totalFitness = 0f;

            foreach (AgentScavenger current in Scavenger)
            {
                current.ApplyFitness();

                if (current.hasEaten)
                {
                    count++;
                    totalFitness += current.mainBrain.fitness;
                }
                
                else
                {
                    current.mainBrain.Set0Fitness();
                    current.flockingBrain.Set0Fitness();
                }
            }

            finalScavengerAlive = count;
            bool isGenerationDead = count <= 1;
            averageScavengerFitness = isGenerationDead ? 0f : totalFitness;

            EpochSingle(scavengerMainBrain, isGenerationDead, ScaMainBrain);
            EpochSingle(scavengerFlockingBrain, isGenerationDead, ScaFlockingBrain);
        }

        /// <summary>
        /// Single epoch for one brain in especific.
        /// </summary>
        /// <param name="brains">The brain</param>
        /// <param name="force">To force evolution</param>
        /// <param name="data">Data to save</param>
        private void EpochSingle(List<Brain.Brain> brains, bool force, GeneticData data)
        {
            Genome[] newGenomes = GeneticAlgorithm.Epoch(GetGenomes(brains), data, force);
            data.lastGenome = newGenomes;

            for (int i = 0; i < brains.Count; i++)
            {
                brains[i] = new Brain.Brain(data.brainStructure);
                brains[i].SetWeights(newGenomes[i].genome);
            }
        }
        
        /// <summary>
        /// Restore the save data.
        /// </summary>
        private void RestoreSave()
        {
            List<GeneticData> dataToPaste = manager.GetAllDatasets();
            
            HeMainBrain = dataToPaste[0];
            HeEatBrain = dataToPaste[1];
            HeMoveEscapeBrain = dataToPaste[2];
            HeMoveFoodBrain = dataToPaste[3];
            CaMainBrain = dataToPaste[4];
            CaEatBrain = dataToPaste[5];
            CaMoveBrain = dataToPaste[6];
            ScaMainBrain = dataToPaste[7];
            ScaFlockingBrain = dataToPaste[8];
            
            manager.ClearDatasets();
            manager.AddDataset(HeMainBrain);
            manager.AddDataset(HeEatBrain);
            manager.AddDataset(HeMoveEscapeBrain);
            manager.AddDataset(HeMoveFoodBrain);
            manager.AddDataset(CaMainBrain);
            manager.AddDataset(CaEatBrain);
            manager.AddDataset(CaMoveBrain);
            manager.AddDataset(ScaMainBrain);
            manager.AddDataset(ScaFlockingBrain);
            
            
            currentGeneration = HeMainBrain.generationCount;
            
            RestoreBrainsData(herbivoreMainBrain, HeMainBrain);
            RestoreBrainsData(herbivoreEatBrain, HeEatBrain);
            RestoreBrainsData(herbivoreMoveEscapeBrain, HeMoveEscapeBrain);
            RestoreBrainsData(herbivoreMoveFoodBrain, HeMoveFoodBrain);
            RestoreBrainsData(carnivoreMainBrain, CaMainBrain);
            RestoreBrainsData(carnivoreEatBrain, CaEatBrain);
            RestoreBrainsData(carnivoreMoveBrain, CaMoveBrain);
            RestoreBrainsData(scavengerMainBrain, ScaMainBrain);
            RestoreBrainsData(scavengerFlockingBrain, ScaFlockingBrain);
            ResetPositions();
            
        }

        /// <summary>
        /// Restore the brain save data.
        /// </summary>
        /// <param name="brains"></param>
        /// <param name="info"></param>
        private void RestoreBrainsData(List<Brain.Brain> brains, GeneticData info)
        {
            for (int i = 0; i < brains.Count; i++)
            {
                int counter = i;

                if (counter >= info.lastGenome.Length)
                {
                    counter -= info.lastGenome.Length;
                }

                brains[counter] = new Brain.Brain(info.brainStructure);
                brains[counter].SetWeights(info.lastGenome[counter].genome);
            }
        }

        /// <summary>
        /// Gets the genomes in one brain.
        /// </summary>
        /// <param name="brains">The brain</param>
        /// <returns>Genomes array</returns>
        private static Genome[] GetGenomes(List<Brain.Brain> brains)
        {
            List<Genome> genomes = new List<Genome>();
            
            foreach (var brain in brains)
            {
                Genome genome = new Genome(brain.GetWeights());
                genome.fitness = brain.fitness;
                genomes.Add(genome);
            }

            return genomes.ToArray();
        }

        /// <summary>
        /// Create a new generation, resets the turn and add 1 to the current generation.
        /// </summary>
        private void CreateNewGeneration()
        {
            currentGeneration++;
            ResetPositions();

            currentTurn = 0;
        }

        /// <summary>
        /// Reset the position of the entities.
        /// </summary>
        private void ResetPositions()
        {
            foreach (var herb in Herbivore)
            {
                herb.Reset();
            }

            foreach (var carn in Carnivore)
            {
                carn.Reset();
            }

            foreach (var scav in Scavenger)
            {
                scav.Reset();
            }
            
            foreach (var plant in Plants)
            {
                plant.Reset();
            }
        }

        /// <summary>
        /// Sets the brains of the entities.
        /// </summary>
        /// <param name="deltaTime">The time</param>
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

        /// <summary>
        /// Update the inputs for the ECS in respective entity.
        /// </summary>
        private void UpdateInputs()
        {
            foreach (KeyValuePair<uint, Brain.Brain> entity in entities)
            {
                InputComponent inputComponent = ECSManager.GetComponent<InputComponent>(entity.Key);
                inputComponent.inputs = entity.Value.inputs;
            }
        }

        /// <summary>
        /// Update the outputs of the entities.
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateOutputs(float deltaTime)
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

        /// <summary>
        /// Load the data of the entities. 
        /// </summary>
        public void Load()
        {
            manager.LoadAll(fileToLoad);
            RestoreSave();
            isActive = false;
            
            foreach (var entity in entities)
            {
                ECSManager.GetComponent<BiasComponent>(entity.Key).X = entity.Value.bias;
                ECSManager.GetComponent<SigmoidComponent>(entity.Key).X = entity.Value.p;
                ECSManager.GetComponent<InputLayerComponent>(entity.Key).layer = entity.Value.GetInputLayer();
                HiddenLayerComponent hiddenLayerComponent = ECSManager.GetComponent<HiddenLayerComponent>(entity.Key);
                hiddenLayerComponent.hiddenLayers = entity.Value.GetHiddenLayers();
                hiddenLayerComponent.SetHighestLayerSize();
                ECSManager.GetComponent<OutputLayerComponent>(entity.Key).layer = entity.Value.GetOutputLayer();
                ECSManager.GetComponent<OutputComponent>(entity.Key).output = entity.Value.outputs;
                ECSManager.GetComponent<InputComponent>(entity.Key).inputs = entity.Value.inputs;
            }

            currentTurn = 0;
        }

        #region Getters

        /// <summary>
        /// Gets the nearest plant agent.
        /// </summary>
        /// <param name="position">Position of the agent</param>
        /// <returns>Nearest position</returns>
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

        /// <summary>
        /// Gets the position of the nearest plant agent.
        /// </summary>
        /// <param name="position">Position of the agent</param>
        /// <returns>The position</returns>
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

        /// <summary>
        /// Gets the nearest carnivore position.
        /// </summary>
        /// <param name="position">Position of the agent</param>
        /// <param name="count">Count of the carnivores</param>
        /// <returns>The near carnivores.</returns>
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

        /// <summary>
        /// Get the neares herbivore agent.
        /// </summary>
        /// <param name="position">The position of the agent</param>
        /// <returns>The near herbivore</returns>
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

        /// <summary>
        /// Gets the position of the near herbivore.
        /// </summary>
        /// <param name="position">The agent position</param>
        /// <returns>The near herbivore position</returns>
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

        /// <summary>
        /// Returns the dead herbivore position.
        /// </summary>
        /// <param name="position">The agent position</param>
        /// <returns>The near dead herbivore position</returns>
        public Vector2? GetNearestDeadHerbivorePosition(Vector2 position)
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

            return nearestPoint?.position;
        }

        /// <summary>
        /// Returns the near scavengers.
        /// </summary>
        /// <param name="position">The agent position</param>
        /// <param name="count">The number of scavengers</param>
        /// <returns>The nearest scavengers</returns>
        public List<AgentScavenger> GetNearestScavengers(Vector2 position, int count)
        {
            List<AgentScavenger> sortedScavengers = Scavenger
                .OrderBy(h => (h.position.X - position.X) * (h.position.X - position.X) +
                              (h.position.Y - position.Y) * (h.position.Y - position.Y))
                .Take(Math.Min(count, Carnivore.Count))
                .ToList();

            return sortedScavengers;
        }

        #endregion
    }
}