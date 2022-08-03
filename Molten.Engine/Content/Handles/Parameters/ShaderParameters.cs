﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten
{
    public class ShaderParameters : ContentParameters
    {
        public string MaterialName = "";

        public override object Clone()
        {
            return new ShaderParameters()
            {
                MaterialName = MaterialName,
                PartCount = PartCount
            };
        }
    }
}