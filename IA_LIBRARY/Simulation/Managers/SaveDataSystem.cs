using System.Collections.Generic;
using System.IO;

namespace IA_Library
{
    public class SaveDataSystem
    {
        private List<GeneticData> _datasets = new List<GeneticData>();
            
        public void AddDataset(GeneticData data)
        {
            _datasets.Add(data);
        }

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

        public List<GeneticData> GetAllDatasets()
        {
            return _datasets;
        }  
        
        public void ClearDatasets()
        {
            _datasets.Clear();
        }
    }
}