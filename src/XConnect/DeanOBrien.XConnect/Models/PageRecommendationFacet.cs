using Sitecore.XConnect;
using System;
using System.Collections.Generic;


namespace DeanOBrien.XConnect.Models
{
    [Serializable]
    [FacetKey(DefaultFacetKey)]
    public class PageRecommendationFacet : Facet
    {
        public const string DefaultFacetKey = "PageRecommendationFacet";

        public PageRecommendationFacet()
        {
            PageRecommendations = new List<PageRecommendation>();
        }
        public List<PageRecommendation> PageRecommendations { get; set; }
    }
}
