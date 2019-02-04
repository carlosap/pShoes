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
using System.Web.Script.Serialization;

namespace Tests.IntegrationTests.Product
{
    //[Ignore]
    [TestClass]
    public class When_searching_for_products_in_highest_category
    {
        public static Response<SearchResponse> _result;
        public static Search _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var menu = new BaseIntegrationTest<Menu, MenuResponse>(config);
            var menuRequest = RequestBuilder.GetMenuRequest();
            var menuResponse = (Response<MenuResponse>)menu.TestObject.Execute(menuRequest);

            var search = new BaseIntegrationTest<Search, SearchResponse>();
            var searchRequest = RequestBuilder.GetCategorySearchRequestForHighestCategory();
            _result = (Response<SearchResponse>)search.TestObject.Execute(searchRequest);

            _testObject = search.TestObject;
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
        public void It_should_return_valid_Sorter()
        {
            var sorter = _result.resultset.Sorter;

            sorter.Should().NotBeNull();
            sorter.SortBy.Should().NotBeNullOrEmpty();
            sorter.Options.Should().NotBeEmpty();
            foreach (var option in sorter.Options)
            {
                option.Name.Should().NotBeNullOrEmpty();
                option.Value.Should().NotBeNullOrEmpty();
            }
            sorter.Options.ToList().Find(x => x.IsSelected).Should().NotBeNull();
        }

        [TestMethod]
        public void It_should_return_valid_Filter()
        {
            var filter = _result.resultset.Filters;
            filter.Should().NotBeNull();

            var subMenu = filter.SubMenu;
            subMenu.Should().NotBeNull();
            subMenu.Subs.Should().NotBeEmpty();
            foreach (var sub in subMenu.Subs)
            {
                sub.Name.Should().NotBeNullOrEmpty();
                sub.Href.Should().NotBeNullOrEmpty();
            }

            var path = filter.Path;
            path.Should().NotBeEmpty();
            foreach (var item in path)
            {
                item.Key.Should().NotBeNullOrEmpty();
                item.Value.Should().NotBeNullOrEmpty();
            }

            var sections = filter.FilterSections;
            sections.Should().NotBeEmpty();
            foreach (var section in sections)
            {
                section.Label.Should().NotBeNullOrEmpty();
                section.Note.Should().NotBeNullOrEmpty();
                section.FilterOptions.Should().NotBeEmpty();
                foreach (var option in section.FilterOptions)
                {
                    option.Label.Should().NotBeNullOrEmpty();
                    option.Value.Should().NotBeNullOrEmpty();
                    option.Note.Should().NotBeNullOrEmpty();
                }
            }

            var applied = filter.AppliedFilterSections;
            applied.Should().NotBeEmpty();
            foreach (var section in applied)
            {
                section.Label.Should().NotBeNullOrEmpty();
                section.Note.Should().NotBeNullOrEmpty();
                section.FilterOptions.Should().NotBeEmpty();
                foreach (var option in section.FilterOptions)
                {
                    option.Label.Should().NotBeNullOrEmpty();
                    option.Value.Should().NotBeNullOrEmpty();
                }
            }
        }

        [TestMethod]
        public void It_should_return_valid_Pager()
        {
            var pager = _result.resultset.Pager;

            pager.Should().NotBeNull();
            pager.PageSize.Should().BeGreaterThan(0);
            pager.RecordCount.Should().BeGreaterThan(0);
            pager.CurrentPage.Should().BeGreaterThan(0);
            pager.TotalPages.Should().BeGreaterThan(0);
            pager.TotalRecords.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void It_should_return_valid_list_of_Products()
        {
            var products = _result.resultset.Products;

            products.Should().NotBeEmpty();
            foreach (var product in products)
            {
                product.ProductId.Should().NotBeNullOrEmpty();
                product.Name.Should().NotBeNullOrEmpty();
                product.Pricing.Should().NotBeEmpty();
                foreach (var price in product.Pricing)
                {
                    price.Value.Should().BeGreaterThan(0.0);
                }
            }
        }
    }

    [Ignore]
    [TestClass]
    public class When_searching_for_products_in_mid_category
    {
        public static Response<SearchResponse> _result;
        public static Search _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var menu = new BaseIntegrationTest<Menu, MenuResponse>(config);
            var menuRequest = RequestBuilder.GetMenuRequest();
            var menuResponse = (Response<MenuResponse>)menu.TestObject.Execute(menuRequest);

