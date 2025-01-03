//Apache2, 2016-present, WinterDev, Sam Hocevar <sam@hocevar.net>

using System;
using System.Collections.Generic;
using System.IO;

namespace Typography.OpenFont.Tables;
//https://docs.microsoft.com/en-us/typography/opentype/spec/gpos

public partial class GPOS : GlyphShapingTableEntry
{
    public const string _N = "GPOS";

    private readonly List<LookupTable> _lookupList = new();
    public override string Name => _N;

    /// <summary>
    ///     heuristic lookback optimization,
    ///     some layout-context may need=> eg. **Emoji**, some complex script
    ///     some layout-context may not need.
    /// </summary>
    public bool EnableLongLookBack { get; set; }

    public IList<LookupTable> LookupList => _lookupList;

    protected override void ReadLookupTable(BinaryReader reader, long lookupTablePos,
        ushort lookupType, ushort lookupFlags,
        ushort[] subTableOffsets, ushort markFilteringSet)
    {
        var lookupTable = new LookupTable(lookupFlags, markFilteringSet);
        var subTables = new LookupSubTable[subTableOffsets.Length];
        lookupTable.SubTables = subTables;

        for (var i = 0; i < subTableOffsets.Length; ++i)
        {
            var subTable = LookupTable.ReadSubTable(lookupType, reader, lookupTablePos + subTableOffsets[i]);
            subTable.OwnerGPos = this;
            subTables[i] = subTable;


            if (lookupType == 9)
                //temp fix 
                // (eg. Emoji) => enable long look back
                EnableLongLookBack = true;
        }


#if DEBUG
        lookupTable.dbugLkIndex = LookupList.Count;
#endif

        LookupList.Add(lookupTable);
    }

    protected override void ReadFeatureVariations(BinaryReader reader, long featureVariationsBeginAt)
    {
        Utils.WarnUnimplemented("GPOS feature variations");
    }

