using System;
using System.Collections.Generic;
using IA_Library_ECS;

namespace IA_Library.Brain
{
    [Serializable]
    public class Brain
    {
        public List<NeuronLayer> layers = new List<NeuronLayer>();
        int totalWeightsCount = 0;
        int inputsCount = 0;
        private float fitness = 1;
        public float FitnessReward;
        public float FitnessMultiplier;
        public float[] outputs;
        int fitnessCount = 0;

        public float bias = 1;
        public float p = 0.5f;
        public float[] inputs;

        public int InputsCount
        {
            get { return inputsCount; }
        }

        public Brain()
        {
        }

        public Brain(byte[] data, ref int currentOffset)
        {
            layers = CreateLayersFromBytes(data, ref currentOffset);
            bias = BitConverter.ToSingle(data, currentOffset);
            currentOffset += sizeof(float);
            p = BitConverter.ToSingle(data, currentOffset);
            currentOffset += sizeof(float);
        }

        public Brain(Brain brain)
        {
            bias = brain.bias;
            this.layers = layers;
            totalWeightsCount = brain.totalWeightsCount;
        }

        public byte[] Serialize()
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(SerializeLayers());

            bytes.AddRange(BitConverter.GetBytes(bias));

            bytes.AddRange(BitConverter.GetBytes(p));

            return bytes.ToArray();
        }

