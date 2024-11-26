using System.Collections.Generic;

namespace IA_Library.Brain
{
    public class BrainData
    {
        private int inputsCount;
        private int[] hiddenLayer;
        private int outputsCount;
        
        private float bias;
        private float sigmoid;

        public BrainData(int inputsCount, int[] hiddenLayer, int outputsCount, float bias, float sigmoid)
        {
            this.inputsCount = inputsCount;
            this.hiddenLayer = hiddenLayer;
            this.outputsCount = outputsCount;
            this.bias = bias;
            this.sigmoid = sigmoid;
        }

        public Brain CreateBrain()
        {
            return Brain.CreateBrain(inputsCount, hiddenLayer, outputsCount, bias, sigmoid);
        }
    }
}