
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
        private AssemblyLoader loader = null;
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
        public DelegateCommand LoadCommand { get; set; }
        public DelegateCommand RunCommand { get; set; }
        public DelegateCommand RemoveCommand { get; set; }

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
                    var file = openFileDialog.FileName;
                    AssemblyResolver resolver = new AssemblyResolver(RevitPath, null);
                    AssemblyLoader loader = new AssemblyLoader(resolver);
                    var model = loader.LoadAssembly(file);
                    if (Models.Where(a => a.Path == file).Count() > 0)
                    {
                        Models.Remove(model);
                    }
                    Models.Add(model);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                
            }
        }

        private void Run()
        {
            try
            {
                loader.RunAssembly(SelectedModel, Parameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }



        private void Remove()
        {
            Models.Remove(selectedmodel);
        }


    }
}