            var search = new BaseIntegrationTest<Search, SearchResponse>();
            var searchRequest = RequestBuilder.GetCategorySearchRequestForMidCategory();
            _result = (Response<SearchResponse>)search.TestObject.Execute(searchRequest);

            _testObject = search.TestObject;
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
        public void It_should_return_valid_Sorter()
        {
            var sorter = _result.resultset.Sorter;

            sorter.Should().NotBeNull();
            sorter.SortBy.Should().NotBeNullOrEmpty();
            sorter.Options.Should().NotBeEmpty();
            foreach (var option in sorter.Options)
            {
                option.Name.Should().NotBeNullOrEmpty();
                option.Value.Should().NotBeNullOrEmpty();
            }
            sorter.Options.ToList().Find(x => x.IsSelected).Should().NotBeNull();
        }

        [TestMethod]
        public void It_should_return_valid_Filter()
        {
            var filter = _result.resultset.Filters;
            filter.Should().NotBeNull();

            var subMenu = filter.SubMenu;
            subMenu.Should().NotBeNull();
            subMenu.Subs.Should().NotBeEmpty();
            foreach (var sub in subMenu.Subs)
            {
                sub.Name.Should().NotBeNullOrEmpty();
                sub.Href.Should().NotBeNullOrEmpty();
            }

            var path = filter.Path;
            path.Should().NotBeEmpty();
            foreach (var item in path)
            {
                item.Key.Should().NotBeNullOrEmpty();
                item.Value.Should().NotBeNullOrEmpty();
            }

            var sections = filter.FilterSections;
            sections.Should().NotBeEmpty();
            foreach (var section in sections)
            {
                section.Label.Should().NotBeNullOrEmpty();
                section.Note.Should().NotBeNullOrEmpty();
                section.FilterOptions.Should().NotBeEmpty();
                foreach (var option in section.FilterOptions)
                {
                    option.Label.Should().NotBeNullOrEmpty();
                    option.Value.Should().NotBeNullOrEmpty();
                    option.Note.Should().NotBeNullOrEmpty();
                }
            }

            var applied = filter.AppliedFilterSections;
            applied.Should().NotBeEmpty();
            foreach (var section in applied)
            {
                section.Label.Should().NotBeNullOrEmpty();
                section.Note.Should().NotBeNullOrEmpty();
                section.FilterOptions.Should().NotBeEmpty();
                foreach (var option in section.FilterOptions)
                {
                    option.Label.Should().NotBeNullOrEmpty();
                    option.Value.Should().NotBeNullOrEmpty();
                }
            }
        }

        [TestMethod]
        public void It_should_return_valid_Pager()
        {
            var pager = _result.resultset.Pager;

            pager.Should().NotBeNull();
            pager.PageSize.Should().BeGreaterThan(0);
            pager.RecordCount.Should().BeGreaterThan(0);
            pager.CurrentPage.Should().BeGreaterThan(0);
            pager.TotalPages.Should().BeGreaterThan(0);
            pager.TotalRecords.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void It_should_return_valid_list_of_Products()
        {
            var products = _result.resultset.Products;

            products.Should().NotBeEmpty();
            foreach (var product in products)
            {
                product.ProductId.Should().NotBeNullOrEmpty();
                product.Name.Should().NotBeNullOrEmpty();
                product.Pricing.Should().NotBeEmpty();
                foreach (var price in product.Pricing)
                {
                    price.Value.Should().BeGreaterThan(0.0);
                }
            }
        }
    }

    [Ignore] // needs working href
    [TestClass]
    public class When_searching_for_products_in_lowest_category
    {
        public static Response<SearchResponse> _result;
        public static Search _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var menu = new BaseIntegrationTest<Menu, MenuResponse>(config);
            var menuRequest = RequestBuilder.GetMenuRequest();
            var menuResponse = (Response<MenuResponse>)menu.TestObject.Execute(menuRequest);

            var search = new BaseIntegrationTest<Search, SearchResponse>();
            var searchRequest = RequestBuilder.GetCategorySearchRequestForLowestCategory();
            _result = (Response<SearchResponse>)search.TestObject.Execute(searchRequest);

            _testObject = search.TestObject;
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
        public void It_should_return_valid_Sorter()
        {
            var sorter = _result.resultset.Sorter;

            sorter.Should().NotBeNull();
            sorter.SortBy.Should().NotBeNullOrEmpty();
            sorter.Options.Should().NotBeEmpty();
            foreach (var option in sorter.Options)
            {
                option.Name.Should().NotBeNullOrEmpty();
                option.Value.Should().NotBeNullOrEmpty();
            }
            sorter.Options.ToList().Find(x => x.IsSelected).Should().NotBeNull();
        }

