﻿using Molten.Collections;
using Molten.Graphics;
using Molten.Input;

namespace Molten.UI
{
    /// <summary>
    /// A <see cref="SceneComponent"/> used for updating and rendering a UI system into a <see cref="Scene"/>.
    /// </summary>
    public sealed class UIManagerComponent : SpriteRenderComponent, IPointerReceiver
    {
        UIElement _root;
        ThreadedQueue<IUIChange> _pendingChanges = new ThreadedQueue<IUIChange>();

        protected override void OnDispose()
        {
            
        }

        public void HandleInput(Vector2F inputPos)
        {
            // TODO Handle keyboard input/focusing here.
        }

        internal void QueueChange(IUIChange change)
        {
            _pendingChanges.Enqueue(change);
        }

        public override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);

            if (Root == null)
                return;

            Root.Update(time);
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            if (Root == null)
                return;

            IUIChange change;
            while (_pendingChanges.TryDequeue(out change))
                change.Process();

            Root.Render(sb);
        }

        public bool Contains(Vector2F point)
        {
            if (Root != null)
                return PickElement(Root, point) != null;
            else
                return false;
        }

        private UIElement PickElement(UIElement e, in Vector2F point)
        {
            UIElement result = null;

            if (e.Contains(point))
            {
                for (int i = e.Children.Count - 1; i >= 0; i--)
                {
                    result = PickElement(e.Children[i], point);
                    if (result != null)
                        return result;
                }

                if (e.Contains(point))
                    return e;
            }

            return result;
        }

        public void CursorClickStarted(Vector2F pos, MouseButton button)
        {
            
        }

        public void CursorClickCompletedOutside(Vector2F pos, MouseButton button)
        {
            
        }

        public void CursorClickCompleted(Vector2F pos, bool wasDragged, MouseButton button)
        {
            
        }

        public void CursorWheelScroll(InputScrollWheel wheel)
        {
            
        }

        public void CursorEnter(Vector2F pos)
        {
            
        }

        public void CursorLeave(Vector2F pos)
        {
           
        }

        public void CursorHover(Vector2F pos)
        {
            if (Root != null)
                HoverElement = PickElement(Root, pos);
            else
                HoverElement = null;
        }

        public void CursorFocus()
        {
            
        }

        public void CursorDrag(Vector2F pos, Vector2F delta, MouseButton button)
        {
            
        }

        public void CursorUnfocus()
        {
           
        }

        public void CursorHeld(Vector2F pos, Vector2F delta, MouseButton button)
        {
            throw new NotImplementedException();
        }

        public void TouchStarted(Vector2F pos, in TouchPointState state)
        {
            throw new NotImplementedException();
        }

        public void TouchCompleted(Vector2F pos, in TouchPointState state)
        {
            throw new NotImplementedException();
        }

        public void TouchDrag(Vector2F pos, in TouchPointState state)
        {
            throw new NotImplementedException();
        }

        public void TouchHeld(Vector2F pos, in TouchPointState state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the Root <see cref="UIElement"/> to be drawn.
        /// </summary>
        public UIElement Root
        {
            get => _root;
            set
            {
                if (_root != value)
                {
                    if (_root != null)
                    {
                        _root.Root = null;
                        _root.RenderComponent = null;
                    }

                    _root = value;

                    if (_root != null)
                    {
                        _root.RenderComponent = this;
                        _root.Root = _root;
                    }
                }
            }
        }

        public UIElement HoverElement { get; private set; }

        public string Tooltip => Name;
    }
}