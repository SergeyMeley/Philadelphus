using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.OtherEntities
{
    public static class GeneralSettings
    {
        public static string RepositoryListPath { get => @"C:\Users\MeleychukSV\Documents\Philadelphus\Repositories.xml"; set => RepositoryListPath = value; }
        public static List<string> RepositoryPathList { get; set; } = new List<string>();
    }
}
