using System;
using System.Collections.Generic;

namespace IA_Library.Brain
{
    /// <summary>
    /// Create the neuron.
    /// </summary>
    [Serializable]
    public class Neuron
    {
        public float[] weights;
        private float bias;
        private float p;

        private int offsetCalculator;

        public int WeightsCount
        {
            get { return weights.Length; }
        }

        /// <summary>
        /// Create the neuron with data and output offset.
        /// </summary>
        /// <param name="data">The data</param>
        /// <param name="outputOffset">The output offset</param>
        public Neuron(byte[] data, ref int outputOffset)
        {
            int length = BitConverter.ToInt32(data, outputOffset);
            outputOffset += sizeof(int);
            weights = new float[length];
            
            for (int i = 0; i < length; i++)
            {
                weights[i] = BitConverter.ToSingle(data, outputOffset);
                outputOffset += sizeof(float);
            }

            bias = BitConverter.ToSingle(data, outputOffset);
            outputOffset += sizeof(float);
            p = BitConverter.ToSingle(data, outputOffset);
            outputOffset += sizeof(float);
        }

        /// <summary>
        /// Serialize the data.
        /// </summary>
        /// <returns>The array of bytes</returns>
        public byte[] Serialize()
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(weights.Length));

            foreach (float weight in weights)
            {
                bytes.AddRange(BitConverter.GetBytes(weight));
            }

            bytes.AddRange(BitConverter.GetBytes(bias));

            bytes.AddRange(BitConverter.GetBytes(p));

            return bytes.ToArray();
        }

        /// <summary>
        /// Create the neuron with weights count, bias and sigmoid.
        /// </summary>
        /// <param name="weightsCount">The weights</param>
        /// <param name="bias">The bias</param>
        /// <param name="p">The sigmoid</param>
        public Neuron(int weightsCount, float bias, float p)
        {
            weights = new float[weightsCount];
            Random random = new Random();
            
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = (float)(random.NextDouble() * 2 - 1);
            }

            this.bias = bias;
            this.p = p;
        }

        /// <summary>
        /// The sigmoid.
        /// </summary>
        /// <param name="input">The inputs</param>
        /// <returns></returns>
        public float Synapsis(float[] input)
        {
            float a = 0;

            for (int i = 0; i < input.Length; i++)
            {
                a += weights[i] * input[i];
            }

            a += bias * weights[weights.Length - 1];

            return Sigmoid(a, p);
        }

        /// <summary>
        /// Setthe weights from one ID.
        /// </summary>
        /// <param name="newWeights">The weight</param>
        /// <param name="fromId">The ID</param>
        /// <returns>The new weights</returns>
        public int SetWeights(float[] newWeights, int fromId)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                this.weights[i] = newWeights[i + fromId];
            }

            return fromId + weights.Length;
        }

        /// <summary>
        /// Gets the weight.
        /// </summary>
        /// <returns>The weight</returns>
        public float[] GetWeights()
        {
            return this.weights;
        }

        /// <summary>
        /// The sigmoid.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="p"></param>
        /// <returns>The sigmoid</returns>
        public static float Sigmoid(float a, float p)
        {
            return (float)Math.Tanh(a / p);
        }
    }
}