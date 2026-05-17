namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Interface for controls used by the bind manager
        /// </summary> 
        public interface IControl
        {
            /// <summary>
            /// Name of the control
            /// </summary>
            string Name { get; }

            /// <summary>
            /// Name of the control as displayed in bind menu
            /// </summary>
            string DisplayName { get; }

            /// <summary>
            /// Index of the control in the bind manager
            /// </summary>
            int Index { get; }

            /// <summary>
            /// Returns true if the control is being pressed
            /// </summary>
            bool IsPressed { get; }

            /// <summary>
            /// Returns true if the control was just pressed
            /// </summary>
            bool IsNewPressed { get; }

            /// <summary>
            /// Returns true if the control was just released
            /// </summary>
            bool IsReleased { get; }

            // <summary>
            /// Returns true if the control doesn't represent a boolean value. For example, MwUp/Dn
            /// represent scroll wheel movement, but don't return an exact position/displacement.
            /// </summary>
            bool Analog { get; }
        }

        public enum ControlAccessors : int
        {
            /// <summary>
            /// out: string
            /// </summary>
            Name = 1,

            /// <summary>
            /// out: string
            /// </summary>
            DisplayName = 2,

            /// <summary>
            /// out: int
            /// </summary>
            Index = 3,

            /// <summary>
            /// out: bool
            /// </summary>
            IsPressed = 4,

            /// <summary>
            /// out: bool
            /// </summary>
            Analog = 5,

            /// <summary>
            /// out: bool
            /// </summary>
            IsNewPressed = 6,

            /// <summary>
            /// out: bool
            /// </summary>
            IsReleased = 7
        }
    }
}