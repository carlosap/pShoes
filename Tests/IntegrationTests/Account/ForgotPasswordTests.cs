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

namespace Tests.IntegrationTests.Account.ForgotPasswordTests
{
    [TestClass]
    public class When_using_forgot_password_functionality
    {
        public static Response<BoolResponse> _result;
        public static ForgotPassword _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var forgotPassword = new BaseIntegrationTest<ForgotPassword, AccountOrderDetailResponse>(config);
            var forgotPasswordRequest = RequestBuilder.GetForgotPasswordRequest();
            _result = (Response<BoolResponse>)forgotPassword.TestObject.Execute(forgotPasswordRequest);

            _testObject = forgotPassword.TestObject;
        }

        [TestMethod]
        public void It_should_return_empty_list_of_errors()
        {
            _testObject._errors.Should().NotBeNull();
            _testObject._errors.Should().BeEmpty();
        }

        [TestMethod]
        public void It_should_return_success()
        {
            _result.resultset.Model.Should().BeTrue();
        }
    }

    [TestClass]
    public class When_using_forgot_password_functionality_with_invalid_user_name
    {
        public static Response<BoolResponse> _result;
        public static ForgotPassword _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var forgotPassword = new BaseIntegrationTest<ForgotPassword, AccountOrderDetailResponse>(config);
            var forgotPasswordRequest = RequestBuilder.GetInvalidForgotPasswordRequest();
            _result = (Response<BoolResponse>)forgotPassword.TestObject.Execute(forgotPasswordRequest);

            _testObject = forgotPassword.TestObject;
        }

        [TestMethod]
        public void It_should_return_errors()
        {
            _testObject._errors.Should().NotBeNull();
            _testObject._errors.Should().NotBeEmpty();
        }

        [TestMethod]
        public void It_should_return_success()
        {
            _result.resultset.Model.Should().BeFalse();
        }
    }
}

