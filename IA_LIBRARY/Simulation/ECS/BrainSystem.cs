using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IA_Library_ECS
{
    /// <summary>
    /// Manage the brains.
    /// </summary>
    public class BrainSystem : ECSSystem
    {
        private ParallelOptions parallelOptions;

        private IDictionary<uint, InputLayerComponent> inputLayerComponent;
        private IDictionary<uint, HiddenLayerComponent> hiddenLayerComponent;
        private IDictionary<uint, OutputLayerComponent> outputLayerComponent;

        private IDictionary<uint, BiasComponent> biasComponent;
        private IDictionary<uint, SigmoidComponent> sigmoidComponent;

        private IDictionary<uint, OutputComponent> outputComponent;
        private IDictionary<uint, InputComponent> inputComponent;

        private IEnumerable<uint> activeEntities;

        /// <summary>
        /// Initialize the parallel options.
        /// </summary>
        public override void Initialize()
        {
            parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
        }

        /// <summary>
        /// Add the compoenets in the layers.
        /// </summary>
        /// <param name="deltaTime">The time</param>
        protected override void PreExecute(float deltaTime)
        {
            inputLayerComponent ??= ECSManager.GetComponents<InputLayerComponent>();
            hiddenLayerComponent ??= ECSManager.GetComponents<HiddenLayerComponent>();
            outputLayerComponent ??= ECSManager.GetComponents<OutputLayerComponent>();

            biasComponent ??= ECSManager.GetComponents<BiasComponent>();
            sigmoidComponent ??= ECSManager.GetComponents<SigmoidComponent>();

            outputComponent ??= ECSManager.GetComponents<OutputComponent>();
            inputComponent ??= ECSManager.GetComponents<InputComponent>();

            activeEntities ??= ECSManager.GetEntitiesWithComponentTypes(
                typeof(InputLayerComponent),
                typeof(HiddenLayerComponent),
                typeof(OutputLayerComponent),
                typeof(BiasComponent),
                typeof(SigmoidComponent),
                typeof(OutputComponent),
                typeof(InputComponent)
            );
        }

        /// <summary>
        /// Execute the inputs.
        /// </summary>
        /// <param name="deltaTime">The time</param>
        protected override void Execute(float deltaTime)
        {
            Parallel.ForEach(activeEntities, parallelOptions, entity =>
            {
                outputComponent[entity].output = inputComponent[entity].inputs;

                outputComponent[entity].output = FirstLayerSynapsis(entity, inputComponent[entity].inputs);
                inputComponent[entity].size = outputComponent[entity].output.Length;
                inputComponent[entity].inputs = outputComponent[entity].output;
                outputComponent[entity].output = new float[hiddenLayerComponent[entity].HiggestLayerSize];
                
                for (int layer = 0; layer < hiddenLayerComponent[entity].hiddenLayers.Length; layer++)
                {
                    LayerSynapsis(entity, inputComponent[entity].inputs, layer, ref inputComponent[entity].size);
                    inputComponent[entity].inputs = outputComponent[entity].output;
                }

                outputComponent[entity].output = OutputLayerSynapsis(entity, inputComponent[entity].inputs,
                    ref inputComponent[entity].size);
            });
        }

        /// <summary>
        /// Post execute method.
        /// </summary>
        /// <param name="deltaTime">The time</param>
        protected override void PostExecute(float deltaTime)
        {
        }

        /// <summary>
        /// The synapsis of the first layer.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="inputs">The inputs of the entity</param>
        /// <returns>Returns the outputs.</returns>
        private float[] FirstLayerSynapsis(uint entity, float[] inputs)
        {
            Parallel.For(0, inputs.Length, parallelOptions,
                
                neuron => 
                { 
                    outputComponent[entity].output[neuron] = FirstNeuronSynapsis(entity, neuron, inputs); 
                }
            );
            
            return outputComponent[entity].output;
        }

        /// <summary>
        /// The synapsis for the other layers.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="inputs">The inputs</param>
        /// <param name="layer">The layers</param>
        /// <param name="size">The size of the neurons</param>
        /// <returns>Returns the outputs</returns>
        private float[] LayerSynapsis(uint entity, float[] inputs, int layer, ref int size)
        {
            int neuronCount = hiddenLayerComponent[entity].hiddenLayers[layer].weights.GetLength(0);
            Array.Resize(ref outputComponent[entity].output, neuronCount);

            Parallel.For(0, neuronCount, parallelOptions,
                
                neuron => 
                { 
                    outputComponent[entity].output[neuron] = NeuronSynapsis(entity, neuron, inputs, layer); 
                }
            );

            size = neuronCount;
           
            return outputComponent[entity].output;
        }

        /// <summary>
        /// The synapsis for the output layer.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="inputs">The inputs</param>
        /// <param name="size">The size</param>
        /// <returns></returns>
        private float[] OutputLayerSynapsis(uint entity, float[] inputs, ref int size)
        {
            int neuronCount = outputLayerComponent[entity].layer.weights.GetLength(0);
            Array.Resize(ref outputComponent[entity].output, neuronCount);
            
            Parallel.For(0, neuronCount, parallelOptions,
                neuron => 
                { 
                    outputComponent[entity].output[neuron] = LastNeuronSynapsis(entity, neuron, inputs); 
                }
            );
            
            return outputComponent[entity].output;
        }

        /// <summary>
        /// Sigmoid algorithm for the first neuron layer.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="neuron">The neurons</param>
        /// <param name="inputs">The inputs</param>
        /// <returns>The sigmoid</returns>
        private float FirstNeuronSynapsis(uint entity, int neuron, float[] inputs)
        {
            float a = 0;
            
            for (int i = 0; i < inputs.Length; i++)
            {
                a += inputLayerComponent[entity].layer.weights[neuron, i] * inputs[i];
            }

            a += biasComponent[entity].X * inputLayerComponent[entity].layer.weights[neuron, inputs.Length - 1];

            return (float)Math.Tanh(a / sigmoidComponent[entity].X);
        }

        /// <summary>
        /// Sigmoid algorithm for the neuron.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="neuron">The neurons</param>
        /// <param name="inputs">The inputs</param>
        /// <param name="layer">The layer</param>
        /// <returns>The sigmoid</returns>
        private float NeuronSynapsis(uint entity, int neuron, float[] inputs, int layer)
        {
            float a = 0;
            int exclusive = hiddenLayerComponent[entity].hiddenLayers[layer].weights.GetLength(1);
            
            for (int i = 0; i < exclusive; i++)
            {
                a += hiddenLayerComponent[entity].hiddenLayers[layer].weights[neuron, i] * inputs[i];
            }

            a += biasComponent[entity].X * hiddenLayerComponent[entity].hiddenLayers[layer].weights[neuron, exclusive - 1];

            return (float)Math.Tanh(a / sigmoidComponent[entity].X);
        }

        /// <summary>
        /// Sigmoid algorithm for the neuron layer.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="neuron">The neuron</param>
        /// <param name="inputs">The inputs</param>
        /// <returns>The sigmoid</returns>
        private float LastNeuronSynapsis(uint entity, int neuron, float[] inputs)
        {
            float a = 0;
            int exclusive = outputLayerComponent[entity].layer.weights.GetLength(1);
           
            for (int i = 0; i < exclusive; i++)
            {
                a += outputLayerComponent[entity].layer.weights[neuron, i] * inputs[i];
            }

            a += biasComponent[entity].X * outputLayerComponent[entity].layer.weights[neuron, exclusive - 1];;

            return (float)Math.Tanh(a / sigmoidComponent[entity].X);
        }
    }
}