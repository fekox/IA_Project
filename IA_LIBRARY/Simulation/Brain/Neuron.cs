using System;

namespace IA_Library.Brain
{
    public class Neuron
    {
        private float[] weights;
        private float bias;
        private float p;

        public int WeightsCount
        {
            get { return weights.Length; }
        }

        public Neuron(int weightsCount, float bias, float p)
        {
            weights = new float[weightsCount];

            Random rand = new Random();

            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = (float)(rand.NextDouble() * 2 - 1); // RandVal (-1,1)
            }

            this.bias = bias;
            this.p = p;
        }

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

        public int SetWeights(float[] newWeights, int fromId)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                this.weights[i] = newWeights[i + fromId];
            }

            return fromId + weights.Length;
        }

        public float[] GetWeights()
        {
            return this.weights;
        }

        public static float Sigmoid(float a, float p)
        {
            return (float)Math.Tanh(a / p);
        }
    }
}