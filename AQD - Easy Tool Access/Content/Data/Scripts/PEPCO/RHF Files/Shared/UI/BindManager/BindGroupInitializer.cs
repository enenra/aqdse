using System;
using System.Collections.Generic;
using VRage;
using VRage.Input;

namespace RichHudFramework
{
    namespace UI
    {
        using Server;
        using Client;
        using System.Collections;

        /// <summary>
        /// Bind data container used to simplify bind registration.
        /// </summary>
        public class BindGroupInitializer : IReadOnlyList<MyTuple<string, IReadOnlyList<int>>>
        {
            public MyTuple<string, IReadOnlyList<int>> this[int index] => bindData[index];

            public int Count => bindData.Count;

            private readonly List<MyTuple<string, IReadOnlyList<int>>> bindData;

            public BindGroupInitializer()
            {
                bindData = new List<MyTuple<string, IReadOnlyList<int>>>();
            }

            public IEnumerator<MyTuple<string, IReadOnlyList<int>>> GetEnumerator() =>
                bindData.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                GetEnumerator();

            /// <summary>
            /// Adds a bind with the given name and the given key combo.
            /// </summary>
            public void Add(string bindName, string con1, string con2 = null, string con3 = null)
            {
                var names = new List<string>();

                if (con1 != null)
                    names.Add(con1);

                if (con2 != null)
                    names.Add(con2);

                if (con3 != null)
                    names.Add(con3);

                bindData.Add(new MyTuple<string, IReadOnlyList<int>>(bindName, BindManager.GetComboIndices(names)));
            }

            /// <summary>
            /// Adds a bind with the given name and the given key combo.
            /// </summary>
            public void Add(string bindName, int con1, int con2 = -1, int con3 = -1)
            {
                var indices = new List<int>();

                if (con1 != -1)
                    indices.Add(con1);

                if (con2 != -1)
                    indices.Add(con2);

                if (con3 != -1)
                    indices.Add(con3);

                bindData.Add(new MyTuple<string, IReadOnlyList<int>>(bindName, indices));
            }

            /// <summary>
            /// Adds a bind with the given name and the given key combo.
            /// </summary>
            public void Add(string bindName, ControlData con1 = null, ControlData con2 = null, ControlData con3 = null)
            {
                var indices = new List<int>();

                if (con1 != null)
                    indices.Add(con1);

                if (con2 != null)
                    indices.Add(con2);

                if (con3 != null)
                    indices.Add(con3);

                bindData.Add(new MyTuple<string, IReadOnlyList<int>>(bindName, indices));
            }

            /// <summary>
            /// Returns group data as a serializable array of BindDefinitions.
            /// </summary>
            public BindDefinition[] GetBindDefinitions()
            {
                var definitions = new BindDefinition[bindData.Count];

                for (int a = 0; a < definitions.Length; a++)
                {
                    var controlNames = new string[bindData[a].Item2.Count];

                    for (int b = 0; b < controlNames.Length; b++)
                        controlNames[b] = BindManager.Controls[bindData[a].Item2[b]].Name;

                    definitions[a] = new BindDefinition(bindData[a].Item1, controlNames);
                }

                return definitions;
            }
        }

        public class ControlData
        {
            public readonly int index;

            public ControlData(MyKeys key)
            {
                index = BindManager.GetControl(key).Index;
            }

            public ControlData(RichHudControls key)
            {
                index = BindManager.GetControl(key).Index;
            }

            public static implicit operator int(ControlData control) =>
                control.index;

            public static implicit operator ControlData(MyKeys key) =>
                new ControlData(key);

            public static implicit operator ControlData(RichHudControls key) =>
                new ControlData(key);
        }
    }
}