using System.Security.Cryptography;

namespace Battlegrounds.Security;

public static class RSAPublicKey {

    public static RSA FromPem(string pem) {

        if (pem.Contains("RSA PUBLIC KEY")) {
            pem = pem.Replace("RSA PUBLIC KEY", "PUBLIC KEY");
        }

        RSA rsa = RSA.Create();
        rsa.ImportFromPem(pem.ToCharArray());

        return rsa;

    }

}
