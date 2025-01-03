//Apache2, 2017-present, WinterDev
//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Typography.OpenFont.Extensions;
using Typography.OpenFont.Tables;
using Typography.OpenFont.Trimmable;
using Typography.OpenFont.WebFont;

namespace Typography.OpenFont;

[Flags]
public enum ReadFlags
{
    Full = 0,
    Name = 1,
    Metrics = 1 << 2,
    AdvancedLayout = 1 << 3,
    Variation = 1 << 4
}

internal static class KnownFontFiles
{
    public static bool IsTtcf(ushort u1, ushort u2)
    {
        //https://docs.microsoft.com/en-us/typography/opentype/spec/otff#ttc-header
        //check if 1st 4 bytes is ttcf or not  
        return ((u1 >> 8) & 0xff) == (byte)'t' &&
               (u1 & 0xff) == (byte)'t' &&
               ((u2 >> 8) & 0xff) == (byte)'c' &&
               (u2 & 0xff) == (byte)'f';
    }

    public static bool IsWoff(ushort u1, ushort u2)
    {
        return ((u1 >> 8) & 0xff) == (byte)'w' && //0x77
               (u1 & 0xff) == (byte)'O' && //0x4f 
               ((u2 >> 8) & 0xff) == (byte)'F' && // 0x46
               (u2 & 0xff) == (byte)'F'; //0x46 
    }

    public static bool IsWoff2(ushort u1, ushort u2)
    {
        return ((u1 >> 8) & 0xff) == (byte)'w' && //0x77
               (u1 & 0xff) == (byte)'O' && //0x4f 
               ((u2 >> 8) & 0xff) == (byte)'F' && //0x46
               (u2 & 0xff) == (byte)'2'; //0x32 
    }
}

public class OpenFontReader
{
    private static string BuildTtcfName(PreviewFontInfo[] members)
    {
        //THIS IS MY CONVENTION for TrueType collection font name
        //you can change this to fit your need.

        var stbuilder = new StringBuilder();
        stbuilder.Append("TTCF: " + members.Length);
        var uniqueNames = new Dictionary<string, bool>();
        for (uint i = 0; i < members.Length; ++i)
        {
            var member = members[i];
            if (!uniqueNames.ContainsKey(member.Name))
            {
                uniqueNames.Add(member.Name, true);
                stbuilder.Append("," + member.Name);
            }
        }

        return stbuilder.ToString();
    }


    /// <summary>
    ///     read only name entry
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public PreviewFontInfo ReadPreview(Stream stream)
    {
        //var little = BitConverter.IsLittleEndian;
        using (var input = new ByteOrderSwappingBinaryReader(stream))
        {
            var majorVersion = input.ReadUInt16();
            var minorVersion = input.ReadUInt16();

            if (KnownFontFiles.IsTtcf(majorVersion, minorVersion))
            {
                //this font stream is 'The Font Collection'
                var ttcHeader = ReadTTCHeader(input);
                var members = new PreviewFontInfo[ttcHeader.numFonts];
                for (uint i = 0; i < ttcHeader.numFonts; ++i)
                {
                    input.BaseStream.Seek(ttcHeader.offsetTables[i], SeekOrigin.Begin);
                    var member = members[i] = ReadActualFontPreview(input, false);
                    member.ActualStreamOffset = ttcHeader.offsetTables[i];
                }

                return new PreviewFontInfo(BuildTtcfName(members), members);
            }

            if (KnownFontFiles.IsWoff(majorVersion, minorVersion))
            {
                //check if we enable woff or not
                var woffReader = new WoffReader();
                input.BaseStream.Position = 0;
                return woffReader.ReadPreview(input);
            }

            if (KnownFontFiles.IsWoff2(majorVersion, minorVersion))
            {
                //check if we enable woff2 or not
                var woffReader = new Woff2Reader();
                input.BaseStream.Position = 0;
                return woffReader.ReadPreview(input);
            }

            return ReadActualFontPreview(input, true); //skip version data (majorVersion, minorVersion)
        }
    }

