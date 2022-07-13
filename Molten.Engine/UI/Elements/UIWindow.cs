﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;
using Newtonsoft.Json;

namespace Molten.UI
{
    public class UIWindow : UIElement
    {
        UIPanel _titleBar;
        UIPanel _panel;
        UIButton _btnClose;
        UIButton _btnMinimize;
        UIButton _btnMaximize;
        UILabel _title;
        int _titleBarSize = 25;
        int _iconSpacing = 5;
        List<UIButton> _titleBarButtons;
        CornerInfo _cornerRadius;

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);

            
            _titleBarButtons = new List<UIButton>();

            // Change _panel corners to only round bottom left/right.
            _titleBar = CompoundElements.Add<UIPanel>();
            _panel = CompoundElements.Add<UIPanel>();
            _title = CompoundElements.Add<UILabel>();
            _title.VerticalAlign = UIVerticalAlignment.Center;

            _btnClose = AddTitleButton("X");
            _btnMaximize = AddTitleButton("^");
            _btnMinimize = AddTitleButton("_");

            Title = Name;
        }

        /*protected override void OnApplyTheme(UITheme theme, UIElementTheme elementTheme, UIStateTheme stateTheme)
        {
            base.OnApplyTheme(theme, elementTheme, stateTheme);

            _cornerRadius = stateTheme.CornerRadius;
            _titleBar.CornerRadius.Set(_cornerRadius.TopLeft, _cornerRadius.TopRight, 0, 0);
            _titleBar.FillColor = stateTheme.BorderColor;
            _panel.CornerRadius.Set(0, 0, _cornerRadius.BottomRight, _cornerRadius.BottomLeft);
        }*/

        private UIButton AddTitleButton(string text)
        {
            UIButton btn = CompoundElements.Add<UIButton>();
            _titleBarButtons.Add(btn);
            btn.Text = text;
            return btn;
        }

        /*private void Btn_OnThemeApplied(UIElement element, UIElementTheme elementTheme, UIStateTheme stateTheme)
        {
            UIButton btn = element as UIButton;
            if (btn == _titleBarButtons.FirstOrDefault())
                btn.CornerRadius.Set(0, _cornerRadius.TopRight, 0, 0);
            else
                btn.CornerRadius.Set(0);
        }*/

        protected override void OnUpdateCompoundBounds()
        {
            base.OnUpdateCompoundBounds();

            ref Rectangle gb = ref BaseData.GlobalBounds;

            _titleBar.LocalBounds = new Rectangle(0, 0, gb.Width, _titleBarSize);
            _panel.LocalBounds = new Rectangle(0, _titleBarSize, gb.Width, gb.Height - _titleBarSize);
            _title.LocalBounds = new Rectangle(_titleBarSize + (_iconSpacing * 2), 0, gb.Width, _titleBarSize);
            for(int i = 0; i < _titleBarButtons.Count; i++)
                _titleBarButtons[i].LocalBounds = new Rectangle(gb.Width - (_titleBarSize * (i+1)), 0, _titleBarSize, _titleBarSize);
        }

        /// <summary>
        /// The title of the current <see cref="UIWindow"/>.
        /// </summary>
        [JsonProperty]
        public string Title
        {
            get => _title.Text;
            set => _title.Text = value;
        }

        /// <summary>
        /// The title bar height of the current <see cref="UIWindow"/>, in pixels. This value will also be the size of any buttons on the title bar.
        /// </summary>
        [JsonProperty]
        public int TitleBarHeight
        {
            get => _titleBarSize;
            set
            {
                _titleBarSize = value;
                OnUpdateBounds();
            }
        }
    }
}
