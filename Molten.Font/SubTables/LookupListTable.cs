﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class LookupListTable<T> : FontSubTable where T : struct
    {
        public LookupTable<T>[] LookupTables { get; private set; }

        internal LookupListTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, Type[] lookupTypeIndex, ushort extensionIndex) :
            base(reader, log, parent, offset)
        {
            ushort lookupCount = reader.ReadUInt16();
            ushort[] lookupOffsets = reader.ReadArray<ushort>(lookupCount);
            log.WriteDebugLine($"Reading lookup list table at {Header.StreamOffset} containing {lookupCount} lookup tables");

            LookupTables = new LookupTable<T>[lookupCount];
            for (int i = 0; i < lookupCount; i++)
                LookupTables[i] = new LookupTable<T>(reader, log, this, lookupOffsets[i], lookupTypeIndex, extensionIndex);
        }
    }

    public class LookupTable<T> : FontSubTable where T : struct
    {
        public LookupFlags Flags { get; private set; }

        public byte MarkAttachmentType { get; private set; }

        public ushort MarkFilteringSet { get; private set; } 

        public LookupSubTable<T>[] SubTables { get; private set; }

        public T LookupType { get; private set; }

        internal LookupTable(EnhancedBinaryReader reader, Logger log, LookupListTable<T> parent, long offset, Type[] typeLookup, ushort extensionIndex) :
            base(reader, log, parent, offset)
        {
            ushort lookupTypeIndex = reader.ReadUInt16();
            LookupType = (T)((object)lookupTypeIndex); 
            Flags = (LookupFlags)reader.ReadByte();
            MarkAttachmentType = reader.ReadByte();  // MS docs: The high byte (of flags) is set to specify the type of mark attachment.
            //Flags = (LookupFlags)reader.ReadUInt16();
            ushort subTableCount = reader.ReadUInt16();
            log.WriteDebugLine($"Reading lookup table containing {subTableCount} sub-tables");

            // Get the offset's for the lookup subtable's own subtables.
            ushort[] subTableOffsets = reader.ReadArray<ushort>(subTableCount);
            if (HasFlag(LookupFlags.UseMarkFilteringSet))
                MarkFilteringSet = reader.ReadUInt16();

            SubTables = new LookupSubTable<T>[subTableCount];
            for (int i = 0; i < subTableCount; i++)
            {
                long subTableOffset = subTableOffsets[i];

                // Check if subtable is an extension table. If true, adjust offset and lookup type accordingly.
                // MS Docs: This lookup provides a mechanism whereby any other lookup type's subtables are stored at a 32-bit offset location in the 'GPOS' table
                if (lookupTypeIndex == extensionIndex)
                {
                    ushort posFormat = reader.ReadUInt16();
                    lookupTypeIndex = reader.ReadUInt16(); // extensionLookupType.
                    uint extensionOffset = reader.ReadUInt32(); // MS docs: Offset to the extension subtable, relative to the start of the ExtensionSubstFormat1 subtable.
                    subTableOffset += extensionOffset;

                    // ExtensionLookupType must be set to any lookup type other than the extension lookup type.
                    if (lookupTypeIndex == extensionIndex)
                    {
                        log.WriteDebugLine($"Nested extension lookup table detected. Ignored.");
                        continue;
                    }
                }

                // Skip unsupported tables.
                Type subTableType = typeLookup[lookupTypeIndex];
                if (lookupTypeIndex >= typeLookup.Length || subTableType == null)
                {
                    log.WriteDebugLine($"Unsupported lookup sub-table type: {lookupTypeIndex}");
                    continue;
                }

                SubTables[i] = Activator.CreateInstance(subTableType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                    new object[] { reader, log, this, subTableOffset }, null) as LookupSubTable<T>;
            }
        }

        public bool HasFlag(LookupFlags flag)
        {
            return (Flags & flag) == flag;
        }
    }

    public abstract class LookupSubTable<T> : FontSubTable where T : struct
    {
        public ushort Format { get; private set; }

        internal LookupSubTable(EnhancedBinaryReader reader,
            Logger log,
            LookupTable<T> parent,
            long offset) :
            base(reader, log, parent, offset)
        {
            Format = reader.ReadUInt16();
        }
    }

    [Flags]
    public enum LookupFlags : byte
    {
        None = 0,

        RightToLeft = 1,

        IgnoreBaseGlyphs = 1 << 1,

        IgnoreLigatures = 1 << 2,

        IgnoreMarks = 1 << 3,

        UseMarkFilteringSet = 1 << 4,

        Reserved6 = 1 << 5,

        Reserved7 = 1 << 6,

        Reserved8 = 1 << 7,
    }
}
