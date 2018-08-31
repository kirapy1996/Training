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
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;

namespace dinhthi01
{
    /// <summary>
    /// Interaction logic for InputForm.xaml
    /// </summary>
    public partial class InputForm : Window
    {
        int th = new int();
        const int th1 = 0;
        const int th2 = 1;
        const int th3 = 2;
        float roundto = new float();
        const double precision = 0.00001;

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
                th = th1;
            }
        }

        private void Column_Checked(object sender, RoutedEventArgs e)
        {
            if (Column.IsChecked == true)
            {
                Frame.IsChecked = false;
                Walls.IsChecked = false;
                th = th2;
            }

        }

        private void Walls_Checked(object sender, RoutedEventArgs e)
        {
            if (Walls.IsChecked == true)
            {
                Column.IsChecked = false;
                Frame.IsChecked = false;
                th = th3;
            }
        }

        private void OK(object sender, RoutedEventArgs e)
        {
            
            if (txtround.Text == "")
            {
                TaskDialog.Show("ERROR", " Please Enter The Round Value");
                return;
            }
            if (Frame.IsChecked == false && Walls.IsChecked == false && Column.IsChecked == false)
            {
                TaskDialog.Show("ERROR", " Please Select Element Types");
                return;
            }
            
            Hide();
            Document doc = Singleton.Instance.RevitData.Document;
            
            Reference gridrefX = Singleton.Instance.RevitData.Selection.PickObject(ObjectType.Element, new GridSelectionFilter(),"Select The First Grid");
            Autodesk.Revit.DB.Grid gridX = doc.GetElement(gridrefX) as Autodesk.Revit.DB.Grid;
            Reference gridrefY = Singleton.Instance.RevitData.Selection.PickObject(ObjectType.Element, new GridSelectionFilter(),"Select The Second Grid");
            Autodesk.Revit.DB.Grid gridY = doc.GetElement(gridrefY) as Autodesk.Revit.DB.Grid;

            switch (th)
            {
                case th1:
                    {
                        List<Element> elemlist = Singleton.Instance.RevitData.Selection.PickElementsByRectangle(new BeamSelectionFilter()) as List<Element>;
                        using (Transaction transactionbeam = new Transaction(doc))
                        {
                            transactionbeam.Start("Beam Moving");
                            foreach (Element elem in elemlist)
                        {

                            LocationCurve elemcurve = elem.Location as LocationCurve;
                            Autodesk.Revit.DB.Line elemline = elemcurve.Curve as Autodesk.Revit.DB.Line;
                            Autodesk.Revit.DB.Line gridlineX = gridX.Curve as Autodesk.Revit.DB.Line;
                            Autodesk.Revit.DB.Line gridlineY = gridY.Curve as Autodesk.Revit.DB.Line;

                            //Kiểm tra và đặt tên Grid, GridX song song với Location.Curve - Line, Grid Y vuông góc,...

                            if (Geometry.GeomUtil.IsSameOrOppositeDirection(elemline.Direction, gridlineY.Direction))
                            {
                                gridX = doc.GetElement(gridrefY) as Autodesk.Revit.DB.Grid;
                                gridY = doc.GetElement(gridrefX) as Autodesk.Revit.DB.Grid;
                                gridlineX = gridX.Curve as Autodesk.Revit.DB.Line;
                                gridlineY = gridY.Curve as Autodesk.Revit.DB.Line;
                            }
                            double movedis = Command.GetMoveDistance(elemline, gridX, roundto);
                            // Move dầm phương song song
                            if (movedis > precision)
                            {
                                XYZ movevector1 = new XYZ(-elemline.Direction.Y, elemline.Direction.X, 0).Normalize() * Geometry.GeomUtil.milimeter2Feet(movedis);
                                elem.Location.Move(movevector1);
                                    if (Math.Abs(Command.GetMoveDistance(elemline, gridX, roundto) % roundto) > precision && Math.Abs(Command.GetMoveDistance(elemline, gridX, roundto) % roundto) < roundto - precision)
                                    {
                                        elem.Location.Move(-2 * movevector1);
                                    }
                            }
                            movedis = Command.GetMoveDistance(elemline.GetEndPoint(0), gridY, roundto);
                            if (movedis >= precision)
                            {
                                XYZ movevector2 = new XYZ(elemline.Direction.X, elemline.Direction.Y, 0).Normalize() * Geometry.GeomUtil.milimeter2Feet(movedis);
                                    elem.Location.Move(movevector2);
                                    if (Math.Abs(Command.GetMoveDistance(elemline.GetEndPoint(0), gridY, roundto) % roundto) > precision && Math.Abs(Command.GetMoveDistance(elemline.GetEndPoint(0), gridY, roundto) % roundto) < (roundto - precision))
                                    {
                                        elem.Location.Move(-2 * movevector2);
                                    }
                                   
                                }
                            }
                            transactionbeam.Commit();
                        }

                        break;
                    }
                case th2:
                    {
                        List<Element> elemlist = Singleton.Instance.RevitData.Selection.PickElementsByRectangle(new StructuralColumnFilter()) as List<Element>;
                        using (Transaction transactioncolumn = new Transaction(doc))
                        {
                            transactioncolumn.Start("Column Moving ");
                            foreach (Element elem in elemlist)
                            {

                                LocationPoint elemcurve = elem.Location as LocationPoint;
                                
                                Autodesk.Revit.DB.Line gridlineX = gridX.Curve as Autodesk.Revit.DB.Line;
                                Autodesk.Revit.DB.Line gridlineY = gridY.Curve as Autodesk.Revit.DB.Line;

                               
                                double movedis = Command.GetMoveDistance(elemcurve.Point, gridX, roundto);
                                if (movedis > precision)
                                {
                                    XYZ movevector1 = new XYZ(-gridlineX.Direction.Y, gridlineX.Direction.X, 0).Normalize() * Geometry.GeomUtil.milimeter2Feet(movedis);
                                    
                                    elem.Location.Move(movevector1);

                                    if (Math.Abs(Command.GetMoveDistance(elemcurve.Point, gridX, roundto) % roundto) > precision && Math.Abs(Command.GetMoveDistance(elemcurve.Point, gridX, roundto) % roundto) < roundto - precision)
                                    {
                                        elem.Location.Move(-2 * movevector1);
                                    }                                  
                                }
                                movedis = Command.GetMoveDistance(elemcurve.Point, gridY, roundto);
                                if (movedis >= precision)
                                {
                                    
                                    XYZ movevector2 = new XYZ(-gridlineY.Direction.Y, gridlineY.Direction.X, 0).Normalize() * Geometry.GeomUtil.milimeter2Feet(movedis);
                                  
                                    TaskDialog.Show("asas", Command.GetMoveDistance(elemcurve.Point, gridY, roundto).ToString());
                                    elem.Location.Move(movevector2);
                                    TaskDialog.Show("asas", Command.GetMoveDistance(elemcurve.Point, gridY, roundto).ToString());
                                    if (Math.Abs(Command.GetMoveDistance(elemcurve.Point, gridY, roundto) % roundto) > precision && Math.Abs(Command.GetMoveDistance(elemcurve.Point, gridY, roundto) % roundto) < (roundto - precision))
                                    {
                                        elem.Location.Move(-2 * movevector2);
                                    }
                                    
                                }
                            }
                            transactioncolumn.Commit();
                        }
                            
                        break;
                    }
                case th3:

                    {
                        List<Element> elemlist = Singleton.Instance.RevitData.Selection.PickElementsByRectangle(new StructuralWallFilter()) as List<Element>;
                        using (Transaction transactionwall = new Transaction(doc))
                        {
                            transactionwall.Start("Walls Moving");
                            foreach (Element elem in elemlist)
                            {

                                LocationCurve elemcurve = elem.Location as LocationCurve;
                                Autodesk.Revit.DB.Line elemline = elemcurve.Curve as Autodesk.Revit.DB.Line;
                                Autodesk.Revit.DB.Line gridlineX = gridX.Curve as Autodesk.Revit.DB.Line;
                                Autodesk.Revit.DB.Line gridlineY = gridY.Curve as Autodesk.Revit.DB.Line;
                                if (Geometry.GeomUtil.IsSameOrOppositeDirection(elemline.Direction, gridlineY.Direction))
                                {
                                    gridX = doc.GetElement(gridrefY) as Autodesk.Revit.DB.Grid;
                                    gridY = doc.GetElement(gridrefX) as Autodesk.Revit.DB.Grid;
                                    gridlineX = gridX.Curve as Autodesk.Revit.DB.Line;
                                    gridlineY = gridY.Curve as Autodesk.Revit.DB.Line;
                                }
                                double movedis = Command.GetMoveDistance(elemline, gridX, roundto);
                                if (movedis > precision)
                                {
                                    XYZ movevector1 = new XYZ(-elemline.Direction.Y, elemline.Direction.X, 0).Normalize() * Geometry.GeomUtil.milimeter2Feet(movedis);
                                    elem.Location.Move(movevector1);
                                    if (Math.Abs(Command.GetMoveDistance(elemline, gridX, roundto) % roundto) > precision && Math.Abs(Command.GetMoveDistance(elemline, gridX, roundto) % roundto) < roundto - precision)
                                    {
                                        elem.Location.Move(-2 * movevector1);
                                    }
                                }
                                movedis = Command.GetMoveDistance(elemline.GetEndPoint(0), gridY, roundto);
                                
                                if (movedis >= precision)
                                {
                                    XYZ movevector2 = new XYZ(elemline.Direction.X, elemline.Direction.Y, 0).Normalize() * Geometry.GeomUtil.milimeter2Feet(movedis);
                                   
                                    elem.Location.Move(movevector2);
                                    if (Math.Abs(Command.GetMoveDistance(elemline.GetEndPoint(0), gridY, roundto) % roundto) > precision && Math.Abs(Command.GetMoveDistance(elemline.GetEndPoint(0), gridY, roundto) % roundto) < (roundto - precision))
                                    {
                                        elem.Location.Move(-2 * movevector2);
                                    }

                                }
                            }
                            transactionwall.Commit();
                        }

                        break;
                    }

            }

        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Hide();            
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            roundto = float.Parse(txtround.Text);
        }
    }
}
