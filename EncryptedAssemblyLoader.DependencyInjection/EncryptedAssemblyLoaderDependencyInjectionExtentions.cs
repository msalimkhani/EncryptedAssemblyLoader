using EncryptedAssemblyLoader.Implementation;
using EncryptedAssemblyLoader.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EncryptedAssemblyLoader.DependencyInjection
{
    public static class EncryptedAssemblyLoaderDependencyInjectionExtentions
    {
        public static IServiceCollection AddEncryptedAssemblyLoaderServicesScoped(this IServiceCollection services)
        {
            services.AddScoped<IAssemblyDecryptor, AssemblyDecryptor>();
            services.AddScoped<IAssemblyEncryptor, AssemblyEncryptor>();
            services.AddScoped<IAssemblyLoader, AssemblyLoader>();
            return services;
        }

        public static IServiceCollection AddEncryptedAssemblyLoaderServicesTransient(this IServiceCollection services)
        {
            services.AddTransient<IAssemblyDecryptor, AssemblyDecryptor>();
            services.AddTransient<IAssemblyEncryptor, AssemblyEncryptor>();
            services.AddTransient<IAssemblyLoader, AssemblyLoader>();
            return services;
        }

        public static IServiceCollection AddEncryptedAssemblyLoaderServicesSingleton(this IServiceCollection services)
        {
            services.AddSingleton<IAssemblyDecryptor, AssemblyDecryptor>();
            services.AddSingleton<IAssemblyEncryptor, AssemblyEncryptor>();
            services.AddSingleton<IAssemblyLoader, AssemblyLoader>();
            return services;
        }
    }
}
