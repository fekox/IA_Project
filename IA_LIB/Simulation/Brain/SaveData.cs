using System.Collections.Generic;

namespace IA_Library.Brain
{
    public class SaveData
    {
        public int generationNumber;
        
        public BrainData mainBrainData;
        
        public BrainData eatBrainData;
        public BrainData moveBrainData;
        
        public float Bias = 0f;
        public float P = 1f;
    }

    public class HerbivoreSaveData : SaveData
    {
        public BrainData escapeBrainData;
    }
    
    public class ScavengerSaveData : SaveData
    {
        public BrainData flokingBrainData;
    }

    public class BrainData
    {
        public int[] BrainSettings;
        public Genome genome;
    }
}