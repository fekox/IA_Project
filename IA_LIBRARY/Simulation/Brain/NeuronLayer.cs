using System;
using System.Collections.Generic;

namespace IA_Library.Brain
{
    /// <summary>
    /// Create the neuron layer.
    /// </summary>
    [Serializable]
    public class NeuronLayer
    {
        public Neuron[] neurons = Array.Empty<Neuron>();
        private float[] outputs = Array.Empty<float>();
        public int totalWeights = 0;
        public int inputsCount = 0;
        private float bias = 1;
        private float p = 0.5f;
        public int NeuronsCount
        {
            get { return neurons.Length; }
        }

        public int InputsCount
        {
            get { return inputsCount; }
        }

        public int OutputsCount
        {
            get { return outputs.Length; }
        }

        /// <summary>
        /// Serialize the data.
        /// </summary>
        /// <returns>The array of bytes</returns>
        public byte[] Serialize()
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(neurons.Length));

            foreach (var neuron in neurons)
            {
                bytes.AddRange(neuron.Serialize());
            }

            bytes.AddRange(BitConverter.GetBytes(bias));
            bytes.AddRange(BitConverter.GetBytes(p));

            return bytes.ToArray();
        }

        /// <summary>
        /// Create a neuron layer with inputs, neuron counts, bias and sigmoid.
        /// </summary>
        /// <param name="inputsCount">The inputs</param>
        /// <param name="neuronsCount">The neurons counter</param>
        /// <param name="bias">The bias</param>
        /// <param name="p">The sigmoid</param>
        public NeuronLayer(int inputsCount, int neuronsCount, float bias, float p)
        {
            this.inputsCount = inputsCount;
            this.bias = bias;
            this.p = p;

            SetNeuronsCount(neuronsCount);
        }

        /// <summary>
        /// Create a neuron layer with data and outputs.
        /// </summary>
        /// <param name="data">The data</param>
        /// <param name="output">The ouputs</param>
        public NeuronLayer(byte[] data, ref int output)
        {
            int length = BitConverter.ToInt32(data, output);
            output += sizeof(int);
            neurons = new Neuron[length];

            for (int i = 0; i < length; i++)
            {
                neurons[i] = new Neuron(data, ref output);
            }

            bias = BitConverter.ToSingle(data, output);
            output += sizeof(float);
            p = BitConverter.ToSingle(data, output);
            output += sizeof(float);
        }

        /// <summary>
        /// Sets the neurons counter.
        /// </summary>
        /// <param name="neuronsCount">The neuron counter</param>
        void SetNeuronsCount(int neuronsCount)
        {
            neurons = new Neuron[neuronsCount];

            for (int i = 0; i < neurons.Length; i++)
            {
                neurons[i] = new Neuron(inputsCount, bias, p);
                totalWeights += inputsCount;
            }

            outputs = new float[neurons.Length];
        }

        /// <summary>
        /// Sets the weight from one ID.
        /// </summary>
        /// <param name="weights">The weight.</param>
        /// <param name="fromId">The ID</param>
        /// <returns>The ID</returns>
        public int SetWeights(float[] weights, int fromId)
        {
            for (int i = 0; i < neurons.Length; i++)
            {
                fromId = neurons[i].SetWeights(weights, fromId);
            }

            return fromId;
        }

        /// <summary>
        /// Gets the weights.
        /// </summary>
        /// <returns>The weight</returns>
        public float[] GetWeights()
        {
            float[] weights = new float[totalWeights];
            int id = 0;

            for (int i = 0; i < neurons.Length; i++)
            {
                float[] ws = neurons[i].GetWeights();

                for (int j = 0; j < ws.Length; j++)
                {
                    weights[id] = ws[j];
                    id++;
                }
            }

            return weights;
        }

        /// <summary>
        /// Gets the weight count.
        /// </summary>
        /// <returns>The ID</returns>
        public int GetWeightCount()
        {
            int id = 0;

            foreach (var neuron in neurons)
            {
                id += neuron.GetWeights().Length;
            }

            return id;
        }

        /// <summary>
        /// The synamsis.
        /// </summary>
        /// <param name="inputs">The inputs</param>
        /// <returns>The outputs</returns>
        public float[] Synapsis(float[] inputs)
        {
            for (int j = 0; j < neurons.Length; j++)
            {
                outputs[j] = neurons[j].Synapsis(inputs);
            }

            return outputs;
        }
    }
}