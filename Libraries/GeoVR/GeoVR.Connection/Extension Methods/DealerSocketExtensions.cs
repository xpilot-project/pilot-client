using MessagePack.CryptoDto;
using MessagePack.CryptoDto.Managed;
using NetMQ;
using NetMQ.Sockets;

namespace GeoVR.Connection
{
    public static class DealerSocketExtensions
    {
        public static long SerialiseCryptoDto<T>(this DealerSocket dealerSocket, object data, CryptoDtoChannel channel, CryptoDtoMode mode)
        {
            T typedData = (T)data;
            byte[] cryptoData = CryptoDtoSerializer.Serialize(channel, mode, typedData);

            dealerSocket
                .SendMoreFrameEmpty()
                .SendFrame(cryptoData);

            return cryptoData.Length;
        }
    }
}
