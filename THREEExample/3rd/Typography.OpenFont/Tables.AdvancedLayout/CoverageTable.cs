﻿//Apache2, 2016-present, WinterDev

using System;
using System.Collections.Generic;
using System.IO;

namespace Typography.OpenFont.Tables;
//https://docs.microsoft.com/en-us/typography/opentype/spec/chapter2

public abstract class CoverageTable
{
    public abstract int FindPosition(ushort glyphIndex);
    public abstract IEnumerable<ushort> GetExpandedValueIter();

    public static CoverageTable CreateFrom(BinaryReader reader, long beginAt)
    {
        reader.BaseStream.Seek(beginAt, SeekOrigin.Begin);
        var format = reader.ReadUInt16();
        switch (format)
        {
            default: throw new NotSupportedException();
            case 1: return CoverageFmt1.CreateFrom(reader);
            case 2: return CoverageFmt2.CreateFrom(reader);
        }
    }

    public static CoverageTable[] CreateMultipleCoverageTables(long initPos, ushort[] offsets, BinaryReader reader)
    {
        var results = new CoverageTable[offsets.Length];
        for (var i = 0; i < results.Length; ++i) results[i] = CreateFrom(reader, initPos + offsets[i]);
        return results;
    }

#if DEBUG

#endif
}

public class CoverageFmt1 : CoverageTable
{
    internal ushort[] _orderedGlyphIdList;

    public static CoverageFmt1 CreateFrom(BinaryReader reader)
    {
        // CoverageFormat1 table: Individual glyph indices
        // Type      Name                     Description
        // uint16    CoverageFormat           Format identifier-format = 1
        // uint16    GlyphCount               Number of glyphs in the GlyphArray
        // uint16    GlyphArray[GlyphCount]   Array of glyph IDs — in numerical order

        var glyphCount = reader.ReadUInt16();
        var glyphs = reader.ReadUInt16Array(glyphCount);
        return new CoverageFmt1 { _orderedGlyphIdList = glyphs };
    }

    public override int FindPosition(ushort glyphIndex)
    {
        // "The glyph indices must be in numerical order for binary searching of the list"
        // (https://www.microsoft.com/typography/otspec/chapter2.htm#coverageFormat1)
        var n = Array.BinarySearch(_orderedGlyphIdList, glyphIndex);
        return n < 0 ? -1 : n;
    }

    public override IEnumerable<ushort> GetExpandedValueIter()
    {
        return _orderedGlyphIdList;
    }

#if DEBUG

    public override string ToString()
    {
        var stringList = new List<string>();
        foreach (var g in _orderedGlyphIdList) stringList.Add(g.ToString());
        return "CoverageFmt1: " + string.Join(",", stringList.ToArray());
    }
#endif
}

public class CoverageFmt2 : CoverageTable
{
    internal ushort[] _coverageIndices;
    internal ushort[] _endIndices;

    internal ushort[] _startIndices;

    private int RangeCount => _startIndices.Length;

    public override int FindPosition(ushort glyphIndex)
    {
        // Ranges must be in glyph ID order, and they must be distinct, with no overlapping.
        // [...] quick calculation of the Coverage Index for any glyph in any range using the
        // formula: Coverage Index (glyphID) = startCoverageIndex + glyphID - startGlyphID.
        // (https://www.microsoft.com/typography/otspec/chapter2.htm#coverageFormat2)
        var n = Array.BinarySearch(_endIndices, glyphIndex);
        n = n < 0 ? ~n : n;
        if (n >= RangeCount || glyphIndex < _startIndices[n]) return -1;
        return _coverageIndices[n] + glyphIndex - _startIndices[n];
    }

    public override IEnumerable<ushort> GetExpandedValueIter()
    {
        for (var i = 0; i < RangeCount; ++i)
        for (var n = _startIndices[i]; n <= _endIndices[i]; ++n)
            yield return n;
    }

    public static CoverageFmt2 CreateFrom(BinaryReader reader)
    {
        // CoverageFormat2 table: Range of glyphs
        // Type      Name                     Description
        // uint16    CoverageFormat           Format identifier-format = 2
        // uint16    RangeCount               Number of RangeRecords
        // struct    RangeRecord[RangeCount]  Array of glyph ranges — ordered by StartGlyphID.
        //
        // RangeRecord
        // Type      Name                Description
        // uint16    StartGlyphID        First glyph ID in the range
        // uint16    EndGlyphID          Last glyph ID in the range
        // uint16    StartCoverageIndex  Coverage Index of first glyph ID in range

        var rangeCount = reader.ReadUInt16();
        var startIndices = new ushort[rangeCount];
        var endIndices = new ushort[rangeCount];
        var coverageIndices = new ushort[rangeCount];
        for (var i = 0; i < rangeCount; ++i)
        {
            startIndices[i] = reader.ReadUInt16();
            endIndices[i] = reader.ReadUInt16();
            coverageIndices[i] = reader.ReadUInt16();
        }

        return new CoverageFmt2
        {
            _startIndices = startIndices,
            _endIndices = endIndices,
            _coverageIndices = coverageIndices
        };
    }

#if DEBUG

    public override string ToString()
    {
        var stringList = new List<string>();
        for (var i = 0; i < RangeCount; ++i) stringList.Add(string.Format("{0}-{1}", _startIndices[i], _endIndices[i]));
        return "CoverageFmt2: " + string.Join(",", stringList.ToArray());
    }
#endif
}