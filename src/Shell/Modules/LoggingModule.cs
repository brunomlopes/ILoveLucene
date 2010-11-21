using System;
using System.Linq;
using Autofac.Core;
using Caliburn.Micro;

namespace ILoveLucene.Modules
{
    public class LoggingModule : IModule
    {
        public void Configure(IComponentRegistry componentRegistry)
        {
            componentRegistry.Registered +=
                (sender, e) => e.ComponentRegistration.Preparing += ComponentRegistration_Preparing;
        }

        private void ComponentRegistration_Preparing(object sender, PreparingEventArgs e)
        {
            Type t = typeof (string);
            e.Parameters = e.Parameters.Union(new[]
                                                  {
                                                      new ResolvedParameter((p, i) => p.ParameterType ==
                                                                                      typeof (ILog),
                                                                            (p, i) => LogManager.GetLog(t))
                                                  });
        }
    }
}