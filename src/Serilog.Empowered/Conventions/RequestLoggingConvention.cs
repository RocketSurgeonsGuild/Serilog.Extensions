﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rocket.Surgery.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.Serilog.Empowered;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rocket.Surgery.Extensions.Serilog.Conventions
{
    class RequestLoggingConvention : IServiceConvention
    {
        public void Register(IServiceConventionContext context)
        {
            context.Services.AddTransient<IDiagnosticListener, HostingDiagnosticListener>();
            context.OnBuild.Subscribe(new ServiceProviderObserver());
        }

        class DiagnosticListenerObserver : IObserver<DiagnosticListener>, IDisposable
        {
            private readonly List<IDisposable> _subscriptions;
            private readonly IEnumerable<IDiagnosticListener> _diagnosticListeners;

            /// <summary>
            /// Initializes a new instance of the <see cref="DiagnosticListenerObserver"/> class.
            /// </summary>
            public DiagnosticListenerObserver(
                IEnumerable<IDiagnosticListener> diagnosticListeners)
            {
                _diagnosticListeners = diagnosticListeners;
                _subscriptions = new List<IDisposable>();
            }

            /// <inheritdoc />
            void IObserver<DiagnosticListener>.OnNext(DiagnosticListener value)
            {
                foreach (var applicationInsightDiagnosticListener in _diagnosticListeners)
                {
                    if (applicationInsightDiagnosticListener.ListenerName == value.Name)
                    {
                        _subscriptions.Add(value.SubscribeWithAdapter(applicationInsightDiagnosticListener));
                    }
                }
            }

            /// <inheritdoc />
            void IObserver<DiagnosticListener>.OnError(Exception error)
            {
            }

            /// <inheritdoc />
            void IObserver<DiagnosticListener>.OnCompleted()
            {
            }

            /// <inheritdoc />
            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    foreach (var subscription in _subscriptions)
                    {
                        subscription.Dispose();
                    }
                }
            }
        }

        class ServiceProviderObserver : IObserver<IServiceProvider>
        {
            void IObserver<IServiceProvider>.OnCompleted()
            {
            }

            void IObserver<IServiceProvider>.OnError(Exception error)
            {
            }

            void IObserver<IServiceProvider>.OnNext(IServiceProvider value)
            {
                var disposable = DiagnosticListener.AllListeners.Subscribe(
                    new DiagnosticListenerObserver(
                        value.GetRequiredService<IEnumerable<IDiagnosticListener>>()));

                value.GetRequiredService<IApplicationLifetime>().ApplicationStopped.Register(() => disposable.Dispose());
            }
        }
    }
}
