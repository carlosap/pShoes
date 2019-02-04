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
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Tests.IntegrationTests.Home
{
    [TestClass]
    public class When_requesting_Menu
    {
        public static Response<MenuResponse> _result;
        public static Menu _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var menu = new BaseIntegrationTest<Menu, MenuResponse>(config);
            var menuRequest = RequestBuilder.GetMenuRequest();
            _result = (Response<MenuResponse>)menu.TestObject.Execute(menuRequest);
         
            _testObject = menu.TestObject;
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

                foreach (var sub in cat.Subs)
                {
                    sub.Name.Should().NotBeNullOrEmpty();
                    sub.Href.Should().NotBeNullOrEmpty();
                }
            }

            menu.Find(x => x.Subs.Any()).Should().NotBeNull();
        }
    }
}

