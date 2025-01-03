//MIT, 2019-present, WinterDev

using System;
using System.IO;

namespace Typography.OpenFont.Tables;

//https://docs.microsoft.com/en-us/typography/opentype/spec/hvar

/// <summary>
///     HVAR — Horizontal Metrics Variations Table
/// </summary>
internal class HVar : TableEntry
{
    public const string _N = "HVAR";

    private const int INNER_INDEX_BIT_COUNT_MASK = 0x000F;
    private const int MAP_ENTRY_SIZE_MASK = 0x0030;
    private const int MAP_ENTRY_SIZE_SHIFT = 4;
    internal DeltaSetIndexMap[] _advanceWidthMapping;


    internal ItemVariationStoreTable _itemVartionStore;
    internal DeltaSetIndexMap[] _lsbMapping;
    internal DeltaSetIndexMap[] _rsbMapping;
    public override string Name => _N;

    protected override void ReadContentFrom(BinaryReader reader)
    {
        var beginAt = reader.BaseStream.Position;

        //Horizontal metrics variations table:
        //Type      Name                        Description
        //uint16    majorVersion                Major version number of the horizontal metrics variations table — set to 1.
        //uint16    minorVersion                Minor version number of the horizontal metrics variations table — set to 0.
        //Offset32  itemVariationStoreOffset    Offset in bytes from the start of this table to the item variation store table.
        //Offset32  advanceWidthMappingOffset   Offset in bytes from the start of this table to the delta-set index mapping for advance widths (may be NULL).
        //Offset32  lsbMappingOffset            Offset in bytes from the start of this table to the delta-set index mapping for left side bearings(may be NULL).
        //Offset32  rsbMappingOffset            Offset in bytes from the start of this table to the delta-set index mapping for right side bearings(may be NULL).            

        var majorVersion = reader.ReadUInt16();
        var minorVersion = reader.ReadUInt16();
        var itemVariationStoreOffset = reader.ReadUInt32();
        var advanceWidthMappingOffset = reader.ReadUInt32();
        var lsbMappingOffset = reader.ReadUInt32();
        var rsbMappingOffset = reader.ReadUInt32();
        //            

        //itemVariationStore
        reader.BaseStream.Position = beginAt + itemVariationStoreOffset;
        _itemVartionStore = new ItemVariationStoreTable();
        _itemVartionStore.ReadContentFrom(reader);

        //-----------------------------------------
        if (advanceWidthMappingOffset > 0)
        {
            reader.BaseStream.Position = beginAt + advanceWidthMappingOffset;
            _advanceWidthMapping = ReadDeltaSetIndexMapTable(reader);
        }

        if (lsbMappingOffset > 0)
        {
            reader.BaseStream.Position = beginAt + lsbMappingOffset;
            _lsbMapping = ReadDeltaSetIndexMapTable(reader);
        }

        if (rsbMappingOffset > 0)
        {
            reader.BaseStream.Position = beginAt + rsbMappingOffset;
            _rsbMapping = ReadDeltaSetIndexMapTable(reader);
        }
    }

    private static DeltaSetIndexMap[] ReadDeltaSetIndexMapTable(BinaryReader reader)
    {
        //DeltaSetIndexMap table:
        //Table 2
        //Type      Name 	        Description
        //uint16 	entryFormat 	A packed field that describes the compressed representation of delta-set indices. See details below.
        //uint16 	mapCount    	The number of mapping entries.
        //uint8 	mapData[variable] 	The delta-set index mapping data. See details below.

        var entryFormat = reader.ReadUInt16();
        var mapCount = reader.ReadUInt16();

        //The mapCount field indicates the number of delta-set index mapping entries.
        //Glyph IDs are used as the index into the mapping array.
        //If a given glyph ID is greater than mapCount - 1, then the last entry is used.

        //Each mapping entry represents a delta-set outer-level index and inner-level index combination. 
        //Logically, each of these indices is a 16-bit, unsigned value.
        //These are represented in a packed format that uses one, two, three or four bytes.
        //The entryFormat field is a packed bitfield that describes the compressed representation used in 
        //the mapData field of the given deltaSetIndexMap table. 
        //The format of the entryFormat field is as follows:

        //EntryFormat Field Masks
        //Table 3
        //Mask  	Name 	                    Description
        //0x000F 	INNER_INDEX_BIT_COUNT_MASK 	Mask for the low 4 bits, which give the count of bits minus one that are used in each entry for the inner-level index.
        //0x0030 	MAP_ENTRY_SIZE_MASK 	    Mask for bits that indicate the size in bytes minus one of each entry.
        //0xFFC0 	Reserved 	                Reserved for future use — set to 0.


        //see also: afdko\c\public\lib\source\varread\varread.c (Apache2)

        var entrySize = ((entryFormat & MAP_ENTRY_SIZE_MASK) >> MAP_ENTRY_SIZE_SHIFT) + 1;
        var innerIndexEntryMask = (1 << ((entryFormat & INNER_INDEX_BIT_COUNT_MASK) + 1)) - 1;
        var outerIndexEntryShift = (entryFormat & INNER_INDEX_BIT_COUNT_MASK) + 1;

        var mapDataSize = mapCount * entrySize;

        var deltaSetIndexMaps = new DeltaSetIndexMap[mapCount];

        for (var i = 0; i < mapCount; ++i)
        {
            int entry;
            switch (entrySize)
            {
                default: throw new NotSupportedException();
                case 1: entry = reader.ReadByte(); break;
                case 2: entry = (reader.ReadByte() << 8) | reader.ReadByte(); break;
                case 3: entry = (reader.ReadByte() << 16) | (reader.ReadByte() << 8) | reader.ReadByte(); break;
                case 4:
                    entry = (reader.ReadByte() << 24) | (reader.ReadByte() << 16) | (reader.ReadByte() << 8) |
                            reader.ReadByte(); break;
            }

            //***
            deltaSetIndexMaps[i] = new DeltaSetIndexMap((ushort)(entry & innerIndexEntryMask),
                (ushort)(entry >> outerIndexEntryShift));
        }

        return deltaSetIndexMaps;
    }

    public readonly struct DeltaSetIndexMap
    {
        public readonly ushort innerIndex;
        public readonly ushort outerIndex;

        public DeltaSetIndexMap(ushort innerIndex, ushort outerIndex)
        {
            this.innerIndex = innerIndex;
            this.outerIndex = outerIndex;
        }
    }
}