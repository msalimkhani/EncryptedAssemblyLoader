using EncryptedAssemblyLoader.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace EncryptedAssemblyLoader.Implementation
{
    public class AssemblyDecryptor : IAssemblyDecryptor
    {
        private Aes _aes;
        private string? _ivString;
        private string _keyString = null!;
        private int _keySize;

        public AssemblyDecryptor()
        {
            this._aes = Aes.Create();
        }

        public string? EncryptedDllFilePath { get; set; }
        public byte[]? EncryptedAssemblyData { get; set; }
        public Stream? EncryptedAssemblyStream { get; set; }

        public Stream Decrypt(Stream outputStream)
        {

            byte[] assemblyData = null;

            if(this.EncryptedDllFilePath is not null)
            {
                using var dllFile = File.OpenRead(this.EncryptedDllFilePath);
                using var streamReader = new StreamReader(dllFile);
                assemblyData = new byte[dllFile.Length];
                streamReader.BaseStream.Read(assemblyData, 0, assemblyData.Length);
            }

            if (this.EncryptedAssemblyData is not null)
            {
                assemblyData = this.EncryptedAssemblyData;
            }

            if (this.EncryptedAssemblyStream is not null)
            {
                using var stream = new MemoryStream();
                this.EncryptedAssemblyStream.CopyTo(stream);
                assemblyData = stream.ToArray();
            }

            if (assemblyData is null)
            {
                throw new InvalidOperationException("No assembly data provided for decryption.");
            }

            this._aes.KeySize = this._keySize;
            _aes.Key = Convert.FromBase64String(_keyString);
            if (this._ivString is not null)
                this._aes.IV = Convert.FromBase64String(this._ivString);
            else
                this._aes.GenerateIV();
            using var decryptor = this._aes.CreateDecryptor(this._aes.Key, this._aes.IV);
            using var memoryStream = new MemoryStream(assemblyData);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var decryptedStream = new MemoryStream();
            cryptoStream.CopyTo(decryptedStream);
            decryptedStream.Position = 0;
            decryptedStream.CopyTo(outputStream);

            return outputStream;
        }

        public IAssemblyDecryptor SetIV(string iv)
        {
            this._ivString = iv;

            return this;
        }

        public IAssemblyDecryptor SetKey(string key, int size)
        {
            this._keyString = key;
            this._keySize = size;
            return this;
        }

        public IAssemblyDecryptor SetPassword(string password)
        {
            byte[] key;
            using var sha = SHA256.Create();
            key = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            var keyString = Convert.ToBase64String(key);
            this._keyString = keyString;
            this._keySize = 256;

            return this;
        }
    }
}
