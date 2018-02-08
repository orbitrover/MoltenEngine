﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Molten.IO;
using Molten.Graphics;
using Molten.Utilities;
using Molten.Graphics.Textures;
using Molten.Input;

namespace Molten.UI
{
    public class UISystem : EngineObject
    {
        const int PROXY_BIAS = 5000000;

        UITooltip _tooltip;
        UIContainer _screen;
        Engine _engine;
        InputManager _input;
        UIWindowManager _windowManager;

        string _defaultFontName = "Arial";
        int _defaultFontSize = 16;
        bool _inputEnabled = true;

        List<UIClickTracker> _trackers;
        Dictionary<MouseButton, UIClickTracker> _trackerByButton;
        UIComponent _focused;
        UIComponent _hoverComponent = null;

        double _tooltipTimer;
        float _tooltipDelay = 500;
        float _dragThreshold = 10.0f;
        IWindowSurface _surface;


        public event UIComponentEventHandler<MouseButton> OnFocus;
        public event UIComponentEventHandler<MouseButton> OnUnfocus;

        internal UISystem(IWindowSurface surface, Engine engine)
        {
            _surface = surface;
            _engine = engine;
            _input = _engine.Input;
            _windowManager = new UIWindowManager(this);
            _trackers = new List<UIClickTracker>();
            _trackerByButton = new Dictionary<MouseButton, UIClickTracker>();

            AddTracker(MouseButton.Left);
            AddTracker(MouseButton.Right);
            AddTracker(MouseButton.Middle);

            // Setup screen component
            _screen = new UIContainer(this)
            {
                LocalBounds = new Rectangle()
                {
                    Width = surface.Width,
                    Height = surface.Height,
                },
                Name = "Screen",
            };

            surface.OnPostResize += MainOutput_OnResize;

            _tooltip = new UITooltip(this);
        }

        void MainOutput_OnResize(ITexture surface)
        {
            _screen.LocalBounds = new Rectangle()
            {
                X = 0,
                Y = 0,
                Width = _surface.Width,
                Height = _surface.Height,
            };
        }

        /// <summary>Removes all active components from the UI system.</summary>
        public void Clear()
        {
            _screen.Children.Clear();
        }

        public void SetFocus(UIComponent component)
        {
            if (_focused != component && component != null)
            {
                component.InvokeFocus();

                OnFocus?.Invoke(new UIEventData<MouseButton>()
                {
                    Component = component,
                    Position = component.GlobalBounds.TopLeft,
                });
            }

            _focused = component;
        }

        public void Unfocus()
        {
            if (_focused != null)
            {
                _focused.InvokeUnfocus();

                OnUnfocus?.Invoke(new UIEventData<MouseButton>()
                {
                    Component = _focused,
                    Position = _focused.GlobalBounds.TopLeft,
                });
            }
        
            _focused = null;
        }

        /// <summary>Adds a root component to UI system.</summary>
        /// <param name="component">The component or parent UI to add to the system.</param>
        /// <returns>True if successful.</returns>
        public void AddUI(UIComponent component)
        {
            UIComponent matchingChild = null;

            if (component.Name == _screen.Name && component != _screen)
            {
                UIComponent child = null;

                // Take all of it's children and add them individuaally
                for (int i = 0; i < component.Children.Count; i++)
                {
                    child = component.Children[i];

                    matchingChild = _screen.HasChild(child.Name);
                    if (matchingChild == null)
                        _screen.AddChild(child);
                }
            }
            else {
                // Check if a child of the same name exists.
                matchingChild = _screen.HasChild(component.Name);
                if (matchingChild == null)
                    _screen.AddChild(component);
            }
        }

        /// <summary>Removes a container component from the UI system.</summary>
        /// <param name="childName">The name of the container to remove.</param>
        /// <returns>True if the component was successfully removed, false if it wasn't found.</returns>
        public void RemoveUI(string childName)
        {
            UIComponent matchingChild = null;

            if (childName == _screen.Name)
            {
                for (int i = _screen.Children.Count - 1; i >= 0; i--)
                    _screen.RemoveChild(_screen.Children[i]);
            }
            else {

                matchingChild = _screen.HasChild(childName);

                if (matchingChild != null)
                    _screen.RemoveChild(matchingChild);
            }
        }

        public bool RemoveUI(UIComponent container)
        {
            return _screen.RemoveChild(container);
        }

        private void AddTracker(MouseButton button)
        {
            UIClickTracker tracker = new UIClickTracker(button);
            _trackers.Add(tracker);
            _trackerByButton.Add(button, tracker);
        }

