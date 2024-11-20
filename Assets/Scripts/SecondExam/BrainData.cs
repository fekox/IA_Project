using System;
using System.Collections.Generic;


[System.Serializable]
public class BrainData 
{
    public int InputsCount = 4;
   
    public int OutputsCount = 2;
    public int[] NeuronsCountPerHL;
    public float Bias = 1f;
    public float P = 1f;
    public List<Genome> genomeCollection { get; set; }

    public BrainData(int inputsCount, int hiddenLayers, int outputsCount, float bias, float p)
    {
       
    }
}