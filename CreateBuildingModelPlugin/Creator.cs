using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateBuildingModelPlugin
{
    class Creator
    {
        //public List<Wall> CreatedWalls { get; private set; }
        //public List<Wall> ListWalls { get; }

        public static List<Wall> NewWall(Document doc, double width, double length, Level firstLevel, Level secondlevel)
        {
            width = UnitUtils.ConvertToInternalUnits(width, UnitTypeId.Millimeters);
            length = UnitUtils.ConvertToInternalUnits(length, UnitTypeId.Millimeters);

            double dx = width / 2;
            double dy = length / 2;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));

            List<Wall> walls = new List<Wall>();

            using (Transaction tr = new Transaction(doc, "Создание стен"))
            {
                tr.Start();

                for (int i = 0; i < points.Count - 1; i++)
                {
                    Line line = Line.CreateBound(points[i], points[i + 1]);
                    Wall wall = Wall.Create(doc, line, firstLevel.Id, false);
                    walls.Add(wall);
                    wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(secondlevel.Id);
                }

                tr.Commit();
            }
            return walls;
        }

        public static void AddDoor(Document doc, Level level1, Wall wall)
        {
            FamilySymbol doorType = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Doors)
                .OfType<FamilySymbol>()
                .Where(x => x.Name.Equals("0915 x 2032 мм"))
                .Where(x => x.FamilyName.Equals("Одиночные-Щитовые"))
                .FirstOrDefault();
            LocationCurve baseCurve = wall.Location as LocationCurve;
            XYZ pt1BaseCurve = baseCurve.Curve.GetEndPoint(0);
            XYZ pt2BaseCurve = baseCurve.Curve.GetEndPoint(1);
            XYZ ptCenterBaseCurve = (pt1BaseCurve + pt2BaseCurve) / 2;

            using (Transaction tr = new Transaction(doc, "Вставка двери"))
            {
                tr.Start();

                if (!doorType.IsActive)
                    doorType.Activate();

                doc.Create.NewFamilyInstance(ptCenterBaseCurve, doorType, wall, level1, StructuralType.NonStructural);

                tr.Commit();
            }
        }

        public static void AddWindow(Document doc, Level level1, Wall wall, double floorOffset)
        {
            floorOffset = UnitUtils.ConvertToInternalUnits(floorOffset, UnitTypeId.Millimeters);

            FamilySymbol windowType = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Windows)
                .OfType<FamilySymbol>()
                .Where(x => x.Name.Equals("0915 x 1830 мм"))
                .Where(x => x.FamilyName.Equals("Фиксированные"))
                .FirstOrDefault();
            LocationCurve baseCurve = wall.Location as LocationCurve;
            XYZ pt1BaseCurve = baseCurve.Curve.GetEndPoint(0);
            XYZ pt2BaseCurve = baseCurve.Curve.GetEndPoint(1);
            XYZ ptCenterBaseCurve = (pt1BaseCurve + pt2BaseCurve) / 2;

            using (Transaction tr = new Transaction(doc, "Вставка окна"))
            {
                tr.Start();

                if (!windowType.IsActive)
                    windowType.Activate();

                FamilyInstance newWindow = doc.Create
                     .NewFamilyInstance(ptCenterBaseCurve, windowType, wall, level1, StructuralType.NonStructural);

                newWindow.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).Set(floorOffset);

                tr.Commit();
            }
        }

        public static void MakeRoof(Document doc, Level roofLevel, List<Wall> wallsContour, double roofOverhang, double roofAngle)
        {
            roofOverhang = UnitUtils.ConvertToInternalUnits(roofOverhang, UnitTypeId.Millimeters); //Свес кровли
            roofAngle = Math.Tan(roofAngle);

            RoofType roofType = new FilteredElementCollector(doc)
                .OfClass(typeof(RoofType))
                .OfType<RoofType>()
                .Where(x => x.Name.Equals("Типовой - 400мм"))
                .Where(x => x.FamilyName.Equals("Базовая крыша"))
                .FirstOrDefault();

            double dt = wallsContour[0].Width / 2 + roofOverhang;
            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dt, -dt, 0));
            points.Add(new XYZ(-dt, dt, 0));
            points.Add(new XYZ(dt, dt, 0));
            points.Add(new XYZ(dt, -dt, 0));
            points.Add(new XYZ(-dt, -dt, 0));

            Application app = doc.Application;

            CurveArray footprint = app.Create.NewCurveArray();
            for (int i = 0; i < wallsContour.Count; i++)
            {
                LocationCurve curve = wallsContour[i].Location as LocationCurve;
                XYZ pt1 = curve.Curve.GetEndPoint(0);
                XYZ pt2 = curve.Curve.GetEndPoint(1);
                Line line = Line.CreateBound(pt1 + points[i], pt2 + points[i + 1]);
                footprint.Append(line);
            }

            ModelCurveArray footPrintToModelCurveMapping = new ModelCurveArray();

            using (Transaction tr = new Transaction(doc, "Создание крыши"))
            {
                tr.Start();
                FootPrintRoof footPrintRoof = doc.Create.NewFootPrintRoof(footprint, roofLevel, roofType, out footPrintToModelCurveMapping);
                foreach (ModelCurve el in footPrintToModelCurveMapping)
                {
                    footPrintRoof.set_DefinesSlope(el, true);
                    footPrintRoof.set_SlopeAngle(el, roofAngle);
                }
                tr.Commit();
            }
        }
    }
}
