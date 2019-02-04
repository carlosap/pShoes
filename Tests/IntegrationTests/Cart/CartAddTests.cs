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

namespace Tests.IntegrationTests.Cart.CartAddTests
{
    [TestClass]
    public class When_adding_product_to_cart
    {
        public static Response<CartResponse> _result;
        public static CartAdd _testObject;

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
            _result = (Response<CartResponse>)cartAdd.TestObject.Execute(cartAddRequest);

            _testObject = cartAdd.TestObject;
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
        public void It_should_return_valid_MiniCart()
        {
            var items = _result.resultset.Cart.CartItems;

            items.Should().NotBeEmpty();
            foreach (var item in items)
            {
                item.Name.Should().NotBeNullOrEmpty();
                item.Quantity.Should().BeGreaterThan(0);
            }

            items.Find(x => x.Quantity > 1).Should().NotBeNull();
        }
    }
}