        [TestMethod]
        public void It_should_return_valid_Filter()
        {
            var filter = _result.resultset.Filters;
            filter.Should().NotBeNull();

            var subMenu = filter.SubMenu;
            subMenu.Should().NotBeNull();
            subMenu.Subs.Should().NotBeEmpty();
            foreach (var sub in subMenu.Subs)
            {
                sub.Name.Should().NotBeNullOrEmpty();
                sub.Href.Should().NotBeNullOrEmpty();
            }

            var path = filter.Path;
            path.Should().NotBeEmpty();
            foreach (var item in path)
            {
                item.Key.Should().NotBeNullOrEmpty();
                item.Value.Should().NotBeNullOrEmpty();
            }

            var sections = filter.FilterSections;
            sections.Should().NotBeEmpty();
            foreach (var section in sections)
            {
                section.Label.Should().NotBeNullOrEmpty();
                section.Note.Should().NotBeNullOrEmpty();
                section.FilterOptions.Should().NotBeEmpty();
                foreach (var option in section.FilterOptions)
                {
                    option.Label.Should().NotBeNullOrEmpty();
                    option.Value.Should().NotBeNullOrEmpty();
                    option.Note.Should().NotBeNullOrEmpty();
                }
            }

            var applied = filter.AppliedFilterSections;
            applied.Should().NotBeEmpty();
            foreach (var section in applied)
            {
                section.Label.Should().NotBeNullOrEmpty();
                section.Note.Should().NotBeNullOrEmpty();
                section.FilterOptions.Should().NotBeEmpty();
                foreach (var option in section.FilterOptions)
                {
                    option.Label.Should().NotBeNullOrEmpty();
                    option.Value.Should().NotBeNullOrEmpty();
                }
            }
        }

        [TestMethod]
        public void It_should_return_valid_Pager()
        {
            var pager = _result.resultset.Pager;

            pager.Should().NotBeNull();
            pager.PageSize.Should().BeGreaterThan(0);
            pager.RecordCount.Should().BeGreaterThan(0);
            pager.CurrentPage.Should().BeGreaterThan(0);
            pager.TotalPages.Should().BeGreaterThan(0);
            pager.TotalRecords.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void It_should_return_valid_list_of_Products()
        {
            var products = _result.resultset.Products;

            products.Should().NotBeEmpty();
            foreach (var product in products)
            {
                product.ProductId.Should().NotBeNullOrEmpty();
                product.Name.Should().NotBeNullOrEmpty();
                product.Pricing.Should().NotBeEmpty();
                foreach (var price in product.Pricing)
                {
                    price.Value.Should().BeGreaterThan(0.0);
                }
            }
        }
    }

    [Ignore]
    [TestClass]
    public class When_filtering_search_results
    {
        public static Response<SearchResponse> _result;
        public static Search _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var menu = new BaseIntegrationTest<Menu, MenuResponse>(config);
            var menuRequest = RequestBuilder.GetMenuRequest();
            var menuResponse = (Response<MenuResponse>)menu.TestObject.Execute(menuRequest);

            var search = new BaseIntegrationTest<Search, SearchResponse>();
            var searchRequest = RequestBuilder.GetFilteredCategorySearchRequestForMidCategory();
            _result = (Response<SearchResponse>)search.TestObject.Execute(searchRequest);

            _testObject = search.TestObject;
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
        public void It_should_return_valid_Sorter()
        {
            var sorter = _result.resultset.Sorter;

            sorter.Should().NotBeNull();
            sorter.SortBy.Should().NotBeNullOrEmpty();
            sorter.Options.Should().NotBeEmpty();
            foreach (var option in sorter.Options)
            {
                option.Name.Should().NotBeNullOrEmpty();
                option.Value.Should().NotBeNullOrEmpty();
            }
            sorter.Options.ToList().Find(x => x.IsSelected).Should().NotBeNull();
        }

