using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.MainEntities
{
    
    public class DbLayer : DbEntityBase
    {
        public long ProjectId { get; set; }
        public string DirectoryPath { get; set; }
        public IEnumerable<long> AttributeIds { get; set; }
        /// <summary>
        /// Коллекция деревьев слоя.
        /// В качестве дерева могут выступать, например, "Проект" на слое проектов, 
        /// </summary>
        public IEnumerable<long> ChildsIds { get; set; }
        /// <summary>
        /// Слой представляет из себя коллекцию деревьев, хранимую в рамках одной БД.
        /// По сути, является корнем дерева.
        /// Благодаря этому сохраняется возможность разворота отдельных слоев в зависимости от потребностей в данной конкретной ситуации.
        /// В качестве слоев могут выступать, например "Коллекция проектов", "Каталог", "Коллекция справочников".
        /// </summary>
        public DbLayer(long id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
