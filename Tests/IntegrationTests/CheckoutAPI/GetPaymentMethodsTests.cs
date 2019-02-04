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

namespace Tests.IntegrationTests.Cart
{
    [TestClass]
    public class When_requesting_list_of_payment_methods
    {
        public static Response<GetPaymentMethodsResponse> _result;
        public static GetPaymentMethods _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var cartAdd = new BaseIntegrationTest<CartAdd, CartResponse>(config);
            var cartAddRequest = RequestBuilder.GetCartAddRequest();
            var cartAddResponse = (Response<CartResponse>)cartAdd.TestObject.Execute(cartAddRequest);

            var getPaymentMethods = new BaseIntegrationTest<GetPaymentMethods, GetPaymentMethodsResponse>();
            var getPaymentMethodsRequest = new EmptyRequest();
            _result = (Response<GetPaymentMethodsResponse>)getPaymentMethods.TestObject.Execute(getPaymentMethodsRequest);

            _testObject = getPaymentMethods.TestObject;
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
        public void It_should_return_valid_Cart()
        {
        }
    }
}

