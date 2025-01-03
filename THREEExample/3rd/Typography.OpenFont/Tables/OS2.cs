//Apache2, 2016-present, WinterDev 

using System;
using System.IO;

namespace Typography.OpenFont.Tables;

//https://docs.microsoft.com/en-us/typography/opentype/spec/os2
/// <summary>
///     OS2 and Windows metrics,
///     consists of a set of metrics and other data
///     that are REQUIRED in OpenType fonts.
/// </summary>
public class OS2Table : TableEntry
{
    public const string _N = "OS/2";

    //Tag 	    achVendID[4] 	    char 4 
    public uint achVendID; //see 'registered venders' at https://www.microsoft.com/typography/links/vendorlist.aspx

    //uint16 	fsSelection 	 
    //uint16 	usFirstCharIndex 	 
    //uint16 	usLastCharIndex 
    public ushort fsSelection; //Contains information concerning the nature of the font patterns
    //as specified by a font designer for the glyphs in a font.
    //Although every glyph in a font may have a different numeric aspect ratio, 
    //each glyph in a font of normal width is considered to have a relative aspect ratio of one.
    //When a new type style is created of a different width class (either by a font designer or by some automated means)
    //the relative aspect ratio of the characters in the new font is some percentage greater or less than those same characters in the normal 
    //font — it is this difference that this parameter specifies. 

    //usWidthClass
    //Value Description 	    C Definition 	        % of normal
    //1 	Ultra-condensed 	FWIDTH_ULTRA_CONDENSED 	50
    //2 	Extra-condensed 	FWIDTH_EXTRA_CONDENSED 	62.5
    //3 	Condensed 	        FWIDTH_CONDENSED 	    75
    //4 	Semi-condensed 	    FWIDTH_SEMI_CONDENSED 	87.5
    //5 	Medium (normal) 	FWIDTH_NORMAL 	        100
    //6 	Semi-expanded 	    FWIDTH_SEMI_EXPANDED 	112.5
    //7 	Expanded 	        FWIDTH_EXPANDED 	    125
    //8 	Extra-expanded 	    FWIDTH_EXTRA_EXPANDED 	150
    //9 	Ultra-expanded      FWIDTH_ULTRA_EXPANDED 	200


    public ushort fsType; //Type flags., embedding licensing rights for the font

    //uint8 	panose[10] 	        (array of bytes,len =10)
    public byte[] panose;
    public short sCapHeight;

    public short
        sFamilyClass; //This parameter is a classification of font-family design. ,see https://www.microsoft.com/typography/otspec/ibmfc.htm

    //int16 	sTypoAscender 	 
    //int16 	sTypoDescender 	 
    //int16 	sTypoLineGap 	 
    public short sTypoAscender;
    public short sTypoDescender;

    public short sTypoLineGap;

    //int16 	sxHeight 	 
    //int16 	sCapHeight 	  
    public short sxHeight;
    public uint ulCodePageRange1;

    public uint ulCodePageRange2;

    //uint32 	ulUnicodeRange1 	Bits 0-31
    //uint32 	ulUnicodeRange2 	Bits 32-63
    //uint32 	ulUnicodeRange3 	Bits 64-95
    //uint32 	ulUnicodeRange4 	Bits 96-127
    public uint ulUnicodeRange1;
    public uint ulUnicodeRange2;
    public uint ulUnicodeRange3;
    public uint ulUnicodeRange4;

    public ushort usBreakChar;

    //uint16 	usDefaultChar 	 
    //uint16 	usBreakChar 	 
    //uint16 	usMaxContext 	 
    //uint16 	usLowerOpticalPointSize 	 
    //uint16 	usUpperOpticalPointSize
    public ushort usDefaultChar;
    public ushort usFirstCharIndex;
    public ushort usLastCharIndex;
    public ushort usLowerOpticalPointSize;
    public ushort usMaxContext;
    public ushort usUpperOpticalPointSize;
    public ushort usWeightClass; //visual weight (degree of blackness or thickness of strokes), 0-1000

