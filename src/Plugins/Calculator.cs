using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Core.Abstractions;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace Plugins
{
    [Export(typeof(IActOnItem))]
    public class Calculator : BaseActOnTypedItemAndReturnTypedItem<string, string>, ICanActOnTypedItem<string>, INotifyPropertyChanged
    {
        private double? _lastResult;
        private ScriptEngine _engine;
        private ScriptScope _scope;

        public Calculator()
        {
            _engine = Python.CreateEngine();

            _scope = _engine.CreateScope();
        }

        public override ITypedItem<string> ActOn(string item)
        {
            // TODO: is this the best way to show the result?
            // my quick usability check says it is (using it for a while)
            CanActOn(item);
            return new TextItem(_lastResult.ToString(), item);
        }

        public override string Text
        {
            get { return _lastResult != null ? _lastResult.ToString() : "Calculator"; }
        }

        public bool CanActOn(string item)
        {
            if (string.IsNullOrWhiteSpace(item))
                return false;
            try
            {
                _lastResult = Calculate(item);
                if(PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Text"));
                return _lastResult != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private double? Calculate(string expression)
        {
            if (expression.StartsWith("=")) expression = expression.Substring(1);
            ScriptSource source =
                _engine.CreateScriptSourceFromString(expression,
                                                     SourceCodeKind.Expression);
            object result = source.Execute(_scope);
            return double.Parse(result.ToString());
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}