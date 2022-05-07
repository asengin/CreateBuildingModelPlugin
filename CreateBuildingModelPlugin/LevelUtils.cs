using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateBuildingModelPlugin
{
    class LevelUtils
    {
        public static Level GetLevel(Document doc, string name)
        {
            Level level = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Where(x => x.Name.Equals(name))
                .OfType<Level>()
                .FirstOrDefault();

            return level;
        }
    }
}
