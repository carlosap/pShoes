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

namespace Tests.IntegrationTests.Checkout.CheckoutBeginTests
{
    [TestClass]
    public class When_proceeding_to_checkout
    {
        public static Response<CheckoutResponse> _result;
        public static CheckoutBegin _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var cartAdd = new BaseIntegrationTest<CartAdd, CartResponse>(config);
            var cartAddRequest = RequestBuilder.GetCartAddRequestForShoes();
            var cartAddResponse = (Response<CartResponse>)cartAdd.TestObject.Execute(cartAddRequest);

            cartAdd = new BaseIntegrationTest<CartAdd, CartResponse>();
            cartAddRequest = RequestBuilder.GetCartAddRequestForGiftCard();
            cartAddResponse = (Response<CartResponse>)cartAdd.TestObject.Execute(cartAddRequest);

            cartAdd = new BaseIntegrationTest<CartAdd, CartResponse>();
            cartAddRequest = RequestBuilder.GetCartAddRequestForAccessories();
            cartAddResponse = (Response<CartResponse>)cartAdd.TestObject.Execute(cartAddRequest);

            var cartDetail = new BaseIntegrationTest<CartDetail, CartResponse>();
            var cartDetailRequest = new EmptyRequest();
            var cartDetailResponse = (Response<CartResponse>)cartDetail.TestObject.Execute(cartDetailRequest);

            var checkoutBegin = new BaseIntegrationTest<CheckoutBegin, CheckoutResponse>();
            var checkoutBeginRequest = new EmptyRequest();
            _result = (Response<CheckoutResponse>)checkoutBegin.TestObject.Execute(checkoutBeginRequest);

            _testObject = checkoutBegin.TestObject;
        }

        [TestMethod]
        public void It_should_return_empty_list_of_errors()
        {
            _testObject._response.Template.TemplateName.Should().Be(Config.TemplateEnum.CheckoutBegin);

            _testObject._errors.Should().NotBeNull();
            _testObject._errors.Should().BeEmpty();

            _result.errors.Should().NotBeNull();
            _result.errors.Should().BeEmpty();
        }

        [TestMethod]
        public void It_should_return_valid_Misc()
        {
            _result.resultset.Cart.CartItemCount.Should().BeGreaterThan(0);
            _result.resultset.DWSecureKey.Should().NotBeNullOrEmpty();
        }
    }
}

