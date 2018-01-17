using Microsoft.Practices.Prism.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainManager
{
    [Serializable]
    public class AssemblyData:NotificationObject
    {
        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                this.RaisePropertyChanged(() => Name);
            }
        }

        private string path;

        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                this.RaisePropertyChanged(() => Path);
            }
        }

        private AssemblyData parent;

        public AssemblyData Parent
        {
            get { return parent; }
            set
            {
                parent = value;
                this.RaisePropertyChanged(() => Parent);
            }
        }

        private ObservableCollection<AssemblyData> children;

        public ObservableCollection<AssemblyData> Children
        {
            get { return children; }
            set
            {
                children = value;
                this.RaisePropertyChanged(() => Children);
            }
        }


    }
}
