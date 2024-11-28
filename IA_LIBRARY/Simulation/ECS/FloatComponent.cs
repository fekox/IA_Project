namespace IA_Library_ECS
{
    /// <summary>
    /// The float component.
    /// </summary>
    public abstract class FloatComponent : ECSComponent
    {
        public float X;

        public FloatComponent(float X)
        {
            this.X = X;
        }
    }
}