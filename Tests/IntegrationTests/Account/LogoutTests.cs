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

namespace Tests.IntegrationTests.Account
{
    [TestClass]
    public class When_loggin_in_with_correct_credentials_and_logging_out
    {
        public static Response<BoolResponse> _result;
        public static Logout _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var login = new BaseIntegrationTest<Login, BoolResponse>(config);
            var loginRequest = RequestBuilder.GetLoginRequest();
            var loginResponse = (Response<BoolResponse>)login.TestObject.Execute(loginRequest);

            var logout = new BaseIntegrationTest<Logout, BoolResponse>();
            var logoutRequest = new EmptyRequest();
            _result = (Response<BoolResponse>)logout.TestObject.Execute(logoutRequest);

            _testObject = logout.TestObject;
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
            _result.resultset.Model.Should().BeTrue();
        }
    }
}

