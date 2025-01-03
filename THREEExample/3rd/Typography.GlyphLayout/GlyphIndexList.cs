//MIT, 2016-present, WinterDev

using System.Collections.Generic;
using Typography.OpenFont.Tables;

namespace Typography.TextLayout;

/// <summary>
///     impl replaceable glyph index list
/// </summary>
public class GlyphIndexList : IGlyphIndexList
{
    private readonly List<ushort> _glyphIndices = new();
    private readonly List<int> _inputCodePointIndexList = new();
    private readonly List<GlyphIndexToUserCodePoint> _mapGlyphIndexToUserCodePoint = new();

#if DEBUG
    private readonly List<GlyphIndexToUserCodePoint> _tmpGlypIndexBackup = new();
#endif
    private ushort _originalCodePointOffset;

    /// <summary>
    ///     glyph count may be more or less than original user char list (from substitution process)
    /// </summary>
    public int Count => _glyphIndices.Count;

    //
    public ushort this[int index] => _glyphIndices[index];

    /// <summary>
    ///     remove:add_new 1:1
    /// </summary>
    /// <param name="index"></param>
    /// <param name="newGlyphIndex"></param>
    public void Replace(int index, ushort newGlyphIndex)
    {
        _glyphIndices[index] = newGlyphIndex;
    }

    /// <summary>
    ///     remove:add_new >=1:1
    /// </summary>
    /// <param name="index"></param>
    /// <param name="removeLen"></param>
    /// <param name="newGlyphIndex"></param>
    public void Replace(int index, int removeLen, ushort newGlyphIndex)
    {
        //eg f-i ligation
        //original f glyph and i glyph are removed 
        //and then replace with a single glyph 
        _glyphIndices.RemoveRange(index, removeLen);
        _glyphIndices.Insert(index, newGlyphIndex);
        //------------------------------------------------  

        var firstRemove = _mapGlyphIndexToUserCodePoint[index];

#if DEBUG
        _tmpGlypIndexBackup.Clear();
        var endAt = index + removeLen;
        for (var i = index; i < endAt; ++i) _tmpGlypIndexBackup.Add(_mapGlyphIndexToUserCodePoint[i]);
        _tmpGlypIndexBackup.Clear();
#endif
        //TODO: check if removeLen > ushort.Max
        var newMap = new GlyphIndexToUserCodePoint(firstRemove.o_codepoint_charOffset, (ushort)removeLen);
#if DEBUG
        //newMap.dbug_glyphIndex = newGlyphIndex;
#endif

        //------------------------------------------------ 
        _mapGlyphIndexToUserCodePoint.RemoveRange(index, removeLen);
        _mapGlyphIndexToUserCodePoint.Insert(index, newMap);
    }

    /// <summary>
    ///     remove: add_new 1:>=1
    /// </summary>
    /// <param name="index"></param>
    /// <param name="newGlyphIndices"></param>
    public void Replace(int index, ushort[] newGlyphIndices)
    {
        _glyphIndices.RemoveAt(index);
        _glyphIndices.InsertRange(index, newGlyphIndices);
        var cur = _mapGlyphIndexToUserCodePoint[index];
        _mapGlyphIndexToUserCodePoint.RemoveAt(index);
        //insert 
        var j = newGlyphIndices.Length;
        for (var i = 0; i < j; ++i)
        {
            var newglyph = new GlyphIndexToUserCodePoint(cur.o_codepoint_charOffset, 1);
#if DEBUG
            //newglyph.dbug_glyphIndex = newGlyphIndices[i];
#endif
            //may point to the same user char                 
            _mapGlyphIndexToUserCodePoint.Insert(index, newglyph);
        }
    }

    public void Clear()
    {
        _glyphIndices.Clear();
        _originalCodePointOffset = 0;
        _inputCodePointIndexList.Clear();
        _mapGlyphIndexToUserCodePoint.Clear();
    }

    /// <summary>
    ///     add codepoint index and its glyph index
    /// </summary>
    /// <param name="codePointIndex">index to codepoint element in code point array</param>
    /// <param name="glyphIndex">map to glyphindex</param>
    public void AddGlyph(int codePointIndex, ushort glyphIndex)
    {
        //so we can monitor what substituion process

        _inputCodePointIndexList.Add(codePointIndex);
        _glyphIndices.Add(glyphIndex);

        var glyphIndexToCharMap = new GlyphIndexToUserCodePoint(_originalCodePointOffset, 1);
#if DEBUG
        //glyphIndexToCharMap.dbug_glyphIndex = glyphIndex;
#endif
        _mapGlyphIndexToUserCodePoint.Add(glyphIndexToCharMap);
        _originalCodePointOffset++;
    }

    //
    public void GetGlyphIndexAndMap(int index, out ushort glyphIndex, out ushort input_codepointOffset,
        out ushort input_mapLen)
    {
        glyphIndex = _glyphIndices[index];
        var glyphIndexToUserCodePoint = _mapGlyphIndexToUserCodePoint[index];
        input_codepointOffset = glyphIndexToUserCodePoint.o_codepoint_charOffset;
        input_mapLen = glyphIndexToUserCodePoint.len;
    }


    public void CreateMapFromUserCodePointToGlyphIndices(List<UserCodePointToGlyphIndex> mapUserCodePointToGlyphIndex)
    {
        //(optional)
        //this method should be called after we finish the substitution process 
        //--------------------------------------
        var codePointCount = _inputCodePointIndexList.Count;
        for (var i = 0; i < codePointCount; ++i)
        {
            //
            var codePointToGlyphIndexMap = new UserCodePointToGlyphIndex();
            //set index that point to original codePointIndex
            codePointToGlyphIndexMap.userCodePointIndex = _inputCodePointIndexList[i];
            //
            mapUserCodePointToGlyphIndex.Add(codePointToGlyphIndexMap);
        }
        //--------------------------------------
        //then fill the user-codepoint with glyph information information 

        var glyphIndexCount = _glyphIndices.Count;
        for (var i = 0; i < glyphIndexCount; ++i)
        {
            var glyphIndexToUserCodePoint = _mapGlyphIndexToUserCodePoint[i];
            //
            var charToGlyphIndexMap = mapUserCodePointToGlyphIndex[glyphIndexToUserCodePoint.o_codepoint_charOffset];
            charToGlyphIndexMap.AppendData((ushort)(i + 1), glyphIndexToUserCodePoint.len);
            //replace with the changed value
            mapUserCodePointToGlyphIndex[glyphIndexToUserCodePoint.o_codepoint_charOffset] = charToGlyphIndexMap;
        }
    }

    /// <summary>
    ///     map from glyph index to original user char
    /// </summary>
    private readonly struct GlyphIndexToUserCodePoint
    {
        /// <summary>
        ///     offset from start layout char
        /// </summary>
        public readonly ushort o_codepoint_charOffset;

        public readonly ushort len;
#if DEBUG
        public readonly short dbug_glyphIndex;
#endif
        public GlyphIndexToUserCodePoint(ushort o_user_charOffset, ushort len)
        {
            this.len = len;
            o_codepoint_charOffset = o_user_charOffset;
#if DEBUG
            dbug_glyphIndex = 0;
#endif
        }
#if DEBUG
        public override string ToString()
        {
            return "codepoint_offset: " + o_codepoint_charOffset + " : len" + len;
        }
#endif
    }
}