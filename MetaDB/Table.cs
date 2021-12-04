namespace MetaDB;

public sealed class Table
{
    public string Name { get; private set; }
    public readonly Dictionary<string, Entry> table = new();

    public bool EntryExists(string name)
    {
        return table.ContainsKey(name);
    }

    public bool TryGetEntry(string name, out Entry entry)
    {
        return table.TryGetValue(name, out entry);
    }

    public bool TryAddEntry(string name, Entry entry)
    {
        return table.TryAdd(name, entry);
    }

    public Table(string name)
    {
        Name = name;
    }

    internal Table(BitReader reader)
    {
        Deserialize(reader);
        MetaDatabase.Log($"Deserialized table {Name} with {table.Count} top-level entries");
    }

    private void Deserialize(BitReader reader)
    {
        Name = reader.GetString(8);
        int entryCount = reader.GetInt();
        for (int i = 0; i < entryCount; i++)
        {
            Entry entry = Entry.GenericDeserialize(reader);
            table.Add(entry.Name, entry);
        }
    }

    internal void Serialize(BitWriter writer)
    {
        writer.Put(Name, 8);
        writer.Put(table.Count);
        foreach (Entry entry in table.Values)
        {
            entry.Serialize(writer);
        }
    }
}