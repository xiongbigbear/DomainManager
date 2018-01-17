
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System.Windows.Documents;
using System.IO;

namespace DomainManager
{
    public class MainWindowVM : NotificationObject
    {
        private MainWindow view = null;
        private string revitPath;
        private DefaultAssemblyResolver resolver = null;
        private AppDomain tempDomain = null;
        private AssemblyLoader loader = null;

        public string RevitPath
        {
            get { return revitPath; }
            set { revitPath = value; }
        }


        private ObservableCollection<AssemblyData> models = new ObservableCollection<AssemblyData>();

        public ObservableCollection<AssemblyData> Models
        {
            get { return models; }
            set
            {
                models = value;
                this.RaisePropertyChanged(() => Models);
            }
        }
        private AssemblyData selectedmodel;

        public AssemblyData SelectedModel
        {
            get { return selectedmodel; }
            set
            {
                selectedmodel = value;
                this.RaisePropertyChanged(() => SelectedModel);
            }
        }
        public DelegateCommand LoadCommand { get; private set; }
        public DelegateCommand RunCommand { get; private set; }
        public DelegateCommand RemoveCommand { get; private set; }

        public MainWindowVM(MainWindow v)
        {
            view = v;
            LoadCommand = new DelegateCommand(Load);
            RunCommand = new DelegateCommand(Run, () => SelectedModel == null ? false : SelectedModel.Children.Count==0);
            RemoveCommand = new DelegateCommand(Remove, () => SelectedModel == null ? false : SelectedModel.Parent == null);
        }

       

        private void Load()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "请选择要加载的程序集..."
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    WsdlClassParser a = new WsdlClassParser();
                    a.CreateWsdlClassParser();
                    var file = openFileDialog.FileName;
                    tempDomain = tempDomain ?? AppDomain.CreateDomain("addinDomain",AppDomain.CurrentDomain.Evidence,AppDomain.CurrentDomain.SetupInformation);
                    resolver = new DefaultAssemblyResolver();
                    var s = resolver.revitDirectory;
                    //tempDomain.AssemblyResolve += resolver.CurrentDomain_AssemblyResolve;
                    var ss = resolver.test();
                    loader = (AssemblyLoader)tempDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().Location, "DomainManager.AssemblyLoader", false, 0, null, null, CultureInfo.InvariantCulture, null);

                    var assembly = loader.ReadAssembly(file);
                    if (assembly == null)
                    {
                        MessageBox.Show("resolve failed");
                        return;
                    }
                    var types = assembly.GetTypes().Where(x => x.GetAttribue<TransactionAttribute>() != null && x.GetInterface(typeof(IExternalCommand).FullName) != null).ToList();
                    if (types.Count == 0)
                    {
                        return;
                    }
                    var addin = Models?.FirstOrDefault(x => x.Name == assembly.GetName().Name);
                    if (addin != null)
                    {
                        Models.Remove(addin);
                    }
                    var model = new AssemblyData() { Name = assembly.GetName().Name, Path = file };
                    foreach (var item in types)
                    {
                        model.Children.Add(new AssemblyData()
                        {
                            Name = item.FullName,
                            Path = file,
                            Parent = model
                        });
                    }
                    Models.Add(model);
                    AppDomain.Unload(tempDomain);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

     

        private void Run()
        {
            //resolver = new DefaultAssemblyResolver(RevitPath, null);
            tempDomain = tempDomain ?? AppDomain.CreateDomain("addinDomain");
            loader = loader ?? (AssemblyLoader)tempDomain.CreateInstanceFromAndUnwrap
                (Assembly.GetExecutingAssembly().Location, "RTF.Framework.AssemblyLoader",
                false, 0, null, new object[] { resolver }, CultureInfo.InvariantCulture, null);
            var assembly = loader.ReadAssembly(SelectedModel.Path);
            if (assembly == null)
            {
                MessageBox.Show("resolve failed");
                return;
            }
            var type = assembly.GetType(SelectedModel.Name);
            if (type == null)
            {
                System.Windows.MessageBox.Show(string.Format(" not found {0} type", SelectedModel.Name));
                return;
            }
            var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, new Type[0], null);
            if (ctor == null)
            {
                System.Windows.MessageBox.Show(string.Format("{0} type not contain empty Constructor", SelectedModel.Name));
                return;
            }
            var method = type.GetMethod("Execute", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
            {
                System.Windows.MessageBox.Show(string.Format("not found {0} Execute", method.Name));
                return;
            }
            view.Close();
            var instance = ctor.Invoke(null);
            method.Invoke(instance, new object[] { null });
            AppDomain.Unload(tempDomain);
        }

        

        private void Remove()
        {
            Models.Remove(selectedmodel);
        }


    }
}
