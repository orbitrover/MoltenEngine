﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class RenderChain
    {
        internal class Link
        {
            public Link Previous;
            public Link Next;
            public RenderStepBase Step;
            public RenderChain Chain;
        }

        internal Link First;
        internal Link Last;

        RendererDX11 _renderer;

        internal RenderChain(RendererDX11 renderer)
        {
            _renderer = renderer;
        }

        private void Next(RenderStepBase step)
        {
            Link link = new Link() { Chain = this, Step = step };
            if (First == null)
            {
                First = link;
                Last = First;
            }
            else
            {
                link.Previous = Last;
                Last.Next = link;
                Last = link;
                Last.Next = null;
            }
        }

        private void Next<T>() where T : RenderStepBase, new()
        {
            RenderStepBase step = _renderer.GetRenderStep<T>();
            Next(step);
        }

        internal void Build(SceneRenderData scene, RenderCamera camera)
        {
            First = null;
            Last = null;

            Next<StartStep>();

            if (camera.Flags.HasFlag(RenderCameraFlags.Deferred))
            {
                Next<GBuffer3dStep>();
                Next<Render2dStep>();

                Next<LightingStep>();
                Next<FinalizeStep>();
            }
            else
            {
                Next<Immediate3dStep>();
                Next<Render2dStep>();
            }
        }

        internal void Render(SceneRenderData<Renderable> scene, RenderCamera camera, Timing time)
        {
            Link link = First;
            while(link != null)
            {
                link.Step.Render(_renderer, camera, scene, time, link);
                link = link.Next;
            }
        }
    }
}
