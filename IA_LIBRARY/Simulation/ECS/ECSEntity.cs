using System;
using System.Collections.Generic;

namespace IA_Library_ECS
{
    /// <summary>
    /// Create and manage the ECS entity.
    /// </summary>
    public class ECSEntity
    {
        /// <summary>
        /// Create the entity ID.
        /// </summary>
        private class EntityID
        {
            private static uint LastEntityID = 0;
            internal static uint GetNew() => LastEntityID++;
        }

        private uint ID;
        private List<Type> componentsType;

        /// <summary>
        /// Create the ECS entity.
        /// </summary>
        public ECSEntity()
        {
            ID = EntityID.GetNew();
            componentsType = new List<Type>();
        }

        public uint GetID() => ID;

        /// <summary>
        /// Crear the components list.
        /// </summary>
        public void Dispose()
        {
            componentsType.Clear();
        }

        /// <summary>
        /// Add a type of component.
        /// </summary>
        /// <typeparam name="ComponentType">Type of component</typeparam>
        public void AddComponentType<ComponentType>() where ComponentType : ECSComponent
        {
            AddComponentType(typeof(ComponentType));
        }

        /// <summary>
        /// Add a type of component.
        /// </summary>
        /// <param name="ComponentType">Type of component</param>
        public void AddComponentType(Type ComponentType)
        {
            componentsType.Add(ComponentType);
        }

        /// <summary>
        /// Return if contains a component type or not.
        /// </summary>
        /// <typeparam name="ComponentType">Type of component</typeparam>
        /// <returns>if contains a component type or not</returns>
        public bool ContainsComponentType<ComponentType>() where ComponentType : ECSComponent
        {
            return ContainsComponentType(typeof(ComponentType));
        }

        /// <summary>
        /// Return if contains a component type or not.
        /// </summary>
        /// <param name="ComponentType">Type of component</param>
        /// <returns>if contains a component type or not</returns>
        public bool ContainsComponentType(Type ComponentType)
        {
            return componentsType.Contains(ComponentType);
        }
    }
}