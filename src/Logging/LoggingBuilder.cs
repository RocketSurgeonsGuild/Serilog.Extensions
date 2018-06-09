﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Builders;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Conventions.Reflection;
using Rocket.Surgery.Conventions.Scanners;
using Rocket.Surgery.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Logging
{
    /// <summary>
    /// Logging Builder
    /// </summary>
    public class LoggingBuilder : ConventionBuilder<ILoggingBuilder, ILoggingConvention, LoggingConventionDelegate>, ILoggingBuilder, ILoggingConventionContext
    {
        public LoggingBuilder(
            IConventionScanner scanner,
            IAssemblyProvider assemblyProvider,
            IAssemblyCandidateFinder assemblyCandidateFinder,
            IServiceCollection services,
            IHostingEnvironment envionment,
            IConfiguration configuration,
            ILogger logger,
            IDictionary<object, object> properties) : base(scanner, assemblyProvider, assemblyCandidateFinder, properties)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Environment = envionment ?? throw new ArgumentNullException(nameof(envionment));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override ILoggingBuilder GetBuilder() => this;

        public IServiceCollection Services { get; }
        public IHostingEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public ILogger Logger { get; }

        public void Build()
        {
            new ConventionComposer(Scanner).Register(
                this,
                typeof(ILoggingConvention),
                typeof(LoggingConventionDelegate)
            );
        }
    }
}
