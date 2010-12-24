using System;
using System.Linq;
using Autofac.Core;
using Caliburn.Micro;
using CaliburnILog = Caliburn.Micro.ILog;
using CoreILog = Core.Abstractions.ILog;

namespace ILoveLucene.Modules
{
    public class LoggingModule : IModule
    {
        private readonly Func<Type, ILog> _logBuilder;

        public LoggingModule(Func<Type, ILog> logBuilder)
        {
            _logBuilder = logBuilder;
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
                                                                                      typeof (CaliburnILog),
                                                                            (p, i) => _logBuilder(t)),
                                                      new ResolvedParameter((p, i) => p.ParameterType ==
                                                                                      typeof (CoreILog),
                                                                            (p, i) => _logBuilder(t))
                                                  });
        }
    }
}