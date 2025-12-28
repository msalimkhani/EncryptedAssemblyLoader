using EncryptedAssemblyLoader.Interfaces;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace EncryptedAssemblyLoader.Implementation
{
    public class AssemblyEncryptor : IAssemblyEncryptor
    {

        private Aes _aes;
        private string? _ivString;
        private string _keyString = null!;
        private int _keySize;
        public string? DllFilePath { get; set; }
        public byte[]? AssemblyData { get; set; }
        public Assembly? Assembly { get; set; }
        public Stream? AssemblyStream { get; set; }

        public AssemblyEncryptor()
        {
            this._aes = Aes.Create();
        }

        private byte[]? GetAssemblyBytes(Assembly assembly)
        {
            string path = assembly.Location;

            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return File.ReadAllBytes(path);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _aes.Dispose();
        }

        public Stream Encrypt(Stream outputStream)
        {

            if (this.DllFilePath is not null)
            {
                using var dllFile = File.OpenRead(this.DllFilePath);
                using var streamReader = new StreamReader(dllFile);
                var assemblyData = new byte[dllFile.Length];
                streamReader.BaseStream.Read(assemblyData, 0, assemblyData.Length);

                inner(assemblyData);
            }

            if (this.AssemblyData is not null)
            {
                inner(this.AssemblyData);
            }

            if (this.Assembly is not null &&
                !string.IsNullOrEmpty(this.Assembly.Location))
            {
                var assemblyData = GetAssemblyBytes(this.Assembly);
                if (assemblyData is not null)
                {
                    inner(assemblyData);
                }
            }

            void inner(byte[] assemblyData)
            {
                this._aes.KeySize = this._keySize;
                _aes.Key = Convert.FromBase64String(_keyString);
                if (this._ivString is not null)
                    this._aes.IV = Convert.FromBase64String(this._ivString);
                else
                    this._aes.GenerateIV();
                using var encryptor = this._aes.CreateEncryptor(this._aes.Key, this._aes.IV);
                using var memoryStream = new MemoryStream(assemblyData);
                using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Read);
                using var encryptedStream = new MemoryStream();
                cryptoStream.CopyTo(encryptedStream);
                encryptedStream.Position = 0;
                encryptedStream.CopyTo(outputStream);
            }

            return outputStream;
        }

        public IAssemblyEncryptor SetIV(string iv)
        {
            this._ivString = iv;

            return this;
        }

        public IAssemblyEncryptor SetKey(string key, int size)
        {
            this._keyString = key;
            this._keySize = size;
            return this;
        }

        public IAssemblyEncryptor SetPassword(string password)
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
