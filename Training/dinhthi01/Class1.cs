using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

// text
using System.IO;


namespace dinhthi01
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand


    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            //Document doc = uidoc.Document;

            Selection sel = uidoc.Selection;
            //Reference dimref = sel.PickObject(ObjectType.Element, new DimSelectionFilter());
            //Dimension dim = doc.GetElement(dimref) as Dimension;
            //double? giatri = dim.Value;
            Reference gridref = sel.PickObject(ObjectType.Element, new GridSelectionFilter());
            Grid grid = uidoc.Document.GetElement(gridref) as Grid;

            //XYZ griddirection = grid.Curve
            //    .GetEndPoint(1)
            //    .Subtract(grid.Curve.GetEndPoint(0))
            //    .Normalize();

            Reference beamref = sel.PickObject(ObjectType.Element, new BeamSelectionFilter());
            FamilyInstance beam = uidoc.Document.GetElement(beamref) as FamilyInstance;
            //XYZ beamdirection = beam.HandOrientation.Normalize();
            LocationCurve beamcurve = beam.Location as LocationCurve;
            Line beamline = beamcurve.Curve as Line;
            Line gridline = grid.Curve as Line;


            //Lấy khoảng cách từ endpoin của Beam tới gridline
            // Gridline phải bao trùm dầm thì kết quả mới chính xác 

            double distance = GetDistance.Distance(beam, grid);
            double movedis = Math.Abs(distance - Lamtron.Round(distance));

            if (movedis <= 0.01)
            {
                TaskDialog.Show("Result", "Ko can chinh");
                return Result.Succeeded;
            }
            //Tim vector move   

            XYZ movevector = new XYZ(-beamline.Direction.Y, beamline.Direction.X, 0).Normalize();
            movevector = movevector * movedis / 12 / 25.4;
            //TaskDialog.Show("movedis",movedis.ToString());

            using (Transaction transaction = new Transaction(uidoc.Document))
            {
                transaction.Start("Beam Moving");
                // unjoin beam
                // thuật toán làm tròn
                // Vector XYZ ? và hướng move

                beam.Location.Move(movevector);                
                if (Math.Abs(GetDistance.Distance(beam, grid)%5) >0.01 && Math.Abs(GetDistance.Distance(beam, grid) % 5)<4.99)
                {
                    //TaskDialog.Show("Distance", (Math.Abs(GetDistance.Distance(beam, grid)-distance)-movedis).ToString());
                    beam.Location.Move(-2 * movevector);
                }
                transaction.Commit();
            }
            return Result.Succeeded;
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
            if (dimvalue % 5 >= 2.5) dimvalue = dimvalue - dimvalue % 5 + 5;
            else if (dimvalue % 5 < 2.5 && dimvalue % 5 > 0) dimvalue = dimvalue - dimvalue % 5;
            return dimvalue;
        }
    }
    public class GetDistance
    {
        public static double Distance(FamilyInstance beam, Grid grid)
        {
            Line gridline = grid.Curve as Line;
            LocationCurve beamcurve = beam.Location as LocationCurve;
            Line beamline = beamcurve.Curve as Line;
            XYZ point = new XYZ(beamline.GetEndPoint(0).X, beamline.GetEndPoint(0).Y, 0);
            double distance = 12 * 25.4 * gridline.Distance(point);
            return distance;
        }
    }

}
