using NaCl.Core;
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace MessagePack.CryptoDto.Managed
{
    public static class CryptoDtoDeserializer
    {
        public static Deserializer Deserialize(CryptoDtoChannelStore channelStore, ReadOnlySpan<byte> bytes)
        {
            return new Deserializer(channelStore, bytes, false);
        }

        public static Deserializer DeserializeIgnoreSequence(CryptoDtoChannelStore channelStore, ReadOnlySpan<byte> bytes)  //This is used for UDP channels where duplication is possible
        {
            return new Deserializer(channelStore, bytes, true);
        }

        public static Deserializer Deserialize(CryptoDtoChannel channel, ReadOnlySpan<byte> bytes)
        {
            return new Deserializer(channel, bytes, false);
        }

        public static Deserializer DeserializeIgnoreSequence(CryptoDtoChannel channel, ReadOnlySpan<byte> bytes)            //This is used for UDP channels where duplication is possible
        {
            return new Deserializer(channel, bytes, true);
        }

        public ref struct Deserializer
        {
            readonly ushort headerLength;
            readonly CryptoDtoHeaderDto header;

            readonly int dtoNameLength;
            ReadOnlySpan<byte> dtoNameBuffer;

            readonly int dataLength;
            ReadOnlySpan<byte> dataBuffer;

            readonly bool sequenceValid;

            internal Deserializer(CryptoDtoChannelStore channelStore, ReadOnlySpan<byte> bytes, bool ignoreSequence)
            {
                sequenceValid = false;
                headerLength = Unsafe.ReadUnaligned<ushort>(ref MemoryMarshal.GetReference(bytes));             //.NET Standard 2.0 doesn't have BitConverter.ToUInt16(Span<T>)               
                if (bytes.Length < (2 + headerLength))
                    throw new CryptographicException("Not enough bytes to process packet.");

                ReadOnlySpan<byte> headerDataBuffer = bytes.Slice(2, headerLength);
                header = MessagePackSerializer.Deserialize<CryptoDtoHeaderDto>(headerDataBuffer.ToArray());
                ReadOnlySpan<byte> receiveKey = channelStore.GetReceiveKey(header.ChannelTag, header.Mode);                 //This will throw exception if channel tag isn't in the store

                switch (header.Mode)
                {
                    case CryptoDtoMode.ChaCha20Poly1305:
                        {
                            int aeLength = bytes.Length - (2 + headerLength);
                            ReadOnlySpan<byte> aePayloadBuffer = bytes.Slice(2 + headerLength, aeLength);

                            ReadOnlySpan<byte> adBuffer = bytes.Slice(0, 2 + headerLength);

                            Span<byte> nonceBuffer = stackalloc byte[Aead.NonceSize];
                            BinaryPrimitives.WriteUInt64LittleEndian(nonceBuffer.Slice(4), header.Sequence);

                            var aead = new ChaCha20Poly1305(receiveKey.ToArray());
                            ReadOnlySpan<byte> decryptedPayload = aead.Decrypt(aePayloadBuffer.ToArray(), adBuffer.ToArray(), nonceBuffer);

                            if (ignoreSequence)
                                sequenceValid = channelStore.IsReceivedSequenceAllowed(header.ChannelTag, header.Sequence);
                            else
                            {
                                channelStore.CheckReceivedSequence(header.ChannelTag, header.Sequence);     //The packet has passed MAC, so now check if it's being duplicated or replayed
                                sequenceValid = true;
                            }

                            dtoNameLength = Unsafe.ReadUnaligned<ushort>(ref MemoryMarshal.GetReference(decryptedPayload));    //.NET Standard 2.0 doesn't have BitConverter.ToUInt16(Span<T>)
                            dtoNameBuffer = decryptedPayload.Slice(2, dtoNameLength);

                            dataLength = Unsafe.ReadUnaligned<ushort>(ref MemoryMarshal.GetReference(decryptedPayload.Slice(2 + dtoNameLength, 2)));    //.NET Standard 2.0 doesn't have BitConverter.ToUInt16(Span<T>)
                            dataBuffer = decryptedPayload.Slice(2 + dtoNameLength + 2, dataLength);
                            break;
                        }
                    default:
                        throw new CryptographicException("Mode not recognised");
                }
            }

            internal Deserializer(CryptoDtoChannel channel, ReadOnlySpan<byte> bytes, bool ignoreSequence)
            {
                sequenceValid = false;
                headerLength = Unsafe.ReadUnaligned<ushort>(ref MemoryMarshal.GetReference(bytes));                                 //.NET Standard 2.0 doesn't have BitConverter.ToUInt16(Span<T>)
                if (bytes.Length < (2 + headerLength))
                    throw new CryptographicException("Not enough bytes to process packet.");

                ReadOnlySpan<byte> headerBuffer = bytes.Slice(2, headerLength);
                header = MessagePackSerializer.Deserialize<CryptoDtoHeaderDto>(headerBuffer.ToArray());

                if (header.ChannelTag != channel.ChannelTag)
                    throw new CryptographicException("Channel Tag doesn't match provided Channel");

                switch (header.Mode)
                {
                    case CryptoDtoMode.ChaCha20Poly1305:
                        {
                            int aeLength = bytes.Length - (2 + headerLength);
                            ReadOnlySpan<byte> aePayloadBuffer = bytes.Slice(2 + headerLength, aeLength);

                            ReadOnlySpan<byte> adBuffer = bytes.Slice(0, 2 + headerLength);

                            Span<byte> nonceBuffer = stackalloc byte[Aead.NonceSize];
                            BinaryPrimitives.WriteUInt64LittleEndian(nonceBuffer.Slice(4), header.Sequence);

                            ReadOnlySpan<byte> receiveKey = channel.GetReceiveKey(header.Mode);
                            var aead = new ChaCha20Poly1305(receiveKey.ToArray());
                            ReadOnlySpan<byte> decryptedPayload = aead.Decrypt(aePayloadBuffer.ToArray(), adBuffer.ToArray(), nonceBuffer);

                            if (ignoreSequence)
                                sequenceValid = channel.IsReceivedSequenceAllowed(header.Sequence);
                            else
                            {
                                channel.CheckReceivedSequence(header.Sequence);     //The packet has passed MAC, so now check if it's being duplicated or replayed
                                sequenceValid = true;
                            }

                            dtoNameLength = Unsafe.ReadUnaligned<ushort>(ref MemoryMarshal.GetReference(decryptedPayload));    //.NET Standard 2.0 doesn't have BitConverter.ToUInt16(Span<T>)

                            if (decryptedPayload.Length < (2 + dtoNameLength))
                                throw new CryptographicException("Not enough bytes to process packet. (2) " + dtoNameLength + " " + decryptedPayload.Length);

                            dtoNameBuffer = decryptedPayload.Slice(2, dtoNameLength);

                            dataLength = Unsafe.ReadUnaligned<ushort>(ref MemoryMarshal.GetReference(decryptedPayload.Slice(2 + dtoNameLength, 2)));    //.NET Standard 2.0 doesn't have BitConverter.ToUInt16(Span<T>)

                            if (decryptedPayload.Length < (2 + dtoNameLength + 2 + dataLength))
                                throw new CryptographicException("Not enough bytes to process packet. (3) " + dataLength + " " + decryptedPayload.Length);
                            dataBuffer = decryptedPayload.Slice(2 + dtoNameLength + 2, dataLength);
                            break;
                        }
                    default:
                        throw new CryptographicException("Mode not recognised");
                }
            }

            public string GetChannelTag()
            {
                return header.ChannelTag;
            }

            public string GetDtoName()
            {
                return Encoding.UTF8.GetString(dtoNameBuffer.ToArray());              //This is fast, so unlikely to optimise without using unsafe or Span support?
            }

            public byte[] GetDtoNameBytes()
            {
                return dtoNameBuffer.ToArray();
            }

            public T GetDto<T>()
            {
                return MessagePackSerializer.Deserialize<T>(dataBuffer.ToArray()); //When MessagePack has Span support, tweak this.
            }

            public byte[] GetDtoBytes()
            {
                return dataBuffer.ToArray();
            }

            public bool IsSequenceValid()           //Use this if the "Ignore Sequence" option was used for UDP channels
            {
                return sequenceValid;
            }
        }
    }
}
