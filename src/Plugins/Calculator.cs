using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Core.Abstractions;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Interpreter;

namespace Plugins
{
    [Export(typeof(IActOnItem))]
    public class Calculator : BaseActOnTypedItem<string>, ICanActOnTypedItem<string>, INotifyPropertyChanged
    {
        private double? _lastResult;
        private ScriptEngine _engine;
        private ScriptScope _scope;
        private ScriptRuntime _runtime;

        public Calculator()
        {
            _engine = Python.CreateEngine();

            _scope = _engine.CreateScope();
        }

        public override void ActOn(ITypedItem<string> item)
        {
            CanActOn(item);
            // TODO: is this the best way?
        }

        public override string Text
        {
            get { return _lastResult != null ? _lastResult.ToString() : "Calculator"; }
        }

        public bool CanActOn(ITypedItem<string> item)
        {
            try
            {
                _lastResult = Calculate(item.Item);
                if(PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                return _lastResult != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private double? Calculate(string expression)
        {
            ScriptSource source =
                _engine.CreateScriptSourceFromString(expression,
                                                     SourceCodeKind.Expression);
            object result = source.Execute(_scope);
            return double.Parse(result.ToString());
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}