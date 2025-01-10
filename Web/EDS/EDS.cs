using System.Security.Cryptography;
using System.Text;
using AuthService.Handler;

namespace AuthService.EDS
{
    public class EDS
    {
        public void Main() 
        {
            CngKey secretKeyForSignature = CngKey.Create(CngAlgorithm.ECDsaP256);

            byte[] publicKeyForSignature = secretKeyForSignature.Export(CngKeyBlobFormat.GenericPublicBlob);

        }
    }
}