        [TestMethod]
        public void It_should_return_valid_Filter()
        {
            var filter = _result.resultset.Filters;
            filter.Should().NotBeNull();

            var subMenu = filter.SubMenu;
            subMenu.Should().NotBeNull();
            subMenu.Subs.Should().NotBeEmpty();
            foreach (var sub in subMenu.Subs)
            {
                sub.Name.Should().NotBeNullOrEmpty();
                sub.Href.Should().NotBeNullOrEmpty();
            }

            var path = filter.Path;
            path.Should().NotBeEmpty();
            foreach (var item in path)
            {
                item.Key.Should().NotBeNullOrEmpty();
                item.Value.Should().NotBeNullOrEmpty();
            }

            var sections = filter.FilterSections;
            sections.Should().NotBeEmpty();
            foreach (var section in sections)
            {
                section.Label.Should().NotBeNullOrEmpty();
                section.Note.Should().NotBeNullOrEmpty();
                section.FilterOptions.Should().NotBeEmpty();
                foreach (var option in section.FilterOptions)
                {
                    option.Label.Should().NotBeNullOrEmpty();
                    option.Value.Should().NotBeNullOrEmpty();
                    option.Note.Should().NotBeNullOrEmpty();
                }
            }

            var applied = filter.AppliedFilterSections;
            applied.Should().NotBeEmpty();
            foreach (var section in applied)
            {
                section.Label.Should().NotBeNullOrEmpty();
                section.Note.Should().NotBeNullOrEmpty();
                section.FilterOptions.Should().NotBeEmpty();
                foreach (var option in section.FilterOptions)
                {
                    option.Label.Should().NotBeNullOrEmpty();
                    option.Value.Should().NotBeNullOrEmpty();
                }
            }
        }

        [TestMethod]
        public void It_should_return_valid_Pager()
        {
            var pager = _result.resultset.Pager;

            pager.Should().NotBeNull();
            pager.PageSize.Should().BeGreaterThan(0);
            pager.RecordCount.Should().BeGreaterThan(0);
            pager.CurrentPage.Should().BeGreaterThan(0);
            pager.TotalPages.Should().BeGreaterThan(0);
            pager.TotalRecords.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void It_should_return_valid_list_of_Products()
        {
            var products = _result.resultset.Products;

            products.Should().NotBeEmpty();
            foreach (var product in products)
            {
                product.ProductId.Should().NotBeNullOrEmpty();
                product.Name.Should().NotBeNullOrEmpty();
                product.Pricing.Should().NotBeEmpty();
                foreach (var price in product.Pricing)
                {
                    price.Value.Should().BeGreaterThan(0.0);
                }
            }
        }
    }

    [Ignore]
    [TestClass]
    public class When_searching_by_Term_and_Sorting
    {
        public static Response<SearchResponse> _result;
        public static Search _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var menu = new BaseIntegrationTest<Menu, MenuResponse>(config);
            var menuRequest = RequestBuilder.GetMenuRequest();
            var menuResponse = (Response<MenuResponse>)menu.TestObject.Execute(menuRequest);

            var search = new BaseIntegrationTest<Search, SearchResponse>();
            var searchRequest = RequestBuilder.GetTermSearchRequestWithSorting();
            _result = (Response<SearchResponse>)search.TestObject.Execute(searchRequest);

            _testObject = search.TestObject;
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
        public void It_should_return_valid_Sorter()
        {
            var sorter = _result.resultset.Sorter;

            sorter.Should().NotBeNull();
            sorter.SortBy.Should().NotBeNullOrEmpty();
            sorter.Options.Should().NotBeEmpty();
            foreach (var option in sorter.Options)
            {
                option.Name.Should().NotBeNullOrEmpty();
                option.Value.Should().NotBeNullOrEmpty();
            }
            sorter.Options.ToList().Find(x => x.IsSelected).Should().NotBeNull();
        }

        [TestMethod]
        public void It_should_return_valid_Filter()
        {
            var filter = _result.resultset.Filters;
            filter.Should().NotBeNull();

            var subMenu = filter.SubMenu;
            subMenu.Should().NotBeNull();
            subMenu.Subs.Should().NotBeEmpty();
            foreach (var sub in subMenu.Subs)
            {
                sub.Name.Should().NotBeNullOrEmpty();
                sub.Href.Should().NotBeNullOrEmpty();
            }

            var path = filter.Path;
            path.Should().NotBeEmpty();
            foreach (var item in path)
            {
                item.Key.Should().NotBeNullOrEmpty();
                item.Value.Should().NotBeNullOrEmpty();
            }

            var sections = filter.FilterSections;
            sections.Should().NotBeEmpty();
            foreach (var section in sections)
            {
                section.Label.Should().NotBeNullOrEmpty();
                section.Note.Should().NotBeNullOrEmpty();
                section.FilterOptions.Should().NotBeEmpty();
                foreach (var option in section.FilterOptions)
                {
                    option.Label.Should().NotBeNullOrEmpty();
                    option.Value.Should().NotBeNullOrEmpty();
                    option.Note.Should().NotBeNullOrEmpty();
                }
            }
        }

