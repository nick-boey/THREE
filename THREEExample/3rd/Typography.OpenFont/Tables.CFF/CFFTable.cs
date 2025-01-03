﻿//Apache2, 2018, Apache/PDFBox Authors ( https://github.com/apache/pdfbox) 
//
//
//Apache PDFBox
//Copyright 2014 The Apache Software Foundation

//This product includes software developed at
//The Apache Software Foundation(http://www.apache.org/).

//Based on source code originally developed in the PDFBox and
//FontBox projects.

//Copyright (c) 2002-2007, www.pdfbox.org

//Based on source code originally developed in the PaDaF project.
//Copyright (c) 2010 Atos Worldline SAS

//Includes the Adobe Glyph List
//Copyright 1997, 1998, 2002, 2007, 2010 Adobe Systems Incorporated.

//Includes the Zapf Dingbats Glyph List
//Copyright 2002, 2010 Adobe Systems Incorporated.

//Includes OSXAdapter
//Copyright (C) 2003-2007 Apple, Inc., All Rights Reserved

//----------------
//ccf1 spec from http://wwwimages.adobe.com/www.adobe.com/content/dam/acom/en/devnet/font/pdfs/5176.CFF.pdf

//------------------------------------------------------------------
//Apache2, 2018-present, WinterDev


using System;
using System.IO;
using Typography.OpenFont.CFF;

namespace Typography.OpenFont.Tables;

//from https://docs.microsoft.com/en-us/typography/opentype/spec/cff
//This table contains a Compact Font Format font representation (also known as a PostScript Type 1, or CIDFont)
//and is structured according to
//Adobe Technical Note #5176: “The Compact Font Format Specification,” (https://wwwimages2.adobe.com/content/dam/acom/en/devnet/font/pdfs/5176.CFF.pdf)
//and 
//Adobe Technical Note #5177: “Type 2 Charstring Format.” (https://wwwimages2.adobe.com/content/dam/acom/en/devnet/font/pdfs/5177.Type2.pdf)

//...
// ... glyph data is accessed through the CharStrings INDEX of the CFF table.

//...
//The CFF Top DICT must specify a CharstringType value of 2. 
//The numGlyphs field in the 'maxp' table must be the same as the number of entries in the CFF's CharStrings INDEX. 
//The OpenType font glyph index is the same as the CFF glyph index for all glyphs in the font.

//Note that, in an OpenType font collection file, a single 'CFF ' table can be shared across multiple fonts;
//names used by applications must be those provided in the 'name' table, not the Name INDEX entry.
//The CFF Top DICT must specify a CharstringType value of 2. 
//The numGlyphs field in the 'maxp' table must be the same as the number of entries in the CFF’s CharStrings INDEX.
//The OpenType font glyph index is the same as the CFF glyph index for all glyphs in the font.

internal class CFFTable : TableEntry
{
    public const string _N = "CFF "; //4 chars, left 1 blank whitespace
    //

    public override string Name => _N;

    //
    internal Cff1FontSet Cff1FontSet { get; private set; }

    protected override void ReadContentFrom(BinaryReader reader)
    {
        var startAt = reader.BaseStream.Position;
        //
        //
        //Table 8 Header Format
        //Type      Name    Description
        //Card8     major   Format major version(starting at 1)
        //Card8     minor   Format minor version(starting at 0)
        //Card8     hdrSize Header size(bytes)
        //OffSize   offSize Absolute offset(0) size
        var header = reader.ReadBytes(4);
        var major = header[0];
        var minor = header[1];
        var hdrSize = header[2];
        var offSize = header[3];
        ////---------
        //name index

        switch (major)
        {
            default: throw new NotSupportedException();
            case 1:
            {
                var cff1 = new Cff1Parser();
                cff1.ParseAfterHeader(startAt, reader);
                Cff1FontSet = cff1.ResultCff1FontSet;
            }
                break;
            case 2:
            {
                var cff2 = new Cff2Parser();
                cff2.ParseAfterHeader(reader);
            }
                break;
        }
    }
}