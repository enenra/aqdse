using System;
using VRageMath;
using RichHudFramework.UI.Rendering;
using RichHudFramework.Internal;

namespace RichHudFramework.UI
{
    using Client;
    using Server;

    /// <summary>
    /// Base type for HUD windows. Supports dragging/resizing like pretty much every other window ever.
    /// </summary>
    public abstract class WindowBase : HudElementBase, IClickableElement
    {
        /// <summary>
        /// Window header text
        /// </summary>
        public RichText HeaderText { get { return HeaderBuilder.GetText(); } set { HeaderBuilder.SetText(value); } }

        /// <summary>
        /// Text builder for the window header
        /// </summary>
        public ITextBuilder HeaderBuilder => header.TextBoard;

        /// <summary>
        /// Determines the color of both the header and the border.
        /// </summary>
        public virtual Color BorderColor
        {
            get { return header.Color; }
            set
            {
                header.Color = value;
                border.Color = value;
            }
        }

        /// <summary>
        /// Determines the color of the body of the window.
        /// </summary>
        public virtual Color BodyColor { get { return bodyBg.Color; } set { bodyBg.Color = value; } }

        /// <summary>
        /// Minimum allowable size for the window.
        /// </summary>
        public Vector2 MinimumSize { get { return minimumSize; } set { minimumSize = value; } }

        /// <summary>
        /// Determines whether or not the window can be resized by the user
        /// </summary>
        public bool AllowResizing { get; set; }

        /// <summary>
        /// Determines whether or not the user can reposition the window
        /// </summary>
        public bool CanDrag { get; set; }

        /// <summary>
        /// Returns true if the window has focus and is accepting input
        /// </summary>
        public bool WindowActive { get; protected set; }

        /// <summary>
        /// Returns true if the cursor is over the window
        /// </summary>
        public override bool IsMousedOver => resizeInput.IsMousedOver;

        /// <summary>
        /// Mouse input element for the window
        /// </summary>
        public IMouseInput MouseInput => resizeInput;

        /// <summary>
        /// Window header element
        /// </summary>
        public readonly LabelBoxButton header;

        /// <summary>
        /// Textured background. Body of the window
        /// </summary>
        public readonly HudElementBase body;

        /// <summary>
        /// Window border
        /// </summary>
        public readonly BorderBox border;

        protected readonly MouseInputElement inputInner, resizeInput;
        protected readonly TexturedBox bodyBg;

        protected readonly Action<byte> LoseFocusCallback;
        protected float cornerSize = 16f;
        protected bool canMoveWindow, canResize;
        protected int resizeDir;
        protected Vector2 cursorOffset, minimumSize;

        public WindowBase(HudParentBase parent) : base(parent)
        {
            header = new LabelBoxButton(this)
            {
                DimAlignment = DimAlignments.Width,
                Height = 32f,
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner,
                ZOffset = 1,
                Format = GlyphFormat.White.WithAlignment(TextAlignment.Center),
                HighlightEnabled = false,
                AutoResize = false,
            };

            body = new EmptyHudElement(header)
            {
                DimAlignment = DimAlignments.Width,
                ParentAlignment = ParentAlignments.Bottom,
            };

            bodyBg = new TexturedBox(body)
            {
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
                ZOffset = -2,
            };

            border = new BorderBox(this)
            {
                ZOffset = 1,
                Thickness = 1f,
                DimAlignment = DimAlignments.Both,
            };

            resizeInput = new MouseInputElement(this)
            {
                ZOffset = sbyte.MaxValue,
                Padding = new Vector2(16f),
                DimAlignment = DimAlignments.Both,
                CanIgnoreMasking = true
            };
            
            inputInner = new MouseInputElement(resizeInput)
            {
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
            };

            AllowResizing = true;
            CanDrag = true;
            UseCursor = true;
            ShareCursor = false;
            IsMasking = true;
            MinimumSize = new Vector2(620f, 200f);

            LoseFocusCallback = LoseFocus;
            GetFocus();
        }

        protected override void Layout()
        {
            body.Height = Height - header.Height;

            if (Visible && WindowActive)
            {
                if (canMoveWindow)
                {
                    Vector3 cursorPos = HudSpace.CursorPos;
                    Offset = new Vector2(cursorPos.X, cursorPos.Y) + cursorOffset - Origin;
                }

                if (canResize)
                    Resize();
            }
            else
            {
                canMoveWindow = false;
                canResize = false;
            }
        }

        protected void Resize()
        {
            Vector3 cursorPos = HudSpace.CursorPos;
            Vector2 center = Origin + Offset, newOffset = Offset;
            float newWidth, newHeight;

            // 1 == horizontal, 3 == both
            if (resizeDir == 1 || resizeDir == 3)
            {
                newWidth = Math.Abs(newOffset.X - cursorPos.X) + Width * .5f;

                if (newWidth >= MinimumSize.X)
                {
                    Width = newWidth;

                    if (cursorPos.X > center.X)
                        newOffset.X = cursorPos.X - Width * .5f;
                    else
                        newOffset.X = cursorPos.X + Width * .5f;
                }
            }

            // 2 == vertical
            if (resizeDir == 2 || resizeDir == 3)
            {
                newHeight = Math.Abs(newOffset.Y - cursorPos.Y) + Height * .5f;

                if (newHeight >= MinimumSize.Y)
                {
                    Height = newHeight;

                    if (cursorPos.Y > center.Y)
                        newOffset.Y = cursorPos.Y - Height * .5f;
                    else
                        newOffset.Y = cursorPos.Y + Height * .5f;
                }
            }

            Offset = newOffset;
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (IsMousedOver)
            {
                if (SharedBinds.LeftButton.IsNewPressed && !WindowActive)
                    GetFocus();
            }

            if (AllowResizing && resizeInput.IsNewLeftClicked && !inputInner.IsMousedOver)
            {
                Vector2 pos = Origin + Offset;
                canResize = true;
                resizeDir = 0;

                if (Width - (2f) * Math.Abs(pos.X - cursorPos.X) <= cornerSize)
                    resizeDir += 1;

                if (Height - (2f) * Math.Abs(pos.Y - cursorPos.Y) <= cornerSize)
                    resizeDir += 2;
            }
            else if (CanDrag && header.MouseInput.IsNewLeftClicked)
            {
                canMoveWindow = true;
                cursorOffset = (Origin + Offset) - cursorPos;
            }

            if (canResize || canMoveWindow)
            {
                if (!SharedBinds.LeftButton.IsPressed)
                {
                    canMoveWindow = false;
                    canResize = false;
                }
            }
        }

        /// <summary>
        /// Brings the window into the foreground
        /// </summary>
        public virtual void GetFocus()
        {
            layerData.zOffsetInner = HudMain.GetFocusOffset(LoseFocusCallback);
            WindowActive = true;
        }

        protected virtual void LoseFocus(byte newOffset)
        {
            layerData.zOffsetInner = newOffset;
            WindowActive = false;
        }
    }
}