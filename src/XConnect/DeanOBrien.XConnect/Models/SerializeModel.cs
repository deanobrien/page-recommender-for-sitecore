using Sitecore.XConnect.Schema;
using Sitecore.XConnect.Serialization;
using System.IO;

namespace DeanOBrien.XConnect.Models
{
    class SerializeModel
    {
        // This is the code required to serialize the collection model.
        // Run this in a console app prior to deploying and copy the JSON file to XConnect as required
        // See generated output "PageCollectionModel, 1.0.json"

        private XdbModel _xDbModel = null;
        private void SerializeXConnectModel()
        {
            _xDbModel = PageCollectionModel.Model;
            var json = XdbModelWriter.Serialize(_xDbModel);

            var filename = _xDbModel + ".json";
            File.WriteAllText(filename, json);
        }
    }
}