    private FontCollectionHeader ReadTTCHeader(ByteOrderSwappingBinaryReader input)
    {
        //https://docs.microsoft.com/en-us/typography/opentype/spec/otff#ttc-header
        //TTC Header Version 1.0:
        //Type 	    Name 	        Description
        //TAG 	    ttcTag 	        Font Collection ID string: 'ttcf' (used for fonts with CFF or CFF2 outlines as well as TrueType outlines)
        //uint16 	majorVersion 	Major version of the TTC Header, = 1.
        //uint16 	minorVersion 	Minor version of the TTC Header, = 0.
        //uint32 	numFonts 	    Number of fonts in TTC
        //Offset32 	offsetTable[numFonts] 	Array of offsets to the OffsetTable for each font from the beginning of the file

        //TTC Header Version 2.0:
        //Type 	    Name 	        Description
        //TAG 	    ttcTag 	        Font Collection ID string: 'ttcf'
        //uint16 	majorVersion 	Major version of the TTC Header, = 2.
        //uint16 	minorVersion 	Minor version of the TTC Header, = 0.
        //uint32 	numFonts 	    Number of fonts in TTC
        //Offset32 	offsetTable[numFonts] 	Array of offsets to the OffsetTable for each font from the beginning of the file
        //uint32 	dsigTag 	    Tag indicating that a DSIG table exists, 0x44534947 ('DSIG') (null if no signature)
        //uint32 	dsigLength 	    The length (in bytes) of the DSIG table (null if no signature)
        //uint32 	dsigOffset 	    The offset (in bytes) of the DSIG table from the beginning of the TTC file (null if no signature)

        var ttcHeader = new FontCollectionHeader();

        ttcHeader.majorVersion = input.ReadUInt16();
        ttcHeader.minorVersion = input.ReadUInt16();
        var numFonts = input.ReadUInt32();
        var offsetTables = new int[numFonts];
        for (uint i = 0; i < numFonts; ++i) offsetTables[i] = input.ReadInt32();

        ttcHeader.numFonts = numFonts;
        ttcHeader.offsetTables = offsetTables;
        //
        if (ttcHeader.majorVersion == 2)
        {
            ttcHeader.dsigTag = input.ReadUInt32();
            ttcHeader.dsigLength = input.ReadUInt32();
            ttcHeader.dsigOffset = input.ReadUInt32();

            if (ttcHeader.dsigTag == 0x44534947)
            {
                //Tag indicating that a DSIG table exists
                //TODO: goto DSIG add read signature
            }
        }

        return ttcHeader;
    }

    private PreviewFontInfo ReadActualFontPreview(ByteOrderSwappingBinaryReader input, bool skipVersionData)
    {
        if (!skipVersionData)
        {
            var majorVersion = input.ReadUInt16();
            var minorVersion = input.ReadUInt16();
        }

        var tableCount = input.ReadUInt16();
        var searchRange = input.ReadUInt16();
        var entrySelector = input.ReadUInt16();
        var rangeShift = input.ReadUInt16();

        var tables = new TableEntryCollection();
        for (var i = 0; i < tableCount; i++) tables.AddEntry(new UnreadTableEntry(ReadTableHeader(input)));
        return ReadPreviewFontInfo(tables, input);
    }

    public Typeface Read(Stream stream, int streamStartOffset = 0, ReadFlags readFlags = ReadFlags.Full)
    {
        var typeface = new Typeface();
        if (Read(typeface, null, stream, streamStartOffset, readFlags)) return typeface;
        return null;
    }

