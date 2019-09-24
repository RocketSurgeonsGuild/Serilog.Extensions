using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Conventions.Scanners;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using System.Diagnostics;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.Logging;
using Rocket.Surgery.Extensions.Serilog.Conventions;
using Serilog.Core;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Hosting;

[assembly: Convention(typeof(SerilogHostingConvention))]

namespace Rocket.Surgery.Extensions.Serilog.Conventions
{
    /// <summary>
    ///  SerilogHostingConvention.
    /// Implements the <see cref="IHostingConvention" />
    /// </summary>
    /// <seealso cref="IHostingConvention" />
    public class SerilogHostingConvention : IHostingConvention
    {
        private readonly IConventionScanner _scanner;
        private readonly ILogger _diagnosticSource;
        private readonly RocketSerilogOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerilogHostingConvention"/> class.
        /// </summary>
        /// <param name="scanner">The scanner.</param>
        /// <param name="diagnosticSource">The diagnostic source.</param>
        /// <param name="options">The options.</param>
        public SerilogHostingConvention(
            IConventionScanner scanner,
            ILogger diagnosticSource,
            RocketSerilogOptions? options = null)
        {
            _scanner = scanner;
            _diagnosticSource = diagnosticSource;
            _options = options ?? new RocketSerilogOptions();
        }

        /// <inheritdoc />
        public void Register(IHostingConventionContext context)
        {
            context.Scanner.ExceptConvention(typeof(SerilogExtensionsConvention));
            context.Builder.ConfigureServices((context, services) => new LoggingBuilder(services).ClearProviders());
            context.Builder.UseSerilog((ctx, loggerConfiguration) =>
            {
                new SerilogBuilder(
                    _scanner,
                    context.AssemblyProvider,
                    context.AssemblyCandidateFinder,
                    new RocketEnvironment(ctx.HostingEnvironment),
                    ctx.Configuration,
                    loggerConfiguration,
                    _diagnosticSource,
                    context.Properties
                ).Configure();
            },
                preserveStaticLogger: _options.PreserveStaticLogger,
                writeToProviders: _options.WriteToProviders
            );
        }

        class LoggingBuilder : Microsoft.Extensions.Logging.ILoggingBuilder
        {
            public LoggingBuilder(IServiceCollection services)
            {
                Services = services;
            }

            public IServiceCollection Services { get; }
        }
    }
}