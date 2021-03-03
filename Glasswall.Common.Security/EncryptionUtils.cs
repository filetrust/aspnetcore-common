using System.Security.Cryptography;
using System.Text;

namespace Glasswall.Common.Security
{
    public static class EncryptionUtils
    {
        public const string EncryptedMarker = "ThisFileIsEncrypted";
        private const int InitializationVectorLength = 16;
        private static readonly byte[] _encryptedMarkerBytes = Encoding.UTF8.GetBytes(EncryptedMarker);

        public static byte[] GenerateSalt(int length)
        {
            var bytes = new byte[length];
            using (var b = new RNGCryptoServiceProvider())
            {
                b.GetBytes(bytes);
            }

            return bytes;
        }
    }
}