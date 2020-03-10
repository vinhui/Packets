using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using NLog;

namespace Packets
{
    public class FileTransfer
    {
        private static readonly Logger Logger = LogManager.GetLogger(nameof(FileTransfer));

        private static ulong _id;

        public ushort ChunkSize = 64;

        private readonly List<ReceivingFileProcess> receivingFiles = new List<ReceivingFileProcess>();

        public event EventHandler<FileStream> FileReceived;

        private class ReceivingFileProcess
        {
            public ulong UniqueId;
            public FileStream Stream;
            public int ReceivedChunks;
            public EndPoint EndPoint;

            public ReceivingFileProcess(FileStream stream, ulong uniqueId, EndPoint endPoint)
            {
                Stream = stream;
                UniqueId = uniqueId;
                EndPoint = endPoint;
            }
        }

        public void OnPacketReceived(EndPoint endPoint, IPacket packet)
        {
            if (!(packet is ChunkedDataPacket p))
                return;

            var f = receivingFiles.FirstOrDefault(x => x.UniqueId == p.UniqueId && x.EndPoint == endPoint);
            if (f == null)
            {
                f = new ReceivingFileProcess(CreateNewTempFile(), p.UniqueId, endPoint);
                receivingFiles.Add(f);
                Logger.Info("Receiving a new file from {endPoint}, saving it to {path}", endPoint, f.Stream.Name);
            }

            f.Stream.Position = p.Offset;
            f.Stream.Write(p.Data, 0, p.Data.Length);
            f.Stream.Flush();
            f.ReceivedChunks++;
            Logger.Debug("Received file chunk {chunk}/{totalChunks} ({pct})", f.ReceivedChunks, p.TotalChunks, (float) f.ReceivedChunks / p.TotalChunks);

            if (f.ReceivedChunks == p.TotalChunks)
            {
                Logger.Info("Received all file chunks");
                f.Stream.Position = 0;
                if (FileReceived != null && FileReceived.GetInvocationList().Length > 0)
                {
                    FileReceived.Invoke(this, f.Stream);
                }
                else
                {
                    f.Stream.Close();
                }

                receivingFiles.RemoveAll(x => x.UniqueId == p.UniqueId && x.EndPoint == endPoint);
            }
        }

        private static FileStream CreateNewTempFile()
        {
            return new FileStream(Path.GetTempFileName(), FileMode.OpenOrCreate);
        }

        public void SendFile(FileStream file, Action<IPacket> sendPacket)
        {
            if (file.Length > uint.MaxValue)
            {
                throw new ArgumentException("The stream is larger than 4294967295 bytes", nameof(file));
            }

            _id++;
            var id = _id;

            var length = file.Length;
            file.Position = 0;
            var buffer = new byte[ChunkSize];
            var totalChunks = (ushort) Math.Ceiling(length / (double) ChunkSize);
            uint offset = 0;
            int read;
            var i = 0;
            while ((read = file.Read(buffer, 0, ChunkSize)) > 0)
            {
                i++;
                Logger.Debug("Sending file chunk {chunk}/{totalChunks} ({pct})", i, totalChunks, (float) i / totalChunks);
                var packet = new ChunkedDataPacket
                {
                    UniqueId = id,
                    Offset = offset,
                    TotalChunks = totalChunks,
                    Data = buffer,
                    DataLength = read
                };
                sendPacket.Invoke(packet);

                offset += (ushort) read;
            }
        }
    }
}