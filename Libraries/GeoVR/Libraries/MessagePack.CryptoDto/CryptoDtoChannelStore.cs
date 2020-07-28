using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace MessagePack.CryptoDto
{
    public class CryptoDtoChannelStore
    {
        readonly object channelStoreLock = new object();
        private Dictionary<string, CryptoDtoChannel> channelStore;

        public CryptoDtoChannelStore()
        {
            channelStore = new Dictionary<string, CryptoDtoChannel>();
        }

        public int Count { get { lock (channelStoreLock) { return channelStore.Count; } } }
        
        public void CreateChannel(string channelTag, int receiveSequenceHistorySize = 10)
        {
            lock (channelStoreLock)
            {
                if (channelStore.ContainsKey(channelTag))
                    throw new CryptographicException("Key tag already exists in store. (" + channelTag + ")");
                channelStore[channelTag] = new CryptoDtoChannel(channelTag, receiveSequenceHistorySize);
            }
        }

        public CryptoDtoChannelConfigDto GetRemoteEndpointChannelConfig(string channelTag)
        {
            lock (channelStoreLock)
            {
                if (!channelStore.ContainsKey(channelTag))
                    throw new CryptographicException("Key tag does not exist in store. (" + channelTag + ")");

                return channelStore[channelTag].GetRemoteEndpointChannelConfig();
            }
        }

        public ReadOnlySpan<byte> GetReceiveKey(string channelTag, CryptoDtoMode mode)
        {
            lock (channelStoreLock)
            {
                if (!channelStore.ContainsKey(channelTag))
                    throw new CryptographicException("Key tag does not exist in store. (" + channelTag + ")");

                return channelStore[channelTag].GetReceiveKey(mode);
            }
        }

        public void CheckReceivedSequence(string channelTag, ulong sequenceReceived)
        {
            lock (channelStoreLock)
            {
                if (!channelStore.ContainsKey(channelTag))
                    throw new CryptographicException("Key tag does not exist in store. (" + channelTag + ")");

                channelStore[channelTag].CheckReceivedSequence(sequenceReceived);
            }
        }

        public bool IsReceivedSequenceAllowed(string channelTag, ulong sequenceReceived)
        {
            lock (channelStoreLock)
            {
                if (!channelStore.ContainsKey(channelTag))
                    throw new CryptographicException("Key tag does not exist in store. (" + channelTag + ")");

                return channelStore[channelTag].IsReceivedSequenceAllowed(sequenceReceived);
            }
        }

        public ReadOnlySpan<byte> GetTransmitKey(string channelTag, CryptoDtoMode mode, out ulong transmitSequence)
        {
            lock (channelStoreLock)
            {
                if (!channelStore.ContainsKey(channelTag))
                    throw new CryptographicException("Key tag does not exist in store. (" + channelTag + ")");

                return channelStore[channelTag].GetTransmitKey(mode, out transmitSequence);
            }
        }

        public void DeleteChannel(string channelTag)
        {
            lock (channelStoreLock)
            {
                if (!channelStore.ContainsKey(channelTag))
                    throw new CryptographicException("Key tag does not exist in store. (" + channelTag + ")");
                channelStore.Remove(channelTag);
            }
        }

        public void DeleteChannelIfExists(string channelTag)
        {
            lock (channelStoreLock)
            {
                if (channelStore.ContainsKey(channelTag))
                    channelStore.Remove(channelTag);
            }
        }
    }
}
