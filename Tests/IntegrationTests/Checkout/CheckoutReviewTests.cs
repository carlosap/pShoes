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

namespace Tests.IntegrationTests.Checkout.CheckoutReviewTests
{
    //[Ignore]
    [TestClass]
    public class When_proceeding_to_checkout_as_guest_and_submitting_Review
    {
        public static Response<CheckoutResponse> _result;
        public static CheckoutReview _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var cartAdd = new BaseIntegrationTest<CartAdd, CartResponse>();
            var cartAddRequest = RequestBuilder.GetCartAddRequestForAccessories();
            var cartAddResponse = (Response<CartResponse>)cartAdd.TestObject.Execute(cartAddRequest);

            var cartDetail = new BaseIntegrationTest<CartDetail, CartResponse>();
            var cartDetailRequest = new EmptyRequest();
            var cartDetailResponse = (Response<CartResponse>)cartDetail.TestObject.Execute(cartDetailRequest);

            var checkoutBegin = new BaseIntegrationTest<CheckoutBegin, CheckoutResponse>();
            var checkoutBeginRequest = new EmptyRequest();
            var checkoutBeginResponse = (Response<CheckoutResponse>)checkoutBegin.TestObject.Execute(checkoutBeginRequest);

            var checkoutGuest = new BaseIntegrationTest<CheckoutGuest, CheckoutResponse>();
            var checkoutGuestRequest = new EmptyRequest();
            var checkoutGuestResponse = (Response<CheckoutResponse>)checkoutGuest.TestObject.Execute(checkoutGuestRequest);

            var checkoutShipping = new BaseIntegrationTest<CheckoutShipping, CheckoutResponse>();
            var checkoutShippingRequest = RequestBuilder.GetCheckoutShippingRequestBESShipHome(checkoutGuestResponse.resultset);
            var checkoutShippingResponse = (Response<CheckoutResponse>)checkoutShipping.TestObject.Execute(checkoutShippingRequest);

            var checkoutBilling = new BaseIntegrationTest<CheckoutBilling, CheckoutResponse>();
            var checkoutBillingRequest = RequestBuilder.GetCheckoutBillingRequestBESShipHome(checkoutGuestResponse.resultset);
            var checkoutBillingResponse = (Response<CheckoutResponse>)checkoutBilling.TestObject.Execute(checkoutBillingRequest);

            var checkoutReview = new BaseIntegrationTest<CheckoutReview, CheckoutResponse>();
            var checkoutReviewRequest = RequestBuilder.GetCheckoutReviewRequest(checkoutGuestResponse.resultset);
            _result = (Response<CheckoutResponse>)checkoutReview.TestObject.Execute(checkoutReviewRequest);

            _testObject = checkoutReview.TestObject;
        }

        [TestMethod]
        public void It_should_return_empty_list_of_errors()
        {
            _testObject._response.Template.TemplateName.Should().Be(Config.TemplateEnum.CheckoutConfirmation);

            _testObject._errors.Should().NotBeNull();
            _testObject._errors.Should().BeEmpty();

            _result.errors.Should().NotBeNull();
            _result.errors.Should().BeEmpty();
        }

