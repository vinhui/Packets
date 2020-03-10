namespace Packets
{
    public interface IPacket
    {
        /// <summary>
        /// Serialize a class to bytes which we can send
        /// </summary>
        /// <returns>Returns the byte array to send</returns>
        byte[] Serialize();

        /// <summary>
        /// Checks if the incoming bytes are a match for this packet type
        /// If it's a match, it will go to <see cref="Deserialize"/>
        /// </summary>
        /// <param name="bytes">Incoming bytes buffer</param>
        /// <param name="start">The index from which we should start checking</param>
        /// <param name="count">The total amount of bytes in the buffer</param>
        /// <returns>If it's a match or not</returns>
        bool IsMatch(byte[] bytes, int start, int count);

        /// <summary>
        /// Deserialize the incoming byte buffer into this object
        /// This will only get called if <see cref="IsMatch"/> was true
        /// </summary>
        /// <param name="bytes">The buffer to read from</param>
        /// <param name="start">The index from which we should start</param>
        /// <param name="count">The total amount of bytes in the buffer</param>
        /// <param name="used">The amount of bytes this packet used</param>
        void Deserialize(byte[] bytes, int start, int count, out int used);

        /// <summary>
        /// Make a clone of this object
        /// </summary>
        /// <returns>Returns a copy of the packet</returns>
        IPacket Clone();
    }
}