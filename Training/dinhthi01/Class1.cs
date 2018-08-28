using Autodesk.Revit.ApplicationServices;
using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.IO;
using System.Collections.Generic;


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
            float roundto = 5;
            const double precision = 0.00001;
            Selection sel = uidoc.Selection;
            Reference gridrefX = sel.PickObject(ObjectType.Element, new GridSelectionFilter());
            Grid gridX = uidoc.Document.GetElement(gridrefX) as Grid;
            Reference gridrefY = sel.PickObject(ObjectType.Element, new GridSelectionFilter());
            Grid gridY = uidoc.Document.GetElement(gridrefY) as Grid;
            
            List<Element> elemlist = sel.PickElementsByRectangle(new StructuralWallFilter()) as List<Element>;
            foreach (Element elem in elemlist)
            {
                
                LocationCurve elemcurve = elem.Location as LocationCurve;
                Line elemline = elemcurve.Curve as Line;
                Line gridlineX = gridX.Curve as Line;
                Line gridlineY = gridY.Curve as Line;

                //Kiểm tra và đặt tên Grid, GridX song song với Location.Curve - Line, Grid Y vuông góc,...

                if (Geometry.GeomUtil.IsSameOrOppositeDirection(elemline.Direction, gridlineY.Direction))
                {
                    gridX = uidoc.Document.GetElement(gridrefY) as Grid;
                    gridY = uidoc.Document.GetElement(gridrefX) as Grid;
                    gridlineX = gridX.Curve as Line;
                    gridlineY = gridY.Curve as Line;
                }
                double movedis = GetMoveDistance(elemline, gridX, roundto);
                // Move dầm phương song song
                if (movedis > precision)
                {
                    XYZ movevector1 = new XYZ(-elemline.Direction.Y, elemline.Direction.X, 0).Normalize() * Geometry.GeomUtil.milimeter2Feet(movedis);

                    using (Transaction transactionX = new Transaction(uidoc.Document))
                    {
                        transactionX.Start("Beam Moving X");
                        elem.Location.Move(movevector1);
                        if (Math.Abs(GetMoveDistance(elemline, gridX, roundto) % roundto) > precision && Math.Abs(GetMoveDistance(elemline, gridX, roundto) % roundto) < roundto - precision)
                        {
                            elem.Location.Move(-2 * movevector1);
                        }
                        transactionX.Commit();
                    }
                }
                movedis = GetMoveDistance(elemline.GetEndPoint(0), gridY, roundto);
                //TaskDialog.Show("ss", movedis.ToString());
                // Move dầm phương vuông góc
                if (movedis >= precision)
                {
                    XYZ movevector2 = new XYZ(elemline.Direction.X, elemline.Direction.Y, 0).Normalize() * Geometry.GeomUtil.milimeter2Feet(movedis);
                    using (Transaction transactionY = new Transaction(uidoc.Document))
                    {
                        transactionY.Start("Beam moving Y");
                        elem.Location.Move(movevector2);
                        if (Math.Abs(GetMoveDistance(elemline.GetEndPoint(0), gridY, roundto) % roundto) > precision && Math.Abs(GetMoveDistance(elemline.GetEndPoint(0), gridY, roundto) % roundto) < (roundto - precision))
                        {
                            elem.Location.Move(-2 * movevector2);
                        }
                        transactionY.Commit();
                    }
                }
            }
            return Result.Succeeded;
        }
        public static double Round(double dimvalue, double roundto)
        {
            // roundto 1,5,10
            if ((dimvalue % roundto) >= (roundto / 2)) dimvalue = dimvalue - dimvalue % roundto + roundto;
            else  dimvalue = dimvalue - dimvalue % roundto;
            return dimvalue;

        }
        public static double GetMoveDistance(Line line, Grid grid, float roundto)
        {   
            Line gridline = grid.Curve as Line;
            XYZ point = new XYZ(Geometry.GeomUtil.GetMiddlePoint(line.GetEndPoint(0), line.GetEndPoint(1)).X, Geometry.GeomUtil.GetMiddlePoint(line.GetEndPoint(0), line.GetEndPoint(1)).Y, 0);
            double distance = Geometry.GeomUtil.feet2Milimeter(gridline.Distance(point));
            double movedis = Math.Abs(distance - Round(distance, roundto));
            return movedis;
        }
        public static double GetMoveDistance(XYZ point, Grid grid, float roundto)
        {
            Line gridline = grid.Curve as Line;
            XYZ zeropoint = new XYZ(point.X, point.Y, 0);
            double distance = Geometry.GeomUtil.feet2Milimeter(gridline.Distance(zeropoint));
            double movedis = Math.Abs(distance - Round(distance, roundto));
            return movedis;
        }
    }
    [Transaction(TransactionMode.Manual)]
    public class WPF : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Singleton.Instance = new Singleton();
            Singleton.Instance.RevitData.UIApplication = commandData.Application;
            Singleton.Instance.OtherData.InputForm.ShowDialog();
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
    public class StructuralColumnFilter : ISelectionFilter
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
}
    
 
    