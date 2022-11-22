using Microsoft.ML.Data;
namespace DeanOBrien.XConnect.Models.ML
{  
    public class PageRating
    {
        [LoadColumn(0)]
        public string contactId;
        [LoadColumn(1)]
        public string pageId;
        [LoadColumn(2)]
        public float Label;
    }
}
