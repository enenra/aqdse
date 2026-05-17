using System;
using System.Collections.Generic;

namespace RichHudFramework
{
    namespace UI
    {
        public interface IBind
        {
            /// <summary>
            /// Name of the keybind
            /// </summary>
            string Name { get; }

            /// <summary>
            /// Index of the bind within its group
            /// </summary>
            int Index { get; }

            /// <summary>
            /// True if any controls in the bind are marked analog. For these types of binds, IsPressed == IsNewPressed.
            /// </summary>
            bool Analog { get; }

            /// <summary>
            /// True if just pressed.
            /// </summary>
            bool IsNewPressed { get; }

            /// <summary>
            /// True if currently pressed.
            /// </summary>
            bool IsPressed { get; }

            /// <summary>
            /// True after being held for more than 500ms.
            /// </summary>
            bool IsPressedAndHeld { get; }

            /// <summary>
            /// True if just released.
            /// </summary>
            bool IsReleased { get; }

            /// <summary>
            /// Invoked when the bind is first pressed.
            /// </summary>
            event EventHandler NewPressed;

            /// <summary>
            /// Invoked after the bind has been held and pressed for at least 500ms.
            /// </summary>
            event EventHandler PressedAndHeld;

            /// <summary>
            /// Invoked after the bind has been released.
            /// </summary>
            event EventHandler Released;

            /// <summary>
            /// Returns a list of the current key combo for this bind.
            /// </summary>
            List<IControl> GetCombo();

            /// <summary>
            /// Returns a list of control indices for the current bind combo
            /// </summary>
            List<int> GetComboIndices();

            /// <summary>
            /// Attempts to set the binds combo to the given controls. Returns true if successful.
            /// </summary>
            bool TrySetCombo(IReadOnlyList<IControl> combo, bool strict = true, bool silent = true);

            /// <summary>
            /// Attempts to set the binds combo to the given controls. Returns true if successful.
            /// </summary>
            bool TrySetCombo(IReadOnlyList<int> combo, bool strict = true, bool silent = true);

            /// <summary>
            /// Attempts to set the binds combo to the given controls. Returns true if successful.
            /// </summary>
            bool TrySetCombo(IReadOnlyList<string> combo, bool strict = true, bool silent = true);

            /// <summary>
            /// Clears the current key combination.
            /// </summary>
            void ClearCombo();

            /// <summary>
            /// Clears all event subscibers for this bind.
            /// </summary>
            void ClearSubscribers();
        }

        public enum BindAccesssors : int
        {
            /// <summary>
            /// out: <see cref="string"/>
            /// </summary>
            Name = 1,

            /// <summary>
            /// out: <see cref="bool"/>
            /// </summary>
            Analog = 2,

            /// <summary>
            /// out: index
            /// </summary>
            Index = 3,

            /// <summary>
            /// out: bool
            /// </summary>
            IsPressed = 4,

            /// <summary>
            /// out: bool
            /// </summary>
            IsNewPressed = 5,

            /// <summary>
            /// out: bool
            /// </summary>
            IsPressedAndHeld = 6,

            /// <summary>
            /// out: bool
            /// </summary>
            IsReleased = 7,

            /// <summary>
            /// in: MyTuple(bool, Action)
            /// </summary>
            OnNewPress = 8,

            /// <summary>
            /// in: MyTuple(bool, Action)
            /// </summary>
            OnPressAndHold = 9,

            /// <summary>
            /// in: MyTuple(bool, Action)
            /// </summary>
            OnRelease = 10,

            /// <summary>
            /// out: <see cref="List{T}(int)"/>
            /// </summary>
            GetCombo = 11,

            /// <summary>
            /// in: MyTuple{List{int}, bool}, out: bool"
            /// </summary>
            TrySetComboWithIndices = 12,

            /// <summary>
            /// in: MyTuple{List{string}, bool}, out: bool"
            /// </summary>
            TrySetComboWithNames = 13,

            /// <summary>
            /// void
            /// </summary>
            ClearCombo = 14,

            /// <summary>
            /// void
            /// </summary>
            ClearSubscribers = 15,
        }

    }
}