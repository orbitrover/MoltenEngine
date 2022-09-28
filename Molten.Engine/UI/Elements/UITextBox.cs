﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Molten.Collections;
using Molten.Graphics;

namespace Molten.UI
{
    public partial class UITextBox : UIElement
    {
        UIScrollBar _vScroll;
        UIScrollBar _hScroll;
        
        ThreadedList<Line> _lines;
        SpriteFont _defaultFont;
        bool _isMultiline;
        string _fontName;
        int _marginWidth = 50;
        int _marginPadding = 10;
        int _scrollbarWidth = 20;
        int _lineSpacing = 5;
        int _lineHeight = 25;
        Rectangle _textBounds;

        // Line numbers
        bool _showLineNumbers;
        Vector2F _lineNumPos;
        Color _lineNumColor = new Color(42, 136, 151, 255);

        // Margin
        Vector2F _marginPos;
        Color _bgColor = new Color(30, 30, 30, 255);
        Color _marginColor = new Color(60,60,60, 255);
        Color _marginLineColor = Color.White;

        // Selector
        int? _selectedLine;
        RectangleF _selectorBounds;
        RectStyle _selectorStyle = new RectStyle(new Color(60,60,60,200), new Color(160,160,160,255), 2);

        /* TODO:
         *  - Allow segment to have OnPressed and OnReleased virtual methods to allow custom segment actions/types, such as:
         *      - Open a URL
         *      - Open an in-game menu.
         *      - An item link e.g. Path of Exile chat items
         * 
         */

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings); 
            
            _lines = new ThreadedList<Line>();
            FontName = settings.DefaultFontName;

            _vScroll = BaseElements.Add<UIScrollBar>();
            _hScroll = BaseElements.Add<UIScrollBar>();
            _hScroll.Direction = UIElementFlowDirection.Horizontal;
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            Rectangle gb = GlobalBounds;
            _textBounds = gb;
            _textBounds.Left += _marginPadding;
            _textBounds.Top += _marginPadding;
            _marginPos = new Vector2F(gb.X + _marginWidth, gb.Y);
            _lineNumPos = _marginPos - new Vector2F(_marginPadding, -_marginPadding);

            if (_showLineNumbers)
                _textBounds.Left += _marginWidth;

            CalcScrollBars();

            if (_hScroll.IsVisible)
            {
                _textBounds.Bottom -= _scrollbarWidth;
                _hScroll.LocalBounds = new Rectangle(0, gb.Height - _scrollbarWidth, gb.Width - _scrollbarWidth, _scrollbarWidth);
            }

            if (_vScroll.IsVisible)
            {
                _textBounds.Right -= _scrollbarWidth;
                _vScroll.LocalBounds = new Rectangle(gb.Width - _scrollbarWidth, 0, _scrollbarWidth, gb.Height - _scrollbarWidth);
            }


            Vector2F tl = (Vector2F)_textBounds.TopLeft;
            Vector2F p = tl;
            Line line;
            for(int i = 0; i < _lines.Count; i++)
            {
                line = _lines[i];
                Segment seg = line.First;
                p.X = tl.X;

                while (seg != null)
                {
                    seg.Bounds.X = p.X;
                    seg.Bounds.Y = p.Y;
                    p.X += seg.Bounds.Width;
                    seg = seg.Next;
                }

                p.Y += _lineHeight;
            }

