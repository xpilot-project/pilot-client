using System;

namespace MessagePack.CryptoDto
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CryptoDtoAttribute : Attribute
    {
        public string ShortDtoName { get; private set; }

        public CryptoDtoAttribute(string shortDtoName)
        {
            ShortDtoName = shortDtoName;
        }
    }
}
