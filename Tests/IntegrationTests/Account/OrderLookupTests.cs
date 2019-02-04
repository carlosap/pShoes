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

namespace Tests.IntegrationTests.Account.OrderLookupTests
{
    [TestClass]
    public class When_checking_order_status
    {
        public static Response<AccountOrderDetailResponse> _result;
        public static OrderLookup _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var orderLookup = new BaseIntegrationTest<OrderLookup, AccountOrderDetailResponse>(config);
            var orderLookupRequest = RequestBuilder.GetOrderLookupRequest();
            _result = (Response<AccountOrderDetailResponse>)orderLookup.TestObject.Execute(orderLookupRequest);

            _testObject = orderLookup.TestObject;
        }

        [TestMethod]
        public void It_should_return_empty_list_of_errors()
        {
            _testObject._errors.Should().NotBeNull();
            _testObject._errors.Should().BeEmpty();
        }
    }

    [Ignore]
    [TestClass]
    public class When_checking_order_status_with_invalid_OrderId
    {
        public static Response<AccountOrderDetailResponse> _result;
        public static OrderLookup _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var orderLookup = new BaseIntegrationTest<OrderLookup, AccountOrderDetailResponse>(config);
            var orderLookupRequest = RequestBuilder.GetInvalidOrderLookupRequest();
            _result = (Response<AccountOrderDetailResponse>)orderLookup.TestObject.Execute(orderLookupRequest);

            _testObject = orderLookup.TestObject;
        }

        [TestMethod]
        public void It_should_return_empty_list_of_errors()
        {
            _testObject._errors.Should().NotBeNull();
            _testObject._errors.Should().NotBeEmpty();
        }
    }
}

