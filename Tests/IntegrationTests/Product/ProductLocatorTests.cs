using System;
using Library.IRequests;
using MadServ.Core.Models.Responses;
using MadServ.Core.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.SampleData;
using FluentAssertions;
using Library.Models;
using MadServ.Core.Models;
using Library.Models.Responses;
using System.Linq;
using Library.Models.Requests;
using MadServ.Core.Models.Responses.PrimitiveResponses;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Tests.IntegrationTests.Product.ProductLocatorTests
{
    [TestClass]
    public class When_looking_for_product_in_local_store
    {
        public static Response<ProductLocatorResponse> _result;
        public static ProductLocator _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var ProductLocator = new BaseIntegrationTest<ProductLocator, ProductLocatorResponse>(config);
            var ProductLocatorRequest = RequestBuilder.GetProductLocatorRequest();
            _result = (Response<ProductLocatorResponse>)ProductLocator.TestObject.Execute(ProductLocatorRequest);

            _testObject = ProductLocator.TestObject;
        }

        [TestMethod]
        public void It_should_return_empty_list_of_errors()
        {
            _testObject._errors.Should().NotBeNull();
            _testObject._errors.Should().BeEmpty();

            _result.errors.Should().NotBeNull();
            _result.errors.Should().BeEmpty();
        }

        [TestMethod]
        public void It_should_return_list_of_stores()
        {
        }
    }
}