        [TestMethod]
        public void It_should_return_valid_Pager()
        {
            var pager = _result.resultset.Pager;

            pager.Should().NotBeNull();
            pager.PageSize.Should().BeGreaterThan(0);
            pager.RecordCount.Should().BeGreaterThan(0);
            pager.CurrentPage.Should().BeGreaterThan(0);
            pager.TotalPages.Should().BeGreaterThan(0);
            pager.TotalRecords.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void It_should_return_valid_list_of_Products()
        {
            var products = _result.resultset.Products;

            products.Should().NotBeEmpty();
            foreach (var product in products)
            {
                product.ProductId.Should().NotBeNullOrEmpty();
                product.Name.Should().NotBeNullOrEmpty();
                product.Pricing.Should().NotBeEmpty();
                foreach (var price in product.Pricing)
                {
                    price.Value.Should().BeGreaterThan(0.0);
                }
            }
        }
    }

    [Ignore]
    [TestClass]
    public class When_getting_Next_Page_of_search
    {
        public static Response<SearchResponse> _result;
        public static Search _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };
            
            var menu = new BaseIntegrationTest<Menu, MenuResponse>(config);
            var menuRequest = RequestBuilder.GetMenuRequest();
            var menuResponse = (Response<MenuResponse>)menu.TestObject.Execute(menuRequest);

            var search = new BaseIntegrationTest<Search, SearchResponse>();
            var searchRequest = RequestBuilder.GetNextPageCategorySearchRequestForMidCategory();
            _result = (Response<SearchResponse>)search.TestObject.Execute(searchRequest);

            _testObject = search.TestObject;
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
        public void It_should_return_valid_Sorter()
        {
            var sorter = _result.resultset.Sorter;

            sorter.Should().NotBeNull();
            sorter.SortBy.Should().NotBeNullOrEmpty();
            sorter.Options.Should().NotBeEmpty();
            foreach (var option in sorter.Options)
            {
                option.Name.Should().NotBeNullOrEmpty();
                option.Value.Should().NotBeNullOrEmpty();
            }
            sorter.Options.ToList().Find(x => x.IsSelected).Should().NotBeNull();
        }

        [TestMethod]
        public void It_should_return_valid_Filter()
        {
            var filter = _result.resultset.Filters;
            filter.Should().NotBeNull();

            var subMenu = filter.SubMenu;
            subMenu.Should().NotBeNull();
            subMenu.Subs.Should().NotBeEmpty();
            foreach (var sub in subMenu.Subs)
            {
                sub.Name.Should().NotBeNullOrEmpty();
                sub.Href.Should().NotBeNullOrEmpty();
            }

            var path = filter.Path;
            path.Should().NotBeEmpty();
            foreach (var item in path)
            {
                item.Key.Should().NotBeNullOrEmpty();
                item.Value.Should().NotBeNullOrEmpty();
            }

            var sections = filter.FilterSections;
            sections.Should().NotBeEmpty();
            foreach (var section in sections)
            {
                section.Label.Should().NotBeNullOrEmpty();
                section.Note.Should().NotBeNullOrEmpty();
                section.FilterOptions.Should().NotBeEmpty();
                foreach (var option in section.FilterOptions)
                {
                    option.Label.Should().NotBeNullOrEmpty();
                    option.Value.Should().NotBeNullOrEmpty();
                    option.Note.Should().NotBeNullOrEmpty();
                }
            }

            var applied = filter.AppliedFilterSections;
            applied.Should().NotBeEmpty();
            foreach (var section in applied)
            {
                section.Label.Should().NotBeNullOrEmpty();
                section.Note.Should().NotBeNullOrEmpty();
                section.FilterOptions.Should().NotBeEmpty();
                foreach (var option in section.FilterOptions)
                {
                    option.Label.Should().NotBeNullOrEmpty();
                    option.Value.Should().NotBeNullOrEmpty();
                }
            }
        }

