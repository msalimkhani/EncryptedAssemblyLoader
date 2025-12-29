using EncryptedAssemblyLoader.Interfaces;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace EncryptedAssemblyLoader.Implementation
{
    public class AssemblyEncryptor : BaseClass<IAssemblyEncryptor, AssemblyEncryptor>, IAssemblyEncryptor
    {
        public string? DllFilePath { get; set; }
        public byte[]? AssemblyData { get; set; }
        public Assembly? Assembly { get; set; }
        public Stream? AssemblyStream { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _aes.Dispose();
        }

        public TStream Encrypt<TStream>(TStream outputStream) where TStream : Stream
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
    }
}
