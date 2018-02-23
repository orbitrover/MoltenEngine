﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class LookupListTable
    {
        List<LookupTable> _subTables = new List<LookupTable>();

        public IReadOnlyCollection<LookupTable> SubTables { get; internal set; }

        internal LookupListTable(BinaryEndianAgnosticReader reader, Logger log, TableHeader header, Type[] lookupTypeIndex, ushort extensionIndex, long startPos)
        {
            reader.Position = startPos;
            ushort lookupCount = reader.ReadUInt16();
            ushort[] lookupOffsets = reader.ReadArrayUInt16(lookupCount);
            log.WriteDebugLine($"[{header.Tag}] Reading lookup list table at {startPos} containing {lookupCount} lookup tables");

            List<LookupTable> subtables = new List<LookupTable>();
            SubTables = subtables.AsReadOnly();

            for (int i = 0; i < lookupCount; i++)
            {
                long lookupStartPos = startPos + lookupOffsets[i];
                reader.Position = lookupStartPos;
                ushort lookupType = reader.ReadUInt16();
                LookupFlags flags = (LookupFlags)reader.ReadUInt16();
                ushort subTableCount = reader.ReadUInt16();
                log.WriteDebugLine($"[{header.Tag}] Reading lookup table {i+1}/{lookupCount} at {lookupStartPos} containing {subTableCount} sub-tables");

                // Get the offset's for the lookup subtable's own subtables.
                uint[] subTableOffsets = new uint[subTableCount];
                reader.ReadArrayUInt16(subTableOffsets, subTableCount);
                ushort markFilteringSet = 0;
                if (HasFlag(flags, LookupFlags.UseMarkFilteringSet))
                    markFilteringSet = reader.ReadUInt16();

                for (int s = 0; s < subTableCount; s++)
                {
                    // Check for extension.
                    // MS Docs: This lookup provides a mechanism whereby any other lookup type's subtables are stored at a 32-bit offset location in the 'GPOS' table
                    if (lookupType == extensionIndex)
                    {
                        ushort posFormat = reader.ReadUInt16();
                        lookupType = reader.ReadUInt16(); // extensionLookupType.
                        subTableOffsets[s] = reader.ReadUInt32(); // overwrite the offset for this table with the 32-bit offset.

                        // ExtensionLookupType must be set to any lookup type other than the extension lookup type.
                        if (lookupType == extensionIndex)
                        {
                            log.WriteDebugLine($"[{header.Tag}] Nested extension lookup table detected. Ignored.");
                            continue;
                        }
                    }

                    // Skip unsupported tables.
                    if (lookupType >= lookupTypeIndex.Length || lookupTypeIndex[lookupType] == null)
                    {
                        log.WriteDebugLine($"[{header.Tag}] Unsupported lookup sub-table type: {lookupType}");
                        continue;
                    }

                    long subTableStartPos = lookupStartPos + subTableOffsets[s];
                    LookupTable subTable = Activator.CreateInstance(lookupTypeIndex[lookupType]) as LookupTable;
                    subTable.SubTableId = s;
                    subTable.Read(reader, log, subTableStartPos, lookupType, flags, markFilteringSet);
                    subtables.Add(subTable);
                }
            }
        }

        private bool HasFlag(LookupFlags value, LookupFlags flag)
        {
            return (value & flag) == flag;
        }
    }

    public abstract class LookupTable
    {
        public int SubTableId { get; internal set; }

        internal abstract void Read(BinaryEndianAgnosticReader reader, 
            Logger log, 
            long startPos, 
            ushort lookupType,
            LookupFlags flags,
            ushort markFilteringSet);
    }

    [Flags]
    public enum LookupFlags
    {
        None = 0,

        RightToLeft = 1,

        IgnoreBaseGlyphs = 1 << 1,

        IgnoreLigatures = 1 << 2,

        IgnoreMarks = 1 << 3,

        UseMarkFilteringSet = 1 << 4,

        Reserved = 1 << 5,

        MarkAttachmentType = 1 << 6,
    }
}
