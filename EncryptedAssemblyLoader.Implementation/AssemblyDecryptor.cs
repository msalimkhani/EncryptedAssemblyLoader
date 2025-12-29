using EncryptedAssemblyLoader.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace EncryptedAssemblyLoader.Implementation
{
    public class AssemblyDecryptor : BaseClass<IAssemblyDecryptor, AssemblyDecryptor>, IAssemblyDecryptor
    {

        public string? EncryptedDllFilePath { get; set; }
        public byte[]? EncryptedAssemblyData { get; set; }
        public Stream? EncryptedAssemblyStream { get; set; }

        public TStream Decrypt<TStream>(TStream outputStream) where TStream : Stream
        {

            byte[] assemblyData = null!;

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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this._aes.Dispose();
        }
    }
}
