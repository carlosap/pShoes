using System;
using System.Collections.Generic;
using System.Linq;
using MadServ.Core.Models;

namespace Library.Models
{
    [Serializable]
    public class ProductReview : ProductReviewBase
    {
        public List<ProductRatingBase> Ratings { get; set; }
        new public string Date { get; set; }
        public string Pros { get; set; }
        public string Cons { get; set; }

        public ProductReview()
        {
            Ratings = new List<ProductRatingBase>();
            Pros = string.Empty;
            Cons = string.Empty;
        }

        public ProductReview(ProductReviewDTO dto)
            : this()
        {
            if (dto != null)
            {
                Date = dto.SubmissionTime.ToString("MMMM dd, yyyy");
                Description = dto.ReviewText;
                Title = dto.Title;
                User = dto.UserNickname;

                if (dto.TagDimensions.Pro.Values.Any())
                    Pros = string.Join(", ", dto.TagDimensions.Pro.Values);

                if (dto.TagDimensions.Con.Values.Any())
                    Cons = string.Join(", ", dto.TagDimensions.Con.Values);

                Ratings.Add(new ProductRatingBase
                {
                    Title = "Overall Rating",
                    Rating = dto.Rating,
                    RatingImage = new Image
                    {
                        Src = string.Format(Config.Urls.ProductRatingStarsTemplate, dto.Rating)
                    }
                });

                if (dto.SecondaryRatings.Value != null)
                    Ratings.Add(SecondaryRating.GetProductRatingBase(dto.SecondaryRatings.Value));

                if (dto.SecondaryRatings.Comfort != null)
                    Ratings.Add(SecondaryRating.GetProductRatingBase(dto.SecondaryRatings.Comfort));

                if (dto.SecondaryRatings.Quality != null)
                    Ratings.Add(SecondaryRating.GetProductRatingBase(dto.SecondaryRatings.Quality));

                if (dto.SecondaryRatings.Style != null)
                    Ratings.Add(SecondaryRating.GetProductRatingBase(dto.SecondaryRatings.Style));

                if (dto.SecondaryRatings.ShoeSize != null)
                    Ratings.Add(SecondaryRating.GetProductRatingBase(dto.SecondaryRatings.ShoeSize));

                if (dto.SecondaryRatings.ShoeWidth != null)
                    Ratings.Add(SecondaryRating.GetProductRatingBase(dto.SecondaryRatings.ShoeWidth));
            }
        }
    }

    public class ProductReviewDTO
    {
        public TagDimensions TagDimensions { get; set; }
        public string UserNickname { get; set; }
        public string UserLocation { get; set; }
        public string ProductId { get; set; }
        public string Title { get; set; }
        public DateTime SubmissionTime { get; set; }
        public int Rating { get; set; }
        public int RatingRange { get; set; }
        public string ReviewText { get; set; }
        public SecondaryRatings SecondaryRatings { get; set; }

        public ProductReviewDTO()
        {
            TagDimensions = new TagDimensions();
            SecondaryRatings = new SecondaryRatings();
        }
    }

    public class TagDimensions
    {
        public TagDimension Pro { get; set; }
        public TagDimension Con { get; set; }

        public TagDimensions()
        {
            Pro = new TagDimension();
            Con = new TagDimension();
        }
    }

    public class TagDimension
    {
        public List<string> Values { get; set; }

        public TagDimension()
        {
            Values = new List<string>();
        }
    }

    public class SecondaryRatings
    {
        public SecondaryRating ShoeWidth { get; set; }
        public SecondaryRating Value { get; set; }
        public SecondaryRating Quality { get; set; }
        public SecondaryRating Style { get; set; }
        public SecondaryRating ShoeSize { get; set; }
        public SecondaryRating Comfort { get; set; }
    }

    public class SecondaryRating
    {
        public int Value { get; set; }
        public string ValueLabel { get; set; }
        public string MaxLabel { get; set; }
        public string Label { get; set; }
        public int ValueRange { get; set; }
        public string MinLabel { get; set; }
        public string DisplayType { get; set; }

        public static ProductRatingBase GetProductRatingBase(SecondaryRating secondaryRating)
        {
            var result = new ProductRatingBase();

            string type = "Secondary";
            if (secondaryRating.Label.ToLower().Contains("size") || secondaryRating.Label.ToLower().Contains("width"))
                type = "Slider";

            result.Rating = secondaryRating.Value;
            result.Title = secondaryRating.Label;
            result.RatingImage = new Image
            {
                Title = secondaryRating.ValueLabel,
                Src = string.Format(Config.Urls.ProductRatingBarsTemplate, secondaryRating.Value, type)
            };

            return result;
        }
    }
}
