using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Windows;
using Newtonsoft.Json;

namespace testDll
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            var ctor = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance));
            JsonConvert.SerializeObject("ss");
            MessageBox.Show(ctor.Count().ToString());
            return Result.Succeeded;
        }
    }
}
