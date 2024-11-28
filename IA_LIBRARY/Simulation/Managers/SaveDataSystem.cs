using System.Collections.Generic;
using System.IO;

namespace IA_Library
{
    /// <summary>
    /// System to save the data.
    /// </summary>
    public class SaveDataSystem
    {
        private List<GeneticData> _datasets = new List<GeneticData>();
            
        /// <summary>
        /// Add data in list.
        /// </summary>
        /// <param name="data">The data</param>
        public void AddDataset(GeneticData data)
        {
            _datasets.Add(data);
        }

        /// <summary>
        /// Save all data.
        /// </summary>
        /// <param name="filePath">tThe filePath of the file</param>
        public void SaveAll(string filePath)
        {
            MemoryStream stream = new MemoryStream();
            
            foreach (GeneticData data in _datasets)
            {
                byte[] dataArray = data.Serialize();
                stream.Capacity += dataArray.Length;
                stream.Write(dataArray, 0, dataArray.Length);
            }

            File.WriteAllBytes(filePath, stream.ToArray());
        }

        /// <summary>
        /// Load all the data.
        /// </summary>
        /// <param name="filePath">the filepat of the file</param>
        /// <exception cref="FileNotFoundException">Exeption if the data not found</exception>
        public void LoadAll(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Save file not found. {filePath}");

            byte[] data = File.ReadAllBytes(filePath);

            int offset = 0;
            
            for (int index = 0; index < _datasets.Count; index++)
            {
                _datasets[index] = new GeneticData(data, ref offset);
            }

        }

        /// <summary>
        /// Returns the data,
        /// </summary>
        /// <returns>The data</returns>
        public List<GeneticData> GetAllDatasets()
        {
            return _datasets;
        }  
        
        /// <summary>
        /// Clear the data list.
        /// </summary>
        public void ClearDatasets()
        {
            _datasets.Clear();
        }
    }
}