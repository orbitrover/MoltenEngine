﻿using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    internal class SceneRemoveObject : SceneChange<SceneRemoveObject>
    {
        public SceneObject Object;

        public override void Clear()
        {
            Object = null;
        }

        public override void Process(Scene scene)
        {
            if (Object.Scene == scene)
            {
                scene.Objects.Remove(Object);
                scene.Updatables.Remove(Object);
                Object.Scene = null;
            }

            Recycle(this);
        }
    }
}
