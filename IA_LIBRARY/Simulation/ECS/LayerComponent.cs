namespace IA_Library_ECS
{
    /// <summary>
    /// The layer component.
    /// </summary>
    public abstract class LayerComponent : ECSComponent
    {
    }

    /// <summary>
    /// Create the layer.
    /// </summary>
    public class Layer
    {
        public int neuronCount;
        public float[,] weights;
        public Layer(int neuronCount, float[,] weights)
        {
            this.neuronCount = neuronCount;
            this.weights = weights;
        }

        public Layer(int neuronCount)
        {
            this.neuronCount = neuronCount;
        }
    }

    /// <summary>
    /// Create the input layer.
    /// </summary>
    public class InputLayerComponent : LayerComponent
    {
        public Layer layer;
        public int inputCount;
        
        public InputLayerComponent(Layer layer)
        {
            this.layer = layer;
        }
    }

    /// <summary>
    /// Create the hidden layer.
    /// </summary>
    public class HiddenLayerComponent : LayerComponent
    {
        public Layer[] hiddenLayers;
        public int HiggestLayerSize = 0;

        public HiddenLayerComponent(Layer[] hiddenLayers)
        {
            this.hiddenLayers = hiddenLayers;
            SetHighestLayerSize();
        }
        
        /// <summary>
        /// Set the highest layer size.
        /// </summary>
        public void SetHighestLayerSize()
        {
            foreach (var layer in this.hiddenLayers)
            {
                if (layer.neuronCount < HiggestLayerSize)
                {
                    HiggestLayerSize = layer.neuronCount;
                }
            }
        }
    }

    /// <summary>
    /// Create the output layer.
    /// </summary>
    public class OutputLayerComponent : LayerComponent
    {
        public Layer layer;

        public OutputLayerComponent(Layer layer)
        {
            this.layer = layer;
        }
    }

    /// <summary>
    /// Create the ouput component.
    /// </summary>
    public class OutputComponent : ECSComponent
    {
        public float[] output;

        public OutputComponent(float[] output)
        {
            this.output = output;
        }
    }
    
    /// <summary>
    /// Create the input component.
    /// </summary>
    public class InputComponent : ECSComponent
    {
        public float[] inputs;
        public int size;
        
        public InputComponent(float[] inputs, int size)
        {
            this.inputs = inputs;
            this.size = size;
        }
    }
}