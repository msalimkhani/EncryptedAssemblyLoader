using EncryptedAssemblyLoader.Interfaces;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace EncryptedAssemblyLoader.Implementation
{
    public abstract class BaseClass<TInterface, TClass> : IBaseInterface<TInterface> where TClass : class, TInterface
    {
        protected readonly Aes _aes;
        protected string? _ivString;
        protected string _keyString = null!;
        protected int _keySize;

        protected BaseClass()
        {
            _aes = Aes.Create();
        }
        public virtual TInterface SetIV(string iv)
        {
            this._ivString = iv;

            return (TInterface)(IBaseInterface<TInterface>)this;
        }

        public virtual TInterface SetKey(string key, int size)
        {
            this._keyString = key;
            this._keySize = size;
            return (TInterface)(IBaseInterface<TInterface>)this;
        }

        public virtual TInterface SetPassword(string password)
        {
            byte[] key;
            using var sha = SHA256.Create();
            key = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            var keyString = Convert.ToBase64String(key);
            this._keyString = keyString;
            this._keySize = 256;

            return (TInterface)(IBaseInterface<TInterface>)this;
        }

        protected byte[]? GetAssemblyBytes(Assembly assembly)
        {
            string path = assembly.Location;

            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return File.ReadAllBytes(path);
        }
    }
}
