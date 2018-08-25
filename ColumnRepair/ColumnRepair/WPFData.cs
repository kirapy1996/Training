using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ColumnRepair
{
    public class WPFData: INotifyPropertyChanged
    {
        private ObservableCollection<Grid> grids;
        private ObservableCollection<Element> elements;

        public ObservableCollection<Grid> Grids
        {
            get
            {
                if (grids == null) grids = new ObservableCollection<Grid>();
                return grids;
            }
            set
            {
                grids = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Element> Elements
        {
            get
            {
                if (elements == null) elements = new ObservableCollection<Element>();
                return elements;
            }
            set
            {
                elements = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
