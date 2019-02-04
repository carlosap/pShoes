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

namespace Tests.IntegrationTests.Cart.CartDetailTests
{
    [TestClass]
    public class When_getting_CartDetail_page
    {
        public static Response<CartResponse> _result;
        public static CartDetail _testObject;

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
            _result = (Response<CartResponse>)cartDetail.TestObject.Execute(cartDetailRequest);

            _testObject = cartDetail.TestObject;
        }

        [TestMethod]
        public void It_should_return_empty_list_of_errors()
        {
            _result.resultset.Cart.Should().NotBeNull();

            _testObject._errors.Should().NotBeNull();
            _testObject._errors.Should().BeEmpty();

            _result.errors.Should().NotBeNull();
            _result.errors.Should().BeEmpty();
        }

        [TestMethod]
        public void It_should_return_valid_CartItems()
        {
            var items = _result.resultset.Cart.CartItems;

            foreach (var item in items)
            {
                item.Image.Should().NotBeNull();
                item.Image.Src.Should().NotBeNullOrEmpty();
                item.Name.Should().NotBeNullOrEmpty();
                item.Href.Should().NotBeNullOrEmpty();
                item.ProductId.Should().NotBeNullOrEmpty();
                item.Quantity.Should().BeGreaterThan(0);
                item.ItemPrice.Should().NotBeNull();
                item.ItemPrice.Value.Should().BeGreaterThan(0);
                item.TotalPrice.Should().NotBeNull();
                item.TotalPrice.Value.Should().BeGreaterThan(0);
            }

            items.Find(x => !string.IsNullOrEmpty(x.Color)).Should().NotBeNull();
            items.Find(x => !string.IsNullOrEmpty(x.Size)).Should().NotBeNull();
            items.Find(x => !string.IsNullOrEmpty(x.Width)).Should().NotBeNull();
            items.Find(x => !string.IsNullOrEmpty(x.Design)).Should().NotBeNull();
            items.Find(x => !string.IsNullOrEmpty(x.GCValue)).Should().NotBeNull();
        }

        [TestMethod]
        public void It_should_return_valid_Summary()
        {
            var summary = _result.resultset.Cart.Summary;

            summary.Should().NotBeNull();
            summary.Total.Should().NotBeNull();
            //summary.Total.Value.Should().BeGreaterThan(0);
            summary.Costs.Should().NotBeEmpty();

            foreach (var cost in summary.Costs)
            {
                cost.Label.Should().NotBeNullOrEmpty();
                cost.Value.Should().BeGreaterThan(0);
            }
        }

        [TestMethod]
        public void It_should_return_valid_Misc()
        {
            _result.resultset.Cart.DWQuery.Should().NotBeNullOrEmpty();
            _result.resultset.Cart.AllowToCheckout.Should().BeTrue();
            _result.resultset.Cart.CartItemCount.Should().BeGreaterThan(0);
        }
    }
}

