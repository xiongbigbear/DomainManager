
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
        private DomainParser parser = null;
        public object[] Parameters = null;

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
        public DelegateCommand LoadCommand { get;  set; }
        public DelegateCommand RunCommand { get;  set; }
        public DelegateCommand RemoveCommand { get;  set; }

        public MainWindowVM(MainWindow v)
        {
            view = v;
            LoadCommand = new DelegateCommand(Load);
            RunCommand = new DelegateCommand(Run);
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
                    parser?.Unload();
                    var file = openFileDialog.FileName;
                    resolver = new DefaultAssemblyResolver(RevitPath);
                    parser = new DomainParser() { revitDirectory = RevitPath };
                    parser.CreateParser();
                    var assembly = parser.ReadAssembly(file);
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
                    parser?.Unload();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                parser?.Unload();
            }
        }



        private void Run()
        {
            try
            {
                resolver = new DefaultAssemblyResolver(RevitPath);
                parser = new DomainParser() { revitDirectory = RevitPath };
                parser.CreateParser();
                parser.Run(SelectedModel.Path, SelectedModel.Name, Parameters);
           
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                parser?.Unload();
            }
        }



        private void Remove()
        {
            Models.Remove(selectedmodel);
        }


    }
}
