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

namespace Tests.IntegrationTests.Location
{
    [TestClass]
    public class When_requesting_Stores_by_Lat_n_Long
    {
        public static Response<StoreLocatorResponse> _result;
        public static StoreLocator _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var stores = new BaseIntegrationTest<StoreLocator, StoreLocatorResponse>(config);
            var storesRequest = RequestBuilder.GetStoresRequestWithLatLong();
            _result = (Response<StoreLocatorResponse>)stores.TestObject.Execute(storesRequest);

            _testObject = stores.TestObject;
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
        public void It_should_return_valid_list_Stores()
        {
            var stores = _result.resultset.Locations;

            stores.Should().NotBeNull();
            stores.Should().NotBeEmpty();

            foreach (var store in stores)
            {
                store.Address.Should().NotBeNull();
                store.Address.Address1.Should().NotBeNullOrEmpty();
                store.Address.City.Should().NotBeNullOrEmpty();
                store.Address.State.Should().NotBeNullOrEmpty();
                store.Address.Zip.Should().NotBeNullOrEmpty();

                store.Id.Should().BeGreaterThan(0);

                store.Latitude.Should().BeGreaterThan(0.0);
                store.Longitude.Equals(0.0).Should().BeFalse();
                store.Distance.Should().BeGreaterThan(0.0);

                store.Hours.Should().NotBeEmpty();
            }
        }
    }

    [TestClass]
    public class When_requesting_Stores_by_Zip
    {
        public static Response<StoreLocatorResponse> _result;
        public static StoreLocator _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var stores = new BaseIntegrationTest<StoreLocator, StoreLocatorResponse>(config);
            var storesRequest = RequestBuilder.GetStoresRequestWithZip();
            _result = (Response<StoreLocatorResponse>)stores.TestObject.Execute(storesRequest);


            var test = _result.resultset.Locations.Find(x => x.Hours.Count != 7);

            _testObject = stores.TestObject;
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
        public void It_should_return_valid_list_Stores()
        {
            var stores = _result.resultset.Locations;

            stores.Should().NotBeNull();
            stores.Should().NotBeEmpty();

            foreach (var store in stores)
            {
                store.Address.Should().NotBeNull();
                store.Address.Address1.Should().NotBeNullOrEmpty();
                store.Address.City.Should().NotBeNullOrEmpty();
                store.Address.State.Should().NotBeNullOrEmpty();
                store.Address.Zip.Should().NotBeNullOrEmpty();

                store.Id.Should().BeGreaterThan(0);

                store.Latitude.Should().BeGreaterThan(0.0);
                store.Longitude.Equals(0.0).Should().BeFalse();
                store.Distance.Should().BeGreaterThan(0.0);

                store.Hours.Should().NotBeEmpty();
            }
        }
    }

}

