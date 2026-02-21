using System.Security.Cryptography;

namespace Battlegrounds.Security;

public record RSAPublicKey(RSA Key, string KeyId)  {

    public static RSA FromPem(string pem) {

        if (pem.Contains("RSA PUBLIC KEY")) {
            pem = pem.Replace("RSA PUBLIC KEY", "PUBLIC KEY");
        }

        RSA rsa = RSA.Create();
        rsa.ImportFromPem(pem.ToCharArray());

        return rsa;

    }

    public static RSAPublicKey FromElements(string n, string e, string kid) {

        var rsaParameters = new RSAParameters {
            Modulus = Convert.FromBase64String(n),
            Exponent = Convert.FromBase64String(e)
        };

        RSA rsa = RSA.Create();
        rsa.ImportParameters(rsaParameters);

        return new RSAPublicKey(rsa, kid);

    } 

}
