using System;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;

namespace Crypto {
	public class DiffieHellman {
		//vars
		private DHParameters dhParams;
		private AsymmetricCipherKeyPair keys;
		private IBasicAgreement agree;

		//constructor
		public DiffieHellman() {
			dhParams = new DHParameters(new BigInteger("FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DDEF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7EDEE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3DC2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F83655D23DCA3AD961C62F356208552BB9ED529077096966D670C354E4ABC9804F1746C08CA18217C32905E462E36CE3BE39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9DE2BCBF6955817183995497CEA956AE515D2261898FA051015728E5A8AACAA68FFFFFFFFFFFFFFFF", 16), new BigInteger("2"));
			
			IAsymmetricCipherKeyPairGenerator gen = GeneratorUtilities.GetKeyPairGenerator("DH");
			DHKeyGenerationParameters keyParams = new DHKeyGenerationParameters(new SecureRandom(), dhParams);
			gen.Init(keyParams);
			keys = gen.GenerateKeyPair();

			agree = AgreementUtilities.GetBasicAgreement("DH");
			agree.Init(keys.Private);
		}

		//public
		public byte[] g {
			get {
				return dhParams.G.ToByteArray();
			}
		}
		public byte[] p {
			get {
				return dhParams.P.ToByteArray();
			}
		}

		public byte[] ab {
			get {
				return (keys.Private as DHPrivateKeyParameters).X.ToByteArray();
			}
		}
		public byte[] AB {
			get {
				return (keys.Public as DHPublicKeyParameters).Y.ToByteArray();
			}
		}

		public byte[] S(byte[] AB) {
			DHPublicKeyParameters key = new DHPublicKeyParameters(new BigInteger(AB), dhParams);
			return agree.CalculateAgreement(key).ToByteArray();
		}

		//private

	}
}