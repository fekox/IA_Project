using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IA_Library_ECS
{
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

        public override void Initialize()
        {
            parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
        }

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

        protected override void Execute(float deltaTime)
        {
            Parallel.ForEach(activeEntities, parallelOptions, entity =>
            {
                float[] inputs = inputComponent[entity].inputs;

               //hacer primera synapsis 
               outputComponent[entity].output = InputLayerSynapsis(entity, inputs);
               inputComponent[entity].inputs = outputComponent[entity].output;

               //hacer loop synapsis 
               for (int i = 0; i < hiddenLayerComponent[entity].hiddenLayers.Length; i++)
               {
                   outputComponent[entity].output = LayerSynapsis(entity, inputs, i);
                   inputs = outputComponent[entity].output;
               }
               
               //hacer ultima synapsis 
               outputComponent[entity].output = OutputLayerSynapsis(entity, inputs);
            });
        }

        protected override void PostExecute(float deltaTime)
        {
            
        }

        private float[] InputLayerSynapsis(uint entity, float[] inputs)
        {
            Parallel.For(0, inputs.Length,
                neuron => { outputComponent[entity].output[neuron] = InputNeuronSynapsis(entity, neuron, inputs); });
            return outputComponent[entity].output;
        }
        
        private float[] LayerSynapsis(uint entity, float[] inputs, int layer)
        {
            Parallel.For(0, inputs.Length,
                neuron => { outputComponent[entity].output[neuron] = NeuronSynapsis(entity, neuron, inputs, layer); });
            return outputComponent[entity].output;
        }
        
        private float[] OutputLayerSynapsis(uint entity, float[] inputs)
        {
            Parallel.For(0, inputs.Length,
                neuron => { outputComponent[entity].output[neuron] = OutputNeuronSynapsis(entity, neuron, inputs); });
            return outputComponent[entity].output;
        }
        
        private float InputNeuronSynapsis(uint entity, int neuron, float[] inputs)
        {
            ConcurrentBag<float> bag = new ConcurrentBag<float>();
            float a = 0;
            //TODO: preguntar
            Parallel.For(0, inputs.Length,
                b => { bag.Add(outputLayerComponent[entity].layer.weights[neuron,b] * inputs[b]); });
            a = bag.Sum();
            a += biasComponent[entity].X;

            return 1.0f / (1.0f + (float)Math.Exp(-a / sigmoidComponent[entity].X));
        }

        private float NeuronSynapsis(uint entity, int neuron, float[] inputs, int layer)
        {
            ConcurrentBag<float> bag = new ConcurrentBag<float>();
            float a = 0;
            Parallel.For(0, inputLayerComponent.Count,
                b => { bag.Add(hiddenLayerComponent[entity].hiddenLayers[layer].weights[neuron, b] * inputs[b]); });
            a = bag.Sum();
            a += biasComponent[entity].X;

            return 1.0f / (1.0f + (float)Math.Exp(-a / sigmoidComponent[entity].X));
        }
        
        private float OutputNeuronSynapsis(uint entity, int neuron, float[] inputs)
        {
            ConcurrentBag<float> bag = new ConcurrentBag<float>();
            float a = 0;
            Parallel.For(0, inputs.Length,
                b => { bag.Add(inputLayerComponent[entity].layer.weights[neuron,b] * inputs[b]); });
            a = bag.Sum();
            a += biasComponent[entity].X;

            return 1.0f / (1.0f + (float)Math.Exp(-a / sigmoidComponent[entity].X));
        }
    }
}