    //usWeightClass:
    //Value Description 	C Definition (from windows.h)
    //100 	Thin 	        FW_THIN
    //200 	Extra-light     FW_EXTRALIGHT
    //      (Ultra-light) 
    //300 	Light 	        FW_LIGHT
    //400 	Normal  	    FW_NORMAL
    //      (Regular)
    //500 	Medium 	        FW_MEDIUM
    //600 	Semi-bold   	FW_SEMIBOLD
    //      (Demi-bold)
    //700 	Bold 	        FW_BOLD
    //800 	Extra-bold  	FW_EXTRABOLD
    //      (Ultra-bold)
    //900 	Black (Heavy) 	FW_BLACK

    public ushort usWidthClass; //A relative change from the normal aspect ratio (width to height ratio), 

    //uint16 	usWinAscent 	 
    //uint16 	usWinDescent 	 
    //uint32 	ulCodePageRange1 	Bits 0-31
    //uint32 	ulCodePageRange2 	Bits 32-63
    public ushort usWinAscent;

    public ushort usWinDescent;
    //

    // Type     Name of  Entry        Comments
    //uint16 	version 	           0x0005
    //int16 	xAvgCharWidth 	    
    //uint16 	usWeightClass 	 
    //uint16 	usWidthClass 	 
    //uint16 	fsType 	 
    public ushort version; //0-5
    public short xAvgCharWidth; //just average, not recommend to use.
    public short yStrikeoutPosition;
    public short yStrikeoutSize;
    public short ySubscriptXOffset;

    //int16 	ySubscriptXSize 	 
    //int16 	ySubscriptYSize 	 
    //int16 	ySubscriptXOffset 	 
    //int16 	ySubscriptYOffset 	 
    //int16 	ySuperscriptXSize 	 
    //int16 	ySuperscriptYSize 	 
    //int16 	ySuperscriptXOffset 	 
    //int16 	ySuperscriptYOffset 	 
    //int16 	yStrikeoutSize 	 
    //int16 	yStrikeoutPosition 	 
    //int16 	sFamilyClass 	
    public short ySubscriptXSize;
    public short ySubscriptYOffset;
    public short ySubscriptYSize;
    public short ySuperscriptXOffset;
    public short ySuperscriptXSize;
    public short ySuperscriptYOffset;
    public short ySuperscriptYSize;
    public override string Name => _N;


#if DEBUG
    public override string ToString()
    {
        return version + "," + Utils.TagToString(achVendID);
    }
#endif
    protected override void ReadContentFrom(BinaryReader reader)
    {
        //Six versions of the OS/2 table have been defined: versions 0 to 5
        //Versions 0 to 4 were defined in earlier versions of the OpenType or
        //TrueType specifications. 

        switch (version = reader.ReadUInt16())
        {
            default: throw new NotSupportedException();
            case 0: //defined in TrueType revision 1.5
                ReadVersion0(reader);
                break;
            case 1: // defined in TrueType revision 1.66
                ReadVersion1(reader);
                break;
            case 2: //defined in OpenType version 1.2
                ReadVersion2(reader);
                break;
            case 3: //defined in OpenType version 1.4
                ReadVersion3(reader);
                break;
            case 4: //defined in OpenType version 1.6
                ReadVersion4(reader);
                break;
            case 5:
                ReadVersion5(reader);
                break;
        }
    }

