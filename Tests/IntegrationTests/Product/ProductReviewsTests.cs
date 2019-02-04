//using System;
//using Library.IRequests;
//using MadServ.Core.Models.Responses;
//using MadServ.Core.Tests;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Tests.SampleData;
//using FluentAssertions;
//using Library.Models;
//using MadServ.Core.Models;
//using Library.Models.Responses;
//using System.Linq;
//using Library.Models.Requests;
//using MadServ.Core.Models.Responses.PrimitiveResponses;
//using System.Text.RegularExpressions;
//using System.Globalization;

//namespace Tests.IntegrationTests.Product.ProductReviewsTests
//{
//    [TestClass]
//    public class When_getting_ProductReviews
//    {
//        //public static Response<ProductReviewsResponse> _result;
//        //public static ProductReviews _testObject;

//        [ClassInitialize]
//        public static void Initialize(TestContext context)
//        {
//            var config = new TestConfig
//            {
//                ResetHttpContext = true
//            };

//            //var productReviews = new BaseIntegrationTest<ProductReviews, ProductReviewsResponse>(config);
//            //var productReviewsRequest = RequestBuilder.GetProductReviewsRequest();
//            //_result = (Response<ProductReviewsResponse>)productReviews.TestObject.Execute(productReviewsRequest);

//            //_testObject = productReviews.TestObject;
//        }

//        [TestMethod]
//        public void It_should_return_empty_list_of_errors()
//        {
//            //_testObject._errors.Should().NotBeNull();
//            //_testObject._errors.Should().BeEmpty();

//            _result.errors.Should().NotBeNull();
//            _result.errors.Should().BeEmpty();
//        }

//        [TestMethod]
//        public void It_should_return_valid_Rating()
//        {
//            //_result.resultset.ProductRating.NumberOfTimesRated.Should().BeGreaterThan(0);
//        }

//        [TestMethod]
//        public void It_should_return_valid_Reviews()
//        {
//            var reviews = _result.resultset.Reviews;

//            reviews.Should().NotBeEmpty();
//            foreach (var review in reviews)
//            {
//                review.User.Should().NotBeNullOrEmpty();
//                review.Date.Should().NotBeNullOrEmpty();
//                review.Title.Should().NotBeNullOrEmpty();
//                review.Description.Should().NotBeNullOrEmpty();
//                review.Ratings.Should().NotBeEmpty();

//                foreach (var rating in review.Ratings)
//                {
//                    rating.Rating.Should().BeGreaterThan(0);
//                    rating.Title.Should().NotBeNullOrEmpty();
//                    rating.RatingImage.Src.Should().NotBeNullOrEmpty();
//                }
//            }
//        }
//    }
//}