            SelectLine(_selectedLine);
        }

        protected override void OnAdjustRenderBounds(ref Rectangle renderbounds)
        {
            base.OnAdjustRenderBounds(ref renderbounds);

            if (_hScroll.IsVisible)
                renderbounds.Height -= _scrollbarWidth;

            if (_vScroll.IsVisible)
                renderbounds.Width -= _scrollbarWidth;
        }

        private void SelectLine(int? lineID)
        {
            _selectedLine = lineID;
            if (!_selectedLine.HasValue)
                return;

            Line l = _lines[lineID.Value];
            _selectorBounds = GetLineBounds(lineID.Value);
            
        }

        private RectangleF GetLineBounds(int lineID)
        {
            Rectangle gb = GlobalBounds;
            return new RectangleF()
            {
                X = _marginPos.X + 2,
                Y = gb.Y + 5 + (_lineHeight * lineID),
                Width = gb.Width - (_marginWidth + 2 + _scrollbarWidth),
                Height = _lineHeight
            };
        }

        public override void OnPressed(UIPointerTracker tracker)
        {
            base.OnPressed(tracker);

            // TODO only test the lines that are in view.

            RectangleF b = GetLineBounds(0);

            b.Height = 20;
            Line l;

            for (int i = 0; i < _lines.Count; i++)
            {
                l = _lines[i];
                b.Height = _lineHeight; // l.MeasuredSize.Y;

                if (b.Contains(tracker.Position))
                {
                    SelectLine(i);
                    break;
                }
                b.Y += _lineHeight;
            }
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            base.OnRender(sb);

            Rectangle gb = GlobalBounds;
            Vector2F tl = (Vector2F)_textBounds.TopLeft;
            Vector2F numPos = _lineNumPos;

            sb.DrawRect(gb, _bgColor, 0, null, 0);

            if (_selectedLine.HasValue)
                sb.DrawRect(_selectorBounds, ref _selectorStyle, 0, null, 0);

            if (_showLineNumbers)
            {
                sb.DrawRect(new RectangleF(gb.X, gb.Y, _marginWidth, gb.Height), _marginColor, 0, null, 0);
                sb.DrawLine(_marginPos, _marginPos + new Vector2F(0, _textBounds.Height), _marginLineColor, 1, 1, 0);
            }

            for (int l = 0; l < _lines.Count; l++)
            {
                Line line = _lines[l];
                Segment seg = line.First;

                if (_showLineNumbers)
                    sb.DrawString(_defaultFont, line.LineNumber.ToString(), numPos - new Vector2F(line.LineNumberSize.X, 0), _lineNumColor, null, 0);

                while (seg != null)
                {
                    seg.Render(sb);
                    seg = seg.Next;
                }

                numPos.Y += _lineHeight;
            }
        }

        private void SetText(string text)
        {
            string[] lines = Regex.Split(text, "\r?\n");
            for (int i = 0; i < lines.Length; i++)
            {
                Line line = new Line();
                line.LineNumber = (uint)i + 1U;
                line.LineNumberSize = _defaultFont.MeasureString(line.LineNumber.ToString());
                line.SetText(_defaultFont, lines[i]);

                _lines.Add(line);
            }

            CalcScrollBars();
        }

        private void CalcScrollBars()
        {
            Line line;
            float distH = 0;
            float distV = 0;

            for(int i = 0; i < _lines.Count; i++)
            {
                line = _lines[i];
                if (line.MeasuredSize.X > _textBounds.Width)
                    distH = Math.Max(distH, line.MeasuredSize.X - _textBounds.Width);

                distV += line.MeasuredSize.Y + _lineSpacing;
            }

            // Horizontal scroll bar
            if(distH > 0)
            {
                _hScroll.IsVisible = true;
                _hScroll.MaxValue = distH + _scrollbarWidth;
            }
            else
            {
                _hScroll.IsVisible = false;
            }

            // Virtual scroll bar
            if(distV > _textBounds.Height)
            {
                _vScroll.IsVisible = true;
                _vScroll.MaxValue = distV + _scrollbarWidth;
            }
            else
            {
                _vScroll.IsVisible = false;
            }
        }

        /// <summary>
        /// Gets or sets whether the current <see cref="UITextBox"/> is a multi-line textbox. If false, any line breaks will be substituted with spaces.
        /// </summary>
        public bool IsMultiLine
        {
            get => _isMultiline;
            set
            {
                if(_isMultiline != value)
                {
                    _isMultiline = value;
                    OnUpdateBounds();
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the default font for the current <see cref="UITextBox"/>. This will attempt to load/retrieve and populate <see cref="Font"/>.
        /// </summary>
        [UIThemeMember]
        public string FontName
        {
            get => _fontName;
            set
            {
                value = (value ?? string.Empty).ToLower();
                if (_fontName != value)
                {
                    _fontName = value;
                    if (!string.IsNullOrWhiteSpace(_fontName))
                    {
                        Engine.Content.LoadFont(_fontName, (font, isReload) =>
                        {
                            _defaultFont = font;
                            for(int i = 0; i < _lines.Count; i++)
                            {
                                Segment seg = _lines[i].First;
                                while(seg != null)
                                {
                                    seg.Font = font;
                                    seg = seg.Next;
                                }
                            }
                        },
                        new SpriteFontParameters()
                        {
                            FontSize = 16,
                        });
                    }
                }
            }
        }

        public string Text
        {
            get => "";
            set => SetText(value);
        }

        /// <summary>Gets or sets whether or not line-numbers are visible.</summary>
        public bool ShowLineNumbers
        {
            get => _showLineNumbers;
            set
            {
                if(_showLineNumbers != value)
                {
                    _showLineNumbers = value;
                    OnUpdateBounds();
                }
            }
        }

        public int ScrollBarWidth
        {
            get => _scrollbarWidth;
            set
            {
                if(_scrollbarWidth != value)
                {
                    _scrollbarWidth = value;
                    UpdateBounds();
                }
            }
        }
    }
}
