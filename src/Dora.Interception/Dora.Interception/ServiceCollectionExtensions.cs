﻿using Dora;
using Dora.DynamicProxy;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Define some extension methods to register interception based services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the interception based service registrations.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> in which the service registrations are added.</param>
        /// <param name="configure">A <see cref="Action{InterceptionBuilder}"/> to perform other configuration.</param>
        /// <returns>The <see cref="IServiceCollection"/> with added service registration.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="services"/> is null.</exception>
        public static IServiceCollection AddInterception(this IServiceCollection services, Action<InterceptionBuilder> configure = null)
        { 
            Guard.ArgumentNotNull(services, nameof(services));
            services.TryAddSingleton(typeof(IInterceptable<>), typeof(Interceptable<>));
            services.TryAddSingleton<IInterceptorChainBuilder, InterceptorChainBuilder>();    
            services.TryAddSingleton<IInterceptingProxyFactory, InterceptingProxyFactory>();
            services.TryAddSingleton<IInstanceDynamicProxyGenerator, InterfaceDynamicProxyGenerator>();
            services.TryAddSingleton<ITypeDynamicProxyGenerator, VirtualMethodDynamicProxyGenerator>();
            services.TryAddSingleton<IDynamicProxyFactoryCache>(new DynamicProxyFactoryCache());

            var builder = new InterceptionBuilder(services);   
            configure?.Invoke(builder);
            services.AddSingleton<IInterceptorResolver>(_=>new InterceptorResolver(_.GetRequiredService<IInterceptorChainBuilder>() , builder.InterceptorProviderResolvers));
            return services; 
        }


        /// <summary>
        /// Builders the interceptable service provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which the service provider is built based on.</param>
        /// <param name="configure">A <see cref="Action{InterceptionBuilder}"/> to perform other configuration.</param>
        /// <returns>The interceptable service provider.</returns>  
        /// <exception cref="ArgumentNullException">Specified <paramref name="services"/> is null.</exception>
        public static IServiceProvider BuildInterceptableServiceProvider(this IServiceCollection services, Action<InterceptionBuilder> configure = null)
        {
            return BuildInterceptableServiceProvider(services, false, configure);
        }

        //public static IServiceProvider BuildInterceptableServiceProvider(this IServiceCollection services, string policyFilePath, Assembly[] references, string[] imports)
        //{
        //    return services.BuildInterceptableServiceProvider(_ => _.AddPolicy(policyFilePath, references, imports));
        //} 


        /// <summary>
        /// Builders the interceptable service provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> which the service provider is built based on.</param>
        /// <param name="validateScopes">if set to <c>true</c> [validate scopes].</param>
        /// <param name="configure">The configure.</param>
        /// <returns>The interceptable service provider.</returns>    
        /// <exception cref="ArgumentNullException">Specified <paramref name="services"/> is null.</exception>
        public static IServiceProvider BuildInterceptableServiceProvider(this IServiceCollection services, bool validateScopes, Action<InterceptionBuilder> configure = null)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            var options = new ServiceProviderOptions { ValidateScopes = validateScopes };
            services.AddInterception(configure);
            var proxyFactory = services.BuildServiceProvider().GetRequiredService<IInterceptingProxyFactory>();
            return new InterceptableServiceProvider(services, options , proxyFactory);
        }
    }
}