    private void ReadVersion0(BinaryReader reader)
    {
        //https://www.microsoft.com/typography/otspec/os2ver0.htm
        //USHORT 	version 	0x0000
        //SHORT 	xAvgCharWidth 	 
        //USHORT 	usWeightClass 	 
        //USHORT 	usWidthClass 	 
        //USHORT 	fsType 	 
        xAvgCharWidth = reader.ReadInt16();
        usWeightClass = reader.ReadUInt16();
        usWidthClass = reader.ReadUInt16();
        fsType = reader.ReadUInt16();

        //SHORT 	ySubscriptXSize 	 
        //SHORT 	ySubscriptYSize 	 
        //SHORT 	ySubscriptXOffset 	 
        //SHORT 	ySubscriptYOffset 	 
        //SHORT 	ySuperscriptXSize 	 
        //SHORT 	ySuperscriptYSize 	 
        //SHORT 	ySuperscriptXOffset 	 
        //SHORT 	ySuperscriptYOffset 	 
        //SHORT 	yStrikeoutSize 	 
        //SHORT 	yStrikeoutPosition 	 
        //SHORT 	sFamilyClass 	 
        ySubscriptXSize = reader.ReadInt16();
        ySubscriptYSize = reader.ReadInt16();
        ySubscriptXOffset = reader.ReadInt16();
        ySubscriptYOffset = reader.ReadInt16();
        ySuperscriptXSize = reader.ReadInt16();
        ySuperscriptYSize = reader.ReadInt16();
        ySuperscriptXOffset = reader.ReadInt16();
        ySuperscriptYOffset = reader.ReadInt16();
        yStrikeoutSize = reader.ReadInt16();
        yStrikeoutPosition = reader.ReadInt16();
        sFamilyClass = reader.ReadInt16();
        //BYTE 	panose[10] 	 
        panose = reader.ReadBytes(10);
        //ULONG 	ulCharRange[4] 	Bits 0-31
        ulUnicodeRange1 = reader.ReadUInt32();
        //CHAR 	achVendID[4] 	 
        achVendID = reader.ReadUInt32();
        //USHORT 	fsSelection 	 
        //USHORT 	usFirstCharIndex 	 
        //USHORT 	usLastCharIndex 	 
        fsSelection = reader.ReadUInt16();
        usFirstCharIndex = reader.ReadUInt16();
        usLastCharIndex = reader.ReadUInt16();
        //SHORT 	sTypoAscender 	 
        //SHORT 	sTypoDescender 	 
        //SHORT 	sTypoLineGap 	 
        sTypoAscender = reader.ReadInt16();
        sTypoDescender = reader.ReadInt16();
        sTypoLineGap = reader.ReadInt16();
        //USHORT 	usWinAscent 	 
        //USHORT 	usWinDescent
        usWinAscent = reader.ReadUInt16();
        usWinDescent = reader.ReadUInt16();
    }

    private void ReadVersion1(BinaryReader reader)
    {
        //https://www.microsoft.com/typography/otspec/os2ver1.htm

        //SHORT 	xAvgCharWidth 	 
        //USHORT 	usWeightClass 	 
        //USHORT 	usWidthClass 	 
        //USHORT 	fsType 	 
        xAvgCharWidth = reader.ReadInt16();
        usWeightClass = reader.ReadUInt16();
        usWidthClass = reader.ReadUInt16();
        fsType = reader.ReadUInt16();
        //SHORT 	ySubscriptXSize 	 
        //SHORT 	ySubscriptYSize 	 
        //SHORT 	ySubscriptXOffset 	 
        //SHORT 	ySubscriptYOffset 	 
        //SHORT 	ySuperscriptXSize 	 
        //SHORT 	ySuperscriptYSize 	 
        //SHORT 	ySuperscriptXOffset 	 
        //SHORT 	ySuperscriptYOffset 	 
        //SHORT 	yStrikeoutSize 	 
        //SHORT 	yStrikeoutPosition 	 
        //SHORT 	sFamilyClass 	 
        ySubscriptXSize = reader.ReadInt16();
        ySubscriptYSize = reader.ReadInt16();
        ySubscriptXOffset = reader.ReadInt16();
        ySubscriptYOffset = reader.ReadInt16();
        ySuperscriptXSize = reader.ReadInt16();
        ySuperscriptYSize = reader.ReadInt16();
        ySuperscriptXOffset = reader.ReadInt16();
        ySuperscriptYOffset = reader.ReadInt16();
        yStrikeoutSize = reader.ReadInt16();
        yStrikeoutPosition = reader.ReadInt16();
        sFamilyClass = reader.ReadInt16();

        //BYTE 	panose[10] 	 
        panose = reader.ReadBytes(10);
        //ULONG 	ulUnicodeRange1 	Bits 0-31
        //ULONG 	ulUnicodeRange2 	Bits 32-63
        //ULONG 	ulUnicodeRange3 	Bits 64-95
        //ULONG 	ulUnicodeRange4 	Bits 96-127
        ulUnicodeRange1 = reader.ReadUInt32();
        ulUnicodeRange2 = reader.ReadUInt32();
        ulUnicodeRange3 = reader.ReadUInt32();
        ulUnicodeRange4 = reader.ReadUInt32();
        //CHAR 	achVendID[4] 	 
        achVendID = reader.ReadUInt32();
        //USHORT 	fsSelection 	 
        //USHORT 	usFirstCharIndex 	 
        //USHORT 	usLastCharIndex 	
        fsSelection = reader.ReadUInt16();
        usFirstCharIndex = reader.ReadUInt16();
        usLastCharIndex = reader.ReadUInt16();
        //SHORT 	sTypoAscender 	 
        //SHORT 	sTypoDescender 	 
        //SHORT 	sTypoLineGap 	 
        sTypoAscender = reader.ReadInt16();
        sTypoDescender = reader.ReadInt16();
        sTypoLineGap = reader.ReadInt16();
        //USHORT 	usWinAscent 	 
        //USHORT 	usWinDescent 	 
        //ULONG 	ulCodePageRange1 	Bits 0-31
        //ULONG 	ulCodePageRange2 	Bits 32-63
        usWinAscent = reader.ReadUInt16();
        usWinDescent = reader.ReadUInt16();
        ulCodePageRange1 = reader.ReadUInt32();
        ulCodePageRange2 = reader.ReadUInt32();
    }

