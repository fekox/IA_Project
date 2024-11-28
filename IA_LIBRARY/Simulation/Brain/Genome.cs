using System;
using System.Collections.Generic;

namespace IA_Library.Brain
{
    /// <summary>
    /// Create and mange the genome.
    /// </summary>
    [Serializable]
    public class Genome
    {
        public float[] genome;
        public float fitness = 0;

        /// <summary>
        /// Create the genome with genes.
        /// </summary>
        /// <param name="genes">The genes</param>
        public Genome(float[] genes)
        {
            this.genome = genes;
            fitness = 1;
        } 

        /// <summary>
        /// Create the genome with data and outputs.
        /// </summary>
        /// <param name="data">The data</param>
        /// <param name="output">The outputs</param>
        public Genome(byte[] data,ref int output)
        {
            int genomeLength = BitConverter.ToInt32(data, output);
            output += sizeof(int);
        
            genome = new float[genomeLength];
            
            for (int i = 0; i < genomeLength; i++)
            {
                genome[i] = BitConverter.ToSingle(data, output);
                output += sizeof(float);
            }
        
            fitness = BitConverter.ToSingle(data, output);
            output += sizeof(float);
        }

        /// <summary>
        /// Serialize the data.
        /// </summary>
        /// <returns>The array of bytes</returns>
        public byte[] Serialize()
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(genome.Length));

            foreach (float gene in genome)
            {
                bytes.AddRange(BitConverter.GetBytes(gene));
            }
        
            bytes.AddRange(BitConverter.GetBytes(fitness));

            return bytes.ToArray();
        }

        /// <summary>
        /// Create a genome with genes count.
        /// </summary>
        /// <param name="genesCount">genes count</param>
        public Genome(int genesCount)
        {
            genome = new float[genesCount];
            Random rand = new Random();
            
            for (int j = 0; j < genesCount; j++)
                genome[j] = (float)(rand.NextDouble() * 2 - 1);

            fitness = 1;
        }

        /// <summary>
        /// Sets the genome fitnnes.
        /// </summary>
        public Genome()
        {
            fitness = 1;
        }
    }
}