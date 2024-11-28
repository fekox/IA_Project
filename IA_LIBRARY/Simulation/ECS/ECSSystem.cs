namespace IA_Library_ECS
{
    /// <summary>
    /// ECS system manager.
    /// </summary>
    public abstract class ECSSystem
    {
        public void Run(float deltaTime)
        {
            PreExecute(deltaTime);
            Execute(deltaTime);
            PostExecute(deltaTime);
        }

        public abstract void Initialize();

        protected abstract void PreExecute(float deltaTime);

        protected abstract void Execute(float deltaTime);

        protected abstract void PostExecute(float deltaTime);
    }

    /// <summary>
    /// ECS responce manager.
    /// </summary>
    public abstract class ECSResponce
    {
        public void Run(float deltaTime)
        {
            PreExecute(deltaTime);
            Execute(deltaTime);
            PostExecute(deltaTime);
        }

        public abstract void Initialize();

        protected abstract void PreExecute(float deltaTime);

        protected abstract void Execute(float deltaTime);

        protected abstract void PostExecute(float deltaTime);
    }
}