    private void ReadVersion2(BinaryReader reader)
    {
        //https://www.microsoft.com/typography/otspec/os2ver2.htm

        // 
        //SHORT 	xAvgCharWidth 	 
        //USHORT 	usWeightClass 	 
        //USHORT 	usWidthClass 	 
        //USHORT 	fsType 	 
        xAvgCharWidth = reader.ReadInt16();
        usWeightClass = reader.ReadUInt16();
        usWidthClass = reader.ReadUInt16();
        fsType = reader.ReadUInt16();
        //SHORT 	ySubscriptXSize 	 
        //SHORT 	ySubscriptYSize 	 
        //SHORT 	ySubscriptXOffset 	 
        //SHORT 	ySubscriptYOffset 	 
        //SHORT 	ySuperscriptXSize 	 
        //SHORT 	ySuperscriptYSize 	 
        //SHORT 	ySuperscriptXOffset 	 
        //SHORT 	ySuperscriptYOffset 	 
        //SHORT 	yStrikeoutSize 	 
        //SHORT 	yStrikeoutPosition 	 
        //SHORT 	sFamilyClass 	 
        ySubscriptXSize = reader.ReadInt16();
        ySubscriptYSize = reader.ReadInt16();
        ySubscriptXOffset = reader.ReadInt16();
        ySubscriptYOffset = reader.ReadInt16();
        ySuperscriptXSize = reader.ReadInt16();
        ySuperscriptYSize = reader.ReadInt16();
        ySuperscriptXOffset = reader.ReadInt16();
        ySuperscriptYOffset = reader.ReadInt16();
        yStrikeoutSize = reader.ReadInt16();
        yStrikeoutPosition = reader.ReadInt16();
        sFamilyClass = reader.ReadInt16();
        //BYTE 	panose[10] 	 
        panose = reader.ReadBytes(10);
        //ULONG 	ulUnicodeRange1 	Bits 0-31
        //ULONG 	ulUnicodeRange2 	Bits 32-63
        //ULONG 	ulUnicodeRange3 	Bits 64-95
        //ULONG 	ulUnicodeRange4 	Bits 96-127
        ulUnicodeRange1 = reader.ReadUInt32();
        ulUnicodeRange2 = reader.ReadUInt32();
        ulUnicodeRange3 = reader.ReadUInt32();
        ulUnicodeRange4 = reader.ReadUInt32();
        //CHAR 	achVendID[4] 	 
        achVendID = reader.ReadUInt32();
        //USHORT 	fsSelection 	 
        //USHORT 	usFirstCharIndex 	 
        //USHORT 	usLastCharIndex 
        fsSelection = reader.ReadUInt16();
        usFirstCharIndex = reader.ReadUInt16();
        usLastCharIndex = reader.ReadUInt16();
        //SHORT 	sTypoAscender 	 
        //SHORT 	sTypoDescender 	 
        //SHORT 	sTypoLineGap 	 
        sTypoAscender = reader.ReadInt16();
        sTypoDescender = reader.ReadInt16();
        sTypoLineGap = reader.ReadInt16();
        //USHORT 	usWinAscent 	 
        //USHORT 	usWinDescent 	 
        //ULONG 	ulCodePageRange1 	Bits 0-31
        //ULONG 	ulCodePageRange2 	Bits 32-63
        usWinAscent = reader.ReadUInt16();
        usWinDescent = reader.ReadUInt16();
        ulCodePageRange1 = reader.ReadUInt32();
        ulCodePageRange2 = reader.ReadUInt32();
        //SHORT 	sxHeight 	 
        //SHORT 	sCapHeight 	 
        //USHORT 	usDefaultChar 	 
        //USHORT 	usBreakChar 	 
        //USHORT 	usMaxContext
        sxHeight = reader.ReadInt16();
        sCapHeight = reader.ReadInt16();
        usDefaultChar = reader.ReadUInt16();
        usBreakChar = reader.ReadUInt16();
        usMaxContext = reader.ReadUInt16();
    }

