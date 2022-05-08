using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateBuildingModelPlugin
{
    [Transaction(TransactionMode.Manual)]

    public class CreateBuildingModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            Level level1 = LevelUtils.GetLevel(doc, "Уровень 1");
            Level level2 = LevelUtils.GetLevel(doc, "Уровень 2");
            const double width = 10000;
            const double length = 12000;
            const double floorOffset = 800;
            const double roofOverhang = 500;
            const double roofAngle = 32;

            List<Wall> walls1Level = Creator.NewWall(doc, width, length, level1, level2);
            
            Creator.AddDoor(doc, level1, walls1Level[0]);
            
            for (int i = 1; i < 4; i++)
                Creator.AddWindow(doc, level1, walls1Level[i], floorOffset);

            Creator.MakeRoof(doc, level2, roofOverhang, length);

            return Result.Succeeded;
        }

    }
}