        [TestMethod]
        public void It_should_return_valid_Pager()
        {
            var pager = _result.resultset.Pager;

            pager.Should().NotBeNull();
            pager.PageSize.Should().BeGreaterThan(0);
            pager.RecordCount.Should().BeGreaterThan(0);
            pager.CurrentPage.Should().BeGreaterThan(0);
            pager.TotalPages.Should().BeGreaterThan(0);
            pager.TotalRecords.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void It_should_return_valid_list_of_Products()
        {
            var products = _result.resultset.Products;

            products.Should().NotBeEmpty();
            foreach (var product in products)
            {
                product.ProductId.Should().NotBeNullOrEmpty();
                product.Name.Should().NotBeNullOrEmpty();
                product.Pricing.Should().NotBeEmpty();
                foreach (var price in product.Pricing)
                {
                    price.Value.Should().BeGreaterThan(0.0);
                }
            }
        }
    }

    //[Ignore]
    [TestClass]
    public class When_searching_for_products_by_Term_and_filtering_by_Price
    {
        public static Response<SearchResponse> _result;
        public static Search _testObject;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            var config = new TestConfig
            {
                ResetHttpContext = true
            };

            var menu = new BaseIntegrationTest<Menu, MenuResponse>(config);
            var menuRequest = RequestBuilder.GetMenuRequest();
            var menuResponse = (Response<MenuResponse>)menu.TestObject.Execute(menuRequest);

            var search = new BaseIntegrationTest<Search, SearchResponse>();
            var searchRequest = RequestBuilder.GetTermSearchRequestFilteredByPrice();
            

            _result = (Response<SearchResponse>)search.TestObject.Execute(searchRequest);

            _testObject = search.TestObject;
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
        public void It_should_return_valid_Sorter()
        {
            var sorter = _result.resultset.Sorter;

            sorter.Should().NotBeNull();
            sorter.SortBy.Should().NotBeNullOrEmpty();
            sorter.Options.Should().NotBeEmpty();
            foreach (var option in sorter.Options)
            {
                option.Name.Should().NotBeNullOrEmpty();
                option.Value.Should().NotBeNullOrEmpty();
            }
            sorter.Options.ToList().Find(x => x.IsSelected).Should().NotBeNull();
        }

        [TestMethod]
        public void It_should_return_valid_Filter()
        {
            var filter = _result.resultset.Filters;
            filter.Should().NotBeNull();

            var subMenu = filter.SubMenu;
            subMenu.Should().NotBeNull();
            subMenu.Subs.Should().NotBeEmpty();
            foreach (var sub in subMenu.Subs)
            {
                sub.Name.Should().NotBeNullOrEmpty();
                sub.Href.Should().NotBeNullOrEmpty();
            }

            var path = filter.Path;
            path.Should().NotBeEmpty();
            foreach (var item in path)
            {
                item.Key.Should().NotBeNullOrEmpty();
                item.Value.Should().NotBeNullOrEmpty();
            }

            var sections = filter.FilterSections;
            sections.Should().NotBeEmpty();
            foreach (var section in sections)
            {
                section.Label.Should().NotBeNullOrEmpty();
                section.Note.Should().NotBeNullOrEmpty();
                section.FilterOptions.Should().NotBeEmpty();
                foreach (var option in section.FilterOptions)
                {
                    option.Label.Should().NotBeNullOrEmpty();
                    option.Value.Should().NotBeNullOrEmpty();
                    option.Note.Should().NotBeNullOrEmpty();
                }
            }

            var applied = filter.AppliedFilterSections;
            applied.Should().NotBeEmpty();
            foreach (var section in applied)
            {
                section.Label.Should().NotBeNullOrEmpty();
                section.Note.Should().NotBeNullOrEmpty();
                section.FilterOptions.Should().NotBeEmpty();
                foreach (var option in section.FilterOptions)
                {
                    option.Label.Should().NotBeNullOrEmpty();
                    option.Value.Should().NotBeNullOrEmpty();
                }
            }
        }

        [TestMethod]
        public void It_should_return_valid_Pager()
        {
            var pager = _result.resultset.Pager;

            pager.Should().NotBeNull();
            pager.PageSize.Should().BeGreaterThan(0);
            pager.RecordCount.Should().BeGreaterThan(0);
            pager.CurrentPage.Should().BeGreaterThan(0);
            pager.TotalPages.Should().BeGreaterThan(0);
            pager.TotalRecords.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void It_should_return_valid_list_of_Products()
        {
            var products = _result.resultset.Products;

            products.Should().NotBeEmpty();
            foreach (var product in products)
            {
                product.ProductId.Should().NotBeNullOrEmpty();
                product.Name.Should().NotBeNullOrEmpty();
                product.Pricing.Should().NotBeEmpty();
                foreach (var price in product.Pricing)
                {
                    price.Value.Should().BeGreaterThan(0.0);
                }
            }
        }
    }
}