    internal bool Read(Typeface typeface, RestoreTicket ticket, Stream stream, int streamStartOffset = 0,
        ReadFlags readFlags = ReadFlags.Full)
    {
        if (streamStartOffset > 0)
            //eg. for ttc
            stream.Seek(streamStartOffset, SeekOrigin.Begin);
        using (var input = new ByteOrderSwappingBinaryReader(stream))
        {
            var majorVersion = input.ReadUInt16();
            var minorVersion = input.ReadUInt16();

            if (KnownFontFiles.IsTtcf(majorVersion, minorVersion))
                //this font stream is 'The Font Collection'                    
                //To read content of ttc=> one must specific the offset
                //so use read preview first=> you will know that what are inside the ttc.                    
                return false;

            if (KnownFontFiles.IsWoff(majorVersion, minorVersion))
            {
                //check if we enable woff or not
                var woffReader = new WoffReader();
                input.BaseStream.Position = 0;
                return woffReader.Read(typeface, input, ticket);
            }

            if (KnownFontFiles.IsWoff2(majorVersion, minorVersion))
            {
                //check if we enable woff2 or not
                var woffReader = new Woff2Reader();
                input.BaseStream.Position = 0;
                return woffReader.Read(typeface, input, ticket);
            }
            //-----------------------------------------------------------------


            var tableCount = input.ReadUInt16();
            var searchRange = input.ReadUInt16();
            var entrySelector = input.ReadUInt16();
            var rangeShift = input.ReadUInt16();
            //------------------------------------------------------------------ 
            var tables = new TableEntryCollection();
            for (var i = 0; i < tableCount; i++) tables.AddEntry(new UnreadTableEntry(ReadTableHeader(input)));
            //------------------------------------------------------------------ 

            return ReadTableEntryCollection(typeface, ticket, tables, input);
        }
    }


    internal PreviewFontInfo ReadPreviewFontInfo(TableEntryCollection tables, BinaryReader input)
    {
        var rd = new EntriesReaderHelper(tables, input);

        var nameEntry = rd.Read(new NameEntry());
        var os2Table = rd.Read(new OS2Table());
        //for preview, read ONLY  script list from gsub and gpos (set OnlyScriptList).
        var metaTable = rd.Read(new Meta());

        var gsub = rd.Read(new GSUB { OnlyScriptList = true });
        var gpos = rd.Read(new GPOS { OnlyScriptList = true });
        var cmap = rd.Read(new Cmap());
        //gsub and gpos contains actual script_list that are in the typeface

        var langs = new Languages();
        langs.Update(os2Table, metaTable, cmap, gsub, gpos);

        return new PreviewFontInfo(
            nameEntry,
            os2Table,
            langs);
    }

    private bool ReadTableEntryCollectionOnRestoreMode(Typeface typeface, RestoreTicket ticket,
        TableEntryCollection tables, BinaryReader input)
    {
        //RESTORE MODE
        //check header matches

        if (!typeface.IsTrimmed() ||
            !typeface.CompareOriginalHeadersWithNewlyLoadOne(tables.CloneTableHeaders()))
            return false;


        var rd = new EntriesReaderHelper(tables, input);
        //PART 1: basic information
        //..
        //------------------------------------
        //PART 2: glyphs detail 
        //2.1 True type font
        var glyphLocations = ticket.HasTtf
            ? rd.Read(new GlyphLocations(typeface.MaxProfile.GlyphCount, typeface.Head.WideGlyphLocations))
            : null;
        var glyf = ticket.HasTtf ? rd.Read(new Glyf(glyphLocations)) : null;

        typeface.GaspTable = ticket.GaspTable ? rd.Read(new Gasp()) : null;

        typeface.SetColorAndPalTable(
            ticket.COLRTable ? rd.Read(new COLR()) : null,
            ticket.CPALTable ? rd.Read(new CPAL()) : null);


        //2.2 Cff font             
        var cff = ticket.HasCff ? rd.Read(new CFFTable()) : null;

        var isPostScriptOutline = false;
        var isBitmapFont = false;

        if (glyf == null)
        {
            //check if this is cff table ?
            if (cff == null)
            {
                //check  cbdt/cblc ?
                if (ticket.HasBitmapSource)
                {
                    //reload only CBDT (embeded bitmap)
                    var cbdt = rd.Read(new CBDT());
                    typeface._bitmapFontGlyphSource.LoadCBDT(cbdt);
                    //just clone existing glyph 
                    isBitmapFont = true;
                }
                else
                {
                    //?
                    throw new NotSupportedException();
                }
            }
            else
            {
                isPostScriptOutline = true;
                typeface.SetCffFontSet(cff.Cff1FontSet);
            }
        }
        else
        {
            typeface.SetTtfGlyphs(glyf.Glyphs);
        }

        if (!isPostScriptOutline && !isBitmapFont)
        {
            //for true-type font outline
            var fpgmTable = rd.Read(new FpgmTable());
            //control values table
            var cvtTable = rd.Read(new CvtTable());
            var propProgramTable = rd.Read(new PrepTable());

            typeface.ControlValues = cvtTable?._controlValues;
            typeface.FpgmProgramBuffer = fpgmTable?._programBuffer;
            typeface.PrepProgramBuffer = propProgramTable?._programBuffer;
        }

        if (ticket.HasSvg) typeface.SetSvgTable(rd.Read(new SvgTable()));


#if DEBUG
        //test
        //int found = typeface.GetGlyphIndexByName("Uacute");
        if (typeface.IsCffFont)
            //optional???
            typeface.UpdateAllCffGlyphBounds();
#endif
        typeface._typefaceTrimMode = TrimMode.Restored;
        return true;
    }

