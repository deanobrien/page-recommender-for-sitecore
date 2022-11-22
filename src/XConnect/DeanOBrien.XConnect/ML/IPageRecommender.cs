using DeanOBrien.XConnect.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeanOBrien.XConnect.ML
{
    public interface IPageRecommender
    {
        void Train(List<PageEngagement> data);
        float Predict(string cId, string crseId);
    }
}
