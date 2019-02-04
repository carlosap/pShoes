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

namespace Tests.IntegrationTests.Checkout.CheckoutShippingTests
{
    //[Ignore]
    [TestClass]
    public class When_proceeding_to_checkout_as_guest_and_submitting_Shipping
    {
        public static Response<CheckoutResponse> _result;
        public static CheckoutShipping _testObject;

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
            cartAddRequest = RequestBuilder.GetCartAddRequestForAccessories();
            cartAddResponse = (Response<CartResponse>)cartAdd.TestObject.Execute(cartAddRequest);

            var cartDetail = new BaseIntegrationTest<CartDetail, CartResponse>();
            var cartDetailRequest = new CartDetailRequest();
            var cartDetailResponse = (Response<CartResponse>)cartDetail.TestObject.Execute(cartDetailRequest);

            var checkoutBegin = new BaseIntegrationTest<CheckoutBegin, CartResponse>();
            var checkoutBeginRequest = new EmptyRequest();
            var checkoutBeginResponse = (Response<CartResponse>)checkoutBegin.TestObject.Execute(checkoutBeginRequest);

            var checkoutGuest = new BaseIntegrationTest<CheckoutGuest, CheckoutResponse>();
            var checkoutGuestRequest = new EmptyRequest();
            var checkoutGuestResponse = (Response<CheckoutResponse>)checkoutGuest.TestObject.Execute(checkoutGuestRequest);

            var checkoutShipping = new BaseIntegrationTest<CheckoutShipping, CheckoutResponse>();
            var checkoutShippingRequest = RequestBuilder.GetCheckoutShippingRequestBESShipHome(checkoutGuestResponse.resultset);
            _result = (Response<CheckoutResponse>)checkoutShipping.TestObject.Execute(checkoutShippingRequest);

            var checkoutCouponOnBillingPage = new BaseIntegrationTest<CheckoutBillingCoupon, CheckoutResponse>();
            //var checkoutCouponOnBillingPageCouponRequest = 

            _testObject = checkoutShipping.TestObject;
        }

