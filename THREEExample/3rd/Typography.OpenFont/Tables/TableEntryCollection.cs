//Apache2, 2017-present, WinterDev
//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System.Collections.Generic;

namespace Typography.OpenFont.Tables;

internal class TableEntryCollection
{
    private readonly Dictionary<string, TableEntry> _tables = new();

    public void AddEntry(TableEntry en)
    {
        _tables.Add(en.Name, en);
    }

    public bool TryGetTable(string tableName, out TableEntry entry)
    {
        return _tables.TryGetValue(tableName, out entry);
    }

    public void ReplaceTable(TableEntry table)
    {
        _tables[table.Name] = table;
    }

    public TableHeader[] CloneTableHeaders()
    {
        var clones = new TableHeader[_tables.Count];
        var i = 0;
        foreach (var en in _tables.Values)
        {
            clones[i] = en.Header.Clone();
            i++;
        }

        return clones;
    }
}