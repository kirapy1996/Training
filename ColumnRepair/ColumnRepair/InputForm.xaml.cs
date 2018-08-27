using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;

namespace ColumnRepair
{
    /// <summary>
    /// Interaction logic for InputForm.xaml
    /// </summary>
    public partial class InputForm : Window
    {
        public InputForm()
        {
            InitializeComponent();
            DataContext = Singleton.Instance.WPFData;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            List<Reference> rfs= Singleton.Instance.RevitData.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new GridSelectionFilter())
                as List<Reference>;
            List<Autodesk.Revit.DB.Grid> grids = rfs.Select(x => Singleton.Instance.RevitData.Document.GetElement(x) as Autodesk.Revit.DB.Grid).ToList();
            grids.ForEach(x => Singleton.Instance.WPFData.Grids.Add(x));
            ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Hide();
            List<Reference> rfs1 = Singleton.Instance.RevitData.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new ColumnSelectionFilter())
                as List<Reference>;
            List<Autodesk.Revit.DB.Element> columns = rfs1.Select(x => Singleton.Instance.RevitData.Document.GetElement(x) as Autodesk.Revit.DB.Element).ToList();
            columns.ForEach(x => Singleton.Instance.WPFData.Elements.Add(x));
            ShowDialog();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
