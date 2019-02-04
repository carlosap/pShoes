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

namespace Tests.IntegrationTests.Home
{
    [TestClass]
    public class When_requesting_Init
    {
        public static Response<InitResponse> _result;
        public static Init _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var init = new BaseIntegrationTest<Init, InitResponse>(config);
            var initRequest = RequestBuilder.GetInitRequest();
            _result = (Response<InitResponse>)init.TestObject.Execute(initRequest);
         
            _testObject = init.TestObject;
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
        public void It_should_return_valid_Menu()
        {
            var menu = _result.resultset.Menu;

            menu.Should().NotBeNull();
            foreach (var cat in menu)
            {
                cat.Name.Should().NotBeNullOrEmpty();
                cat.Href.Should().NotBeNullOrEmpty();
                cat.Subs.Should().NotBeEmpty();

                foreach (var sub in cat.Subs)
                {
                    sub.Name.Should().NotBeNullOrEmpty();
                    sub.Href.Should().NotBeNullOrEmpty();
                    sub.Subs.Should().BeEmpty();
                }
            }
        }

        //[TestMethod]
        //public void It_should_return_valid_Banners()
        //{
        //    var banners = _result.resultset.Banners;

        //    banners.Should().NotBeNull();
        //    banners.Should().NotBeEmpty();

        //    foreach (var banner in banners)
        //    {
        //        banner.Src.Should().NotBeNullOrEmpty();
        //        banner.Description.Should().NotBeNullOrEmpty();
        //    }
        //}

        //[TestMethod]
        //public void It_should_return_valid_FeatureImages()
        //{
        //    var featureImages = _result.resultset.FeatureImages;

        //    featureImages.Should().NotBeNull();
        //    featureImages.Should().NotBeEmpty();

        //    foreach (var image in featureImages)
        //    {
        //        image.Src.Should().NotBeNullOrEmpty();
        //        image.Description.Should().NotBeNullOrEmpty();
        //    }
        //}
    }
}

