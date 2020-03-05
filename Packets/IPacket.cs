namespace Packets
{
    public interface IPacket
    {
        byte[] Serialize();
        bool IsMatch(byte[] bytes, int start, int count);
        void Deserialize(byte[] bytes, int start, int count, out int used);
        IPacket Clone();
    }
}