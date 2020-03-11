# Packets

This is a repo that has a simple yet versatile packets (or messages) framework. This makes it easy to send and receive TCP, UDP, possibly Bluetooth or even serial messages.

This repo also includes a TCP server and client with example console applications.

## Creating your own Packets

It's quite easy to create your own packets. A good practice is to create a custom header thats unique for each packet type. This makes it easy to distinguish between different types of packets.

The following is an example of a very basic packet.

``` csharp
using Packets;

public struct EmptyPacket : IPacket
{
    private static readonly byte[] Header =
    {
        (byte) '1',
        (byte) '2',
        (byte) '3',
        (byte) '4',
        (byte) '5',
        (byte) '6',
        (byte) '7',
        (byte) '8',
    };

    public byte[] Serialize()
    {
        var bytes = new byte[Header.Length];
        PacketUtils.WriteHeader(bytes, 0, Header);

        return bytes;
    }

    public bool IsMatch(byte[] bytes, int start, int count)
    {
        return PacketUtils.MatchesHeader(bytes, start, count, Header);
    }

    public void Deserialize(byte[] bytes, int start, int count, out int used)
    {
        used = Header.Length;
    }

    public IPacket Clone()
    {
        return this;
    }
}
```

For a more complete example that also sends some data back and forth, check out the [PingPacket](Tcp/PingPacket.cs).