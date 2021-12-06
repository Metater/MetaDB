namespace MetaDB;

public class Entry : ISerializable
{
    public string Name { get; protected set; }
    public bool HasChildren => children != null;
    public Dictionary<string, Entry> children = null;

    public bool ChildExists(string name)
    {
        if (!HasChildren) return false;
        return children.ContainsKey(name);
    }

    public bool TryGetChild(string name, out Entry child)
    {
        if (!HasChildren)
        {
            child = null;
            return false;
        }
        return children.TryGetValue(name, out child);
    }

    public bool TryAddChild(Entry child)
    {
        if (!HasChildren) children = new();
        return children.TryAdd(child.Name, child);
    }

    protected Entry() { }

    public Entry(string name)
    {
        Name = name;
    }

    private Entry(BitReader reader)
    {
        Deserialize(reader);
    }

    internal static Entry GenericDeserialize(BitReader reader)
    {
        EntryType entryType = (EntryType)reader.GetByte();
        return entryType switch
        {
            EntryType.Base => new Entry(reader),
            EntryType.ULong => new ULongEntry(reader),
            EntryType.Double => new DoubleEntry(reader),
            EntryType.Byte => new ByteEntry(reader),
            EntryType.String => new StringEntry(reader),
            _ => throw new Exception("Unknown entry type while deserializing database"),
        };
    }

    protected void DeserializeBase(BitReader reader)
    {
        Name = reader.GetString(8);
        int childrenCount = reader.GetUShort();
        if (childrenCount == 0) return;
        children = new Dictionary<string, Entry>(childrenCount);
        for (int i = 0; i < childrenCount; i++)
        {
            Entry child = GenericDeserialize(reader);
            children.Add(child.Name, child);
        }
    }

    public virtual void Deserialize(BitReader reader)
    {
        DeserializeBase(reader);
    }

    public virtual void Serialize(BitWriter writer)
    {
        writer.Put((byte)EntryType.Base);
        SerializeBase(writer);
    }

    protected void SerializeBase(BitWriter writer)
    {
        writer.Put(Name, 8);
        if (!HasChildren)
        {
            writer.Put((ushort)0);
        }
        else
        {
            writer.Put((ushort)children.Count);
            foreach (Entry child in children.Values)
            {
                child.Serialize(writer);
            }
        }
    }
}

public class ULongEntry : Entry
{
    public ulong value;

    public ULongEntry(string name, ulong value) : base(name)
    {
        this.value = value;
    }

    public ULongEntry(BitReader reader)
    {
        Deserialize(reader);
    }

    public override void Deserialize(BitReader reader)
    {
        DeserializeBase(reader);
        value = reader.GetULong();
    }

    public override void Serialize(BitWriter writer)
    {
        writer.Put((byte)EntryType.ULong);
        SerializeBase(writer);
        writer.Put(value);
    }
}

public class LongEntry : Entry
{
    public long value;

    public LongEntry(string name, long value) : base(name)
    {
        this.value = value;
    }

    public LongEntry(BitReader reader)
    {
        Deserialize(reader);
    }

    public override void Deserialize(BitReader reader)
    {
        DeserializeBase(reader);
        value = reader.GetLong();
    }

    public override void Serialize(BitWriter writer)
    {
        writer.Put((byte)EntryType.Long);
        SerializeBase(writer);
        writer.Put(value);
    }
}

public class DoubleEntry : Entry
{
    public double value;

    public DoubleEntry(string name, double value) : base(name)
    {
        this.value = value;
    }

    public DoubleEntry(BitReader reader)
    {
        Deserialize(reader);
    }

    public override void Deserialize(BitReader reader)
    {
        DeserializeBase(reader);
        value = reader.GetDouble();
    }

    public override void Serialize(BitWriter writer)
    {
        writer.Put((byte)EntryType.Double);
        SerializeBase(writer);
        writer.Put(value);
    }
}

public class ByteEntry : Entry
{
    public byte value;

    public ByteEntry(string name, byte value) : base(name)
    {
        this.value = value;
    }

    public ByteEntry(BitReader reader)
    {
        Deserialize(reader);
    }

    public override void Deserialize(BitReader reader)
    {
        DeserializeBase(reader);
        value = reader.GetByte();
    }

    public override void Serialize(BitWriter writer)
    {
        writer.Put((byte)EntryType.String);
        SerializeBase(writer);
        writer.Put(value);
    }
}

public class StringEntry : Entry
{
    public string value;

    public StringEntry(string name, string value) : base(name)
    {
        this.value = value;
    }

    public StringEntry(BitReader reader)
    {
        Deserialize(reader);
    }

    public override void Deserialize(BitReader reader)
    {
        DeserializeBase(reader);
        value = reader.GetString(8);
    }

    public override void Serialize(BitWriter writer)
    {
        writer.Put((byte)EntryType.String);
        SerializeBase(writer);
        writer.Put(value, 8);
    }
}