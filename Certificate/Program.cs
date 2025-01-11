using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;


void GenerateKeyPair() 
{
    var keyGenerationParameters = new KeyGenerationParameters(new SecureRandom(), 2048);
    var keyPairGenerator = new RsaKeyPairGenerator();
    keyPairGenerator.Init(keyGenerationParameters);

    var rsaKeyPair = keyPairGenerator.GenerateKeyPair();
}

