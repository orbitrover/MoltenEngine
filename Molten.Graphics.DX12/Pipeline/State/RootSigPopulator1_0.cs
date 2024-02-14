﻿using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class RootSigPopulator1_0 : RootSignaturePopulatorDX12
{
    internal override unsafe void Populate(ref VersionedRootSignatureDesc versionedDesc, 
        ref readonly GraphicsPipelineStateDesc psoDesc, 
        ShaderPassDX12 pass)
    {
        ref RootSignatureDesc desc = ref versionedDesc.Desc10;
        PopulateStaticSamplers(ref desc.PStaticSamplers, ref desc.NumStaticSamplers, pass);

        desc.NumParameters = (uint)pass.Parent.Resources.Length;
        desc.PParameters = EngineUtil.AllocArray<RootParameter>(desc.NumParameters);
        desc.Flags = GetFlags(in psoDesc, pass);

        List<DescriptorRange> ranges = new();
        PopulateRanges(DescriptorRangeType.Srv, ranges, pass.Parent.Resources);
        PopulateRanges(DescriptorRangeType.Uav, ranges, pass.Parent.UAVs);
        PopulateRanges(DescriptorRangeType.Cbv, ranges, pass.Parent.ConstBuffers);

        desc.PParameters = EngineUtil.AllocArray<RootParameter>((uint)ranges.Count);
        for (int i = 0; i < ranges.Count; i++)
        {
            ref RootParameter param = ref desc.PParameters[i];

            param.ParameterType = RootParameterType.TypeDescriptorTable;
            param.DescriptorTable.NumDescriptorRanges = 1;
            param.DescriptorTable.PDescriptorRanges = EngineUtil.Alloc<DescriptorRange>();
            param.DescriptorTable.PDescriptorRanges[0] = ranges[i];
            param.ShaderVisibility = ShaderVisibility.All; // TODO If a parameter is only used on 1 stage, set this to that stage.
        }
    }

    internal unsafe override void Free(ref VersionedRootSignatureDesc versionedDesc)
    {
        ref RootSignatureDesc desc = ref versionedDesc.Desc10;

        for (int i = 0; i < desc.NumParameters; i++)
            EngineUtil.Free(ref desc.PParameters[i].DescriptorTable.PDescriptorRanges);

        EngineUtil.Free(ref desc.PParameters);
        EngineUtil.Free(ref desc.PStaticSamplers);
    }

    private void PopulateRanges(DescriptorRangeType type, List<DescriptorRange> ranges, Array variables)
    {
        uint last = 0;
        uint i = 0;
        DescriptorRange r = new();

        for (; i < variables.Length; i++)
        {
            if (variables.GetValue(i) == null)
                continue;

            // Create a new range if there was a gap.
            uint prev = i - 1; // What the previous should be.
            if (last == i || last == prev)
            {
                // Finalize previous range
                if (last != i)
                    r.NumDescriptors = i - r.BaseShaderRegister;

                // Start new range.
                r = new DescriptorRange();
                r.BaseShaderRegister = i;
                r.RangeType = type;
                r.RegisterSpace = 0; // TODO Populate - Requires reflection enhancements to support new HLSL register syntax: register(t0, space1);
                r.OffsetInDescriptorsFromTableStart = 0;
                ranges.Add(r);
            }

            last = i;
        }

        // Finalize last range to the list
        r.NumDescriptors = i - r.BaseShaderRegister;
    }
}
