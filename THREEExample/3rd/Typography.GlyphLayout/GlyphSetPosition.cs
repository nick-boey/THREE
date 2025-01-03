//MIT, 2016-present, WinterDev

using System.Collections.Generic;
using System.Diagnostics;
using Typography.OpenFont;
using Typography.OpenFont.Tables;

namespace Typography.TextLayout;

/// <summary>
///     glyph set position manager
/// </summary>
internal class GlyphSetPosition
{
    private readonly GPOS _gposTable;

#if DEBUG
    private readonly Typeface dbugTypeface;
#endif
    internal List<GPOS.LookupTable> _lookupTables;
#if DEBUG
    public string dbugScriptLang;
#endif
    public GlyphSetPosition(Typeface typeface, uint scriptTag, uint langTag)
    {
        ScriptTag = scriptTag; //script tag
        LangTag = langTag; //lang tag


        //check if this lang has 
        _gposTable = typeface.GPOSTable;

        if (_gposTable == null) return;

        var scriptTable = _gposTable.ScriptList[scriptTag];
        if (scriptTable == null) return; // early exit if no lookup tables 


        var selectedLang = scriptTable.defaultLang;
        if (selectedLang == null) return; //no default

        if (LangTag != 0 && scriptTable.langSysTables != null) //use default
            //find matching lang
            for (var i = 0; i < scriptTable.langSysTables.Length; ++i)
                if (scriptTable.langSysTables[i].langSysTagIden == LangTag)
                {
                    //found
                    selectedLang = scriptTable.langSysTables[i];
                    break;
                }

#if DEBUG
        dbugTypeface = typeface;
        if (selectedLang.HasRequireFeature) Debugger.Break();
#endif
        //other feature
        if (selectedLang.featureIndexList == null) return; // early exit

        //---------
        //get features 
        _lookupTables = new List<GPOS.LookupTable>();

        for (var i = 0; i < selectedLang.featureIndexList.Length; ++i)
        {
            var feature = _gposTable.FeatureList.featureTables[selectedLang.featureIndexList[i]];
            var includeThisFeature = false;
            switch (feature.TagName)
            {
                case "mark": //mark=> mark to base
                case "mkmk": //mkmk => mark to mask 
                    //current version we implement this 2 features
                    includeThisFeature = true;
                    break;
                case "kern":
                    //test with Candara font
                    includeThisFeature = true;
                    //If palt is activated, there is no requirement that kern must also be activated. 
                    //If kern is activated, palt must also be activated if it exists.
                    //https://www.microsoft.com/typography/OTSpec/features_pt.htm#palt
                    break;
                //case "palt":
                //    break;

                case "abvm":
                case "blwm":
                case "dist":
                    includeThisFeature = true;
                    break;
                default:
                    Debug.WriteLine("gpos_skip_tag:" + feature.TagName);
                    break;
            }

            if (includeThisFeature)
                foreach (var lookupIndex in feature.LookupListIndices)
                    _lookupTables.Add(_gposTable.LookupList[lookupIndex]);
        }
    }

    public uint ScriptTag { get; }
    public uint LangTag { get; }

    public void DoGlyphPosition(IGlyphPositions glyphPositions)
    {
        //early exit if no lookup tables
        //load
        if (_lookupTables == null) return;
        //
        var j = _lookupTables.Count;
        for (var i = 0; i < j; ++i) _lookupTables[i].DoGlyphPosition(glyphPositions, 0, glyphPositions.Count);
    }
}