    internal bool ReadTableEntryCollection(Typeface typeface, RestoreTicket ticket, TableEntryCollection tables,
        BinaryReader input)
    {
        if (ticket != null) return ReadTableEntryCollectionOnRestoreMode(typeface, ticket, tables, input);

        typeface.SetTableEntryCollection(tables.CloneTableHeaders());

        var rd = new EntriesReaderHelper(tables, input);
        //PART 1: basic information
        var os2Table = rd.Read(new OS2Table());
        var meta = rd.Read(new Meta());
        var nameEntry = rd.Read(new NameEntry());
        var head = rd.Read(new Head());
        var maxProfile = rd.Read(new MaxProfile());
        var horizontalHeader = rd.Read(new HorizontalHeader());
        var horizontalMetrics =
            rd.Read(new HorizontalMetrics(horizontalHeader.NumberOfHMetrics, maxProfile.GlyphCount));
        var vhea = rd.Read(new VerticalHeader());
        if (vhea != null)
        {
            var vmtx = rd.Read(new VerticalMetrics(vhea.NumOfLongVerMetrics));
        }

        var os2Select = new OS2FsSelection(os2Table.fsSelection);
        typeface._useTypographicMertic = os2Select.USE_TYPO_METRICS;

        var cmaps = rd.Read(new Cmap());
        var vdmx = rd.Read(new VerticalDeviceMetrics());
        var kern = rd.Read(new Kern());
        //------------------------------------
        //PART 2: glyphs detail 
        //2.1 True type font

        var glyphLocations = rd.Read(new GlyphLocations(maxProfile.GlyphCount, head.WideGlyphLocations));
        var glyf = rd.Read(new Glyf(glyphLocations));
        var gaspTable = rd.Read(new Gasp());
        var colr = rd.Read(new COLR());
        var cpal = rd.Read(new CPAL());

        //2.2 Cff font
        var postTable = rd.Read(new PostTable());
        var cff = rd.Read(new CFFTable());

        //additional math table (if available)
        var mathtable = rd.Read(new MathTable());
        //------------------------------------

        //PART 3: advanced typography             
        var gdef = rd.Read(new GDEF());
        var gsub = rd.Read(new GSUB());
        var gpos = rd.Read(new GPOS());
        var baseTable = rd.Read(new BASE());
        var jstf = rd.Read(new JSTF());

        var stat = rd.Read(new STAT());
        if (stat != null)
        {
            //variable font
            var fvar = rd.Read(new FVar());
            if (fvar != null)
            {
                var gvar = rd.Read(new GVar());
                var cvar = rd.Read(new CVar());
                var hvar = rd.Read(new HVar());
                var mvar = rd.Read(new MVar());
                var avar = rd.Read(new AVar());
            }
        }

        var isPostScriptOutline = false;
        var isBitmapFont = false;

        typeface.SetBasicTypefaceTables(os2Table, nameEntry, head, horizontalMetrics);
        if (glyf == null)
        {
            //check if this is cff table ?
            if (cff == null)
            {
                //check  cbdt/cblc ?
                var cblcTable = rd.Read(new CBLC());
                if (cblcTable != null)
                {
                    var cbdtTable = rd.Read(new CBDT());
                    //read cbdt 
                    //bitmap font

                    var bmpFontGlyphSrc = new BitmapFontGlyphSource(cblcTable);
                    bmpFontGlyphSrc.LoadCBDT(cbdtTable);
                    var glyphs = bmpFontGlyphSrc.BuildGlyphList();
                    typeface.SetBitmapGlyphs(glyphs, bmpFontGlyphSrc);
                    isBitmapFont = true;
                }
                else
                {
                    //TODO:
                    var fontBmpTable = rd.Read(new EBLC());
                    throw new NotSupportedException();
                }
            }
            else
            {
                isPostScriptOutline = true;
                typeface.SetCffFontSet(cff.Cff1FontSet);
            }
        }
        else
        {
            typeface.SetTtfGlyphs(glyf.Glyphs);
        }

        //----------------------------
        typeface.CmapTable = cmaps;
        typeface.KernTable = kern;
        typeface.MaxProfile = maxProfile;
        typeface.HheaTable = horizontalHeader;
        //----------------------------
        typeface.GaspTable = gaspTable;
        typeface.GlyphLocations = glyphLocations;
        if (!isPostScriptOutline && !isBitmapFont)
        {
            //for true-type font outline
            var fpgmTable = rd.Read(new FpgmTable());
            //control values table
            var cvtTable = rd.Read(new CvtTable());
            var propProgramTable = rd.Read(new PrepTable());

            typeface.ControlValues = cvtTable?._controlValues;
            typeface.FpgmProgramBuffer = fpgmTable?._programBuffer;
            typeface.PrepProgramBuffer = propProgramTable?._programBuffer;
        }

        //-------------------------
        typeface.LoadOpenFontLayoutInfo(
            gdef,
            gsub,
            gpos,
            baseTable,
            colr,
            cpal);
        //------------

        typeface.SetSvgTable(rd.Read(new SvgTable()));
        typeface.PostTable = postTable;

        if (mathtable != null) MathGlyphLoader.LoadMathGlyph(typeface, mathtable);
#if DEBUG
        //test
        //int found = typeface.GetGlyphIndexByName("Uacute");
        if (typeface.IsCffFont)
            //optional
            typeface.UpdateAllCffGlyphBounds();
#endif
        typeface.UpdateLangs(meta);
        typeface.UpdateFrequentlyUsedValues();
        return true;
    }

