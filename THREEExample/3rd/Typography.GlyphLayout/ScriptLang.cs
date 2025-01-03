//MIT, 2016-present, WinterDev

using System;
using System.Text;

namespace Typography.OpenFont;

/// <summary>
///     script tag and lang_feature tag request for GSUB, GPOS
/// </summary>
public readonly struct ScriptLang
{
    /// <summary>
    ///     script tag
    /// </summary>
    public readonly uint scriptTag;

    /// <summary>
    ///     syslang tag
    /// </summary>
    public readonly uint sysLangTag;

    public ScriptLang(uint scriptTag, uint sysLangTag = 0)
    {
        this.scriptTag = scriptTag;
        this.sysLangTag = sysLangTag;
    }

    public ScriptLang(string scriptTag, string sysLangTag = null)
    {
        this.sysLangTag = sysLangTag == null ? 0 : StringToTag(sysLangTag);
        this.scriptTag = StringToTag(scriptTag);
    }
#if DEBUG
    public override string ToString()
    {
        return TagToString(scriptTag) + ":" + TagToString(sysLangTag);
    }
#endif
    public bool IsEmpty()
    {
        return scriptTag == 0 && sysLangTag == 0;
    }

    private static byte GetByte(char c)
    {
        if (c >= 0 && c < 256) return (byte)c;
        return 0;
    }

    private static uint StringToTag(string str)
    {
        if (string.IsNullOrEmpty(str) || str.Length != 4) return 0;

        var buff = str.ToCharArray();
        var b0 = GetByte(buff[0]);
        var b1 = GetByte(buff[1]);
        var b2 = GetByte(buff[2]);
        var b3 = GetByte(buff[3]);

        return (uint)((b0 << 24) | (b1 << 16) | (b2 << 8) | b3);
    }

    private static string TagToString(uint tag)
    {
        var bytes = BitConverter.GetBytes(tag);
        Array.Reverse(bytes);
        return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
    }

    public string GetScriptTagString()
    {
        return TagToString(scriptTag);
    }

    public string GetLangTagString()
    {
        return TagToString(sysLangTag);
    }
}