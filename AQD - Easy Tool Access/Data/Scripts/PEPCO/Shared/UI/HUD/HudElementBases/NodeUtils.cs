using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    namespace UI
    {
        using Server;
        using Client;

        public abstract partial class HudNodeBase
        {
            /// <summary>
            /// Collection of utilities used internally to manage HUD nodes
            /// </summary>
            protected static class NodeUtils
            {
                /// <summary>
                /// Used internally quickly register a list of child nodes to a parent.
                /// </summary>
                public static void RegisterNodes(HudParentBase newParent, List<HudNodeBase> children, IReadOnlyList<HudNodeBase> nodes, bool canPreload)
                {
                    children.EnsureCapacity(children.Count + nodes.Count);

                    for (int n = 0; n < nodes.Count; n++)
                    {
                        HudNodeBase node = nodes[n];
                        node.Parent = newParent;
                        node.State |= HudElementStates.IsRegistered;
                        node.ParentVisible = newParent.Visible;

                        children.Add(node);

                        if (canPreload)
                            node.State |= HudElementStates.CanPreload;
                        else
                            node.State &= ~HudElementStates.CanPreload;
                    }
                }

                /// <summary>
                /// Used internally quickly register a list of child nodes to a parent.
                /// </summary>
                public static void RegisterNodes<TCon, TNode>(HudParentBase newParent, List<HudNodeBase> children, IReadOnlyList<TCon> nodes, bool canPreload)
                    where TCon : IHudElementContainer<TNode>, new()
                    where TNode : HudNodeBase
                {
                    children.EnsureCapacity(children.Count + nodes.Count);

                    for (int n = 0; n < nodes.Count; n++)
                    {
                        HudNodeBase node = nodes[n].Element;
                        node.Parent = newParent;
                        node.State |= HudElementStates.IsRegistered;
                        node.ParentVisible = newParent.Visible;

                        children.Add(node);

                        if (canPreload)
                            node.State |= HudElementStates.CanPreload;
                        else
                            node.State &= ~HudElementStates.CanPreload;
                    }
                }

                /// <summary>
                /// Used internally to quickly unregister child nodes from their parent. Removes the range of nodes
                /// specified in the node list from the child list.
                /// </summary>
                public static void UnregisterNodes(HudParentBase parent, List<HudNodeBase> children, IReadOnlyList<HudNodeBase> nodes, int index, int count)
                {
                    if (count > 0)
                    {
                        int conEnd = index + count - 1;

                        if (!(index >= 0 && index < nodes.Count && conEnd <= nodes.Count))
                            throw new Exception("Specified indices are out of range.");

                        if (parent == null)
                            throw new Exception("Parent cannot be null");

                        for (int i = index; i <= conEnd; i++)
                        {
                            int start = 0;

                            while (start < children.Count && children[start] != nodes[i])
                                start++;

                            if (children[start] == nodes[i])
                            {
                                int j = start, end = start;

                                while (j < children.Count && i <= conEnd && children[j] == nodes[i])
                                {
                                    end = j;
                                    i++;
                                    j++;
                                }

                                children.RemoveRange(start, end - start + 1);
                            }
                        }

                        for (int n = index; n < count; n++)
                        {
                            HudNodeBase node = nodes[n];
                            HudParentBase nodeParent = node._parent;

                            if (nodeParent != parent)
                                throw new Exception("The child node specified is not registered to the parent given.");

                            node.Parent = null;
                            node.State &= ~(HudElementStates.IsRegistered | HudElementStates.WasParentVisible);
                            node.ParentVisible = false;
                        }
                    }
                }

                /// <summary>
                /// Used internally to quickly unregister child nodes from their parent. Removes the range of nodes
                /// specified in the node list from the child list.
                /// </summary>
                public static void UnregisterNodes<TCon, TNode>(HudParentBase parent, List<HudNodeBase> children, IReadOnlyList<TCon> nodes, int index, int count)
                    where TCon : IHudElementContainer<TNode>, new()
                    where TNode : HudNodeBase
                {
                    if (count > 0)
                    {
                        int conEnd = index + count - 1;

                        if (!(index >= 0 && index < nodes.Count && conEnd <= nodes.Count))
                            throw new Exception("Specified indices are out of range.");

                        if (parent == null)
                            throw new Exception("Parent cannot be null");

                        for (int i = index; i <= conEnd; i++)
                        {
                            int start = 0;

                            while (start < children.Count && children[start] != nodes[i].Element)
                                start++;

                            if (children[start] == nodes[i].Element)
                            {
                                int j = start, end = start;

                                while (j < children.Count && i <= conEnd && children[j] == nodes[i].Element)
                                {
                                    end = j;
                                    i++;
                                    j++;
                                }

                                children.RemoveRange(start, end - start + 1);
                            }
                        }

                        for (int n = index; n < count; n++)
                        {
                            HudNodeBase node = nodes[n].Element;
                            HudParentBase nodeParent = node._parent;

                            if (nodeParent != parent)
                                throw new Exception("The child node specified is not registered to the parent given.");

                            node.Parent = null;
                            node.State &= ~(HudElementStates.IsRegistered | HudElementStates.WasParentVisible);
                        }
                    }
                }

                /// <summary>
                /// Used internally to modify the state of hud nodes
                /// </summary>
                public static void SetNodesState(HudElementStates state, bool mask, IReadOnlyList<HudNodeBase> nodes, int index, int count)
                {
                    if (count > 0)
                    {
                        int end = index + count - 1;
                        Utils.Debug.Assert(index >= 0 && end < nodes.Count, $"Range out of bounds. Index: {index}, End: {end}");

                        if (mask)
                        {
                            for (int i = index; i <= end; i++)
                                nodes[i].State &= ~state;
                        }
                        else
                        {
                            for (int i = index; i <= end; i++)
                                nodes[i].State |= state;
                        }
                    }
                }

                /// <summary>
                /// Used internally to modify the state of hud nodes
                /// </summary>
                public static void SetNodesState<TCon, TNode>(HudElementStates state, bool mask, IReadOnlyList<TCon> nodes, int index, int count)
                    where TCon : IHudElementContainer<TNode>, new()
                    where TNode : HudNodeBase
                {
                    if (count > 0)
                    {
                        int end = index + count - 1;
                        Utils.Debug.Assert(index >= 0 && end < nodes.Count, $"Range out of bounds. Index: {index}, End: {end}");

                        if (mask)
                        {
                            for (int i = index; i <= end; i++)
                                nodes[i].Element.State &= ~state;
                        }
                        else
                        {
                            for (int i = index; i <= end; i++)
                                nodes[i].Element.State |= state;
                        }
                    }
                }
            }
        }
    }
}