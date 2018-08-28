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
        Autodesk.Revit.DB.Grid grid;
        Autodesk.Revit.DB.Grid grid1;
        List<Autodesk.Revit.DB.Element> elems;
        public InputForm()
        {
            InitializeComponent();
            DataContext = Singleton.Instance.WPFData;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            //List<Reference> rfs= Singleton.Instance.RevitData.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new GridSelectionFilter())
            //    as List<Reference>;
            //List<Autodesk.Revit.DB.Grid> grids = rfs.Select(x => Singleton.Instance.RevitData.Document.GetElement(x) as Autodesk.Revit.DB.Grid).ToList();
            Reference gridref = Singleton.Instance.RevitData.Selection.PickObject(ObjectType.Element, new GridSelectionFilter());
            grid = Singleton.Instance.RevitData.Document.GetElement(gridref) as Autodesk.Revit.DB.Grid;
            //grids.ForEach(x => Singleton.Instance.WPFData.Grids.Add(x));
            Singleton.Instance.WPFData.Grids.Add(grid);

            Reference gridref1 = Singleton.Instance.RevitData.Selection.PickObject(ObjectType.Element, new GridSelectionFilter());
            grid1 = Singleton.Instance.RevitData.Document.GetElement(gridref1) as Autodesk.Revit.DB.Grid;
            Singleton.Instance.WPFData.Grids.Add(grid1);
            ShowDialog();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Hide();
            List<Reference> rfs1 = Singleton.Instance.RevitData.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new ColumnSelectionFilter())
                as List<Reference>;
            elems = rfs1.Select(x => Singleton.Instance.RevitData.Document.GetElement(x) as Autodesk.Revit.DB.Element).ToList();
            elems.ForEach(x => Singleton.Instance.WPFData.Elements.Add(x));
            ShowDialog();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Hide();
            Document doc = Singleton.Instance.RevitData.Document;

            Autodesk.Revit.DB.Line gridline = grid.Curve as Autodesk.Revit.DB.Line;

            Autodesk.Revit.DB.Line gridline1 = grid1.Curve as Autodesk.Revit.DB.Line;

            Autodesk.Revit.DB.Grid gridX = grid;
            Autodesk.Revit.DB.Grid gridY = grid1;
            Autodesk.Revit.DB.Line gridlineX = gridX.Curve as Autodesk.Revit.DB.Line;
            Autodesk.Revit.DB.Line gridlineY = gridY.Curve as Autodesk.Revit.DB.Line;

            float roundto = float.Parse(txtroundto.Text);
            const double precision = 0.00001;

            foreach (Element emt in elems)
            {
                if (emt is FamilyInstance && emt.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns)
                {
                    FamilyInstance column = emt as FamilyInstance;
                    LocationPoint locationPoint = column.Location as LocationPoint;
                    XYZ currentLocation = locationPoint.Point;

                    double distance = GetDistance.Distance(column, grid);
                    double movedis = (distance - Lamtron.Round(distance, roundto)) / 12 / 25.4;

                    XYZ newLocation = new XYZ(-gridline.Direction.Y * movedis, gridline.Direction.X * movedis, 0);

                    using (Transaction t = new Transaction(doc, "move"))
                    {
                        t.Start("move");
                        ElementTransformUtils.MoveElement(doc, column.Id, newLocation);
                        if (Math.Abs(GetDistance.Distance(column, grid) % roundto) > precision && Math.Abs(GetDistance.Distance(column, grid) % roundto) < (roundto-precision))
                        {
                            XYZ newLocationrepair = new XYZ(2 * gridline.Direction.Y * movedis, -2 * gridline.Direction.X * movedis, 0);
                            ElementTransformUtils.MoveElement(doc, column.Id, newLocationrepair);
                        }
                        t.Commit();
                    }
                    double distance1 = GetDistance.Distance(column, grid1);
                    double movedis1 = (distance1 - Lamtron.Round(distance1, roundto)) / 12 / 25.4;

                    XYZ newLocation1 = new XYZ(-gridline1.Direction.Y * movedis1, gridline1.Direction.X * movedis1, 0);

                    using (Transaction t = new Transaction(doc, "move"))
                    {
                        t.Start("move");
                        ElementTransformUtils.MoveElement(doc, column.Id, newLocation1);
                        if (Math.Abs(GetDistance.Distance(column, grid1) % roundto) > 0.01 && Math.Abs(GetDistance.Distance(column, grid1) % roundto) < 4.99)
                        {
                            XYZ newLocationrepair1 = new XYZ(2 * gridline1.Direction.Y * movedis1, -2 * gridline1.Direction.X * movedis1, 0);
                            ElementTransformUtils.MoveElement(doc, column.Id, newLocationrepair1);
                        }
                        t.Commit();
                    }
                }
                if (emt is FamilyInstance && emt.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFraming)
                {
                    FamilyInstance elem = emt as FamilyInstance;
                    LocationCurve elemcurve = elem.Location as LocationCurve;
                    Autodesk.Revit.DB.Line elemline = elemcurve.Curve as Autodesk.Revit.DB.Line;


                    //Kiểm tra và đặt tên Grid, GridX song song với Location.Curve - Line, Grid Y vuông góc,...

                    if (Geometry.GeomUtil.IsSameOrOppositeDirection(elemline.Direction, gridlineY.Direction))
                    {
                        gridX = grid1;
                        gridY = grid;
                        gridlineX = gridX.Curve as Autodesk.Revit.DB.Line;
                        gridlineY = gridY.Curve as Autodesk.Revit.DB.Line;
                    }
                    double distance = GetDistance.GetMoveDistance(elemline, gridX, roundto);
                    double movedis = Math.Abs(distance - Lamtron.Round(distance, roundto));
                    // Move dầm phương song song
                    if (movedis > precision)
                    {
                        XYZ movevector1 = new XYZ(-elemline.Direction.Y, elemline.Direction.X, 0).Normalize() * Geometry.GeomUtil.milimeter2Feet(movedis);

                        using (Transaction transactionX = new Transaction(Singleton.Instance.RevitData.Document))
                        {
                            transactionX.Start("Beam Moving X");
                            elem.Location.Move(movevector1);
                            if (Math.Abs(GetDistance.GetMoveDistance(elemline, gridX, roundto) % roundto) > precision && Math.Abs(GetDistance.GetMoveDistance(elemline, gridX, roundto) % roundto) < roundto - precision)
                            {
                                elem.Location.Move(-2 * movevector1);
                            }
                            transactionX.Commit();
                        }
                    }
                    distance = GetDistance.GetMoveDistance(elemline.GetEndPoint(0), gridY, roundto);
                    movedis = Math.Abs(distance - Lamtron.Round(distance, roundto));
                    // Move dầm phương vuông góc
                    if (movedis >= precision)
                    {
                        XYZ movevector2 = new XYZ(elemline.Direction.X, elemline.Direction.Y, 0).Normalize() * Geometry.GeomUtil.milimeter2Feet(movedis);
                        using (Transaction transactionY = new Transaction(Singleton.Instance.RevitData.Document))
                        {
                            transactionY.Start("Beam moving Y");
                            elem.Location.Move(movevector2);
                            if (Math.Abs(GetDistance.GetMoveDistance(elemline.GetEndPoint(0), gridY, roundto) % roundto) > precision && Math.Abs(GetDistance.GetMoveDistance(elemline.GetEndPoint(0), gridY, roundto) % roundto) < (roundto - precision))
                            {
                                elem.Location.Move(-2 * movevector2);
                            }
                            transactionY.Commit();
                        }
                    }
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Hide();
            List<Reference> rfs1 = Singleton.Instance.RevitData.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new BeamSelectionFilter())
                as List<Reference>;
            elems = rfs1.Select(x => Singleton.Instance.RevitData.Document.GetElement(x) as Autodesk.Revit.DB.Element).ToList();
            elems.ForEach(x => Singleton.Instance.WPFData.Elements.Add(x));
            ShowDialog();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Hide();
            List<Reference> rfs1 = Singleton.Instance.RevitData.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new StructuralWallFilter())
                as List<Reference>;
            elems = rfs1.Select(x => Singleton.Instance.RevitData.Document.GetElement(x) as Autodesk.Revit.DB.Element).ToList();
            elems.ForEach(x => Singleton.Instance.WPFData.Elements.Add(x));
            ShowDialog();
        }
    }
}