        [TestMethod]
        public void It_should_return_empty_list_of_errors()
        {
            _testObject._response.Template.TemplateName.Should().Be(Config.TemplateEnum.CheckoutBilling);

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

        [TestMethod]
        public void It_should_return_valid_ShippingOptions()
        {
            var shippingOptions = _result.resultset.ShippingOptions;

            shippingOptions.Should().NotBeNull();
            shippingOptions.SelectedOption.Should().NotBeNullOrEmpty();
            shippingOptions.Options.Should().NotBeEmpty();
            foreach (var option in shippingOptions.Options)
            {
                option.Label.Should().NotBeNullOrEmpty();
                option.Value.Should().NotBeNullOrEmpty();
                option.Description.Should().NotBeNullOrEmpty();
            }

            shippingOptions.Options.ToList().Find(x => x.IsSelected).Should().NotBeNull();
        }

        [TestMethod]
        public void It_should_return_valid_BillingInfo()
        {
            var billingInfo = _result.resultset.BillingInfo;

            billingInfo.FirstName.Should().NotBeNullOrEmpty();
            billingInfo.LastName.Should().NotBeNullOrEmpty();
            billingInfo.Address1.Should().NotBeNullOrEmpty();
            billingInfo.Address2.Should().NotBeNullOrEmpty();
            billingInfo.City.Should().NotBeNullOrEmpty();
            billingInfo.State.Should().NotBeNullOrEmpty();
            billingInfo.Zip.Should().NotBeNullOrEmpty();
            billingInfo.Phone.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void It_should_return_valid_PaymentInfo()
        {
            var paymentInfo = _result.resultset.PaymentInfo;

            paymentInfo.Should().NotBeNull();
            
            paymentInfo.AvailableCardTypes.Should().NotBeEmpty();
            foreach (var card in paymentInfo.AvailableCardTypes)
            {
                card.Name.Should().NotBeNullOrEmpty();
                card.Value.Should().NotBeNullOrEmpty();
            }

            paymentInfo.AvailableCardTypes.ToList().Find(x => x.IsSelected).Should().NotBeNull();

            paymentInfo.CardInfo.Should().NotBeNull();
            paymentInfo.CardInfo.Years.Should().NotBeEmpty();
            foreach (var year in paymentInfo.CardInfo.Years)
            {
                year.Name.Should().NotBeNullOrEmpty();
            }
            paymentInfo.CardInfo.Years.ToList().Find(x => x.IsSelected).Should().NotBeNull();
            paymentInfo.CardInfo.Years.ToList().Find(x => !string.IsNullOrEmpty(x.Value)).Should().NotBeNull();
            
            paymentInfo.CardInfo.Months.Should().NotBeEmpty();
            foreach (var month in paymentInfo.CardInfo.Months)
            {
                month.Name.Should().NotBeNullOrEmpty();
            }
            paymentInfo.CardInfo.Months.ToList().Find(x => x.IsSelected).Should().NotBeNull();
            paymentInfo.CardInfo.Months.ToList().Find(x => !string.IsNullOrEmpty(x.Value)).Should().NotBeNull();
        }
    }

    [Ignore]
    [TestClass]
    public class When_proceeding_to_checkout_as_guest_and_going_to_review_and_coming_back_to_shipping
    {
        public static Response<CheckoutResponse> _result;
        public static CheckoutShipping _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var cartAdd = new BaseIntegrationTest<CartAdd, CartResponse>(config);
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

            checkoutShipping = new BaseIntegrationTest<CheckoutShipping, CheckoutResponse>();
            checkoutShippingRequest = RequestBuilder.GetCheckoutShippingRefreshRequest();
            _result = (Response<CheckoutResponse>)checkoutShipping.TestObject.Execute(checkoutShippingRequest);

            _testObject = checkoutShipping.TestObject;
        }

        [TestMethod]
        public void It_should_return_empty_list_of_errors()
        {
            _testObject._response.Template.TemplateName.Should().Be(Config.TemplateEnum.CheckoutShipping);

            _testObject._errors.Should().NotBeNull();
            _testObject._errors.Should().BeEmpty();

            _result.errors.Should().NotBeNull();
            _result.errors.Should().BeEmpty();
        }

        [TestMethod]
        public void It_should_return_valid_shippingInfo()
        {
            var shippingInfo = _result.resultset.ShippingInfo;

            shippingInfo.FirstName.Should().NotBeNullOrEmpty();
            shippingInfo.LastName.Should().NotBeNullOrEmpty();
            shippingInfo.Address1.Should().NotBeNullOrEmpty();
            shippingInfo.Address2.Should().NotBeNullOrEmpty();
            shippingInfo.City.Should().NotBeNullOrEmpty();
            shippingInfo.State.Should().NotBeNullOrEmpty();
            shippingInfo.Zip.Should().NotBeNullOrEmpty();
            shippingInfo.Phone.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void It_should_return_valid_Misc()
        {
            _result.resultset.Cart.CartItemCount.Should().BeGreaterThan(0);
            _result.resultset.DWSecureKey.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void It_should_return_valid_ShippingOption()
        {
            var shippingOptions = _result.resultset.ShippingOptions;

            shippingOptions.Should().NotBeNull();
            shippingOptions.SelectedOption.Should().NotBeNullOrEmpty();
            shippingOptions.Options.Should().NotBeEmpty();
            foreach (var option in shippingOptions.Options)
            {
                option.Label.Should().NotBeNullOrEmpty();
                option.Value.Should().NotBeNullOrEmpty();
                option.Description.Should().NotBeNullOrEmpty();
            }

            shippingOptions.Options.ToList().Find(x => x.IsSelected).Should().NotBeNull();
        }
    }

    [Ignore]
    [TestClass]
    public class When_proceeding_to_checkout_as_guest_and_submitting_Shipping_BNES
    {
        public static Response<CheckoutResponse> _result;
        public static CheckoutShipping _testObject;

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
            cartAddRequest = RequestBuilder.GetCartAddRequestForAccessories();
            cartAddResponse = (Response<CartResponse>)cartAdd.TestObject.Execute(cartAddRequest);

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
            _result = (Response<CheckoutResponse>)checkoutShipping.TestObject.Execute(checkoutShippingRequest);

            _testObject = checkoutShipping.TestObject;
        }

        [TestMethod]
        public void It_should_return_empty_list_of_errors()
        {
            _testObject._response.Template.TemplateName.Should().Be(Config.TemplateEnum.CheckoutBilling);

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

        [TestMethod]
        public void It_should_return_valid_ShippingOptions()
        {
            var shippingOptions = _result.resultset.ShippingOptions;

            shippingOptions.Should().NotBeNull();
            shippingOptions.SelectedOption.Should().NotBeNullOrEmpty();
            shippingOptions.Options.Should().NotBeEmpty();
            foreach (var option in shippingOptions.Options)
            {
                option.Label.Should().NotBeNullOrEmpty();
                option.Value.Should().NotBeNullOrEmpty();
                option.Description.Should().NotBeNullOrEmpty();
            }

            shippingOptions.Options.ToList().Find(x => x.IsSelected).Should().NotBeNull();
        }

        [TestMethod]
        public void It_should_return_valid_BillingInfo()
        {
            var billingInfo = _result.resultset.BillingInfo;
        }

        [TestMethod]
        public void It_should_return_valid_PaymentInfo()
        {
            var paymentInfo = _result.resultset.PaymentInfo;

            paymentInfo.Should().NotBeNull();

            paymentInfo.AvailableCardTypes.Should().NotBeEmpty();
            foreach (var card in paymentInfo.AvailableCardTypes)
            {
                card.Name.Should().NotBeNullOrEmpty();
                card.Value.Should().NotBeNullOrEmpty();
            }

            paymentInfo.AvailableCardTypes.ToList().Find(x => x.IsSelected).Should().NotBeNull();

            paymentInfo.CardInfo.Should().NotBeNull();
            paymentInfo.CardInfo.Years.Should().NotBeEmpty();
            foreach (var year in paymentInfo.CardInfo.Years)
            {
                year.Name.Should().NotBeNullOrEmpty();
            }
            paymentInfo.CardInfo.Years.ToList().Find(x => x.IsSelected).Should().NotBeNull();
            paymentInfo.CardInfo.Years.ToList().Find(x => !string.IsNullOrEmpty(x.Value)).Should().NotBeNull();

            paymentInfo.CardInfo.Months.Should().NotBeEmpty();
            foreach (var month in paymentInfo.CardInfo.Months)
            {
                month.Name.Should().NotBeNullOrEmpty();
            }
            paymentInfo.CardInfo.Months.ToList().Find(x => x.IsSelected).Should().NotBeNull();
            paymentInfo.CardInfo.Months.ToList().Find(x => !string.IsNullOrEmpty(x.Value)).Should().NotBeNull();
        }
    }

    [TestClass]
    public class When_proceeding_to_checkout_as_guest_and_submitting_Shipping_ShipToStore
    {
        public static Response<CheckoutResponse> _result;
        public static CheckoutShipping _testObject;

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
            cartAddRequest = RequestBuilder.GetCartAddRequestForAccessories();
            cartAddResponse = (Response<CartResponse>)cartAdd.TestObject.Execute(cartAddRequest);

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
            _result = (Response<CheckoutResponse>)checkoutShipping.TestObject.Execute(checkoutShippingRequest);

            _testObject = checkoutShipping.TestObject;
        }

        [TestMethod]
        public void It_should_return_empty_list_of_errors()
        {
            _testObject._response.Template.TemplateName.Should().Be(Config.TemplateEnum.CheckoutBilling);

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

        [TestMethod]
        public void It_should_return_valid_ShippingOptions()
        {
            var shippingOptions = _result.resultset.ShippingOptions;

            shippingOptions.Should().NotBeNull();
            shippingOptions.SelectedOption.Should().NotBeNullOrEmpty();
            shippingOptions.Options.Should().NotBeEmpty();
            foreach (var option in shippingOptions.Options)
            {
                option.Label.Should().NotBeNullOrEmpty();
                option.Value.Should().NotBeNullOrEmpty();
                option.Description.Should().NotBeNullOrEmpty();
            }

            shippingOptions.Options.ToList().Find(x => x.IsSelected).Should().NotBeNull();
        }

        [TestMethod]
        public void It_should_return_valid_PaymentInfo()
        {
            var paymentInfo = _result.resultset.PaymentInfo;

            paymentInfo.Should().NotBeNull();

            paymentInfo.AvailableCardTypes.Should().NotBeEmpty();
            foreach (var card in paymentInfo.AvailableCardTypes)
            {
                card.Name.Should().NotBeNullOrEmpty();
                card.Value.Should().NotBeNullOrEmpty();
            }

            paymentInfo.AvailableCardTypes.ToList().Find(x => x.IsSelected).Should().NotBeNull();

            paymentInfo.CardInfo.Should().NotBeNull();
            paymentInfo.CardInfo.Years.Should().NotBeEmpty();
            foreach (var year in paymentInfo.CardInfo.Years)
            {
                year.Name.Should().NotBeNullOrEmpty();
            }
            paymentInfo.CardInfo.Years.ToList().Find(x => x.IsSelected).Should().NotBeNull();
            paymentInfo.CardInfo.Years.ToList().Find(x => !string.IsNullOrEmpty(x.Value)).Should().NotBeNull();

            paymentInfo.CardInfo.Months.Should().NotBeEmpty();
            foreach (var month in paymentInfo.CardInfo.Months)
            {
                month.Name.Should().NotBeNullOrEmpty();
            }
            paymentInfo.CardInfo.Months.ToList().Find(x => x.IsSelected).Should().NotBeNull();
            paymentInfo.CardInfo.Months.ToList().Find(x => !string.IsNullOrEmpty(x.Value)).Should().NotBeNull();
        }
    }
}