        private void SetDragThreshold(float threshold)
        {
            _dragThreshold = threshold;
            for (int i = 0; i < _trackers.Count; i++)
                _trackers[i].DragThreshold = threshold;
        }

        private UIComponent HandlePressStarted(Vector2 inputPos)
        {
            //check message dialog first
            //if (_mDialog.HandleInput(inputPos) == true) return null;

            UIComponent result = _screen.GetComponent(inputPos);

            return result;
        }

        internal void Update(Timing time)
        {
            Vector2 mousePos = _input.Mouse.Position;
            Vector2 mouseMove = _input.Mouse.Moved;

            //----UPDATE----
            _screen.Update(time);

            UIComponent newHover = HandlePressStarted(mousePos);

            if (newHover == null)
            {
                //trigger leave on previous hover component.
                if (_hoverComponent != null)
                {
                    _hoverComponent.InvokeLeave(mousePos);

                    // Set tooltip.
                    _tooltip.Text.Text = "";
                }

                //set new-current as null.
                _hoverComponent = null;
            }
            else
            {
                if (_hoverComponent != newHover)
                {
                    //trigger leave on old hover component.
                    if (_hoverComponent != null)
                        _hoverComponent.InvokeLeave(mousePos);

                    //set new hover component and trigger it's enter event
                    _hoverComponent = newHover;
                    _hoverComponent.InvokeEnter(mousePos);

                    // Set tooltip.
                    _tooltipTimer = 0;
                    _tooltip.Text.Text = _hoverComponent.Tooltip;
                }
            }

            // Update all button trackers
            if (_inputEnabled)
            {
                for (int i = 0; i < _trackers.Count; i++)
                    _trackers[i].Update(this, _input, time);
            }

            // Invoke hover event if possible
            if (_hoverComponent != null)
            {
                _hoverComponent.InvokeHover(mousePos);

                // Update tooltip status
                if (_tooltipTimer < _tooltipDelay)
                {
                    _tooltip.IsVisible = false;
                    _tooltipTimer += time.ElapsedTime.TotalMilliseconds;
                }
                else
                {
                    _tooltip.IsVisible = true;
                }

                _tooltip.Position = mousePos + new Vector2(16);

                // Handle scroll wheel event
                if (_input.Mouse.WheelDelta != 0)
                    _hoverComponent.InvokeScrollWheel(_input.Mouse.WheelDelta);
            }
        }

        /// <summary>Renders the UI.</summary>
        /// <param name="sb">The spritebatch to use for drawing the final result to the current render surface.</param>
        internal void Render()
        {
            _screen.Draw(_sb, _renderProxy);
            _tooltip.Render(_sb);
            _renderProxy.DrawBatch(_sb, BatchSortMode.Depth);
            _renderProxy.Submit(RenderCommitMode.Overwrite);
        }

        /// <summary>Gets or sets the name of the default font.</summary>
        public string DefaultFontName
        {
            get { return _defaultFontName; }
            set { _defaultFontName = value; }
        }

        /// <summary>Gets or sets the default font size.</summary>
        public int DefaultFontSize
        {
            get { return _defaultFontSize; }
            set { _defaultFontSize = value; }
        }

        /// <summary>Gets the root UI component which represents the screen.</summary>
        public UIContainer Screen { get { return _screen; } }

        /// <summary>Gets the component which is currently focused.</summary>
        public UIComponent Focused
        {
            get { return _focused; }
        }

        /// <summary>Gets the component that the pointer is currently hovering over.</summary>
        public UIComponent Hovered { get { return _hoverComponent; } }

        /// <summary>Gets the window manager bound to the UI system.</summary>
        public UIWindowManager WindowManager { get { return _windowManager; } }

        /// <summary>Gets or sets the number of pixels the mouse must be dragged before it 
        /// begins triggering drag events. Resets when the left mouse button is released.</summary>
        public float DragThreshold
        {
            get { return _dragThreshold; }
            set
            {
                _dragThreshold = value;
                SetDragThreshold(value);
            }
        }

        /// <summary>Gets or sets the delay before a tooltip is shown when the mouse is kept stationary.</summary>
        public float TooltipDelay
        {
            get { return _tooltipDelay; }
            set { _tooltipDelay = value; }
        }

        /// <summary>Gets or sets whether or not the UI system is accepting input.</summary>
        public bool InputEnabled
        {
            get { return _inputEnabled; }
            set { _inputEnabled = value; }
        }
    }
}