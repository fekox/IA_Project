using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.Example
{
    public class ECSFlocking : MonoBehaviour
    {
        public int entityCount = 5;
        public int caravanCount = 2;
        public float velocity = 2f;
        public float radius = 1.0f;

        public float detectionRadious = 3.0f;
        public float aligmentWeight = 1;
        public float cohesionWeight = 1.5f;
        public float separationWeight = -2;


        public GameObject prefab;
        public GrapfView GrapfView;

        private const int MAX_OBJS_PER_DRAWCALL = 1000;
        private Mesh prefabMesh;
        private Material prefabMaterial;
        private Material prefabMaterial2;
        private Vector3 prefabScale;
        public Agent agentPrefab;
        public Caravan Caravan;

        private Dictionary<uint, IFlock> entities;


        [ContextMenu("RaiseAlarm")]
        public void RaiseAlarm()
        {
            foreach (KeyValuePair<uint, IFlock> entity in entities)
            {
               ((IAlarmable) entity.Value).InvokeAlarmOn();
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                if (entities?.Count > 0)
                {
                    foreach (KeyValuePair<uint, IFlock> entity in entities)

                    {
                        SetBoidParams(entity.Value.GetBoid());
                    }
                }
            }
        }

        [ContextMenu("StopAlarm")]
        public void StopAlarm()
        {
            foreach (KeyValuePair<uint, IFlock> entity in entities)
            {
                ((IAlarmable) entity.Value).InvokeAlarmOff();
            }
        }

        void Start()
        {
            ECSManager.Init();
            entities = new Dictionary<uint, IFlock>();
            for (int i = 0; i < entityCount; i++)
            {
                uint entityID = ECSManager.CreateEntity();
                ECSManager.AddComponent<PositionComponent>(entityID, new PositionComponent(0, 0, 0));
                ECSManager.AddComponent<AlignmentComponent>(entityID, new AlignmentComponent(0, 0, 0));
                ECSManager.AddComponent<CohesionComponent>(entityID, new CohesionComponent(0, 0, 0));
                ECSManager.AddComponent<SeparationComponent>(entityID, new SeparationComponent(0, 0, 0));
                ECSManager.AddComponent<DirectionComponent>(entityID, new DirectionComponent(0, 0, 0));
                ECSManager.AddComponent<ObjectiveComponent>(entityID, new ObjectiveComponent(0, 0, 0));
                ECSManager.AddComponent<FowardComponent>(entityID, new FowardComponent(0, 1, 0));
                ECSManager.AddComponent<SpeedComponent>(entityID, new SpeedComponent(velocity));
                ECSManager.AddComponent<RadiusComponent>(entityID, new RadiusComponent(radius));
                entities.Add(entityID, Instantiate(agentPrefab, Vector3.zero, Quaternion.identity));
                ((ITraveler)entities[entityID]).SetGraph(GrapfView);
                entities[entityID].SetActive(true);
                SetBoidParams(entities[entityID].GetBoid());
                // entities[entityID].gameObject.SetActive(true);
            }
            for (int i = 0; i < caravanCount; i++)
            {
                uint entityID = ECSManager.CreateEntity();
                ECSManager.AddComponent<PositionComponent>(entityID, new PositionComponent(0, 0, 0));
                ECSManager.AddComponent<AlignmentComponent>(entityID, new AlignmentComponent(0, 0, 0));
                ECSManager.AddComponent<CohesionComponent>(entityID, new CohesionComponent(0, 0, 0));
                ECSManager.AddComponent<SeparationComponent>(entityID, new SeparationComponent(0, 0, 0));
                ECSManager.AddComponent<DirectionComponent>(entityID, new DirectionComponent(0, 0, 0));
                ECSManager.AddComponent<ObjectiveComponent>(entityID, new ObjectiveComponent(0, 0, 0));
                ECSManager.AddComponent<FowardComponent>(entityID, new FowardComponent(0, 1, 0));
                ECSManager.AddComponent<SpeedComponent>(entityID, new SpeedComponent(velocity));
                ECSManager.AddComponent<RadiusComponent>(entityID, new RadiusComponent(radius));
                entities.Add(entityID, Instantiate(Caravan, Vector3.zero, Quaternion.identity));
                ((ITraveler)entities[entityID]).SetGraph(GrapfView);

                entities[entityID].SetActive(true);
                SetBoidParams(entities[entityID].GetBoid());
            }


            prefabMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
            prefabMaterial = prefab.GetComponent<MeshRenderer>().sharedMaterial;
            prefabScale = prefab.transform.localScale;
        }

        void Update()
        {
            ECSManager.Tick(Time.deltaTime);
        }

        void LateUpdate()
        {
            foreach (KeyValuePair<uint, IFlock> entity in entities)
            {
                PositionComponent position = ECSManager.GetComponent<PositionComponent>(entity.Key);
                AlignmentComponent alignment = ECSManager.GetComponent<AlignmentComponent>(entity.Key);
                CohesionComponent cohesion = ECSManager.GetComponent<CohesionComponent>(entity.Key);
                SeparationComponent separation = ECSManager.GetComponent<SeparationComponent>(entity.Key);
                DirectionComponent direction = ECSManager.GetComponent<DirectionComponent>(entity.Key);
                ObjectiveComponent objetive = ECSManager.GetComponent<ObjectiveComponent>(entity.Key);
                FowardComponent foware = ECSManager.GetComponent<FowardComponent>(entity.Key);


                var Alig = new Vector3(alignment.X, alignment.Y, alignment.Z);
                var Cohe = new Vector3(cohesion.X, cohesion.Y, cohesion.Z);
                var Sep = new Vector3(separation.X, separation.Y, separation.Z);
                var dir = new Vector3(direction.X, direction.Y, direction.Z);
                var Pos = new Vector3(position.X, position.Y, position.Z);

                entity.Value.GetBoid().SetACS(Alig, Cohe, Sep, dir);
                Matrix4x4 drawMatrix = new Matrix4x4();
                for (int j = 0; j < prefabMesh.subMeshCount; j++)
                {
                    drawMatrix.SetTRS(entity.Value.GetBoid().position, quaternion.identity,
                        prefab.transform.localScale);
                    Graphics.DrawMesh(prefabMesh, drawMatrix,entity.Key<entityCount ?prefabMaterial: prefabMaterial2 , 0, null, j);
                }

                position.X = entity.Value.GetBoid().parent.position.x;
                position.Y = entity.Value.GetBoid().parent.position.y;
                position.Z = entity.Value.GetBoid().parent.position.z;
                objetive.X = entity.Value.GetBoid().objective.x;
                objetive.Y = entity.Value.GetBoid().objective.y;
                objetive.Z = entity.Value.GetBoid().objective.z;
                foware.X = entity.Value.GetBoid().parent.forward.x;
                foware.Y = entity.Value.GetBoid().parent.forward.y;
                foware.Z = entity.Value.GetBoid().parent.forward.z;
            }
        }

        private void SetBoidParams(BoidAgent boid)
        {
            boid.detectionRadious = detectionRadious;
            boid.aligmentWeight = aligmentWeight;
            boid.cohesionWeight = cohesionWeight;
            boid.separationWeight = separationWeight;
            boid.speed = velocity;
        }
    }
}