using System;
using System.Collections.Generic;
using IA_Library_ECS;

namespace IA_Library.Brain
{
    public class Brain
    {
        public List<NeuronLayer> layers = new List<NeuronLayer>();
        private int totalWeightsCount = 0;
        private int inputsCount = 0;

        public float[] outputs;
        public float[] inputs;

        private float fitness = 1;
        public float FitnessReward;
        public float FitnessMultiplier;
        private int fitnessCount = 0;

        public float bias = 1;
        public float p = 0.5f;

        public int InputsCount => inputsCount;

        public Brain()
        {
        }

        public Brain(Brain brain) 
        {
            bias = brain.bias;
            layers = layers;
            totalWeightsCount = brain.totalWeightsCount;
        }

        public void CopyStructureFrom(Brain brain) 
        {
            layers = brain.layers;
        }

        public void ApplyFitness()
        {
            fitness *= (FitnessReward * FitnessMultiplier > 0f) ? (FitnessReward + FitnessMultiplier) : 0f;
        }

        public void DestroyFitness() 
        {
            fitness *= 0f;
        }

        public bool AddNeuronLayer(int neuronsCount, float bias, float p)
        {
            if (layers.Count == 0)
            {
                return false;
            }

            return AddNeuronLayer(layers[layers.Count - 1].OutputsCount, neuronsCount, bias, p);
        }

        private bool AddNeuronLayer(int inputsCount, int neuronsCount, float bias, float p)
        {
            if (layers.Count > 0 && layers[layers.Count - 1].OutputsCount != inputsCount)
            {
                return false;
            }

            NeuronLayer layer = new NeuronLayer(inputsCount, neuronsCount, bias, p);

            totalWeightsCount += inputsCount * neuronsCount;

            layers.Add(layer);

            return true;
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
                throw new Exception("No previous Layer or out of range");
            }

            NeuronLayer item = new NeuronLayer(layers[layerPosition].OutputsCount, neuronsCount, bias, p);
            totalWeightsCount -= layers[layerPosition].OutputsCount * layers[layerPosition + 1].OutputsCount;
            layers[layerPosition + 1] = new NeuronLayer(neuronsCount, layers[layerPosition + 1].NeuronsCount, bias, p);
            totalWeightsCount += layers[layerPosition + 1].OutputsCount * neuronsCount;
            totalWeightsCount += layers[layerPosition].OutputsCount * neuronsCount;
            layers.Insert(layerPosition + 1, item);
            totalWeightsCount = GetWeightsCount();
            
            return true;
        }

        public bool AddNeuronAtLayer(int neuronsCountToAdd, int layerPosition)
        {
            NeuronLayer neuronLayer = layers[layerPosition];
            layers[layerPosition] = new NeuronLayer(neuronLayer.InputsCount, neuronLayer.NeuronsCount + neuronsCountToAdd, bias, p);
            NeuronLayer neuronLayer2 = layers[layerPosition + 1];
            layers[layerPosition + 1] = new NeuronLayer(layers[layerPosition].OutputsCount, neuronLayer2.NeuronsCount, bias, p);
            totalWeightsCount += layers[layerPosition].OutputsCount * neuronsCountToAdd;
            totalWeightsCount += layers[layerPosition + 1].OutputsCount * neuronsCountToAdd;
            totalWeightsCount = GetWeightsCount();
            
            return true;
        }

        public int GetWeightsCount()
        {
            int num = 0;
            foreach (NeuronLayer layer in layers)
            {
                num += layer.GetWeightCount();
            }

            return num;
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

        public float[] GetGenome()
        {
            float[] weights = new float[totalWeightsCount];
            int id = 0;

            for (int i = 0; i < layers.Count; i++)
            {
                float[] ws = layers[i].GetWeights();

                for (int j = 0; j < ws.Length; j++)
                {
                    weights[id++] = ws[j];
                }
            }

            return weights;
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
            float[,] weights = new float[layers[layerIndex].neurons.Length, layers[0].neurons[0].WeightsCount];
            
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
    }
}