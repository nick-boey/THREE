﻿//MIT, 2016-present, WinterDev

using System.Collections.Generic;
using Typography.OpenFont;

namespace Typography.Contours;

//see also: PixelFarm's  class HintedVxsGlyphCollection 

//TODO: review this class name again 

public class GlyphMeshCollection<T>
{
    private readonly Dictionary<GlyphKey, Dictionary<ushort, T>> _registerGlyphCollection = new();

    private readonly List<GlyphKey> _tempKeys = new();

    //hint glyph collection        
    //per typeface
    private Dictionary<ushort, T> _currentGlyphDic;

    public void SetCacheInfo(Typeface typeface, float sizeInPts, HintTechnique hintTech)
    {
        //TODO: review key object again, if we need to store a typeface object ?
        //check if we have create the context for this request parameters?
        var key = new GlyphKey { hintTech = hintTech, sizeInPts = sizeInPts, typeface = typeface };
        if (!_registerGlyphCollection.TryGetValue(key, out _currentGlyphDic))
        {
            //if not found 
            //create new
            _currentGlyphDic = new Dictionary<ushort, T>();
            _registerGlyphCollection.Add(key, _currentGlyphDic);
        }
    }

    public bool TryGetCacheGlyph(ushort glyphIndex, out T vxs)
    {
        return _currentGlyphDic.TryGetValue(glyphIndex, out vxs);
    }

    public void RegisterCachedGlyph(ushort glyphIndex, T vxs)
    {
        _currentGlyphDic[glyphIndex] = vxs;
    }

    public void ClearAll()
    {
        _currentGlyphDic = null;
        _registerGlyphCollection.Clear();
    }

    public void Clear(Typeface typeface)
    {
        //clear all registered typeface glyph
        _tempKeys.Clear();
        foreach (var k in _registerGlyphCollection.Keys)
            //collect ...
            if (k.typeface == typeface)
                _tempKeys.Add(k);

        //
        for (var i = _tempKeys.Count - 1; i >= 0; --i) _registerGlyphCollection.Remove(_tempKeys[i]);
        _tempKeys.Clear();
    }

    private struct GlyphKey
    {
        public HintTechnique hintTech;
        public Typeface typeface;
        public float sizeInPts;
    }
}