    public abstract class LookupSubTable
    {
        public GPOS OwnerGPos;
        public abstract void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len);
    }

    /// <summary>
    ///     Subtable for unhandled/unimplemented features
    /// </summary>
    public class UnImplementedLookupSubTable : LookupSubTable
    {
        private readonly string _msg;

        public UnImplementedLookupSubTable(string message)
        {
            _msg = message;
            Utils.WarnUnimplemented(message);
        }

        public override string ToString()
        {
            return _msg;
        }

        public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
        {
        }
    }

    /// <summary>
    ///     sub table of a lookup list
    /// </summary>
    public class LookupTable
    {
        public readonly ushort lookupFlags;

        public readonly ushort markFilteringSet;

        //--------------------------
#if DEBUG
        public int dbugLkIndex;
#endif
        public LookupTable(ushort lookupFlags, ushort markFilteringSet)
        {
            this.lookupFlags = lookupFlags;
            this.markFilteringSet = markFilteringSet;
        }

        public LookupSubTable[] SubTables { get; internal set; }

        public void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
        {
            foreach (var subTable in SubTables)
            {
                subTable.DoGlyphPosition(inputGlyphs, startAt, len);
                //update len
                len = inputGlyphs.Count;
            }
        }

        public static LookupSubTable ReadSubTable(int lookupType, BinaryReader reader, long subTableStartAt)
        {
            switch (lookupType)
            {
                case 1: return ReadLookupType1(reader, subTableStartAt);
                case 2: return ReadLookupType2(reader, subTableStartAt);
                case 3: return ReadLookupType3(reader, subTableStartAt);
                case 4: return ReadLookupType4(reader, subTableStartAt);
                case 5: return ReadLookupType5(reader, subTableStartAt);
                case 6: return ReadLookupType6(reader, subTableStartAt);
                case 7: return ReadLookupType7(reader, subTableStartAt);
                case 8: return ReadLookupType8(reader, subTableStartAt);
                case 9: return ReadLookupType9(reader, subTableStartAt);
            }

            return new UnImplementedLookupSubTable(string.Format("GPOS Lookup Type {0}", lookupType));
        }

        private static int FindGlyphBackwardByKind(IGlyphPositions inputGlyphs, GlyphClassKind kind, int pos, int lim)
        {
            for (var i = pos; --i >= lim;)
                if (inputGlyphs.GetGlyphClassKind(i) == kind)
                    return i;

            return -1;
        }

        /// <summary>
        ///     Lookup Type 1: Single Adjustment Positioning Subtable
        /// </summary>
        /// <param name="reader"></param>
        private static LookupSubTable ReadLookupType1(BinaryReader reader, long subTableStartAt)
        {
            // Single Adjustment Positioning: Format 1
            // Value         Type          Description
            // uint16        PosFormat     Format identifier-format = 1
            // Offset16      Coverage      Offset to Coverage table-from beginning of SinglePos subtable
            // uint16        ValueFormat   Defines the types of data in the ValueRecord
            // ValueRecord   Value         Defines positioning value(s)-applied to all glyphs in the Coverage table

            // Single Adjustment Positioning: Format 2
            // Value         Type                Description
            // USHORT        PosFormat           Format identifier-format = 2
            // Offset16      Coverage            Offset to Coverage table-from beginning of SinglePos subtable
            // uint16        ValueFormat         Defines the types of data in the ValueRecord
            // uint16        ValueCount          Number of ValueRecords
            // ValueRecord   Value[ValueCount]   Array of ValueRecords-positioning values applied to glyphs

            reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);

            var format = reader.ReadUInt16();
            var coverage = reader.ReadUInt16();
            var valueFormat = reader.ReadUInt16();
            switch (format)
            {
                default: throw new NotSupportedException();
                case 1:
                {
                    var valueRecord = ValueRecord.CreateFrom(reader, valueFormat);
                    var coverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverage);
                    return new LkSubTableType1(coverageTable, valueRecord);
                }
                case 2:
                {
                    var valueCount = reader.ReadUInt16();
                    var valueRecords = new ValueRecord[valueCount];
                    for (var n = 0; n < valueCount; ++n) valueRecords[n] = ValueRecord.CreateFrom(reader, valueFormat);
                    var coverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverage);
                    return new LkSubTableType1(coverageTable, valueRecords);
                }
            }
        }

        /// <summary>
        ///     Lookup Type 2: Pair Adjustment Positioning Subtable
        /// </summary>
        /// <param name="reader"></param>
        private static LookupSubTable ReadLookupType2(BinaryReader reader, long subTableStartAt)
        {
            //A pair adjustment positioning subtable(PairPos) is used to adjust the positions of two glyphs
            //in relation to one another-for instance, 
            //to specify kerning data for pairs of glyphs.
            //
            //Compared to a typical kerning table, however, a PairPos subtable offers more flexiblity and 
            //precise control over glyph positioning.

            //The PairPos subtable can adjust each glyph in a pair independently in both the X and Y directions, 
            //and it can explicitly describe the particular type of adjustment applied to each glyph.
            //
            //PairPos subtables can be either of two formats: 
            //1) one that identifies glyphs individually by index(Format 1),
            //or 2) one that identifies glyphs by class (Format 2).
            //-----------------------------------------------
            //FORMAT1:
            //Format 1 uses glyph indices to access positioning data for one or more specific pairs of glyphs
            //All pairs are specified in the order determined by the layout direction of the text.
            //
            //Note: For text written from right to left, the right - most glyph will be the first glyph in a pair;
            //conversely, for text written from left to right, the left - most glyph will be first.
            //
            //A PairPosFormat1 subtable contains a format identifier(PosFormat) and two ValueFormats:
            //ValueFormat1 applies to the ValueRecord of the first glyph in each pair.
            //ValueRecords for all first glyphs must use ValueFormat1.
            //If ValueFormat1 is set to zero(0), 
            //the corresponding glyph has no ValueRecord and, therefore, should not be repositioned.
            //
            //ValueFormat2 applies to the ValueRecord of the second glyph in each pair.
            //ValueRecords for all second glyphs must use ValueFormat2.
            //If ValueFormat2 is set to null, then the second glyph of the pair is the “next” glyph for which a lookup should be performed.
            //
            //A PairPos subtable also defines an offset to a Coverage table(Coverage) that lists the indices of the first glyphs in each pair.
            //More than one pair can have the same first glyph, but the Coverage table will list that glyph only once.
            //
            //The subtable also contains an array of offsets to PairSet tables(PairSet) and a count of the defined tables(PairSetCount).
            //The PairSet array contains one offset for each glyph listed in the Coverage table and uses the same order as the Coverage Index.

            //-----------------
            //PairPosFormat1 subtable: Adjustments for glyph pairs
            //uint16 	PosFormat 	    Format identifier-format = 1
            //Offset16 	Coverage 	    Offset to Coverage table-from beginning of PairPos subtable-only the first glyph in each pair
            //uint16 	ValueFormat1 	Defines the types of data in ValueRecord1-for the first glyph in the pair -may be zero (0)
            //uint16 	ValueFormat2 	Defines the types of data in ValueRecord2-for the second glyph in the pair -may be zero (0)
            //uint16 	PairSetCount 	Number of PairSet tables
            //Offset16 	PairSetOffset[PairSetCount] Array of offsets to PairSet tables-from beginning of PairPos subtable-ordered by Coverage Index                // 	
            //-----------------
            //
            //PairSet table
            //Value 	Type 	            Description
            //uint16 	PairValueCount 	    Number of PairValueRecords
            //struct 	PairValueRecord[PairValueCount] 	Array of PairValueRecords-ordered by GlyphID of the second glyph
            //-----------------
            //A PairValueRecord specifies the second glyph in a pair (SecondGlyph) and defines a ValueRecord for each glyph (Value1 and Value2). 
            //If ValueFormat1 is set to zero (0) in the PairPos subtable, ValueRecord1 will be empty; similarly, if ValueFormat2 is 0, Value2 will be empty.


            //PairValueRecord
            //Value 	    Type 	        Description
            //GlyphID 	    SecondGlyph 	GlyphID of second glyph in the pair-first glyph is listed in the Coverage table
            //ValueRecord 	Value1 	        Positioning data for the first glyph in the pair
            //ValueRecord 	Value2 	        Positioning data for the second glyph in the pair
            //-----------------------------------------------

            //PairPosFormat2 subtable: Class pair adjustment
            //Value 	Type 	            Description
            //uint16 	PosFormat 	        Format identifier-format = 2
            //Offset16 	Coverage 	        Offset to Coverage table-from beginning of PairPos subtable-for the first glyph of the pair
            //uint16 	ValueFormat1 	    ValueRecord definition-for the first glyph of the pair-may be zero (0)
            //uint16 	ValueFormat2 	    ValueRecord definition-for the second glyph of the pair-may be zero (0)
            //Offset16 	ClassDef1 	        Offset to ClassDef table-from beginning of PairPos subtable-for the first glyph of the pair
            //Offset16 	ClassDef2 	        Offset to ClassDef table-from beginning of PairPos subtable-for the second glyph of the pair
            //uint16 	Class1Count 	    Number of classes in ClassDef1 table-includes Class0
            //uint16 	Class2Count 	    Number of classes in ClassDef2 table-includes Class0
            //struct 	Class1Record[Class1Count] 	Array of Class1 records-ordered by Class1

            //Each Class1Record contains an array of Class2Records (Class2Record), which also are ordered by class value. 
            //One Class2Record must be declared for each class in the ClassDef2 table, including Class 0.
            //--------------------------------
            //Class1Record
            //Value 	Type 	Description
            //struct 	Class2Record[Class2Count] 	Array of Class2 records-ordered by Class2
            //--------------------------------

            //A Class2Record consists of two ValueRecords,
            //one for the first glyph in a class pair (Value1) and one for the second glyph (Value2).
            //If the PairPos subtable has a value of zero (0) for ValueFormat1 or ValueFormat2, 
            //the corresponding record (ValueRecord1 or ValueRecord2) will be empty.


            //Class2Record
            //--------------------------------
            //Value 	    Type 	Description
            //ValueRecord 	Value1 	Positioning for first glyph-empty if ValueFormat1 = 0
            //ValueRecord 	Value2 	Positioning for second glyph-empty if ValueFormat2 = 0
            //--------------------------------

            reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);

            var format = reader.ReadUInt16();
            switch (format)
            {
                default:
                    return new UnImplementedLookupSubTable(string.Format("GPOS Lookup Table Type 2 Format {0}",
                        format));
                case 1:
                {
                    var coverage = reader.ReadUInt16();
                    var value1Format = reader.ReadUInt16();
                    var value2Format = reader.ReadUInt16();
                    var pairSetCount = reader.ReadUInt16();
                    var pairSetOffsetArray = reader.ReadUInt16Array(pairSetCount);
                    var pairSetTables = new PairSetTable[pairSetCount];
                    for (var n = 0; n < pairSetCount; ++n)
                    {
                        reader.BaseStream.Seek(subTableStartAt + pairSetOffsetArray[n], SeekOrigin.Begin);
                        var pairSetTable = new PairSetTable();
                        pairSetTable.ReadFrom(reader, value1Format, value2Format);
                        pairSetTables[n] = pairSetTable;
                    }

                    var subTable = new LkSubTableType2Fmt1(pairSetTables);
                    //coverage
                    subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverage);
                    return subTable;
                }
                case 2:
                {
                    var coverage = reader.ReadUInt16();
                    var value1Format = reader.ReadUInt16();
                    var value2Format = reader.ReadUInt16();
                    var classDef1_offset = reader.ReadUInt16();
                    var classDef2_offset = reader.ReadUInt16();
                    var class1Count = reader.ReadUInt16();
                    var class2Count = reader.ReadUInt16();

                    var class1Records = new Lk2Class1Record[class1Count];
                    for (var c1 = 0; c1 < class1Count; ++c1)
                    {
                        //for each c1 record

                        var class2Records = new Lk2Class2Record[class2Count];
                        for (var c2 = 0; c2 < class2Count; ++c2)
                            class2Records[c2] = new Lk2Class2Record(
                                ValueRecord.CreateFrom(reader, value1Format),
                                ValueRecord.CreateFrom(reader, value2Format));
                        class1Records[c1] = new Lk2Class1Record(class2Records);
                    }

                    var subTable = new LkSubTableType2Fmt2(class1Records,
                        ClassDefTable.CreateFrom(reader, subTableStartAt + classDef1_offset),
                        ClassDefTable.CreateFrom(reader, subTableStartAt + classDef2_offset));


                    subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverage);
                    return subTable;
                }
            }
        }

        /// <summary>
        ///     Lookup Type 3: Cursive Attachment Positioning Subtable
        /// </summary>
        /// <param name="reader"></param>
        private static LookupSubTable ReadLookupType3(BinaryReader reader, long subTableStartAt)
        {
            // TODO: implement this

            return new UnImplementedLookupSubTable("GPOS Lookup Table Type 3");
        }

        /// <summary>
        ///     Lookup Type 4: MarkToBase Attachment Positioning Subtable
        /// </summary>
        /// <param name="reader"></param>
        private static LookupSubTable ReadLookupType4(BinaryReader reader, long subTableStartAt)
        {
            //The MarkToBase attachment (MarkBasePos) subtable is used to position combining mark glyphs with respect to base glyphs. 
            //For example, the Arabic, Hebrew, and Thai scripts combine vowels, diacritical marks, and tone marks with base glyphs.

            //In the MarkBasePos subtable, every mark glyph has an anchor point and is associated with a class of marks. 
            //Each base glyph then defines an anchor point for each class of marks it uses.

            //For example, assume two mark classes: all marks positioned above base glyphs (Class 0),
            //and all marks positioned below base glyphs (Class 1). 
            //In this case, each base glyph that uses these marks would define two anchor points, 
            //one for attaching the mark glyphs listed in Class 0,
            //and one for attaching the mark glyphs listed in Class 1.

            //To identify the base glyph that combines with a mark,
            //the text-processing client must look backward in the glyph string from the mark to the preceding base glyph.
            //To combine the mark and base glyph, the client aligns their attachment points,
            //positioning the mark with respect to the final pen point (advance) position of the base glyph.

            //The MarkToBase Attachment subtable has one format: MarkBasePosFormat1. 
            //The subtable begins with a format identifier (PosFormat) and
            //offsets to two Coverage tables: one that lists all the mark glyphs referenced in the subtable (MarkCoverage), 
            //and one that lists all the base glyphs referenced in the subtable (BaseCoverage).

            //For each mark glyph in the MarkCoverage table,
            //a record specifies its class and an offset to the Anchor table that describes the mark's attachment point (MarkRecord).
            //A mark class is identified by a specific integer, called a class value.
            //ClassCount specifies the total number of distinct mark classes defined in all the MarkRecords.

            //The MarkBasePosFormat1 subtable also contains an offset to a MarkArray table, 
            //which contains all the MarkRecords stored in an array (MarkRecord) by MarkCoverage Index. 
            //A MarkArray table also contains a count of the defined MarkRecords (MarkCount). 
            //(For details about MarkArrays and MarkRecords, see the end of this chapter.)

            //The MarkBasePosFormat1 subtable also contains an offset to a BaseArray table (BaseArray).

            //MarkBasePosFormat1 subtable: MarkToBase attachment point
            //----------------------------------------------
            //Value 	Type 	        Description
            //uint16 	PosFormat 	    Format identifier-format = 1
            //Offset16 	MarkCoverage 	Offset to MarkCoverage table-from beginning of MarkBasePos subtable ( all the mark glyphs referenced in the subtable)
            //Offset16 	BaseCoverage 	Offset to BaseCoverage table-from beginning of MarkBasePos subtable (all the base glyphs referenced in the subtable)
            //uint16 	ClassCount 	    Number of classes defined for marks
            //Offset16 	MarkArray 	    Offset to MarkArray table-from beginning of MarkBasePos subtable
            //Offset16 	BaseArray 	    Offset to BaseArray table-from beginning of MarkBasePos subtable
            //----------------------------------------------

            //The BaseArray table consists of an array (BaseRecord) and count (BaseCount) of BaseRecords. 
            //The array stores the BaseRecords in the same order as the BaseCoverage Index. 
            //Each base glyph in the BaseCoverage table has a BaseRecord.

            //BaseArray table
            //Value 	Type 	Description
            //uint16 	BaseCount 	Number of BaseRecords
            //struct 	BaseRecord[BaseCount] 	Array of BaseRecords-in order of BaseCoverage Index

            reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);

            var format = reader.ReadUInt16();
            if (format != 1)
                return new UnImplementedLookupSubTable(string.Format("GPOS Lookup Sub Table Type 4 Format {0}",
                    format));
            var markCoverageOffset = reader.ReadUInt16(); //offset from
            var baseCoverageOffset = reader.ReadUInt16();
            var markClassCount = reader.ReadUInt16();
            var markArrayOffset = reader.ReadUInt16();
            var baseArrayOffset = reader.ReadUInt16();

            //read mark array table
            var lookupType4 = new LkSubTableType4();
            lookupType4.MarkCoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + markCoverageOffset);
            lookupType4.BaseCoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + baseCoverageOffset);
            lookupType4.MarkArrayTable = MarkArrayTable.CreateFrom(reader, subTableStartAt + markArrayOffset);
            lookupType4.BaseArrayTable =
                BaseArrayTable.CreateFrom(reader, subTableStartAt + baseArrayOffset, markClassCount);
