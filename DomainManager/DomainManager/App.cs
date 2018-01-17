using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DomainManager
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    public class App : IExternalApplication
    {
        public string m_AssemblyPath = "";
        public string m_ribbonPath = "";
        private string strProductNameChinese = "Addin";
        private string tabName = "DomainManager";
        private string strPanelChinese = "DomainMgr";
        private static string MutexName = "InterProcessSyncName";
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            m_AssemblyPath = typeof(App).Assembly.Location;
            m_ribbonPath = Path.GetDirectoryName(m_AssemblyPath) + "\\Icons";
            try
            {
                application.CreateRibbonTab(tabName);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                return Result.Succeeded;
            }
            finally
            {
                Autodesk.Revit.UI.RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, strPanelChinese);
                PushButtonData familyParameterEdit = new PushButtonData("familyParameterEdit", strProductNameChinese, m_AssemblyPath, typeof(Addin).FullName);
                familyParameterEdit.LargeImage = new BitmapImage(new Uri(m_ribbonPath + "\\familyParameterEdit.png"));
                familyParameterEdit.ToolTip = "Domain Manager tool";
                ribbonPanel.AddItem(familyParameterEdit);

            }

        }
       
    }
}
