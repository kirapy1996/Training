using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColumnRepair
{
    [Transaction(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            Reference gridref = uidoc.Selection.PickObject(ObjectType.Element, new GridSelectionFilter());
            Grid grid = uidoc.Document.GetElement(gridref) as Grid;

            Reference gridref1 = uidoc.Selection.PickObject(ObjectType.Element, new GridSelectionFilter());
            Grid grid1 = uidoc.Document.GetElement(gridref1) as Grid;

            Line gridline = grid.Curve as Line;

            Line gridline1 = grid1.Curve as Line;

            ColumnSelectionFilter columnSelFil = new ColumnSelectionFilter();
            List<Reference> rfs1 = uidoc.Selection.PickObjects(ObjectType.Element, new ColumnSelectionFilter()) as List<Reference>;
            List<Element> elems = rfs1.Select(x => uidoc.Document.GetElement(x) as Element).ToList();
            foreach (Element e in elems)
            {
                //Reference columnref = uidoc.Selection.PickObject(ObjectType.Element, new ColumnSelectionFilter());
                FamilyInstance column = e as FamilyInstance;

                //FamilyInstance column = uidoc.Document.GetElement(columnref) as FamilyInstance;
                LocationPoint locationPoint = column.Location as LocationPoint;
                XYZ currentLocation = locationPoint.Point;

                double distance = GetDistance.Distance(column, grid);
                double movedis = (distance - Lamtron.Round(distance)) / 12 / 25.4;

                XYZ newLocation = new XYZ(-gridline.Direction.Y * movedis, gridline.Direction.X * movedis, 0);

                using (Transaction t = new Transaction(doc, "move"))
                {
                    t.Start("move");
                    ElementTransformUtils.MoveElement(doc, column.Id, newLocation);
                    if (Math.Abs(GetDistance.Distance(column, grid) % 5) > 0.01 && Math.Abs(GetDistance.Distance(column, grid) % 5) < 4.99)
                    {
                        XYZ newLocationrepair = new XYZ(2 * gridline.Direction.Y * movedis, -2 * gridline.Direction.X * movedis, 0);
                        ElementTransformUtils.MoveElement(doc, column.Id, newLocationrepair);
                    }
                    t.Commit();
                }
                double distance1 = GetDistance.Distance(column, grid1);
                double movedis1 = (distance1 - Lamtron.Round(distance1)) / 12 / 25.4;

                XYZ newLocation1 = new XYZ(-gridline1.Direction.Y * movedis1, gridline1.Direction.X * movedis1, 0);

                using (Transaction t = new Transaction(doc, "move"))
                {
                    t.Start("move");
                    ElementTransformUtils.MoveElement(doc, column.Id, newLocation1);
                    if (Math.Abs(GetDistance.Distance(column, grid1) % 5) > 0.01 && Math.Abs(GetDistance.Distance(column, grid1) % 5) < 4.99)
                    {
                        XYZ newLocationrepair1 = new XYZ(2 * gridline1.Direction.Y * movedis1, -2 * gridline1.Direction.X * movedis1, 0);
                        ElementTransformUtils.MoveElement(doc, column.Id, newLocationrepair1);
                    }
                    t.Commit();
                }
            }

            return Result.Succeeded;
        }
        //public Element SelectionElement(UIDocument uidoc, Document doc)
        //{
        //    Reference reference = uidoc.Selection.PickObject(ObjectType.Element);
        //    Element element = uidoc.Document.GetElement(reference);
        //    return element;
        //}
    }

    [Transaction(TransactionMode.Manual)]
    public class Class2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Singleton.Instance = new Singleton();
            Singleton.Instance.RevitData.UIApplication = commandData.Application;
            Singleton.Instance.OtherData.InputForm.ShowDialog();

            return Result.Succeeded;
        }
    }
    public class ColumnSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is FamilyInstance && elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns) return true;
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
    public class GridSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is Grid) return true;
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    public class BeamSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is FamilyInstance && elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFraming) return true;
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    public class StructuralWallFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Walls) return true;
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }

    public class DimSelectionFilter : ISelectionFilter
    {             //Chọn dim
        public bool AllowElement(Element elem)
        {
            if (elem is Dimension) return true;
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
    public class Lamtron
    {
        public static double Round(double dimvalue)
        {
            // Thay 5 = roundvalue cho bài toán tổng quát
            if (dimvalue % 5 >= 2.5) dimvalue = dimvalue - dimvalue % 5 + 5;
            else if (dimvalue % 5 < 2.5 && dimvalue % 5 > 0) dimvalue = dimvalue - dimvalue % 5;
            return dimvalue;
        }
        public static double Round(double dimvalue, double roundto)
        {
            // roundto 1,5,10
            if ((dimvalue % roundto) >= (roundto / 2)) dimvalue = dimvalue - dimvalue % roundto + roundto;
            else dimvalue = dimvalue - dimvalue % roundto;
            return dimvalue;
        }
    }

    public class GetDistance
    {
        public static double Distance(FamilyInstance column, Grid grid)
        {
            Line gridline = grid.Curve as Line;
            LocationPoint locationPoint = column.Location as LocationPoint;
            XYZ currentLocation = locationPoint.Point;
            double distance = 12 * 25.4 * gridline.Distance(currentLocation);
            return distance;
        }
        public static double GetMoveDistance(Line line, Grid grid, float roundto)
        {
            Line gridline = grid.Curve as Line;
            XYZ point = new XYZ(Geometry.GeomUtil.GetMiddlePoint(line.GetEndPoint(0), line.GetEndPoint(1)).X, Geometry.GeomUtil.GetMiddlePoint(line.GetEndPoint(0), line.GetEndPoint(1)).Y, 0);
            double distance = Geometry.GeomUtil.feet2Milimeter(gridline.Distance(point));
            return distance;
        }
        public static double GetMoveDistance(XYZ point, Grid grid, float roundto)
        {
            Line gridline = grid.Curve as Line;
            XYZ zeropoint = new XYZ(point.X, point.Y, 0);
            double distance = 12 * 25.4 * gridline.Distance(zeropoint);
            return distance;
        }
    }
}