#if DEBUG
            //lookupType4.dbugTest();
#endif
            return lookupType4;
        }

        /// <summary>
        ///     Lookup Type 5: MarkToLigature Attachment Positioning Subtable
        /// </summary>
        /// <param name="reader"></param>
        private static LookupSubTable ReadLookupType5(BinaryReader reader, long subTableStartAt)
        {
            //uint16 	PosFormat 	        Format identifier-format = 1
            //Offset16 	MarkCoverage 	    Offset to Mark Coverage table-from beginning of MarkLigPos subtable
            //Offset16 	LigatureCoverage 	Offset to Ligature Coverage table-from beginning of MarkLigPos subtable
            //uint16 	ClassCount 	        Number of defined mark classes
            //Offset16 	MarkArray 	        Offset to MarkArray table-from beginning of MarkLigPos subtable
            //Offset16 	LigatureArray 	    Offset to LigatureArray table-from beginning of MarkLigPos subtable

            reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);

            var format = reader.ReadUInt16();
            if (format != 1)
                return new UnImplementedLookupSubTable(string.Format("GPOS Lookup Sub Table Type 5 Format {0}",
                    format));
            var markCoverageOffset = reader.ReadUInt16(); //from beginning of MarkLigPos subtable
            var ligatureCoverageOffset = reader.ReadUInt16();
            var classCount = reader.ReadUInt16();
            var markArrayOffset = reader.ReadUInt16();
            var ligatureArrayOffset = reader.ReadUInt16();
            //-----------------------
            var subTable = new LkSubTableType5();
            subTable.MarkCoverage = CoverageTable.CreateFrom(reader, subTableStartAt + markCoverageOffset);
            subTable.LigatureCoverage = CoverageTable.CreateFrom(reader, subTableStartAt + ligatureCoverageOffset);
            subTable.MarkArrayTable = MarkArrayTable.CreateFrom(reader, subTableStartAt + markArrayOffset);

            reader.BaseStream.Seek(subTableStartAt + ligatureArrayOffset, SeekOrigin.Begin);
            var ligatureArrayTable = new LigatureArrayTable();
            ligatureArrayTable.ReadFrom(reader, classCount);
            subTable.LigatureArrayTable = ligatureArrayTable;

            return subTable;
        }

        /// <summary>
        ///     Lookup Type 6: MarkToMark Attachment Positioning Subtable
        /// </summary>
        /// <param name="reader"></param>
        private static LookupSubTable ReadLookupType6(BinaryReader reader, long subTableStartAt)
        {
            // uint16     PosFormat      Format identifier-format = 1
            // Offset16   Mark1Coverage  Offset to Combining Mark Coverage table-from beginning of MarkMarkPos subtable
            // Offset16   Mark2Coverage  Offset to Base Mark Coverage table-from beginning of MarkMarkPos subtable
            // uint16     ClassCount     Number of Combining Mark classes defined
            // Offset16   Mark1Array     Offset to MarkArray table for Mark1-from beginning of MarkMarkPos subtable
            // Offset16   Mark2Array     Offset to Mark2Array table for Mark2-from beginning of MarkMarkPos subtable

            reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);

            var format = reader.ReadUInt16();
            if (format != 1)
                return new UnImplementedLookupSubTable(string.Format("GPOS Lookup Sub Table Type 6 Format {0}",
                    format));
            var mark1CoverageOffset = reader.ReadUInt16();
            var mark2CoverageOffset = reader.ReadUInt16();
            var classCount = reader.ReadUInt16();
            var mark1ArrayOffset = reader.ReadUInt16();
            var mark2ArrayOffset = reader.ReadUInt16();
            //
            var subTable = new LkSubTableType6();
            subTable.MarkCoverage1 = CoverageTable.CreateFrom(reader, subTableStartAt + mark1CoverageOffset);
            subTable.MarkCoverage2 = CoverageTable.CreateFrom(reader, subTableStartAt + mark2CoverageOffset);
            subTable.Mark1ArrayTable = MarkArrayTable.CreateFrom(reader, subTableStartAt + mark1ArrayOffset);
            subTable.Mark2ArrayTable =
                Mark2ArrayTable.CreateFrom(reader, subTableStartAt + mark2ArrayOffset, classCount);

            return subTable;
        }

        /// <summary>
        ///     Lookup Type 7: Contextual Positioning Subtables
        /// </summary>
        /// <param name="reader"></param>
        private static LookupSubTable ReadLookupType7(BinaryReader reader, long subTableStartAt)
        {
            reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);

            var format = reader.ReadUInt16();
            switch (format)
            {
                default:
                    return new UnImplementedLookupSubTable(string.Format("GPOS Lookup Sub Table Type 7 Format {0}",
                        format));
                case 1:
                {
                    //Context Positioning Subtable: Format 1
                    //ContextPosFormat1 subtable: Simple context positioning
                    //Value 	Type 	            Description
                    //uint16 	PosFormat 	        Format identifier-format = 1
                    //Offset16 	Coverage 	        Offset to Coverage table-from beginning of ContextPos subtable
                    //uint16 	PosRuleSetCount 	Number of PosRuleSet tables
                    //Offset16 	PosRuleSet[PosRuleSetCount]
                    //
                    var coverageOffset = reader.ReadUInt16();
                    var posRuleSetCount = reader.ReadUInt16();
                    var posRuleSetOffsets = reader.ReadUInt16Array(posRuleSetCount);

                    var subTable = new LkSubTableType7Fmt1();
                    subTable.PosRuleSetTables =
                        CreateMultiplePosRuleSetTables(subTableStartAt, posRuleSetOffsets, reader);
                    subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverageOffset);
                    return subTable;
                }
                case 2:
                {
                    //Context Positioning Subtable: Format 2
                    //uint16 	PosFormat 	        Format identifier-format = 2
                    //Offset16 	Coverage 	        Offset to Coverage table-from beginning of ContextPos subtable
                    //Offset16 	ClassDef 	        Offset to ClassDef table-from beginning of ContextPos subtable
                    //uint16 	PosClassSetCnt      Number of PosClassSet tables
                    //Offset16 	PosClassSet[PosClassSetCnt] 	Array of offsets to PosClassSet tables-from beginning of ContextPos subtable-ordered by class-may be NULL

                    var coverageOffset = reader.ReadUInt16();
                    var classDefOffset = reader.ReadUInt16();
                    var posClassSetCount = reader.ReadUInt16();
                    var posClassSetOffsets = reader.ReadUInt16Array(posClassSetCount);

                    var subTable = new LkSubTableType7Fmt2();
                    subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverageOffset);
                    subTable.ClassDef = ClassDefTable.CreateFrom(reader, subTableStartAt + classDefOffset);

                    var posClassSetTables = new PosClassSetTable[posClassSetCount];
                    subTable.PosClassSetTables = posClassSetTables;
                    for (var n = 0; n < posClassSetCount; ++n)
                    {
                        var offset = posClassSetOffsets[n];
                        if (offset > 0)
                            posClassSetTables[n] = PosClassSetTable.CreateFrom(reader, subTableStartAt + offset);
                    }

                    return subTable;
                }
                case 3:
                {
                    //ContextPosFormat3 subtable: Coverage-based context glyph positioning
                    //Value 	Type 	    Description
                    //uint16 	PosFormat 	Format identifier-format = 3
                    //uint16 	GlyphCount 	Number of glyphs in the input sequence
                    //uint16 	PosCount 	Number of PosLookupRecords
                    //Offset16 	Coverage[GlyphCount] 	Array of offsets to Coverage tables-from beginning of ContextPos subtable
                    //struct 	PosLookupRecord[PosCount] Array of positioning lookups-in design order
                    var subTable = new LkSubTableType7Fmt3();
                    var glyphCount = reader.ReadUInt16();
                    var posCount = reader.ReadUInt16();
                    //read each lookahead record
                    var coverageOffsets = reader.ReadUInt16Array(glyphCount);
                    subTable.PosLookupRecords = CreateMultiplePosLookupRecords(reader, posCount);
                    subTable.CoverageTables =
                        CoverageTable.CreateMultipleCoverageTables(subTableStartAt, coverageOffsets, reader);

                    return subTable;
                }
            }
        }

        /// <summary>
        ///     LookupType 8: Chaining Contextual Positioning Subtable
        /// </summary>
        /// <param name="reader"></param>
        private static LookupSubTable ReadLookupType8(BinaryReader reader, long subTableStartAt)
        {
            reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);

            var format = reader.ReadUInt16();
            switch (format)
            {
                default:
                    return new UnImplementedLookupSubTable(string.Format("GPOS Lookup Table Type 8 Format {0}",
                        format));
                case 1:
                {
                    //Chaining Context Positioning  Format 1: Simple Chaining Context Glyph Positioning
                    //uint16 	PosFormat 	        Format identifier-format = 1
                    //Offset16 	Coverage 	        Offset to Coverage table-from beginning of ContextPos subtable
                    //uint16 	ChainPosRuleSetCount 	Number of ChainPosRuleSet tables
                    //Offset16 	ChainPosRuleSet[ChainPosRuleSetCount] 	Array of offsets to ChainPosRuleSet tables-from beginning of ContextPos subtable-ordered by Coverage Index

                    var coverageOffset = reader.ReadUInt16();
                    var chainPosRuleSetCount = reader.ReadUInt16();
                    var chainPosRuleSetOffsetList = reader.ReadUInt16Array(chainPosRuleSetCount);

                    var subTable = new LkSubTableType8Fmt1();
                    subTable.PosRuleSetTables =
                        CreateMultiplePosRuleSetTables(subTableStartAt, chainPosRuleSetOffsetList, reader);
                    subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverageOffset);
                    return subTable;
                }
                case 2:
                {
                    //Chaining Context Positioning Format 2: Class-based Chaining Context Glyph Positioning
                    //uint16 	PosFormat 	                Format identifier-format = 2
                    //Offset16 	Coverage 	                Offset to Coverage table-from beginning of ChainContextPos subtable
                    //Offset16 	BacktrackClassDef 	        Offset to ClassDef table containing backtrack sequence context-from beginning of ChainContextPos subtable
                    //Offset16 	InputClassDef 	            Offset to ClassDef table containing input sequence context-from beginning of ChainContextPos subtable
                    //Offset16 	LookaheadClassDef                   	Offset to ClassDef table containing lookahead sequence context-from beginning of ChainContextPos subtable
                    //uint16 	ChainPosClassSetCnt 	                Number of ChainPosClassSet tables
                    //Offset16 	ChainPosClassSet[ChainPosClassSetCnt] 	Array of offsets to ChainPosClassSet tables-from beginning of ChainContextPos subtable-ordered by input class-may be NULL

                    var coverageOffset = reader.ReadUInt16();
                    var backTrackClassDefOffset = reader.ReadUInt16();
                    var inputClassDefOffset = reader.ReadUInt16();
                    var lookadheadClassDefOffset = reader.ReadUInt16();
                    var chainPosClassSetCnt = reader.ReadUInt16();
                    var chainPosClassSetOffsetArray = reader.ReadUInt16Array(chainPosClassSetCnt);

                    var subTable = new LkSubTableType8Fmt2();
                    subTable.BackTrackClassDef =
                        ClassDefTable.CreateFrom(reader, subTableStartAt + backTrackClassDefOffset);
                    subTable.InputClassDef = ClassDefTable.CreateFrom(reader, subTableStartAt + inputClassDefOffset);
                    subTable.LookaheadClassDef =
                        ClassDefTable.CreateFrom(reader, subTableStartAt + lookadheadClassDefOffset);

                    //----------
                    var posClassSetTables = new PosClassSetTable[chainPosClassSetCnt];
                    for (var n = 0; n < chainPosClassSetCnt; ++n)
                    {
                        var offset = chainPosClassSetOffsetArray[n];
                        if (offset > 0)
                            posClassSetTables[n] = PosClassSetTable.CreateFrom(reader, subTableStartAt + offset);
                    }

                    subTable.PosClassSetTables = posClassSetTables;
                    subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverageOffset);

                    return subTable;
                }
                case 3:
                {
                    //Chaining Context Positioning Format 3: Coverage-based Chaining Context Glyph Positioning
                    //uint16 	PosFormat 	                    Format identifier-format = 3
                    //uint16 	BacktrackGlyphCount 	        Number of glyphs in the backtracking sequence
                    //Offset16 	Coverage[BacktrackGlyphCount] 	Array of offsets to coverage tables in backtracking sequence, in glyph sequence order
                    //uint16 	InputGlyphCount 	            Number of glyphs in input sequence
                    //Offset16 	Coverage[InputGlyphCount] 	    Array of offsets to coverage tables in input sequence, in glyph sequence order
                    //uint16 	LookaheadGlyphCount 	        Number of glyphs in lookahead sequence
                    //Offset16 	Coverage[LookaheadGlyphCount] 	Array of offsets to coverage tables in lookahead sequence, in glyph sequence order
                    //uint16 	PosCount 	                    Number of PosLookupRecords
                    //struct 	PosLookupRecord[PosCount] 	    Array of PosLookupRecords,in design order

                    var subTable = new LkSubTableType8Fmt3();

                    var backtrackGlyphCount = reader.ReadUInt16();
                    var backtrackCoverageOffsets = reader.ReadUInt16Array(backtrackGlyphCount);
                    var inputGlyphCount = reader.ReadUInt16();
                    var inputGlyphCoverageOffsets = reader.ReadUInt16Array(inputGlyphCount);
                    var lookaheadGlyphCount = reader.ReadUInt16();
                    var lookaheadCoverageOffsets = reader.ReadUInt16Array(lookaheadGlyphCount);

                    var posCount = reader.ReadUInt16();
                    subTable.PosLookupRecords = CreateMultiplePosLookupRecords(reader, posCount);

                    subTable.BacktrackCoverages =
                        CoverageTable.CreateMultipleCoverageTables(subTableStartAt, backtrackCoverageOffsets, reader);
                    subTable.InputGlyphCoverages =
                        CoverageTable.CreateMultipleCoverageTables(subTableStartAt, inputGlyphCoverageOffsets, reader);
                    subTable.LookaheadCoverages =
                        CoverageTable.CreateMultipleCoverageTables(subTableStartAt, lookaheadCoverageOffsets, reader);

                    return subTable;
                }
            }
        }

        /// <summary>
        ///     LookupType 9: Extension Positioning
        /// </summary>
        /// <param name="reader"></param>
        private static LookupSubTable ReadLookupType9(BinaryReader reader, long subTableStartAt)
        {
            reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);
            var format = reader.ReadUInt16();
            var extensionLookupType = reader.ReadUInt16();
            var extensionOffset = reader.ReadUInt32();
            if (extensionLookupType == 9) throw new NotSupportedException();
            // Simply read the lookup table again with updated offsets

            return ReadSubTable(extensionLookupType, reader, subTableStartAt + extensionOffset);
        }

        private class LkSubTableType1 : LookupSubTable
        {
            private readonly CoverageTable _coverageTable;
            private readonly ValueRecord[] _valueRecords;

            public LkSubTableType1(CoverageTable coverage, ValueRecord singleValue)
            {
                Format = 1;
                _coverageTable = coverage;
                _valueRecords = new[] { singleValue };
            }

            public LkSubTableType1(CoverageTable coverage, ValueRecord[] valueRecords)
            {
                Format = 2;
                _coverageTable = coverage;
                _valueRecords = valueRecords;
            }

            public int Format { get; }

            public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
            {
                var lim = Math.Min(startAt + len, inputGlyphs.Count);
                for (var i = startAt; i < lim; ++i)
                {
                    var glyph_index = inputGlyphs.GetGlyph(i, out var glyph_advW);
                    var cov_index = _coverageTable.FindPosition(glyph_index);
                    if (cov_index > -1)
                    {
                        var vr = _valueRecords[Format == 1 ? 0 : cov_index];
                        inputGlyphs.AppendGlyphOffset(i, vr.XPlacement, vr.YPlacement);
                        inputGlyphs.AppendGlyphAdvance(i, vr.XAdvance, 0);
                    }
                }
            }
        }

        /// <summary>
        ///     Lookup Type 2, Format1: Pair Adjustment Positioning Subtable
        /// </summary>
        private class LkSubTableType2Fmt1 : LookupSubTable
        {
            internal readonly PairSetTable[] _pairSetTables;

            public LkSubTableType2Fmt1(PairSetTable[] pairSetTables)
            {
                _pairSetTables = pairSetTables;
            }

            public CoverageTable CoverageTable { get; set; }

            public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
            {
                //find marker
                var covTable = CoverageTable;
                var lim = inputGlyphs.Count - 1;
                for (var i = 0; i < lim; ++i)
                {
                    var firstGlyphFound = covTable.FindPosition(inputGlyphs.GetGlyph(i, out var glyph_advW));
                    if (firstGlyphFound > -1)
                    {
                        //test this with Palatino A-Y sequence
                        var pairSet = _pairSetTables[firstGlyphFound];

                        //check second glyph  
                        var second_glyph_index = inputGlyphs.GetGlyph(i + 1, out var second_glyph_w);

                        if (pairSet.FindPairSet(second_glyph_index, out var foundPairSet))
                        {
                            var v1 = foundPairSet.value1;
                            var v2 = foundPairSet.value2;
                            //TODO: recheck for vertical writing ... (YAdvance)
                            if (v1 != null)
                            {
                                inputGlyphs.AppendGlyphOffset(i, v1.XPlacement, v1.YPlacement);
                                inputGlyphs.AppendGlyphAdvance(i, v1.XAdvance, 0);
                            }

                            if (v2 != null)
                            {
                                inputGlyphs.AppendGlyphOffset(i + 1, v2.XPlacement, v2.YPlacement);
                                inputGlyphs.AppendGlyphAdvance(i + 1, v2.XAdvance, 0);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Lookup Type2, Format2: Class pair adjustment
        /// </summary>
        private class LkSubTableType2Fmt2 : LookupSubTable
        {
            internal readonly ClassDefTable _class1Def;

            //Format 2 defines a pair as a set of two glyph classes and modifies the positions of all the glyphs in a class
            internal readonly Lk2Class1Record[] _class1records;
            internal readonly ClassDefTable _class2Def;

            public LkSubTableType2Fmt2(Lk2Class1Record[] class1records, ClassDefTable class1Def,
                ClassDefTable class2Def)
            {
                _class1records = class1records;
                _class1Def = class1Def;
                _class2Def = class2Def;
            }

            public CoverageTable CoverageTable { get; set; }

            public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
            {
                //coverage
                //The Coverage table lists the indices of the first glyphs that may appear in each glyph pair.
                //More than one pair may begin with the same glyph, 
                //but the Coverage table lists the glyph index only once

                var covTable = CoverageTable;
                var lim = inputGlyphs.Count - 1;
                for (var i = 0; i < lim; ++i) //start at 0
                {
                    var glyph1_index = inputGlyphs.GetGlyph(i, out var glyph_advW);
                    var record1Index = covTable.FindPosition(glyph1_index);
                    if (record1Index > -1)
                    {
                        var class1_no = _class1Def.GetClassValue(glyph1_index);
                        if (class1_no > -1)
                        {
                            var glyph2_index = inputGlyphs.GetGlyph(i + 1, out var glyph_advW2);
                            var class2_no = _class2Def.GetClassValue(glyph2_index);

                            if (class2_no > -1)
                            {
                                var class1Rec = _class1records[class1_no];
                                //TODO: recheck for vertical writing ... (YAdvance)
                                var pair = class1Rec.class2Records[class2_no];

                                var v1 = pair.value1;
                                var v2 = pair.value2;

                                if (v1 != null)
                                {
                                    inputGlyphs.AppendGlyphOffset(i, v1.XPlacement, v1.YPlacement);
                                    inputGlyphs.AppendGlyphAdvance(i, v1.XAdvance, 0);
                                }

                                if (v2 != null)
                                {
                                    inputGlyphs.AppendGlyphOffset(i + 1, v2.XPlacement, v2.YPlacement);
                                    inputGlyphs.AppendGlyphAdvance(i + 1, v2.XAdvance, 0);
                                }
                            }
                        }
                    }
                }
            }
        }

        private readonly struct Lk2Class1Record
        {
            // a Class1Record enumerates all pairs that contain a particular class as a first component.
            //The Class1Record array stores all Class1Records according to class value.

            //Note: Class1Records are not tagged with a class value identifier.
            //Instead, the index value of a Class1Record in the array defines the class value represented by the record.
            //For example, the first Class1Record enumerates pairs that begin with a Class 0 glyph,
            //the second Class1Record enumerates pairs that begin with a Class 1 glyph, and so on.

            //Each Class1Record contains an array of Class2Records (Class2Record), which also are ordered by class value. 
            //One Class2Record must be declared for each class in the ClassDef2 table, including Class 0.
            //--------------------------------
            //Class1Record
            //Value 	Type 	Description
            //struct 	Class2Record[Class2Count] 	Array of Class2 records-ordered by Class2
            //--------------------------------
            public readonly Lk2Class2Record[] class2Records;

            public Lk2Class1Record(Lk2Class2Record[] class2Records)
            {
                this.class2Records = class2Records;
            }
            //#if DEBUG
            //                public override string ToString()
            //                {
            //                    System.Text.StringBuilder stbuilder = new System.Text.StringBuilder();
            //                    for (int i = 0; i < class2Records.Length; ++i)
            //                    {
            //                        Lk2Class2Record rec = class2Records[i];
            //                        string str = rec.ToString();

            //                        if (str != "value1:,value2:")
            //                        {
            //                            //skip
            //                            stbuilder.Append("i=" + i + "=>" + str + "    ");
            //                        }
            //                    }
            //                    return stbuilder.ToString();
            //                    //return base.ToString();
            //                }
            //#endif
        }

        private class Lk2Class2Record
        {
            //A Class2Record consists of two ValueRecords,
            //one for the first glyph in a class pair (Value1) and one for the second glyph (Value2).
            //If the PairPos subtable has a value of zero (0) for ValueFormat1 or ValueFormat2, 
            //the corresponding record (ValueRecord1 or ValueRecord2) will be empty.

            //Class2Record
            //--------------------------------
            //Value 	    Type 	Description
            //ValueRecord 	Value1 	Positioning for first glyph-empty if ValueFormat1 = 0
            //ValueRecord 	Value2 	Positioning for second glyph-empty if ValueFormat2 = 0
            //--------------------------------
            public readonly ValueRecord value1; //null= empty
            public readonly ValueRecord value2; //null= empty

            public Lk2Class2Record(ValueRecord value1, ValueRecord value2)
            {
                this.value1 = value1;
                this.value2 = value2;
            }

#if DEBUG
            public override string ToString()
            {
                return "value1:" + value1 + ",value2:" + value2;
            }
#endif
        }

        /// <summary>
        ///     Lookup Type 4: MarkToBase Attachment Positioning, or called (MarkBasePos) table
        /// </summary>
        private class LkSubTableType4 : LookupSubTable
        {
            public CoverageTable MarkCoverageTable { get; set; }
            public CoverageTable BaseCoverageTable { get; set; }
            public BaseArrayTable BaseArrayTable { get; set; }
            public MarkArrayTable MarkArrayTable { get; set; }

            public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
            {
                var lim = Math.Min(startAt + len, inputGlyphs.Count);

                // Find the mark glyph, starting at 1
                var longLookBack = OwnerGPos.EnableLongLookBack;
                for (var i = Math.Max(startAt, 1); i < lim; ++i)
                {
                    var markFound = MarkCoverageTable.FindPosition(inputGlyphs.GetGlyph(i, out var glyph_advW));
                    if (markFound < 0) continue;

                    // Look backwards for the base glyph
                    var j = FindGlyphBackwardByKind(inputGlyphs, GlyphClassKind.Base, i,
                        longLookBack ? startAt : i - 1);
                    if (j < 0)
                    {
                        // Fall back to type 0
                        j = FindGlyphBackwardByKind(inputGlyphs, GlyphClassKind.Zero, i,
                            longLookBack ? startAt : i - 1);
                        if (j < 0) continue;
                    }

                    var prev_glyph = inputGlyphs.GetGlyph(j, out var prev_glyph_adv_w);
                    var baseFound = BaseCoverageTable.FindPosition(prev_glyph);
                    if (baseFound < 0) continue;

                    var baseRecord = BaseArrayTable.GetBaseRecords(baseFound);
                    var markClass = MarkArrayTable.GetMarkClass(markFound);
                    // find anchor on base glyph
                    var anchor = MarkArrayTable.GetAnchorPoint(markFound);
                    var prev_anchor = baseRecord.anchors[markClass];
                    inputGlyphs.GetOffset(j, out var prev_glyph_xoffset, out var prev_glyph_yoffset);
                    inputGlyphs.GetOffset(i, out var glyph_xoffset, out var glyph_yoffset);
                    var xoffset = prev_glyph_xoffset + prev_anchor.xcoord -
                                  (prev_glyph_adv_w + glyph_xoffset + anchor.xcoord);
                    var yoffset = prev_glyph_yoffset + prev_anchor.ycoord - (glyph_yoffset + anchor.ycoord);
                    inputGlyphs.AppendGlyphOffset(i, (short)xoffset, (short)yoffset);
                }
            }

#if DEBUG
            public void dbugTest()
            {
                //count base covate
                var expandedMarks = new List<ushort>(MarkCoverageTable.GetExpandedValueIter());
                if (expandedMarks.Count != MarkArrayTable.dbugGetAnchorCount()) throw new NotSupportedException();
                //--------------------------
                var expandedBase = new List<ushort>(BaseCoverageTable.GetExpandedValueIter());
                if (expandedBase.Count != BaseArrayTable.dbugGetRecordCount()) throw new NotSupportedException();
            }
#endif
        }


        //Lookup Type 5: MarkToLigature Attachment Positioning Subtable
        private class LkSubTableType5 : LookupSubTable
        {
            public CoverageTable MarkCoverage { get; set; }
            public CoverageTable LigatureCoverage { get; set; }
            public MarkArrayTable MarkArrayTable { get; set; }
            public LigatureArrayTable LigatureArrayTable { get; set; }

            public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
            {
                Utils.WarnUnimplemented("GPOS Lookup Sub Table Type 5");
            }
        }

        //-----------------------------------------------------------------
        //https://docs.microsoft.com/en-us/typography/opentype/otspec180/gpos#lookup-type-6--marktomark-attachment-positioning-subtable
        /// <summary>
        ///     Lookup Type 6: MarkToMark Attachment
        ///     defines the position of one mark relative to another mark
        /// </summary>
        private class LkSubTableType6 : LookupSubTable
        {
            public CoverageTable MarkCoverage1 { get; set; }
            public CoverageTable MarkCoverage2 { get; set; }
            public MarkArrayTable Mark1ArrayTable { get; set; }

            public Mark2ArrayTable
                Mark2ArrayTable
            {
                get;
                set;
            } // Mark2 attachment points used to attach Mark1 glyphs to a specific Mark2 glyph. 


            public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
            {
                //The attaching mark is Mark1, 
                //and the base mark being attached to is Mark2.

                //The Mark2 glyph (that combines with a Mark1 glyph) is the glyph preceding the Mark1 glyph in glyph string order 
                //(skipping glyphs according to LookupFlags)

                //@prepare: we must found mark2 glyph before mark1
                var longLookBack = OwnerGPos.EnableLongLookBack;
#if DEBUG
                if (len == 3 || len == 4)
                {
                }
#endif
                //find marker
                var lim = Math.Min(startAt + len, inputGlyphs.Count);

                for (var i = Math.Max(startAt, 1); i < lim; ++i)
                {
                    // Find first mark glyph
                    var mark1Found = MarkCoverage1.FindPosition(inputGlyphs.GetGlyph(i, out var glyph_adv_w));
                    if (mark1Found < 0) continue;

                    // Look back for previous mark glyph
                    var prev_mark = FindGlyphBackwardByKind(inputGlyphs, GlyphClassKind.Mark, i,
                        longLookBack ? startAt : i - 1);
                    if (prev_mark < 0) continue;

                    var mark2Found =
                        MarkCoverage2.FindPosition(inputGlyphs.GetGlyph(prev_mark, out var prev_pos_adv_w));
                    if (mark2Found < 0) continue;

                    // Examples:
                    // 👨🏻‍👩🏿‍👧🏽‍👦🏽‍👦🏿 in Segoe UI Emoji

                    int mark1ClassId = Mark1ArrayTable.GetMarkClass(mark1Found);
                    var prev_anchor = Mark2ArrayTable.GetAnchorPoint(mark2Found, mark1ClassId);
                    var anchor = Mark1ArrayTable.GetAnchorPoint(mark1Found);
                    if (anchor.ycoord < 0)
                    {
                        //temp HACK!   น้ำ in Tahoma
                        inputGlyphs.AppendGlyphOffset(prev_mark /*PREV*/, anchor.xcoord, anchor.ycoord);
                    }
                    else
                    {
                        inputGlyphs.GetOffset(prev_mark, out var prev_glyph_xoffset, out var prev_glyph_yoffset);
                        inputGlyphs.GetOffset(i, out var glyph_xoffset, out var glyph_yoffset);
                        var xoffset = prev_glyph_xoffset + prev_anchor.xcoord -
                                      (prev_pos_adv_w + glyph_xoffset + anchor.xcoord);
                        var yoffset = prev_glyph_yoffset + prev_anchor.ycoord - (glyph_yoffset + anchor.ycoord);
                        inputGlyphs.AppendGlyphOffset(i, (short)xoffset, (short)yoffset);
                    }
                }
            }
        }

        private class LkSubTableType7Fmt1 : LookupSubTable
        {
            public CoverageTable CoverageTable { get; set; }
            public PosRuleSetTable[] PosRuleSetTables { get; set; }

            public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
            {
                Utils.WarnUnimplemented("GPOS Lookup Sub Table Type 7 Format 1");
            }
        }

        private class LkSubTableType7Fmt2 : LookupSubTable
        {
            public ClassDefTable ClassDef { get; set; }
            public CoverageTable CoverageTable { get; set; }
            public PosClassSetTable[] PosClassSetTables { get; set; }

            public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
            {
                var lim = Math.Min(startAt + len, inputGlyphs.Count);
                for (var i = startAt; i < lim; ++i)
                {
                    var glyph1_index = inputGlyphs.GetGlyph(i, out var unused);
                    if (CoverageTable.FindPosition(glyph1_index) < 0) continue;

                    var glyph1_class = ClassDef.GetClassValue(glyph1_index);
                    if (glyph1_class >= PosClassSetTables.Length || PosClassSetTables[glyph1_class] == null) continue;

                    foreach (var rule in PosClassSetTables[glyph1_class].PosClassRules)
                    {
                        var glyphIds = rule.InputGlyphIds;
                        var matches = 0;
                        for (var n = 0; n < glyphIds.Length && i + 1 + n < lim; ++n)
                        {
                            var glyphn_index = inputGlyphs.GetGlyph(i + 1 + n, out unused);
                            var glyphn_class = ClassDef.GetClassValue(glyphn_index);
                            if (glyphn_class != glyphIds[n]) break;
                            ++matches;
                        }

                        if (matches == glyphIds.Length)
                        {
                            foreach (var plr in rule.PosLookupRecords)
                            {
                                var lookup = OwnerGPos.LookupList[plr.lookupListIndex];
                                lookup.DoGlyphPosition(inputGlyphs, i + plr.seqIndex, glyphIds.Length - plr.seqIndex);
                            }

                            break;
                        }
                    }
                }
            }
        }

        private class LkSubTableType7Fmt3 : LookupSubTable
        {
            public CoverageTable[] CoverageTables { get; set; }
            public PosLookupRecord[] PosLookupRecords { get; set; }

            public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
            {
                Utils.WarnUnimplemented("GPOS Lookup Sub Table Type 7 Format 3");
            }
        }

        //----------------------------------------------------------------
        private class LkSubTableType8Fmt1 : LookupSubTable
        {
            public CoverageTable CoverageTable { get; set; }
            public PosRuleSetTable[] PosRuleSetTables { get; set; }

            public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
            {
                Utils.WarnUnimplemented("GPOS Lookup Sub Table Type 8 Format 1");
            }
        }

        private class LkSubTableType8Fmt2 : LookupSubTable
        {
            public CoverageTable CoverageTable { get; set; }
            public PosClassSetTable[] PosClassSetTables { get; set; }

            public ClassDefTable BackTrackClassDef { get; set; }
            public ClassDefTable InputClassDef { get; set; }
            public ClassDefTable LookaheadClassDef { get; set; }

            public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
            {
                var glyphIndex = inputGlyphs.GetGlyph(startAt, out var advW);

                var coverage_pos = CoverageTable.FindPosition(glyphIndex);
                if (coverage_pos < 0) return;


                Utils.WarnUnimplemented("GPOS Lookup Sub Table Type 8 Format 2");
            }
        }

        private class LkSubTableType8Fmt3 : LookupSubTable
        {
            public CoverageTable[] BacktrackCoverages { get; set; }
            public CoverageTable[] InputGlyphCoverages { get; set; }
            public CoverageTable[] LookaheadCoverages { get; set; }
            public PosLookupRecord[] PosLookupRecords { get; set; }

            public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
            {
                startAt = Math.Max(startAt, BacktrackCoverages.Length);
                var lim = Math.Min(startAt + len, inputGlyphs.Count) - (InputGlyphCoverages.Length - 1) -
                          LookaheadCoverages.Length;
                for (var pos = startAt; pos < lim; ++pos) DoGlyphPositionAt(inputGlyphs, pos);
            }

            protected void DoGlyphPositionAt(IGlyphPositions inputGlyphs, int pos)
            {
                // Check all coverages: if any of them does not match, abort substitution
                for (var i = 0; i < InputGlyphCoverages.Length; ++i)
                    if (InputGlyphCoverages[i].FindPosition(inputGlyphs.GetGlyph(pos + i, out var unused)) < 0)
                        return;

                for (var i = 0; i < BacktrackCoverages.Length; ++i)
                    if (BacktrackCoverages[i].FindPosition(inputGlyphs.GetGlyph(pos - 1 - i, out var unused)) < 0)
                        return;

                for (var i = 0; i < LookaheadCoverages.Length; ++i)
                    if (LookaheadCoverages[i]
                            .FindPosition(inputGlyphs.GetGlyph(pos + InputGlyphCoverages.Length + i, out var unused)) <
                        0)
                        return;

                foreach (var plr in PosLookupRecords)
                {
                    var lookup = OwnerGPos.LookupList[plr.lookupListIndex];
                    lookup.DoGlyphPosition(inputGlyphs, pos + plr.seqIndex, InputGlyphCoverages.Length - plr.seqIndex);
                }
            }
        }
    }

#if DEBUG
#endif
}