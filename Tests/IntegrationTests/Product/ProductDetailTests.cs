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

namespace Tests.IntegrationTests.Product
{
    [TestClass]
    public class When_getting_ProductDetail
    {
        public static Response<ProductDetailResponse> _result;
        public static ProductDetail _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var productDetail = new BaseIntegrationTest<ProductDetail, ProductDetailResponse>(config);
            var productDetailRequest = RequestBuilder.GetProductDetailRequest();
            _result = (Response<ProductDetailResponse>)productDetail.TestObject.Execute(productDetailRequest);

            _testObject = productDetail.TestObject;
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
        public void It_should_return_valid_Product()
        {
            var product = _result.resultset.Product;

            product.Should().NotBeNull();
            product.ProductId.Should().NotBeNullOrEmpty();
            product.Name.Should().NotBeNullOrEmpty();
            product.TotalPrice.Should().NotBeNull();
            product.TotalPrice.Value.Should().BeGreaterThan(0.0);
            product.Description.Should().NotBeNullOrEmpty();
            product.AdditionalInfo.Should().NotBeNullOrEmpty();
            product.CareInstructions.Should().NotBeNullOrEmpty();

            product.AvailableVariations.Should().NotBeEmpty();
            product.Images.Should().NotBeEmpty();
        }
    }
}




