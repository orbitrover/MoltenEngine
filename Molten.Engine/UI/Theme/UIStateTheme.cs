﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public class UIStateTheme
    {
        [DataMember]
        public Color TextColor { get; set; } = Color.White;

        [DataMember]
        public Color BackgroundColor { get; set; } = new Color(40, 170, 40, 200);

        [DataMember]
        public Color BorderColor { get; set; } = new Color(80, 220, 80);


        [DataMember]
        public float BorderThickness { get; set; } = 2f;

        [DataMember]
        public float CornerRadius { get; set; } = 0f;

        /// <summary>
        /// Gets or sets the default size (in spaces) of the a tab character.
        /// </summary>
        [DataMember]
        public int TabSize { get; set; } = 3;

        /// <summary>
        /// Gets or sets the default number of points per curve, when rendering font characters.
        /// </summary>
        [DataMember]
        public int FontPointsPerCurve { get; set; } = 12;

        /// <summary>
        /// Gets or sets the default font character padding.
        /// </summary>
        [DataMember]
        public int CharacterPadding { get; set; } = 2;

        [DataMember]
        public UIVerticalAlignment VerticalAlign { get; set; }

        [DataMember]
        public UIHorizonalAlignment HorizontalAlign { get; set; }

        [DataMember]
        public Dictionary<string, Color> CustomColors { get; private set; } = new Dictionary<string, Color>();
    }
}