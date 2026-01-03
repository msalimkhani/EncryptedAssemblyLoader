using EncryptedAssemblyLoader.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Reflection.Emit;

namespace EncryptedAssemblyLoader.Implementation
{
    public class AssemblyLoader(IAssemblyDecryptor assemblyDecryptor) : BaseClass<IAssemblyLoader, AssemblyLoader>, IAssemblyLoader
    {
        private byte[]? _assemblyData;

        private Stream? _assemblyStream;

        private string? _assemblyPath;

        private Assembly? _loadedAssembly;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this._aes.Dispose();
        }

        public Assembly? Load()
        {
            byte[] bytes = null!;

            if (_assemblyData is not null) bytes = _assemblyData;

            if (_assemblyStream is not null)
            {
                _assemblyStream.Position = 0;
                using var ms = new MemoryStream();
                _assemblyStream.Position = 0;
                _assemblyStream.CopyTo(ms);
                bytes = ms.ToArray();
            }

            if (_assemblyPath is not null)
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
            this._loadedAssembly = asm;
            return asm;
        }

        public IAssemblyLoader SetEncryptedAssemblyData(byte[] data)
        {
            if (this._assemblyPath is not null ||
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

        public Type GetType(string name)
        {
            if (_loadedAssembly is null)
                throw new InvalidOperationException("Assembly not loaded!");

            var type = _loadedAssembly.GetType(name);

            if (type == null)
                throw new InvalidOperationException("I Can't find requested type in loaded assembly");

            return type;
        }

        private static void CreatePassThroughConstructors(TypeBuilder typeBuilder, Type baseClass)
        {
            var constructors = baseClass.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            foreach (var baseCtor in constructors)
            {
                var parameters = baseCtor.GetParameters();
                var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

                var ctorBuilder = typeBuilder.DefineConstructor(
                    MethodAttributes.Public,
                    CallingConventions.Standard,
                    parameterTypes);

                ILGenerator il = ctorBuilder.GetILGenerator();

                // 1. Push 'this' onto the stack
                il.Emit(OpCodes.Ldarg_0);

                // 2. Push all arguments onto the stack to pass to base(...)
                for (int i = 1; i <= parameters.Length; i++)
                {
                    il.Emit(OpCodes.Ldarg, i);
                }

                // 3. Call the base constructor
                il.Emit(OpCodes.Call, baseCtor);
                il.Emit(OpCodes.Ret);
            }
        }

        private static Type CreateProxyType(Type baseClass, Type interfaceType)
        {
            var asmName = new AssemblyName($"DynamicProxyAssembly_{Guid.NewGuid()}");
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            var modBuilder = asmBuilder.DefineDynamicModule($"ProxyModule_{Guid.NewGuid()}");

            var typeBuilder = modBuilder.DefineType(
                $"{baseClass.Name}_Proxy_{Guid.NewGuid()}",
                TypeAttributes.Public | TypeAttributes.Class,
                baseClass);

            typeBuilder.AddInterfaceImplementation(interfaceType);

            CreatePassThroughConstructors(typeBuilder, baseClass);

            return typeBuilder.CreateType();
        }

        private static T? CreateInstance<T>(
            Type type,
            IServiceProvider? serviceProvider = null,
            params object[] parameters)
        {
            try
            {
                if (serviceProvider != null)
                {
                    // This handles BOTH parameterless and DI-heavy constructors
                    return (T)ActivatorUtilities.CreateInstance(serviceProvider, type, parameters);
                }

                // Fallback for when no DI container is available
                return (T?)Activator.CreateInstance(type, parameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create instance of {type.Name}. Ensure dependencies are registered.", ex);
            }
        }

        public T? GetClassType<T>(
            string name,
            IServiceProvider? serviceProvider = null,
            params object[] parameters)
        {
            var type = GetType(name);
            var interfaceType = typeof(T);

            if (interfaceType == null ||
                !interfaceType.IsInterface)
            {
                throw new InvalidOperationException("Target Generic type provided must be not null and interface");
            }

            if (!type.IsClass)
            {
                throw new InvalidOperationException($"I can only load Class Type");
            }

            if (type.IsAssignableTo(interfaceType))
            {
                return CreateInstance<T>(type, serviceProvider, parameters);
            }

            else
            {
                var interfaceMethods = interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                var classMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                bool allMethodsMatched = interfaceMethods.All(im =>
                    classMethods.Any(cm =>
                        cm.Name == im.Name &&
                        cm.ReturnType == im.ReturnType &&
                        cm.GetParameters().Select(p => p.ParameterType)
                          .SequenceEqual(im.GetParameters().Select(p => p.ParameterType))
                    )
                );

                var interfaceProps = interfaceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var classProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                bool propertiesMatch = interfaceProps.All(ip =>
                {
                    var cp = classProps.FirstOrDefault(c => c.Name == ip.Name && c.PropertyType == ip.PropertyType);
                    if (cp == null) return false;

                    // Ensure if interface needs a getter, class has a getter
                    if (ip.CanRead && !cp.CanRead) return false;
                    // Ensure if interface needs a setter, class has a setter
                    if (ip.CanWrite && !cp.CanWrite) return false;

                    return true;
                });

                if (!allMethodsMatched || !propertiesMatch)
                {
                    throw new InvalidOperationException($"The type {name} does not fully implement the signature of {interfaceType.Name}");
                }

                var proxy_Type = CreateProxyType(type, interfaceType);

                return CreateInstance<T>(proxy_Type, serviceProvider, parameters);
            }
        }
    }
}