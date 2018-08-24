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

            Reference columnref = uidoc.Selection.PickObject(ObjectType.Element, new ColumnSelectionFilter());
            FamilyInstance column = uidoc.Document.GetElement(columnref) as FamilyInstance;

            LocationPoint locationPoint = column.Location as LocationPoint;
            XYZ currentLocation = locationPoint.Point;

            Line gridline = grid.Curve as Line;

            double distance = GetDistance.Distance(column, grid);
            double movedis = (distance - Lamtron.Round(distance)) / 12 / 25.4;

            //if (movedis <= 0.01)
            //{
            //    TaskDialog.Show("Result", "Ko can chinh");
            //    return Result.Succeeded;
            //}

            XYZ newLocation = new XYZ(-gridline.Direction.Y * movedis, gridline.Direction.X * movedis, 0);
            //XYZ newLocation = new XYZ(10, 10, 0);
            using (Transaction t = new Transaction(doc, "move"))
            {
                t.Start("move");
                ElementTransformUtils.MoveElement(doc, column.Id, newLocation);
                if (Math.Abs(GetDistance.Distance(column, grid) % 5) > 0.01 && Math.Abs(GetDistance.Distance(column, grid) % 5) < 4.99)
                {
                    XYZ newLocation1 = new XYZ(2* gridline.Direction.Y * movedis, -2* gridline.Direction.X * movedis, 0);
                    ElementTransformUtils.MoveElement(doc, column.Id, newLocation1);
                }
                t.Commit();
            }

            return Result.Succeeded;
        }
        public Element SelectionElement(UIDocument uidoc, Document doc)
        {
            Reference reference = uidoc.Selection.PickObject(ObjectType.Element);
            Element element = uidoc.Document.GetElement(reference);
            return element;
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
    }
}