        [TestMethod]
        public void It_should_return_valid_OrderDetail()
        {
            var orderDetail = _result.resultset.OrderDetail;

            orderDetail.Should().NotBeNull();
            orderDetail.OrderConfirmationNumber.Should().NotBeNullOrEmpty();
            orderDetail.Message.Should().NotBeNullOrEmpty();
            orderDetail.Status.Should().NotBeNullOrEmpty();
        }
    }

    [Ignore]
    [TestClass]
    public class When_proceeding_to_checkout_as_guest_and_submitting_Review_BNES
    {
        public static Response<CheckoutResponse> _result;
        public static CheckoutReview _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var cartAdd = new BaseIntegrationTest<CartAdd, CartResponse>();
            var cartAddRequest = RequestBuilder.GetCartAddRequestForAccessories();
            var cartAddResponse = (Response<CartResponse>)cartAdd.TestObject.Execute(cartAddRequest);

            var cartDetail = new BaseIntegrationTest<CartDetail, CartResponse>();
            var cartDetailRequest = new EmptyRequest();
            var cartDetailResponse = (Response<CartResponse>)cartDetail.TestObject.Execute(cartDetailRequest);

            var checkoutBegin = new BaseIntegrationTest<CheckoutBegin, CheckoutResponse>();
            var checkoutBeginRequest = new EmptyRequest();
            var checkoutBeginResponse = (Response<CheckoutResponse>)checkoutBegin.TestObject.Execute(checkoutBeginRequest);

            var checkoutGuest = new BaseIntegrationTest<CheckoutGuest, CheckoutResponse>();
            var checkoutGuestRequest = new EmptyRequest();
            var checkoutGuestResponse = (Response<CheckoutResponse>)checkoutGuest.TestObject.Execute(checkoutGuestRequest);

            var checkoutShipping = new BaseIntegrationTest<CheckoutShipping, CheckoutResponse>();
            var checkoutShippingRequest = RequestBuilder.GetCheckoutShippingRequestBNESShipHome(checkoutGuestResponse.resultset);
            var checkoutShippingResponse = (Response<CheckoutResponse>)checkoutShipping.TestObject.Execute(checkoutShippingRequest);

            var checkoutBilling = new BaseIntegrationTest<CheckoutBilling, CheckoutResponse>();
            var checkoutBillingRequest = RequestBuilder.GetCheckoutBillingRequestBNESShipHome(checkoutGuestResponse.resultset);
            var checkoutBillingResponse = (Response<CheckoutResponse>)checkoutBilling.TestObject.Execute(checkoutBillingRequest);

            var checkoutReview = new BaseIntegrationTest<CheckoutReview, CheckoutResponse>();
            var checkoutReviewRequest = RequestBuilder.GetCheckoutReviewRequest(checkoutGuestResponse.resultset);
            _result = (Response<CheckoutResponse>)checkoutReview.TestObject.Execute(checkoutReviewRequest);

            _testObject = checkoutReview.TestObject;
        }

        [TestMethod]
        public void It_should_return_empty_list_of_errors()
        {
            _testObject._response.Template.TemplateName.Should().Be(Config.TemplateEnum.CheckoutConfirmation);

            _testObject._errors.Should().NotBeNull();
            _testObject._errors.Should().BeEmpty();

            _result.errors.Should().NotBeNull();
            _result.errors.Should().BeEmpty();
        }

        [TestMethod]
        public void It_should_return_valid_OrderDetail()
        {
            var orderDetail = _result.resultset.OrderDetail;

            orderDetail.Should().NotBeNull();
            orderDetail.OrderConfirmationNumber.Should().NotBeNullOrEmpty();
            orderDetail.Message.Should().NotBeNullOrEmpty();
            orderDetail.Status.Should().NotBeNullOrEmpty();
        }
    }

    [Ignore]
    [TestClass]
    public class When_proceeding_to_checkout_as_guest_and_submitting_Review_ShipToStore
    {
        public static Response<CheckoutResponse> _result;
        public static CheckoutReview _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var cartAdd = new BaseIntegrationTest<CartAdd, CartResponse>();
            var cartAddRequest = RequestBuilder.GetCartAddRequestForAccessories();
            var cartAddResponse = (Response<CartResponse>)cartAdd.TestObject.Execute(cartAddRequest);

            var cartDetail = new BaseIntegrationTest<CartDetail, CartResponse>();
            var cartDetailRequest = new EmptyRequest();
            var cartDetailResponse = (Response<CartResponse>)cartDetail.TestObject.Execute(cartDetailRequest);

            var checkoutBegin = new BaseIntegrationTest<CheckoutBegin, CheckoutResponse>();
            var checkoutBeginRequest = new EmptyRequest();
            var checkoutBeginResponse = (Response<CheckoutResponse>)checkoutBegin.TestObject.Execute(checkoutBeginRequest);

            var checkoutGuest = new BaseIntegrationTest<CheckoutGuest, CheckoutResponse>();
            var checkoutGuestRequest = new EmptyRequest();
            var checkoutGuestResponse = (Response<CheckoutResponse>)checkoutGuest.TestObject.Execute(checkoutGuestRequest);

            var checkoutShipping = new BaseIntegrationTest<CheckoutShipping, CheckoutResponse>();
            var checkoutShippingRequest = RequestBuilder.GetCheckoutShippingRequestShipToStore(checkoutGuestResponse.resultset);
            var checkoutShippingResponse = (Response<CheckoutResponse>)checkoutShipping.TestObject.Execute(checkoutShippingRequest);

            var checkoutBilling = new BaseIntegrationTest<CheckoutBilling, CheckoutResponse>();
            var checkoutBillingRequest = RequestBuilder.GetCheckoutBillingRequestShipToStore(checkoutGuestResponse.resultset);
            var checkoutBillingResponse = (Response<CheckoutResponse>)checkoutBilling.TestObject.Execute(checkoutBillingRequest);

            var checkoutReview = new BaseIntegrationTest<CheckoutReview, CheckoutResponse>();
            var checkoutReviewRequest = RequestBuilder.GetCheckoutReviewRequest(checkoutGuestResponse.resultset);
            _result = (Response<CheckoutResponse>)checkoutReview.TestObject.Execute(checkoutReviewRequest);

            _testObject = checkoutReview.TestObject;
        }

        [TestMethod]
        public void It_should_return_empty_list_of_errors()
        {
            _testObject._response.Template.TemplateName.Should().Be(Config.TemplateEnum.CheckoutConfirmation);

            _testObject._errors.Should().NotBeNull();
            _testObject._errors.Should().BeEmpty();

            _result.errors.Should().NotBeNull();
            _result.errors.Should().BeEmpty();
        }

        [TestMethod]
        public void It_should_return_valid_OrderDetail()
        {
            var orderDetail = _result.resultset.OrderDetail;

            orderDetail.Should().NotBeNull();
            orderDetail.OrderConfirmationNumber.Should().NotBeNullOrEmpty();
            orderDetail.Message.Should().NotBeNullOrEmpty();
            orderDetail.Status.Should().NotBeNullOrEmpty();
        }
    }
}

