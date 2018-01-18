using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Reflection;
using System.Windows;
using System.IO;

namespace DomainManager
{
    [Transaction(TransactionMode.Manual)]
    public class Addin : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var doc = commandData.Application.ActiveUIDocument.Document;
                MainWindow view = new MainWindow();
                MainWindowVM vm = new MainWindowVM(view);
                view.DataContext = vm;
                vm.Parameters = new object[] { commandData, message, elements };
                vm.RevitPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(IExternalCommand)).Location);
                view.ShowDialog();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                return Result.Failed;
            }
        }
    }
}