    private static TableHeader ReadTableHeader(BinaryReader input)
    {
        return new TableHeader(
            input.ReadUInt32(),
            input.ReadUInt32(),
            input.ReadUInt32(),
            input.ReadUInt32());
    }

    private class FontCollectionHeader
    {
        public uint dsigLength;

        public uint dsigOffset;

        //
        //if version 2
        public uint dsigTag;
        public ushort majorVersion;
        public ushort minorVersion;
        public uint numFonts;
        public int[] offsetTables;
    }


    private readonly struct EntriesReaderHelper
    {
        //a simple helper class
        private readonly TableEntryCollection _tables;
        private readonly BinaryReader _input;

        public EntriesReaderHelper(TableEntryCollection tables, BinaryReader input)
        {
            _tables = tables;
            _input = input;
        }

        /// <summary>
        ///     read table if exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName"></param>
        /// <param name="tables"></param>
        /// <param name="reader"></param>
        /// <param name="newTableDel"></param>
        /// <returns></returns>
        public T Read<T>(T resultTable) where T : TableEntry
        {
            if (_tables.TryGetTable(resultTable.Name, out var found))
            {
                //found table name
                //check if we have read this table or not
                if (found is UnreadTableEntry unreadTableEntry)
                {
                    //set header before actal read 
                    resultTable.Header = found.Header;
                    if (unreadTableEntry.HasCustomContentReader)
                        resultTable = unreadTableEntry.CreateTableEntry(_input, resultTable);
                    else
                        resultTable.LoadDataFrom(_input);
                    //then replace
                    _tables.ReplaceTable(resultTable);
                    return resultTable;
                }
#if DEBUG
                Debug.WriteLine("this table is already loaded");
                if (!(found is T)) throw new NotSupportedException();
#endif
                return found as T;
            }

            //not found
            return null;
        }
    }
}