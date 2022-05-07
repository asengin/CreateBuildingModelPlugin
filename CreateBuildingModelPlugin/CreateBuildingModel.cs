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

            Creator.NewWall(doc, 10000, 12000, level1, level2);

            return Result.Succeeded;
        }

    }
}
