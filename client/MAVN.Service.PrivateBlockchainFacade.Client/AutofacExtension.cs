using Autofac;
using JetBrains.Annotations;
using Lykke.HttpClientGenerator;
using Lykke.HttpClientGenerator.Infrastructure;
using System;

namespace MAVN.Service.PrivateBlockchainFacade.Client
{
    /// <summary>
    /// Extension for client registration
    /// </summary>
    [PublicAPI]
    public static class AutofacExtension
    {
        /// <summary>
        /// Registers <see cref="IPrivateBlockchainFacadeClient"/> in Autofac container using <see cref="PrivateBlockchainFacadeServiceClientSettings"/>.
        /// </summary>
        /// <param name="builder">Autofac container builder.</param>
        /// <param name="settings">PrivateBlockchainFacade client settings.</param>
        /// <param name="builderConfigure">Optional <see cref="HttpClientGeneratorBuilder"/> configure handler.</param>
        public static void RegisterPrivateBlockchainFacadeClientWithApiKey(
            [NotNull] this ContainerBuilder builder,
            [NotNull] PrivateBlockchainFacadeServiceClientSettings settings,
            [CanBeNull] Func<HttpClientGeneratorBuilder, HttpClientGeneratorBuilder> builderConfigure)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.ServiceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(PrivateBlockchainFacadeServiceClientSettings.ServiceUrl));

            var clientBuilder = HttpClientGenerator
                .BuildForUrl(settings.ServiceUrl)
                .WithApiKey(settings.ApiKey)
                .WithAdditionalCallsWrapper(new ExceptionHandlerCallsWrapper());

            clientBuilder = builderConfigure?.Invoke(clientBuilder) ?? clientBuilder.WithoutRetries();

            builder.RegisterInstance(new PrivateBlockchainFacadeClient(clientBuilder.Create()))
                .As<IPrivateBlockchainFacadeClient>()
                .SingleInstance();
        }

        /// <summary>
        /// Registers <see cref="IPrivateBlockchainFacadeClient"/> in Autofac container using <see cref="PrivateBlockchainFacadeServiceClientSettings"/>.
        /// </summary>
        /// <param name="builder">Autofac container builder.</param>
        /// <param name="settings">PrivateBlockchainFacade client settings.</param>
        /// <param name="builderConfigure">Optional <see cref="HttpClientGeneratorBuilder"/> configure handler.</param>
        public static void RegisterPrivateBlockchainFacadeClient(
            [NotNull] this ContainerBuilder builder,
            [NotNull] PrivateBlockchainFacadeServiceClientSettings settings,
            [CanBeNull] Func<HttpClientGeneratorBuilder, HttpClientGeneratorBuilder> builderConfigure)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.ServiceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(PrivateBlockchainFacadeServiceClientSettings.ServiceUrl));

            var clientBuilder = HttpClientGenerator
                .BuildForUrl(settings.ServiceUrl)
                .WithAdditionalCallsWrapper(new ExceptionHandlerCallsWrapper());

            clientBuilder = builderConfigure?.Invoke(clientBuilder) ?? clientBuilder.WithoutRetries();

            builder.RegisterInstance(new PrivateBlockchainFacadeClient(clientBuilder.Create()))
                .As<IPrivateBlockchainFacadeClient>()
                .SingleInstance();
        }
    }
}
