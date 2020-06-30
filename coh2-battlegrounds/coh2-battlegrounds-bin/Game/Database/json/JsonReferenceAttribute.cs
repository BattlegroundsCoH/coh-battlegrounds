using System;

namespace Battlegrounds.Game.Database.json {

    /// <summary>
    /// <see cref="Attribute"/> for marking a field as using the Json reference value when serializing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class JsonReferenceAttribute : Attribute {
    
        /// <summary>
        /// The <see cref="Type"/> that contains the derefence method.
        /// </summary>
        public Type DereferenceType { get; }

        /// <summary>
        /// Referencable json attribute without a dereference method type defined.
        /// </summary>
        public JsonReferenceAttribute() {
            this.DereferenceType = null;
        }

        /// <summary>
        /// Referencable json attribute with a type containing a dereference method.
        /// </summary>
        /// <param name="derefType">The type of object with dereference method.</param>
        /// <exception cref="ArgumentException"/>
        public JsonReferenceAttribute(Type derefType) {
            this.DereferenceType = derefType;
            if (this.DereferenceType.GetMethod("JsonDereference", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public) == null) {
                throw new ArgumentException($"Type '{derefType.Name}' does not have static method 'JsonDereference'");
            }
        }

        /// <summary>
        /// Get a static method pointer to the dereference method.
        /// </summary>
        /// <returns>A reference to the dereference method. Null if no type is specified.</returns>
        public Func<string, object> GetReferenceFunction()
            => (this.DereferenceType != null) ? (Func<string, object>)this.DereferenceType
            .GetMethod("JsonDereference", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
            .CreateDelegate(typeof(Func<string, object>)) : null;

    }

}
