using RichHudFramework.Internal;
using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
    namespace UI
    {
        using HudUpdateAccessors = MyTuple<
            ApiMemberAccessor,
            MyTuple<Func<ushort>, Func<Vector3D>>, // ZOffset + GetOrigin
            Action, // DepthTest
            Action, // HandleInput
            Action<bool>, // BeforeLayout
            Action // BeforeDraw
        >;

        /// <summary>
        /// Base class for HUD elements to which other elements are parented. Types deriving from this class cannot be
        /// parented to other elements; only types of <see cref="HudNodeBase"/> can be parented.
        /// </summary>
        public abstract partial class HudParentBase : IReadOnlyHudParent
        {
            /// <summary>
            /// Node defining the coordinate space used to render the UI element
            /// </summary>
            public virtual IReadOnlyHudSpaceNode HudSpace { get; protected set; }

            /// <summary>
            /// Returns true if the element can be drawn and/or accept input
            /// </summary>
            public virtual bool Visible
            {
                get { return (State & HudElementStates.IsVisible) > 0; }
                set
                {
                    if (value)
                        State |= HudElementStates.IsVisible;
                    else
                        State &= ~HudElementStates.IsVisible;
                }
            }

            /// <summary>
            /// Returns true if input is enabled can update
            /// </summary>
            public virtual bool InputEnabled
            {
                get { return (State & HudElementStates.IsInputEnabled) > 0; }
                set
                {
                    if (value)
                        State |= HudElementStates.IsInputEnabled;
                    else
                        State &= ~HudElementStates.IsInputEnabled;
                }
            }

            /// <summary>
            /// Determines whether the UI element will be drawn in the Back, Mid or Foreground
            /// </summary>
            public sbyte ZOffset
            {
                get { return layerData.zOffset; }
                set { layerData.zOffset = value; }
            }

            public HudElementStates State { get; protected set; }

            protected HudLayerData layerData;
            protected readonly List<HudNodeBase> children;
            protected HudUpdateAccessors accessorDelegates;

            public HudParentBase()
            {
                State |= HudElementStates.IsRegistered;
                InputEnabled = true;
                Visible = true;
                children = new List<HudNodeBase>();

                accessorDelegates = new HudUpdateAccessors()
                {
                    Item1 = GetOrSetApiMember,
                    Item2 = new MyTuple<Func<ushort>, Func<Vector3D>>(() => layerData.fullZOffset, null),
                    Item3 = BeginInputDepth,
                    Item4 = BeginInput,
                    Item5 = BeginLayout,
                    Item6 = BeginDraw
                };
            }

            /// <summary>
            /// Starts cursor depth check in a try-catch block. Useful for manually updating UI elements.
            /// Exceptions are reported client-side. Do not override this unless you have a good reason for it.
            /// If you need to do cursor depth testing use InputDepth();
            /// </summary>
            public virtual void BeginInputDepth()
            {
                if (!ExceptionHandler.ClientsPaused)
                {
                    try
                    {
                        if ((State & HudElementStates.CanUseCursor) > 0 && Visible && InputEnabled)
                            InputDepth();
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.ReportException(e);
                    }
                }
            }

            /// <summary>
            /// Starts input update in a try-catch block. Useful for manually updating UI elements.
            /// Exceptions are reported client-side. Do not override this unless you have a good reason for it.
            /// If you need to update input, use HandleInput().
            /// </summary>
            public virtual void BeginInput()
            {
                if (!ExceptionHandler.ClientsPaused)
                {
                    try
                    {
                        if (Visible && InputEnabled)
                        {
                            Vector3 cursorPos = HudSpace.CursorPos;
                            HandleInput(new Vector2(cursorPos.X, cursorPos.Y));
                        }
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.ReportException(e);
                    }
                }
            }

            /// <summary>
            /// Starts layout update in a try-catch block. Useful for manually updating UI elements.
            /// Exceptions are reported client-side. Do not override this unless you have a good reason for it.
            /// If you need to update layout, use Layout().
            /// </summary>
            public virtual void BeginLayout(bool refresh)
            {
                if (!ExceptionHandler.ClientsPaused)
                {
                    try
                    {
                        layerData.fullZOffset = ParentUtils.GetFullZOffset(layerData);

                        if (Visible || refresh)
                            Layout();
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.ReportException(e);
                    }
                }
            }

            /// <summary>
            /// Starts UI draw in a try-catch block. Useful for manually updating UI elements.
            /// Exceptions are reported client-side. Do not override this unless you have a good reason for it.
            /// If you need to draw billboards, use Draw().
            /// </summary>
            public virtual void BeginDraw()
            {
                if (!ExceptionHandler.ClientsPaused)
                {
                    try
                    {
                        if (Visible)
                            Draw();
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.ReportException(e);
                    }
                }
            }

            /// <summary>
            /// Used to check whether the cursor is moused over the element and whether its being
            /// obstructed by another element.
            /// </summary>
            protected virtual void InputDepth() { }

            /// <summary>
            /// Updates the input of this UI element. Invocation order affected by z-Offset and depth sorting.
            /// </summary>
            protected virtual void HandleInput(Vector2 cursorPos) { }

            /// <summary>
            /// Updates the layout of this UI element. Not affected by depth or z-Offset sorting.
            /// Executes before input and draw.
            /// </summary>
            protected virtual void Layout() { }

            /// <summary>
            /// Used to immediately draw billboards. Invocation order affected by z-Offset and depth sorting.
            /// </summary>
            protected virtual void Draw() { }

            /// <summary>
            /// Adds update delegates for members in the order dictated by the UI tree
            /// </summary>
            public virtual void GetUpdateAccessors(List<HudUpdateAccessors> UpdateActions, byte preloadDepth)
            {
                if (Visible)
                {
                    layerData.fullZOffset = ParentUtils.GetFullZOffset(layerData);

                    UpdateActions.EnsureCapacity(UpdateActions.Count + children.Count + 1);
                    accessorDelegates.Item2.Item2 = HudSpace.GetNodeOriginFunc;

                    UpdateActions.Add(accessorDelegates);

                    for (int n = 0; n < children.Count; n++)
                        children[n].GetUpdateAccessors(UpdateActions, preloadDepth);
                }
            }

            /// <summary>
            /// Registers a child node to the object.
            /// </summary>
            /// <param name="preregister">Adds the element to the update tree without registering.</param>
            public virtual bool RegisterChild(HudNodeBase child)
            {
                if (child.Parent == this && !child.Registered)
                {
                    children.Add(child);
                    return true;
                }
                else if (child.Parent == null)
                    return child.Register(this);
                else
                    return false;
            }

            /// <summary>
            /// Unregisters the specified node from the parent.
            /// </summary>
            /// <param name="fast">Prevents registration from triggering a draw list
            /// update. Meant to be used in conjunction with pooled elements being
            /// unregistered/reregistered to the same parent.</param>
            public virtual bool RemoveChild(HudNodeBase child)
            {
                if (child.Parent == this)
                    return child.Unregister();
                else if (child.Parent == null)
                    return children.Remove(child);
                else
                    return false;
            }

            protected virtual object GetOrSetApiMember(object data, int memberEnum)
            {
                switch ((HudElementAccessors)memberEnum)
                {
                    case HudElementAccessors.GetType:
                        return GetType();
                    case HudElementAccessors.ZOffset:
                        return ZOffset;
                    case HudElementAccessors.FullZOffset:
                        return layerData.fullZOffset;
                    case HudElementAccessors.Position:
                        return Vector2.Zero;
                    case HudElementAccessors.Size:
                        return Vector2.Zero;
                    case HudElementAccessors.GetHudSpaceFunc:
                        return HudSpace?.GetHudSpaceFunc;
                    case HudElementAccessors.ModName:
                        return ExceptionHandler.ModName;
                    case HudElementAccessors.LocalCursorPos:
                        return HudSpace?.CursorPos ?? Vector3.Zero;
                    case HudElementAccessors.PlaneToWorld:
                        return HudSpace?.PlaneToWorldRef[0] ?? default(MatrixD);
                    case HudElementAccessors.IsInFront:
                        return HudSpace?.IsInFront ?? false;
                    case HudElementAccessors.IsFacingCamera:
                        return HudSpace?.IsFacingCamera ?? false;
                    case HudElementAccessors.NodeOrigin:
                        return HudSpace?.PlaneToWorldRef[0].Translation ?? Vector3D.Zero;
                }

                return null;
            }
        }
    }
}