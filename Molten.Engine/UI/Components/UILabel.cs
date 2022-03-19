﻿using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public class UILabel : UIComponent<UILabel.RenderData>
    {
        /// <summary>
        /// Container for <see cref="UILabel"/> render data.
        /// </summary>
        public struct RenderData : IUIRenderData
        {
            [DataMember]
            public Color Color;

            [DataMember]
            public string Text;

            public SpriteFont Font;

            public Vector2F Position;

            public IMaterial Material;

            public void Render(SpriteBatcher sb, UIRenderData data)
            {
                if (Font != null && Color.A > 0)
                    sb.DrawString(Font, Text, Position, Color, Material);
            }
        }

        UIHorizonalAlignment _hAlign;
        UIVerticalAlignment _vAlign;

        protected override void OnInitialize(Engine engine, UISettings settings, UITheme theme)
        {
            base.OnInitialize(engine, settings, theme);

            Properties.Color = theme.TextColor;
            Properties.Text = this.Name;
            theme.RequestFont(engine, LoadFont_Request);
        }

        private void LoadFont_Request(ContentRequest cr)
        {
            Properties.Font = cr.Get<SpriteFont>(0);
            OnUpdateBounds();
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            if (Properties.Font == null)
                return;

            Properties.Position = (Vector2F)LocalBounds.TopLeft;
            Vector2F textSize = Properties.Font.MeasureString(Properties.Text);

            switch (_hAlign)
            {
                case UIHorizonalAlignment.Center:
                    Properties.Position.X = RenderBounds.Center.X - (textSize.X / 2);
                    break;

                case UIHorizonalAlignment.Right:
                    Properties.Position.X = RenderBounds.Right - textSize.X;
                    break;
            }

            switch (_vAlign)
            {
                case UIVerticalAlignment.Center:
                    Properties.Position.Y = RenderBounds.Center.Y - (textSize.Y / 2);
                    break;

                case UIVerticalAlignment.Bottom:
                    Properties.Position.Y = RenderBounds.Bottom - textSize.Y;
                    break;
            }
        }

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
    }
}