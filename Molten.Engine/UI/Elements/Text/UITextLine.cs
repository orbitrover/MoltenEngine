﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    /// <summary>
    /// Represents a line of text that is displayed by an implemented <see cref="UIElement"/>.
    /// </summary>
    public class UITextLine
    {
        int _height;

        internal UITextLine(UITextElement element)
        {
            Parent = element;
            HasText = false;
        }

        public void Clear()
        {
            Width = 0;
            _height = 0;
            HasText = false;
            FirstSegment = new UITextSegment("", Color.White, null);
            LastSegment = FirstSegment;
        }

        internal bool Pick(ref Rectangle lBounds, ref Vector2I pickPoint, UITextCaret.CaretPoint caretPoint)
        {
            UITextSegment seg = FirstSegment;
            Vector2F fPos = (Vector2F)pickPoint;

            if (lBounds.Contains(pickPoint))
            {
                caretPoint.Line = this;
                RectangleF segBounds = (RectangleF)lBounds;

                while (seg != null)
                {
                    segBounds.Width = seg.Size.X;
                    if (segBounds.Contains(fPos))
                    {
                        caretPoint.Segment = seg;
                        SpriteFont segFont = seg.Font ?? Parent.DefaultFont;

                        if (!string.IsNullOrWhiteSpace(seg.Text))
                        {
                            float dist = 0;
                            float prevDist = 0;
                            float charDist = 0;
                            float halfDist = 0;

                            for (int i = 0; i < seg.Text.Length; i++)
                            {
                                charDist = segFont.GetAdvanceWidth(seg.Text[i]);
                                dist += charDist;
                                halfDist = prevDist + (charDist / 2);

                                if (pickPoint.X <= segBounds.Left + dist)
                                {
                                    if (pickPoint.X >= segBounds.Left + halfDist)
                                    {
                                        caretPoint.Char.Index = i+1;
                                        caretPoint.Char.StartOffset = dist;
                                        caretPoint.Char.EndOffset = segBounds.Width - dist;
                                    }
                                    else
                                    {
                                        caretPoint.Char.Index = i;
                                        caretPoint.Char.StartOffset = prevDist;
                                        caretPoint.Char.EndOffset = segBounds.Width - prevDist;
                                    }                                    
                                    break;
                                }

                                prevDist = dist;
                            }
                        } 

                        return true;
                    }
                    segBounds.X += seg.Size.X;
                    seg = seg.Next;
                }
            }

            return false;
        }

        public UITextSegment NewSegment(string text, Color color, SpriteFont font)
        {
            UITextSegment segment = new UITextSegment(text, color, font);
            AppendSegment(segment);
            return segment;
        }

        public void AppendSegment(UITextSegment seg)
        {
            if (seg != null)
            {
                if (LastSegment != null)
                {
                    LastSegment.Next = seg;
                    seg.Previous = LastSegment;
                }
                else
                {
                    LastSegment = seg;
                    FirstSegment = seg;
                }

                LastSegment = seg;
                Width += seg.Size.X;
                _height = Math.Max(_height, (int)Math.Ceiling(seg.Size.Y));
            }
        }

        /// <summary>
        /// Creates and inserts a new <see cref="UITextSegment"/> after the specified <see cref="UITextSegment"/>.
        /// </summary>
        /// <param name="seg">The <see cref="UITextSegment"/> that the new segment should be inserted after.
        /// <para>If null, the new segment will be insert at the beginning of the line.</para></param>
        /// <param name="color">The color of the new segment's text.</param>
        /// <param name="font">The font of the new segment.</param>
        /// <param name="type">The type of the new segment.</param>
        /// <returns></returns>
        public UITextSegment InsertSegment(UITextSegment seg, Color color, SpriteFont font)
        {
            UITextSegment next = new UITextSegment("", Color.White, font);

            // Do we need to insert before another "next" segment also?
            if (seg != null)
            {
                if (seg.Next != null)
                {
                    seg.Next.Previous = next;
                    next.Next = seg.Next;
                }

                seg.Next = next;
                next.Previous = seg;
            }
            else
            {
                if(FirstSegment != null)
                {
                    FirstSegment.Previous = seg;
                    seg.Next = FirstSegment;
                    FirstSegment = seg;
                }
                else
                {
                    FirstSegment = seg;
                    LastSegment = seg;
                }
            }

            Width += seg.Size.X;
            _height = Math.Max(_height, (int)Math.Ceiling(seg.Size.Y));

            return next;
        }

        /// <summary>
        /// Gets the string of text represented by all <see cref="UITextSegment"/>s of the current <see cref="UITextLine"/>.
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            string result = "";
            UITextSegment seg = FirstSegment;
            while(seg != null)
            {
                result += seg.Text;
                seg = seg.Next;
            }

            return result;
        }

        /// <summary>
        /// Gets the string of text represented by all <see cref="UITextSegment"/>s of the current <see cref="UITextLine"/> and appends it to the provided <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/> to output the line contents into.</param>
        /// <returns></returns>
        public void GetText(StringBuilder sb)
        {
            UITextSegment seg = FirstSegment;
            while (seg != null)
            {
                sb.Append(seg.Text);
                seg = seg.Next;
            }
        }

        /// <summary>
        /// Gets the measured width of the current <see cref="UITextLine"/>.
        /// </summary>
        public float Width { get; private set; }

        /// <summary>
        /// Gets the measured height of the current <see cref="UITextLine"/>.
        /// <para>If the line contains no text, the default line height of the <see cref="Parent"/> will be used instead.</para>
        /// </summary>
        public int Height => HasText ? _height : Parent.DefaultLineHeight;

        /// <summary>
        /// Gets the parent <see cref="UITextElement"/>, or null if none.
        /// </summary>
        public UITextElement Parent { get; internal set; }

        /// <summary>
        /// Gets the first <see cref="UITextSegment"/> on the current <see cref="UITextLine"/>.
        /// </summary>
        public UITextSegment FirstSegment { get; private set; }

        /// <summary>
        /// Gets the last <see cref="UITextSegment"/> on the current <see cref="UITextLine"/>.
        /// </summary>
        public UITextSegment LastSegment { get; private set; }

        public UITextLine Previous { get; internal set; }

        public UITextLine Next { get; internal set; }

        /// <summary>
        /// Gets whether or not the current <see cref="UITextLine"/> contains text.
        /// </summary>
        public bool HasText { get; private set; }
    }
}
