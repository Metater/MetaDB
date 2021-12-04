namespace MetaDB;

interface ISerializable
{
    void Serialize(BitWriter writer);
    void Deserialize(BitReader reader);
}