//Apache2, 2017-present, WinterDev
//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.IO;

namespace Typography.OpenFont.Tables;

/// <summary>
///     this is base class of all 'top' font table
/// </summary>
public abstract class TableEntry
{
    internal TableHeader Header { get; set; }
    public abstract string Name { get; }
    public uint TableLength => Header.Length;
    protected abstract void ReadContentFrom(BinaryReader reader);

    internal void LoadDataFrom(BinaryReader reader)
    {
        //ensure that we always start at the correct offset***
        reader.BaseStream.Seek(Header.Offset, SeekOrigin.Begin);
        ReadContentFrom(reader);
    }
}

internal class UnreadTableEntry : TableEntry
{
    public UnreadTableEntry(TableHeader header)
    {
        Header = header;
    }

    public override string Name => Header.Tag;

    public bool HasCustomContentReader { get; protected set; }

    //
    protected sealed override void ReadContentFrom(BinaryReader reader)
    {
        //intend ***
        throw new NotImplementedException();
    }

    public virtual T CreateTableEntry<T>(BinaryReader reader, T expectedResult)
        where T : TableEntry
    {
        throw new NotImplementedException();
    }
#if DEBUG
    public override string ToString()
    {
        return Name;
    }
#endif
}