﻿using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    /// <summary>
    /// A UI component dedicated to presenting text.
    /// </summary>
    public class UIText : UIElement<UIText.Data>
    {
        public struct Data : IUIRenderData
        {
            [DataMember]
            public Color Color;

            [DataMember]
            public string Text;

            internal TextFont Font;

            [DataMember]
            public Vector2F Position;

            [IgnoreDataMember]
            public IMaterial Material;

            public void Render(SpriteBatcher sb, UIRenderData data)
            {
                if (Font != null && Color.A > 0)
                    sb.DrawString(Font, Text, Position, Color, Material);
            }

            public void ApplyTheme(UITheme theme, UIElementTheme eTheme, UIStateTheme stateTheme)
            {
                Color = stateTheme.TextColor;
                Font = eTheme.Font;
            }
        }

        UIHorizonalAlignment _hAlign;
        UIVerticalAlignment _vAlign;

        protected override void OnInitialize(Engine engine, UISettings settings, UITheme theme)
        {
            base.OnInitialize(engine, settings, theme);
            Properties.Text = Name;
        }

        public override void ApplyStateTheme(UIElementState state)
        {
            TextFont curFont = Properties.Font;

            base.ApplyStateTheme(state);

            if (ElementTheme.Font != curFont)
                OnUpdateBounds();
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            if (Properties.Font == null || string.IsNullOrEmpty(Properties.Text))
                return;

            Rectangle gBounds = GlobalBounds;
            Properties.Position = (Vector2F)gBounds.TopLeft;
            Vector2F textSize = Properties.Font.MeasureString(Properties.Text);

            switch (_hAlign)
            {
                case UIHorizonalAlignment.Center:
                    Properties.Position.X = gBounds.Center.X - (textSize.X / 2);
                    break;

                case UIHorizonalAlignment.Right:
                    Properties.Position.X = gBounds.Right - textSize.X;
                    break;
            }

            switch (_vAlign)
            {
                case UIVerticalAlignment.Center:
                    Properties.Position.Y = gBounds.Center.Y - (textSize.Y / 2);
                    break;

                case UIVerticalAlignment.Bottom:
                    Properties.Position.Y = gBounds.Bottom - textSize.Y;
                    break;
            }
        }

        /// <summary>
        /// Gets or sets the horizontal alignment.
        /// </summary>
        public UIHorizonalAlignment HorizontalAlign
        {
            get => _hAlign;
            set
            {
                if(_hAlign != value)
                {
                    _hAlign = value;
                    OnUpdateBounds();
                }
            }
        }

        /// <summary>
        /// Gets or sets the vertical alignment.
        /// </summary>
        public UIVerticalAlignment VerticalAlign
        {
            get => _vAlign;
            set
            {
                if (_vAlign != value)
                {
                    _vAlign = value;
                    OnUpdateBounds();
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TextFont"/> of the current <see cref="UIText"/>.
        /// </summary>
        public TextFont Font
        {
            get => Properties.Font;
            set
            {
                if(Properties.Font != value)
                {
                    Properties.Font = value;
                    OnUpdateBounds();
                }
            }
        }

        public Color Color
        {
            get => Properties.Color;
            set => Properties.Color = value;
        }

        public string Text
        {
            get => Properties.Text;
            set => Properties.Text = value;
        }
    }
}