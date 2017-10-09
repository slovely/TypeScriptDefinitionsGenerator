using System;
using System.Linq.Expressions;
using System.Reflection;
using TypeScriptDefinitionsGenerator;
using TypeScriptDefinitionsGenerator.Extensions;
using Xunit;

namespace TypeScriptGenerator.Tests
{
    public class WebApiUrlGenerationTests
    {
        private readonly WebApiUrlGenerator _generator = new WebApiUrlGenerator();
        
        [Fact]
        public void ControllerActionId_Routes()
        {
            var method = GetMethodInfo<FakeController>(c => c.Simple());

            var url = _generator.GetUrl(method);

            Assert.Equal("\"api/Fake/simple\"", url);
        }

        [Fact]
        public void CustomActionName()
        {
            var method = GetMethodInfo<FakeController>(c => c.DifferentActionName());
            var url = _generator.GetUrl(method);
            Assert.Equal("\"api/Fake/attrActionName\"", url);
        }

        [Fact]
        public void AppendIdTemplateIfNeeded()
        {
            var method = GetMethodInfo<FakeController>(c => c.ActionWithIdParameter(null));
            var url = _generator.GetUrl(method);
            Assert.Equal("\"api/Fake/actionWithIdParameter/\" + id", url);
        }

        [Fact]
        public void CustomRouteAttribute()
        {
            var method = GetMethodInfo<FakeController>(c => c.SimpleCustomRoute());
            var url = _generator.GetUrl(method);
            Assert.Equal("\"api/custom\"", url);
        }
        
        [Fact]
        public void CustomRouteAttributeWithParameters()
        {
            var method = GetMethodInfo<FakeController>(c => c.CustomRouteWithParameters(null, null));
            var url = _generator.GetUrl(method);
            Assert.Equal("\"api/custom/\" + customerId + \"/orders/\" + orderId + \"\"", url);
        }

        [Fact]
        public void StandardRouteWithQueryParameters()
        {
            var method = GetMethodInfo<FakeController>(c => c.ActionMethodWithBodyAndQueryString(null, 11));
            var url = _generator.GetUrl(method);
            Assert.Equal("\"api/Fake/actionMethodWithBodyAndQueryString\" + \"?queryId=\" + queryId + \"\"", url);
        }

        [Fact]
        public void OverrideUriParameterName()
        {
            var method = GetMethodInfo<FakeController>(c => c.OverrideParamName(123));
            var url = _generator.GetUrl(method);
            Assert.Equal("\"api/Fake/overrideParamName\" + \"?x=\" + x + \"\"", url);
        }

        [Fact]
        public void Issue9RegressionTest()
        {
            var method = GetMethodInfo<FakeController>(c => c.GetFlightDetails("ABC"));
            var url = _generator.GetUrl(method);
            Assert.Equal("\"api/v1/flights/\" + flightIdentifier + \"/flightdetails\"", url);
        }
        
        private MethodInfo GetMethodInfo<TCtrl>(Expression<Action<TCtrl>> expression)
        {
            return ((MethodCallExpression) expression.Body).Method;
        }
    }
}