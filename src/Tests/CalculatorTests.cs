using Core.Abstractions;
using Plugins;
using Xunit;

namespace Tests
{
    public class CalculatorTests
    {
        [Fact]
        public void CanCalculateSimpleExpressions()
        {
            var action = new Calculator();
            action.ActOn(new TextItem("21 + 21"));
            Assert.Equal("42", action.Text);
        }
        
        [Fact]
        public void CanCalculatePowerOf2Expressions()
        {
            var action = new Calculator();
            action.ActOn(new TextItem("pow(10,2)"));
            Assert.Equal("100", action.Text);
        }
    }
}