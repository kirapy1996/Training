using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
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

namespace dinhthi01
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

        private void Frame_Checked(object sender, RoutedEventArgs e)
        {
            if (Frame.IsChecked == true)
            {
                Column.IsChecked = false;
                Walls.IsChecked = false;
            }
        }

        private void Column_Checked(object sender, RoutedEventArgs e)
        {
            if (Column.IsChecked == true)
            {
                Frame.IsChecked = false;
                Walls.IsChecked = false;
            }

        }

        private void Walls_Checked(object sender, RoutedEventArgs e)
        {
            if (Walls.IsChecked == true)
            {
                Column.IsChecked = false;
                Frame.IsChecked = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
                       
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Hide();            
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
