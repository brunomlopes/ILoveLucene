using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Autofac.Core;

namespace ILoveLucene.Modules
{
    public class SatisfyMefImports : IModule
    {
        private readonly CompositionContainer _mefContainer;

        public SatisfyMefImports(CompositionContainer mefContainer)
        {
            _mefContainer = mefContainer;
        }

        public void Configure(IComponentRegistry componentRegistry)
        {
            componentRegistry.Registered +=
                (_sender, e) => e.ComponentRegistration.Activated += OnComponentRegistrationOnActivated;
        }

        private void OnComponentRegistrationOnActivated(object sender, ActivatedEventArgs<object> activation_event)
        {
            _mefContainer.SatisfyImportsOnce(activation_event.Instance);
        }
    }
}