    private void ReadVersion3(BinaryReader reader)
    {
        //https://www.microsoft.com/typography/otspec/os2ver3.htm
        //            USHORT 	version 	0x0003
        //SHORT 	xAvgCharWidth 	 
        //USHORT 	usWeightClass 	 
        //USHORT 	usWidthClass 	 
        //USHORT 	fsType 	 
        xAvgCharWidth = reader.ReadInt16();
        usWeightClass = reader.ReadUInt16();
        usWidthClass = reader.ReadUInt16();
        fsType = reader.ReadUInt16();
        //SHORT 	ySubscriptXSize 	 
        //SHORT 	ySubscriptYSize 	 
        //SHORT 	ySubscriptXOffset 	 
        //SHORT 	ySubscriptYOffset 	 
        //SHORT 	ySuperscriptXSize 	 
        //SHORT 	ySuperscriptYSize 	 
        //SHORT 	ySuperscriptXOffset 	 
        //SHORT 	ySuperscriptYOffset 	 
        //SHORT 	yStrikeoutSize 	 
        //SHORT 	yStrikeoutPosition 	 
        //SHORT 	sFamilyClass 	 
        ySubscriptXSize = reader.ReadInt16();
        ySubscriptYSize = reader.ReadInt16();
        ySubscriptXOffset = reader.ReadInt16();
        ySubscriptYOffset = reader.ReadInt16();
        ySuperscriptXSize = reader.ReadInt16();
        ySuperscriptYSize = reader.ReadInt16();
        ySuperscriptXOffset = reader.ReadInt16();
        ySuperscriptYOffset = reader.ReadInt16();
        yStrikeoutSize = reader.ReadInt16();
        yStrikeoutPosition = reader.ReadInt16();
        sFamilyClass = reader.ReadInt16();
        //BYTE 	panose[10] 	 
        panose = reader.ReadBytes(10);
        //ULONG 	ulUnicodeRange1 	Bits 0-31
        //ULONG 	ulUnicodeRange2 	Bits 32-63
        //ULONG 	ulUnicodeRange3 	Bits 64-95
        //ULONG 	ulUnicodeRange4 	Bits 96-127
        ulUnicodeRange1 = reader.ReadUInt32();
        ulUnicodeRange2 = reader.ReadUInt32();
        ulUnicodeRange3 = reader.ReadUInt32();
        ulUnicodeRange4 = reader.ReadUInt32();
        //CHAR 	achVendID[4] 	 
        achVendID = reader.ReadUInt32();
        //USHORT 	fsSelection 	 
        //USHORT 	usFirstCharIndex 	 
        //USHORT 	usLastCharIndex 	 
        fsSelection = reader.ReadUInt16();
        usFirstCharIndex = reader.ReadUInt16();
        usLastCharIndex = reader.ReadUInt16();
        //SHORT 	sTypoAscender 	 
        //SHORT 	sTypoDescender 	 
        //SHORT 	sTypoLineGap 
        sTypoAscender = reader.ReadInt16();
        sTypoDescender = reader.ReadInt16();
        sTypoLineGap = reader.ReadInt16();
        //USHORT 	usWinAscent 	 
        //USHORT 	usWinDescent 	 
        //ULONG 	ulCodePageRange1 	Bits 0-31
        //ULONG 	ulCodePageRange2 	Bits 32-63
        usWinAscent = reader.ReadUInt16();
        usWinDescent = reader.ReadUInt16();
        ulCodePageRange1 = reader.ReadUInt32();
        ulCodePageRange2 = reader.ReadUInt32();
        //SHORT 	sxHeight 	 
        //SHORT 	sCapHeight 	 
        //USHORT 	usDefaultChar 	 
        //USHORT 	usBreakChar 	 
        //USHORT 	usMaxContext
        sxHeight = reader.ReadInt16();
        sCapHeight = reader.ReadInt16();
        usDefaultChar = reader.ReadUInt16();
        usBreakChar = reader.ReadUInt16();
        usMaxContext = reader.ReadUInt16();
    }

