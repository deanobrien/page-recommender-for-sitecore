using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeanOBrien.XConnect.Models
{
    public class PageCollectionModel
    {
        public static XdbModel Model { get; } = BuildModel();

        private static XdbModel BuildModel()
        {
            XdbModelBuilder modelBuilder = new XdbModelBuilder("PageCollectionModel", new XdbModelVersion(1, 0));

            // Reference the default collection model
            modelBuilder.ReferenceModel(CollectionModel.Model);

            // Register contact facets
            modelBuilder.DefineFacet<Contact, PageRecommendationFacet>(PageRecommendationFacet.DefaultFacetKey);

            return modelBuilder.BuildModel();
        }
    }
}
