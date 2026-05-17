namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Read-only interface for hud elements that can be parented to another element.
        /// </summary>
        public interface IReadOnlyHudNode : IReadOnlyHudParent
        {
            /// <summary>
            /// Parent object of the node.
            /// </summary>
            IReadOnlyHudParent Parent { get; }

            /// <summary>
            /// Indicates whether or not the node has been registered to its parent.
            /// </summary>
            bool Registered { get; }
        }
    }
}