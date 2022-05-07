using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateBuildingModelPlugin
{
    class Creator
    {
        public static void NewWall(Document doc, double width, double length, Level firstLevel, Level secondlevel)
        {
            width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters);
            length = UnitUtils.ConvertToInternalUnits(8000, UnitTypeId.Millimeters);

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
        }
    }
}
