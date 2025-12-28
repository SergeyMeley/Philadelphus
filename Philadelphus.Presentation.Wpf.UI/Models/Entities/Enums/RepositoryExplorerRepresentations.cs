using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Presentation.Wpf.UI.Models.Entities.Enums
{
    internal enum RepositoryExplorerRepresentations
    {
        ByChildren_tree,
        ByContent_tree,
        ByTags_graph,
        ByLinks_graph,
        ByWindowsFileSystem_tree,
        Combo_tree,
        ByUserRepresentation,
    }
}
