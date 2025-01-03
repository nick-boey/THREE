//MIT, 2019-present, WinterDev

using System.IO;

namespace Typography.OpenFont.Tables;

internal class BitmapFontGlyphSource
{
    private readonly CBLC _cblc; //bitmap locator
    private CBDT _cbdt;

    public BitmapFontGlyphSource(CBLC cblc)
    {
        _cblc = cblc;
    }

    /// <summary>
    ///     load new bitmap embeded data
    /// </summary>
    /// <param name="cbdt"></param>
    public void LoadCBDT(CBDT cbdt)
    {
        _cbdt = cbdt;
    }

    /// <summary>
    ///     clear and remove existing bitmap embeded data
    /// </summary>
    public void UnloadCBDT()
    {
        if (_cbdt != null)
        {
            _cbdt.RemoveOldMemoryStreamAndReaders();
            _cbdt = null;
        }
    }

    public void CopyBitmapContent(Glyph glyph, Stream outputStream)
    {
        _cbdt.CopyBitmapContent(glyph, outputStream);
    }

    public Glyph[] BuildGlyphList()
    {
        var glyphs = _cblc.BuildGlyphList();
        for (var i = 0; i < glyphs.Length; ++i) _cbdt.FillGlyphInfo(glyphs[i]);
        return glyphs;
    }
}