    private void ReadVersion4(BinaryReader reader)
    {
        //https://www.microsoft.com/typography/otspec/os2ver4.htm

        //SHORT 	xAvgCharWidth 	 
        //USHORT 	usWeightClass 	 
        //USHORT 	usWidthClass 	 
        //USHORT 	fsType 	 
        xAvgCharWidth = reader.ReadInt16();
        usWeightClass = reader.ReadUInt16();
        usWidthClass = reader.ReadUInt16();
        fsType = reader.ReadUInt16();
        //SHORT 	ySubscriptXSize 	 
        //SHORT 	ySubscriptYSize 	 
        //SHORT 	ySubscriptXOffset 	 
        //SHORT 	ySubscriptYOffset 	 
        //SHORT 	ySuperscriptXSize 	 
        //SHORT 	ySuperscriptYSize 	 
        //SHORT 	ySuperscriptXOffset 	 
        //SHORT 	ySuperscriptYOffset 	 
        //SHORT 	yStrikeoutSize 	 
        //SHORT 	yStrikeoutPosition 	 
        //SHORT 	sFamilyClass 	 
        ySubscriptXSize = reader.ReadInt16();
        ySubscriptYSize = reader.ReadInt16();
        ySubscriptXOffset = reader.ReadInt16();
        ySubscriptYOffset = reader.ReadInt16();
        ySuperscriptXSize = reader.ReadInt16();
        ySuperscriptYSize = reader.ReadInt16();
        ySuperscriptXOffset = reader.ReadInt16();
        ySuperscriptYOffset = reader.ReadInt16();
        yStrikeoutSize = reader.ReadInt16();
        yStrikeoutPosition = reader.ReadInt16();
        sFamilyClass = reader.ReadInt16();
        //BYTE 	panose[10] 	 
        panose = reader.ReadBytes(10);
        //ULONG 	ulUnicodeRange1 	Bits 0-31
        //ULONG 	ulUnicodeRange2 	Bits 32-63
        //ULONG 	ulUnicodeRange3 	Bits 64-95
        //ULONG 	ulUnicodeRange4 	Bits 96-127
        ulUnicodeRange1 = reader.ReadUInt32();
        ulUnicodeRange2 = reader.ReadUInt32();
        ulUnicodeRange3 = reader.ReadUInt32();
        ulUnicodeRange4 = reader.ReadUInt32();
        //CHAR 	achVendID[4] 	 
        achVendID = reader.ReadUInt32();
        //USHORT 	fsSelection 	 
        //USHORT 	usFirstCharIndex 	 
        //USHORT 	usLastCharIndex 	 
        fsSelection = reader.ReadUInt16();
        usFirstCharIndex = reader.ReadUInt16();
        usLastCharIndex = reader.ReadUInt16();
        //SHORT 	sTypoAscender 	 
        //SHORT 	sTypoDescender 	 
        //SHORT 	sTypoLineGap 	 
        sTypoAscender = reader.ReadInt16();
        sTypoDescender = reader.ReadInt16();
        sTypoLineGap = reader.ReadInt16();
        //USHORT 	usWinAscent 	 
        //USHORT 	usWinDescent 	 
        //ULONG 	ulCodePageRange1 	Bits 0-31
        //ULONG 	ulCodePageRange2 	Bits 32-63
        usWinAscent = reader.ReadUInt16();
        usWinDescent = reader.ReadUInt16();
        ulCodePageRange1 = reader.ReadUInt32();
        ulCodePageRange2 = reader.ReadUInt32();
        //SHORT 	sxHeight 	 
        //SHORT 	sCapHeight 	 
        //USHORT 	usDefaultChar 	 
        //USHORT 	usBreakChar 	 
        //USHORT 	usMaxContext
        sxHeight = reader.ReadInt16();
        sCapHeight = reader.ReadInt16();
        usDefaultChar = reader.ReadUInt16();
        usBreakChar = reader.ReadUInt16();
        usMaxContext = reader.ReadUInt16();
    }