        private byte[] SerializeLayers()
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(layers.Count));

            foreach (var layer in layers)
            {
                bytes.AddRange(layer.Serialize());
            }

            return bytes.ToArray();
        }

        public List<NeuronLayer> CreateLayersFromBytes(byte[] data, ref int offset)
        {
            int layerCount = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);

            List<NeuronLayer> layersToAdd = new List<NeuronLayer>();

            for (int i = 0; i < layerCount; i++)
            {
                layersToAdd.Add(new NeuronLayer(data, ref offset));
            }

            return layersToAdd;
        }

        public void ApplyFitness()
        {
            fitness *= FitnessReward * FitnessMultiplier > 0 ? FitnessMultiplier : 0;
        }

        public bool AddNeuronLayer(int neuronsCount, float bias, float p)
        {
            if (layers.Count == 0)
            {
                return false;
            }

            return AddNeuronLayer(layers[^1].OutputsCount, neuronsCount, bias, p);
        }

        private bool AddNeuronLayer(int inputsCount, int neuronsCount, float bias, float p)
        {
            if (layers.Count > 0 && layers[^1].OutputsCount != inputsCount)
            {
                return false;
            }

            NeuronLayer layer = new NeuronLayer(inputsCount, neuronsCount, bias, p);

            totalWeightsCount += inputsCount * neuronsCount;

            layers.Add(layer);

            return true;
        }

        public void Set0Fitness()
        {
            fitness *= 0;
        }

        public bool AddFirstNeuronLayer(int inputsCount, float bias, float p)
        {
            if (layers.Count != 0)
            {
                return false;
            }

            this.inputsCount = inputsCount;

            return AddNeuronLayer(inputsCount, inputsCount, bias, p);
        }

        public bool AddNeuronLayerAtPosition(int neuronsCount, int layerPosition)
        {
            if (layers.Count <= 0 || layerPosition >= layers.Count)
            {
                return false;
            }

            NeuronLayer layer = new NeuronLayer(layers[layerPosition].OutputsCount, neuronsCount, bias, p);
            totalWeightsCount -= layers[layerPosition].OutputsCount * layers[layerPosition + 1].OutputsCount;
            layers[layerPosition + 1] = new NeuronLayer(neuronsCount, layers[layerPosition + 1].NeuronsCount, bias, p);

            totalWeightsCount += layers[layerPosition + 1].OutputsCount * neuronsCount;
            totalWeightsCount += layers[layerPosition].OutputsCount * neuronsCount;
            layers.Insert(layerPosition + 1, layer);

            totalWeightsCount = GetWeightsCount();
            
            return true;
        }

        public bool AddNeuronAtLayer(int neuronsCountToAdd, int layerPosition)
        {
            NeuronLayer oldLayer = layers[layerPosition];
            layers[layerPosition] = new NeuronLayer(oldLayer.InputsCount, oldLayer.NeuronsCount + neuronsCountToAdd, bias, p);

            NeuronLayer oldNextLayer = layers[layerPosition + 1];
            layers[layerPosition + 1] = new NeuronLayer(layers[layerPosition].OutputsCount, oldNextLayer.NeuronsCount, bias, p);

            totalWeightsCount += layers[layerPosition].OutputsCount * neuronsCountToAdd;
            totalWeightsCount += layers[layerPosition + 1].OutputsCount * neuronsCountToAdd;

            totalWeightsCount = GetWeightsCount();
            
            return true;
        }

        public int GetTotalWeightsCount()
        {
            return totalWeightsCount;
        }

        public void SetWeights(float[] newWeights)
        {
            int fromId = 0;

            for (int i = 0; i < layers.Count; i++)
            {
                fromId = layers[i].SetWeights(newWeights, fromId);
            }
        }

        public float[] GetWeights()
        {
            float[] weights = new float[totalWeightsCount];
            int id = 0;

            for (int i = 0; i < layers.Count; i++)
            {
                float[] ws = layers[i].GetWeights();

                for (int j = 0; j < ws.Length; j++)
                {
                    weights[id] = ws[j];
                    id++;
                }
            }

            return weights;
        }

        public int GetWeightsCount()
        {
            int id = 0;
            
            foreach (var layer in layers)
            {
                id += layer.GetWeightCount();
            }

            return id;
        }

        public Layer GetInputLayer()
        {
            int id = layers[0].neurons.Length;
            float[,] weights = new float[layers[0].neurons.Length, layers[0].neurons[0].WeightsCount];
            
            for (var index = 0; index < layers[0].neurons.Length; index++)
            {
                for (var j = 0; j < layers[0].neurons[index].WeightsCount; j++)
                {
                    weights[index, j] = layers[0].neurons[index].GetWeights()[j];
                }
            }

            Layer layer = new Layer(id, weights);
            
            return layer;
        }

        public Layer GetOutputLayer()
        {
            Index layerIndex = ^1;
            int id = layers[layerIndex].neurons.Length;
            float[,] weights = new float[layers[layerIndex].neurons.Length, layers[layerIndex].neurons[0].WeightsCount];
            
            for (var index = 0; index < layers[layerIndex].neurons.Length; index++)
            {
                for (var j = 0; j < layers[layerIndex].neurons[index].WeightsCount; j++)
                {
                    weights[index, j] = layers[layerIndex].neurons[index].GetWeights()[j];
                }
            }

            Layer layer = new Layer(id, weights);
            
            return layer;
        }

        public Layer[] GetHiddenLayers()
        {
            Layer[] layersToReturn = new Layer[layers.Count - 2 > 0 ? layers.Count - 2 : 0];
            var count = 0;
            
            for (var k = 0; k < this.layers.Count; k++)
            {
                if (k == 0 || k == this.layers.Count - 1)
                {
                    continue;
                }

                int id = layers[k].neurons.Length;
                float[,] weights = new float[layers[k].neurons.Length, layers[k].neurons[0].WeightsCount];
            
                for (var index = 0; index < layers[k].neurons.Length; index++)
                {
                    for (var j = 0; j < layers[k].neurons[index].WeightsCount; j++)
                    {
                        weights[index, j] = layers[k].neurons[index].GetWeights()[j];
                    }
                }

                layersToReturn[count] = new Layer(id, weights);
                count++;
            }

            return layersToReturn;
        }

        public float[] Synapsis(float[] inputs)
        {
            float[] outputs = inputs;

            for (int i = 0; i < layers.Count; i++)
            {
                outputs = layers[i].Synapsis(outputs);
            }

            return outputs;
        }

        public static Brain CreateBrain(int inputsCount, int[] HidenLayers, int outputsCount, float bias, float sigmoid)
        {
            Brain newbrain = new Brain();

            newbrain.AddFirstNeuronLayer(inputsCount, bias, sigmoid);

            for (int i = 0; i < HidenLayers.Length; i++)
            {
                newbrain.AddNeuronLayer(HidenLayers[i], bias, sigmoid);
            }

            newbrain.AddNeuronLayer(outputsCount, bias, sigmoid);

            return newbrain;
        }
    }
}