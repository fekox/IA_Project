namespace IA_Library_ECS
{
    public abstract class FloatComponent : ECSComponent
    {
        public float X;

        public FloatComponent(float X)
        {
            this.X = X;
        }
    }
}