using System;
using VRage;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Interface for mouse input of a UI element.
        /// </summary>
        public interface IMouseInput
        {
            /// <summary>
            /// Invoked when the cursor enters the element's bounds
            /// </summary>
            event EventHandler CursorEntered;

            /// <summary>
            /// Invoked when the cursor leaves the element's bounds
            /// </summary>
            event EventHandler CursorExited;

            /// <summary>
            /// Invoked when the element is clicked with the left mouse button
            /// </summary>
            event EventHandler LeftClicked;

            /// <summary>
            /// Invoked when the left click is released
            /// </summary>
            event EventHandler LeftReleased;

            /// <summary>
            /// Invoked when the element is clicked with the right mouse button
            /// </summary>
            event EventHandler RightClicked;

            /// <summary>
            /// Invoked when the right click is released
            /// </summary>
            event EventHandler RightReleased;

            /// <summary>
            /// Invoked when taking focus
            /// </summary>
            event EventHandler GainedInputFocus;

            /// <summary>
            /// Invoked when focus is lost
            /// </summary>
            event EventHandler LostInputFocus;

            /// <summary>
            /// Determines whether the input element is enabled and accepting input
            /// </summary>
            bool InputEnabled { get; set; }

            /// <summary>
            /// True if the element is being clicked with the left mouse button
            /// </summary>
            bool IsLeftClicked { get; }

            /// <summary>
            /// True if the element is being clicked with the right mouse button
            /// </summary>
            bool IsRightClicked { get; }

            /// <summary>
            /// True if the element was just clicked with the left mouse button
            /// </summary>
            bool IsNewLeftClicked { get; }

            /// <summary>
            /// True if the element was just clicked with the right mouse button
            /// </summary>
            bool IsNewRightClicked { get; }

            /// <summary>
            /// True if the element was just released after being left clicked
            /// </summary>
            bool IsLeftReleased { get; }

            /// <summary>
            /// True if the element was just released after being right clicked
            /// </summary>
            bool IsRightReleased { get; }

            /// <summary>
            /// Indicates whether or not the cursor is currently over this element.
            /// </summary>
            bool HasFocus { get; }

            /// <summary>
            /// Returns true if the element is moused over
            /// </summary>
            bool IsMousedOver { get; }

            /// <summary>
            /// Gets input focus for keyboard controls. Input focus normally taken when an
            /// element with mouse input is clicked.
            /// </summary>
            void GetInputFocus();

            /// <summary>
            /// Clears all subscribers to mouse input events.
            /// </summary>
            void ClearSubscribers();
        }

        public interface IClickableElement : IReadOnlyHudElement
        {
            IMouseInput MouseInput { get; }
        }
    }
}