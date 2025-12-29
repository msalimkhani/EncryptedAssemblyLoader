using EncryptedAssemblyLoader.Interfaces;
using System.Reflection;

namespace EncryptedAssemblyLoader.Implementation
{
    public class AssemblyLoader(IAssemblyDecryptor assemblyDecryptor) : BaseClass<IAssemblyLoader, AssemblyLoader>, IAssemblyLoader
    {
        private byte[]? _assemblyData;

        private Stream? _assemblyStream;

        private string? _assemblyPath;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this._aes.Dispose();
        }

        public Assembly? Load()
        {
            byte[] bytes = null!;

            if(_assemblyData is not null) bytes = _assemblyData;

            if(_assemblyStream is not null)
            {
                _assemblyStream.Position = 0;
                using var ms = new MemoryStream();
                _assemblyStream.Position = 0;
                _assemblyStream.CopyTo(ms);
                bytes = ms.ToArray();
            }

            if(_assemblyPath is not null)
            {
                using var fs = File.OpenRead(_assemblyPath);
                bytes = new byte[fs.Length];
                using var ms = new MemoryStream();
                fs.CopyTo(ms);
                bytes = ms.ToArray();
            }

            assemblyDecryptor.EncryptedAssemblyData = bytes;
            using var assemblyStream = assemblyDecryptor.Decrypt(new MemoryStream());
            assemblyStream.Position = 0;
            var decryptedAssemblyData = assemblyStream.ToArray();
            var asm = Assembly.Load(decryptedAssemblyData);
            return asm;
        }

        public IAssemblyLoader SetEncryptedAssemblyData(byte[] data)
        {
            if(this._assemblyPath is not null ||
                this._assemblyStream is not null)
            {
                throw new InvalidOperationException("Only One Data Set Method Allowed");
            }
            this._assemblyData = data;

            return this;
        }

        public IAssemblyLoader SetEncryptedAssemblyPath(string path)
        {
            if (this._assemblyData is not null ||
                this._assemblyStream is not null)
            {
                throw new InvalidOperationException("Only One Data Set Method Allowed");
            }
            _assemblyPath = path;

            return this;
        }

        public IAssemblyLoader SetEncryptedAssemblyStream(Stream stream)
        {
            if (this._assemblyPath is not null ||
                this._assemblyData is not null)
            {
                throw new InvalidOperationException("Only One Data Set Method Allowed");
            }
            _assemblyStream = stream;

            return this;
        }

        public override IAssemblyLoader SetPassword(string password)
        {
            assemblyDecryptor.SetPassword(password);
            return this;
        }

        public override IAssemblyLoader SetIV(string iv)
        {
            assemblyDecryptor.SetIV(iv);
            return this;
        }

        public override IAssemblyLoader SetKey(string key, int size)
        {
            assemblyDecryptor.SetKey(key, size);
            return this;
        }
    }
}
