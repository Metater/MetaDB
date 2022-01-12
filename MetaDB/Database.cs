namespace MetaDB;

public sealed class Database
{
    private readonly string path;
    private readonly BitWriter writer = new(32768, 64);

    public readonly Dictionary<string, Table> database = new();

    public void EnsureTableExists(string name)
    {
        if (!database.ContainsKey(name)) database.Add(name, new Table(name));
    }

    public bool TableExists(string name)
    {
        return database.ContainsKey(name);
    }

    public bool TryGetTable(string name, out Table table)
    {
        return database.TryGetValue(name, out table);
    }

    public bool TryAddTable(Table table)
    {
        return database.TryAdd(table.Name, table);
    }

    public Table GetTable(string name)
    {
        return database[name];
    }

    public Database(string path)
    {
        this.path = path;
        if (File.Exists(path))
        {
            MetaDatabase.Log($"Loading database from {path}");
            byte[] db = File.ReadAllBytes(path);
            BitReader reader = new(db);
            Deserialize(reader);
            MetaDatabase.Log($"Loaded database from {path}");
        }
        else
        {
            MetaDatabase.Log($"Creating a new database, could not find database at {path}");
        }
    }

    public void Save()
    {
        Serialize(writer);
        File.WriteAllBytes(path, writer.Assemble());
        writer.Reset();
        //MetaDatabase.Log("Saved");
    }


    private void Deserialize(BitReader reader)
    {
        int tableCount = reader.GetInt();
        for (int i = 0; i < tableCount; i++)
        {
            Table table = new(reader);
            database.Add(table.Name, table);
        }
    }

    private void Serialize(BitWriter writer)
    {
        writer.Put(database.Count);
        foreach (Table table in database.Values)
        {
            table.Serialize(writer);
        }
    }
}