using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using Newtonsoft.Json;

namespace skinchanger_loader.SDK.Api
{
    public class LocalProto: IDisposable
    {

        private byte[] _pKey;
        private TcpListener _listener;
        private Socket _cheat;

        public void Dispose()
        {
            Array.Clear(_pKey, 0, _pKey.Length);
            _cheat.Close();
            _listener.Stop();
        }

        public LocalProto(int port)
        {
            _pKey = new byte[8];
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            _listener = new TcpListener(ip, port);
            _listener.Start();
            _cheat = _listener.AcceptSocket();
            _cheat.Receive(_pKey);
        }

        public int Send(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] ^= _pKey[i % 8];

            return _cheat.Send(bytes);
        }

        public byte[] Recive()
        {
            byte[] packetLengthBytes = new byte[4];
            int packetLength;
            _cheat.Receive(packetLengthBytes);
            for (int i = 0; i < packetLengthBytes.Length; i++)
                packetLengthBytes[i] ^= _pKey[i % 8];

            packetLength = BitConverter.ToInt32(packetLengthBytes, 0);

            byte[] recv = new byte[packetLength];
            _cheat.Receive(recv);
            for (int i = 0; i < packetLength; i++)
                recv[i] ^= _pKey[i % 8];
            return recv;
        }

        public bool SendJson<T>(T jsonObj)
        {
            List<byte> json = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(jsonObj)).ToList();
            json.Add(0);

            if (Send(BitConverter.GetBytes(json.Count)) != 4)
                return false;

            return Send(json.ToArray()) == json.Count;
        }

        public T ReciveJson<T>()
        {
            string json = Encoding.ASCII.GetString(Recive()).Replace("\0", "");
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}