    private void ReadVersion5(BinaryReader reader)
    {
        xAvgCharWidth = reader.ReadInt16();
        usWeightClass = reader.ReadUInt16();
        usWidthClass = reader.ReadUInt16();
        fsType = reader.ReadUInt16();
        //SHORT 	ySubscriptXSize 	 
        //SHORT 	ySubscriptYSize 	 
        //SHORT 	ySubscriptXOffset 	 
        //SHORT 	ySubscriptYOffset 	 
        //SHORT 	ySuperscriptXSize 	 
        //SHORT 	ySuperscriptYSize 	 
        //SHORT 	ySuperscriptXOffset 	 
        //SHORT 	ySuperscriptYOffset 	 
        //SHORT 	yStrikeoutSize 	 
        //SHORT 	yStrikeoutPosition 	 
        //SHORT 	sFamilyClass 	 
        ySubscriptXSize = reader.ReadInt16();
        ySubscriptYSize = reader.ReadInt16();
        ySubscriptXOffset = reader.ReadInt16();
        ySubscriptYOffset = reader.ReadInt16();
        ySuperscriptXSize = reader.ReadInt16();
        ySuperscriptYSize = reader.ReadInt16();
        ySuperscriptXOffset = reader.ReadInt16();
        ySuperscriptYOffset = reader.ReadInt16();
        yStrikeoutSize = reader.ReadInt16();
        yStrikeoutPosition = reader.ReadInt16();
        sFamilyClass = reader.ReadInt16();

        //BYTE 	panose[10] 	 
        panose = reader.ReadBytes(10);
        //ULONG 	ulUnicodeRange1 	Bits 0-31
        //ULONG 	ulUnicodeRange2 	Bits 32-63
        //ULONG 	ulUnicodeRange3 	Bits 64-95
        //ULONG 	ulUnicodeRange4 	Bits 96-127
        ulUnicodeRange1 = reader.ReadUInt32();
        ulUnicodeRange2 = reader.ReadUInt32();
        ulUnicodeRange3 = reader.ReadUInt32();
        ulUnicodeRange4 = reader.ReadUInt32();

        //CHAR 	achVendID[4] 	 
        achVendID = reader.ReadUInt32();
        //USHORT 	fsSelection 	 
        //USHORT 	usFirstCharIndex 	 
        //USHORT 	usLastCharIndex 
        fsSelection = reader.ReadUInt16();
        usFirstCharIndex = reader.ReadUInt16();
        usLastCharIndex = reader.ReadUInt16();
        //SHORT 	sTypoAscender 	 
        //SHORT 	sTypoDescender 	 
        //SHORT 	sTypoLineGap 	 
        sTypoAscender = reader.ReadInt16();
        sTypoDescender = reader.ReadInt16();
        sTypoLineGap = reader.ReadInt16();
        //USHORT 	usWinAscent 	 
        //USHORT 	usWinDescent 	 
        //ULONG 	ulCodePageRange1 	Bits 0-31
        //ULONG 	ulCodePageRange2 	Bits 32-63
        usWinAscent = reader.ReadUInt16();
        usWinDescent = reader.ReadUInt16();
        ulCodePageRange1 = reader.ReadUInt32();
        ulCodePageRange2 = reader.ReadUInt32();
        //SHORT 	sxHeight 	 
        //SHORT 	sCapHeight 	 
        //USHORT 	usDefaultChar 	 
        //USHORT 	usBreakChar 	 
        //USHORT 	usMaxContext 	 
        sxHeight = reader.ReadInt16();
        sCapHeight = reader.ReadInt16();
        usDefaultChar = reader.ReadUInt16();
        usBreakChar = reader.ReadUInt16();
        usMaxContext = reader.ReadUInt16();
        //USHORT 	usLowerOpticalPointSize 	 
        //USHORT 	usUpperOpticalPointSize 	 

        usLowerOpticalPointSize = reader.ReadUInt16();
        usUpperOpticalPointSize = reader.ReadUInt16();
    }
}