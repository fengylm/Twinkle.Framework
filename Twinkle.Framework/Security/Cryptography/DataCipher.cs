using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Twinkle.Framework.Security.Cryptography
{
    /// <summary>
    /// 提供数据加密
    /// </summary>
    public static class DataCipher
    {
        #region RSA加密
        private static string PrivateKey = "MIIEpQIBAAKCAQEAyw1Y6MG4nEVQ4ZaqIxwRwSLNZHltGGceEFW1LPe7exekSqXPdZP6dXUUEUydmpMVdLKGq6aVkamSu31JSbqwSjG38YR8EaCU4+Oq3pAIlxbRyiqYgnlj7gAFw/Pjp/w5VgwF81+okYPahCg7v3WECoRUpFLrdHv9puLgKEigPppsp+wn72pTE0+3qHTeJAzWQj4CvhX7M/U6y51ZMv37VBJiwOkQ16f1RXZDvnB3yjskFfQetqgwP5M+m6GhHpq+dRtG+QrVoGvzorWpkq/VCQgMgnoKI2G1aeVwax+rYpllRyJko/+6P8AkBk4HHuMreGC+SG28nGCyN7hPGVEbNwIDAQABAoIBAQCHYe7VwdQE3XJ/9qSZpC1ySHIJe6xxiM9StNKHmOU3vRudadBY5MEpb1Zh8mNinI7BsAZ6jCdNZ3Kd73rd3cuMrHmoXl0ao6aiCznyCravhld6I8xrseQe24T8kbBIYLMZ3ApbqnwKCY+4bdroXMIdAP4uUdsLX2BP1RT/xuyQz9UdLEi6ImUJAN4ZSgdPHD+p3UPQ6pspiJqunZu2lXxRjsygs0ykQNVyh8X9ushttTZrFA77HSWxzpGyX+0ukJET7VX6sob1u0rFvvETyHGT3mX9V8fnHez57DvDKPi4vtwS4Sod5+ETd0cgv5vSdVHZtxR75DZ6+Cw+OepXh02BAoGBAO91gMWcUyMmSjEecl3yBa4fTWUBLDaYg4mQPMXwBtP2fLcmiPi/YEJD+A+kcem9P80GxlBjSskoXw4DqO+f6E+IZDPi/NeOinxT99GiEvVA1lSDp+N6iS8RVgPg1Sc9e1qQf0k6n4akKDb/lI1YjCFbpYRbDHyxT0HKnG7+thbbAoGBANkUCjyCpQS9Rindq7ICw0wYpYuQNYgfH+12qkl2+7reaPe4v0Ebq2FW/7NFxAakeiToT04wnEvpnzf+Uw9WBxTQylOf/OweDbUrQGGgORrb4SO516aegBr4JJ9ZnO+/Ldr9hQjbWaVXBYm1yeZn2EecQdhr+D+kTau5qD7denXVAoGAFx8JaAtIG8S+tS9za47K7Z0oI/CRDFR9nvLGa3ZZsm3CbQzTfPE9oihd82S1exRC7NESpQBxYCr9wqSn0ztlewh4ZGoub8HbrVWOQDeeDQBNsatksppKVLkfzRAQhNFy77O0FqYKcT24hFb5SQybuzzWJomEEyYruPaPVEhT3LECgYEAwOe1HQbAHFESy9uIW31nDfWND1QMrjVyivR76SMKGorQ3urXUsWC69KDEk26IGkDfk7PQt2h1zf53KluKF+7lTzhv6SZ/z3J0qhn4xmeBYdGMBhTUmbh0pWW/eCFvOu4lTXbhf5fULXfXvwkOlMn+KMcRHXWOsAIocUvhQYOcC0CgYEApwKBQBAJOnwwqoal3ZlkgD3aY9PHcQFxO+N9+d2GFc4xKNomSACP04NkBHU0tj77cHswcv6fUUYM/OpBg8N1/3fYSQIulQZbyPvLCmnSQtS0CSF+jn6yXx5s6KsXrtitguekJr6tL8HjxhJwRW26AOLVwDBeyQySPD8vzrxK9qs=";
        private static string PublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAyw1Y6MG4nEVQ4ZaqIxwRwSLNZHltGGceEFW1LPe7exekSqXPdZP6dXUUEUydmpMVdLKGq6aVkamSu31JSbqwSjG38YR8EaCU4+Oq3pAIlxbRyiqYgnlj7gAFw/Pjp/w5VgwF81+okYPahCg7v3WECoRUpFLrdHv9puLgKEigPppsp+wn72pTE0+3qHTeJAzWQj4CvhX7M/U6y51ZMv37VBJiwOkQ16f1RXZDvnB3yjskFfQetqgwP5M+m6GhHpq+dRtG+QrVoGvzorWpkq/VCQgMgnoKI2G1aeVwax+rYpllRyJko/+6P8AkBk4HHuMreGC+SG28nGCyN7hPGVEbNwIDAQAB";


        #region 非对称加密
        /// <summary>
        /// 非对称加密
        /// </summary>
        /// <param name="message">要加密的字符串</param>
        /// <param name="publicKey">自定义加密公钥</param>
        /// <returns></returns>
        public static string RSAEncrypt(string message, string publicKey = null)
        {
            RSA rsa = CreateRsaFromPublicKey(publicKey ?? PublicKey);
            var plainTextBytes = Encoding.UTF8.GetBytes(message);
            var cipherBytes = rsa.Encrypt(plainTextBytes, RSAEncryptionPadding.Pkcs1);
            return Convert.ToBase64String(cipherBytes);
        }

        #endregion

        #region 非对称解密
        /// <summary>
        /// 非对称解密
        /// </summary>
        /// <param name="encryptMessage">要解密的字符串</param>
        /// <param name="privateKey">自定义解密私钥</param>
        /// <returns></returns>
        public static string RSADecrypt(string encryptMessage, string privateKey = null)
        {
            RSA rsa = CreateRsaFromPrivateKey(privateKey ?? PrivateKey);
            byte[] cipherBytes = System.Convert.FromBase64String(encryptMessage);
            byte[] plainTextBytes = rsa.Decrypt(cipherBytes, RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(plainTextBytes);
        }
        #endregion

        #region 签名
        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="data">要签名的内容</param>
        /// <param name="privateKey">自定义签名私钥</param>
        /// <returns></returns>
        public static string Sign(string data, string privateKey = null)
        {
            RSA rsa = CreateRsaFromPrivateKey(privateKey ?? PrivateKey);

            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            byte[] signatureBytes = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signatureBytes);
        }
        #endregion

        #region 验证签名
        /// <summary>
        /// 验证签名
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="sign">签名后的数据</param>
        /// <param name="publicKey">自定义验签公钥</param>
        /// <returns></returns>
        public static bool Verify(string data, string sign, string publicKey = null)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] signBytes = Convert.FromBase64String(sign);

            RSA rsa = CreateRsaFromPublicKey(publicKey ?? PublicKey);

            var verify = rsa.VerifyData(dataBytes, signBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return verify;
        }
        #endregion

        #region 算法实现
        private static RSA CreateRsaFromPrivateKey(string privateKey)
        {
            var privateKeyBits = System.Convert.FromBase64String(privateKey);
            var rsa = RSA.Create();
            var RSAparams = new RSAParameters();

            using (var binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                {
                    binr.ReadByte();
                }
                else if (twobytes == 0x8230)
                {
                    binr.ReadInt16();
                }
                else
                {
                    throw new Exception("Unexpected value read binr.ReadUInt16()");
                }

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                {
                    throw new Exception("Unexpected version");
                }

                bt = binr.ReadByte();
                if (bt != 0x00)
                {
                    throw new Exception("Unexpected value read binr.ReadByte()");
                }

                RSAparams.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.D = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.P = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Q = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DP = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DQ = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }

            rsa.ImportParameters(RSAparams);
            return rsa;
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
            {
                return 0;
            }

            bt = binr.ReadByte();

            if (bt == 0x81)
            {
                count = binr.ReadByte();
            }
            else
                if (bt == 0x82)
            {
                highbyte = binr.ReadByte();
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }

        private static RSA CreateRsaFromPublicKey(string publicKeyString)
        {
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] x509key;
            byte[] seq = new byte[15];
            int x509size;

            x509key = Convert.FromBase64String(publicKeyString);
            x509size = x509key.Length;

            using (var mem = new MemoryStream(x509key))
            {
                using (var binr = new BinaryReader(mem))
                {
                    byte bt = 0;
                    ushort twobytes = 0;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)
                    {
                        binr.ReadByte();
                    }
                    else if (twobytes == 0x8230)
                    {
                        binr.ReadInt16();
                    }
                    else
                    {
                        return null;
                    }

                    seq = binr.ReadBytes(15);
                    if (!CompareBytearrays(seq, SeqOID))
                    {
                        return null;
                    }

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103)
                    {
                        binr.ReadByte();
                    }
                    else if (twobytes == 0x8203)
                    {
                        binr.ReadInt16();
                    }
                    else
                    {
                        return null;
                    }

                    bt = binr.ReadByte();
                    if (bt != 0x00)
                    {
                        return null;
                    }

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)
                    {
                        binr.ReadByte();
                    }
                    else if (twobytes == 0x8230)
                    {
                        binr.ReadInt16();
                    }
                    else
                    {
                        return null;
                    }

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102)
                    {
                        lowbyte = binr.ReadByte();
                    }
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte();
                        lowbyte = binr.ReadByte();
                    }
                    else
                    {
                        return null;
                    }

                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    int modsize = BitConverter.ToInt32(modint, 0);

                    int firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {
                        binr.ReadByte();
                        modsize -= 1;
                    }

                    byte[] modulus = binr.ReadBytes(modsize);

                    if (binr.ReadByte() != 0x02)
                    {
                        return null;
                    }

                    int expbytes = binr.ReadByte();
                    byte[] exponent = binr.ReadBytes(expbytes);

                    var rsa = RSA.Create();
                    var rsaKeyInfo = new RSAParameters
                    {
                        Modulus = modulus,
                        Exponent = exponent
                    };
                    rsa.ImportParameters(rsaKeyInfo);
                    return rsa;
                }

            }
        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                {
                    return false;
                }

                i++;
            }
            return true;
        }
        #endregion
        #endregion

        #region MD5加密
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="orginStr">要加密的字符串</param>
        /// <returns></returns>
        public static string MD5Encrypt(string orginStr)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(orginStr));
                var strResult = BitConverter.ToString(result);
                return strResult.Replace("-", "");
            }
        }
        #endregion
    }
}
