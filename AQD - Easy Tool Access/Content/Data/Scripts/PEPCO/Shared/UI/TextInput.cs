
using RichHudFramework.UI.Rendering;
using Sandbox.ModAPI;
using System.Text;
using VRage.Collections;
using System;
using VRageMath;

namespace RichHudFramework.UI
{
    public class TextInput
    {
        public Func<char, bool> IsCharAllowedFunc;

        private readonly Action<char> OnAppendAction;
        private readonly Action OnBackspaceAction;

        public TextInput(Action<char> OnAppendAction, Action OnBackspaceAction, Func<char, bool> IsCharAllowedFunc = null) 
        {
            this.OnAppendAction = OnAppendAction;
            this.OnBackspaceAction = OnBackspaceAction;
            this.IsCharAllowedFunc = IsCharAllowedFunc;
        }

        private void Backspace()
        {
            OnBackspaceAction?.Invoke();
        }

        public void HandleInput()
        {
            ListReader<char> input = MyAPIGateway.Input.TextInput;

            if (SharedBinds.Back.IsPressedAndHeld || SharedBinds.Back.IsNewPressed)
                Backspace();

            for (int n = 0; n < input.Count; n++)
            {
                if (input[n] != '\b' && (IsCharAllowedFunc == null || IsCharAllowedFunc(input[n])))
                {
                    OnAppendAction?.Invoke(input[n]);
                }
            }
        }
    }
}