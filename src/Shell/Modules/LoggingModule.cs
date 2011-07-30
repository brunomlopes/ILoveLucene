using System;
using System.Linq;
using Autofac.Core;
using Caliburn.Micro;
using NLog;
using CoreILog = Core.Abstractions.ILog;

namespace ILoveLucene.Modules
{
    public class LoggingModule : IModule
    {
        private readonly Func<Type, CoreILog> _logBuilder;
        private readonly Func<Type, Logger> _nlogBuilder;

        public LoggingModule(Func<Type, CoreILog> logBuilder, Func<Type, Logger> nlogBuilder)
        {
            _logBuilder = logBuilder;
            _nlogBuilder = nlogBuilder;
        }

        public void Configure(IComponentRegistry componentRegistry)
        {
            componentRegistry.Registered +=
                (sender, e) => e.ComponentRegistration.Preparing += ComponentRegistration_Preparing;
        }

        private void ComponentRegistration_Preparing(object sender, PreparingEventArgs e)
        {
            Type t = e.Component.Target.Activator.LimitType;
            e.Parameters = e.Parameters.Union(new[]
                                                  {
                                                      new ResolvedParameter((p, i) => p.ParameterType ==
                                                                                      typeof (ILog),
                                                                            (p, i) => _logBuilder(t)),
                                                      new ResolvedParameter((p, i) => p.ParameterType ==
                                                                                      typeof (CoreILog),
                                                                            (p, i) => _logBuilder(t)),
                                                      new ResolvedParameter((p, i) => p.ParameterType ==
                                                                                      typeof (Logger),
                                                                            (p, i) => _nlogBuilder(t))
                                                  });
        }
    }
}