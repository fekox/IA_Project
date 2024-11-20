using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using IA_Library_ECS;
using IA_Library_FSM;

namespace IA_Library
{
    public enum HerbivoreStates
    {
        Alive,
        Death,
        Corpse,
    }

    public class Simulation
    {
        private GridManager gridManager;

        private int totalHervivores;
        private int totalCarnivores;
        private int totalScavengers;
        private int totalElite;

        private float mutationChance;
        private float mutationRate;

        private float generationLifeTime;

        private List<AgentHerbivore> Herbivore = new List<AgentHerbivore>();
        private List<AgentCarnivore> Carnivore = new List<AgentCarnivore>();
        private List<AgentScavenger> Scavenger = new List<AgentScavenger>();
        private List<AgentPlant> Plants = new List<AgentPlant>();

        private List<Brain.Brain> herbivoreMainBrain = new List<Brain.Brain>();
        private List<Brain.Brain> herbivoreMoveFoodBrain = new List<Brain.Brain>();
        private List<Brain.Brain> herbivoreMoveEscapeBrain = new List<Brain.Brain>();
        private List<Brain.Brain> herbivoreEatBrain = new List<Brain.Brain>();

        private List<Brain.Brain> carnivoreMainBrain = new List<Brain.Brain>();
        private List<Brain.Brain> carnivoreMoveBrain = new List<Brain.Brain>();
        private List<Brain.Brain> carnivoreEatBrain = new List<Brain.Brain>();

        private List<Brain.Brain> ScavengerMainBrain = new List<Brain.Brain>();
        private List<Brain.Brain> ScavengerFlockingBrain = new List<Brain.Brain>();

        private bool isActive;
        private Dictionary<uint, Brain.Brain> entities;

        public Simulation(Vector2 grid, int totalHervivores, int totalCarnivores, int totalScavengers,
            int totalElite, float mutationChance, float mutationRate, float generationLifeTime)
        {
            gridManager = new GridManager(grid, 1);

            for (int i = 0; i < totalHervivores * 2; i++)
            {
                Plants.Add(new AgentPlant(this, gridManager));
            }

            CreateEntities();
            entities = new Dictionary<uint, Brain.Brain>();
            CreateECSEntities();
        }

        private void CreateEntities()
        {
            for (int i = 0; i < totalHervivores; i++)
            {
                Herbivore.Add(new AgentHerbivore(this, gridManager));
                herbivoreMainBrain.Add(Herbivore[i].mainBrain);
                herbivoreMoveFoodBrain.Add(Herbivore[i].moveToFoodBrain);
                herbivoreMoveEscapeBrain.Add(Herbivore[i].moveToEscapeBrain);
                herbivoreEatBrain.Add(Herbivore[i].eatBrain);
            }

            for (int i = 0; i < totalCarnivores; i++)
            {
                Carnivore.Add(new AgentCarnivore(this, gridManager));
                carnivoreMainBrain.Add(Carnivore[i].mainBrain);
                carnivoreMoveBrain.Add(Carnivore[i].moveToFoodBrain);
                carnivoreEatBrain.Add(Carnivore[i].eatBrain);
            }

            for (int i = 0; i < totalScavengers; i++)
            {
                Scavenger.Add(new AgentScavenger(this, gridManager));
                ScavengerMainBrain.Add(Scavenger[i].mainBrain);
                ScavengerFlockingBrain.Add(Scavenger[i].flockingBrain);
            }
        }

        private void CreateECSEntities()
        {
            for (int i = 0; i < totalHervivores; i++)
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

        private void CreateEntity(Brain.Brain brain)
        {
            uint entityID = ECSManager.CreateEntity();
            ECSManager.AddComponent<InputLayerComponent>(entityID, new InputLayerComponent(brain.GetInputLayer()));
            ECSManager.AddComponent<HiddenLayerComponent>(entityID, new HiddenLayerComponent(brain.GetHiddenLayers()));
            ECSManager.AddComponent<OutputLayerComponent>(entityID, new OutputLayerComponent(brain.GetOutputLayer()));

            ECSManager.AddComponent<OutputComponent>(entityID, new OutputComponent(brain.outputs));
            ECSManager.AddComponent<InputComponent>(entityID, new InputComponent(brain.inputs));

            ECSManager.AddComponent<BiasComponent>(entityID, new BiasComponent(brain.bias));
            ECSManager.AddComponent<SigmoidComponent>(entityID, new SigmoidComponent(brain.p));
            entities.Add(entityID, brain);
        }

        public void UpdateSimulation(float deltaTime)
        {
            SettingBrain(deltaTime);
            UpdateInputs();
            ECSManager.Tick(deltaTime);
            UpdateOutputs();
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

        #region GetAgents

        public Dictionary<Vector2, HerbivoreStates> GetHerbivoreAgentsPositionsState()
        {
            Dictionary<Vector2, HerbivoreStates> returnValue = new Dictionary<Vector2, HerbivoreStates>();

            foreach (AgentHerbivore agent in Herbivore)
            {
                returnValue.Add(agent.position, agent.GetState());
            }

            return returnValue;
        }

        public List<Vector2> GetCarnivoreAgentsPositions()
        {
            List<Vector2> returnValue = new List<Vector2>();

            foreach (AgentCarnivore agent in Carnivore)
            {
                returnValue.Add(agent.position);
            }

            return returnValue;
        }

        public List<Vector2> GetScavengerAgentsPositions()
        {
            List<Vector2> returnValue = new List<Vector2>();

            foreach (AgentScavenger agent in Scavenger)
            {
                returnValue.Add(agent.position);
            }

            return returnValue;
        }

        #endregion
    }
}