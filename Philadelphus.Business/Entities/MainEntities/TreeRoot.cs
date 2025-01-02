using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Philadelphus.Business.Entities.MainEntities
{
    public class TreeRoot : MainEntityBase
    {
        /// <summary>
        /// Проект, в рамках которого существует данный слой.
        /// Под проектом в данном случае подразумевается глобальный бизнес-проект, пронизывающий насквозь всю организацию.
        /// Примером такого проекта может быть "Проект номенклатуры предприятия" или "Проект 3D-моделирования".
        /// Привычные нам проекты, такие как "Нефтегазовый завод в г.Ноябрьск" целесообразнее реализовывать в виде слоев одного из проектов, или же в виде узлов, существующих в рамках одного из слоев проекта.
        /// </summary>
        public long RepositoryId { get; set; }
        /// <summary>
        /// Путь к директории операционной системы, где располагается данный слой.
        /// </summary>
        public string DirectoryPath { get; set; }
        public IEnumerable<Attribute> Attributes { get; set; }
        /// <summary>
        /// Коллекция деревьев слоя.
        /// В качестве дерева могут выступать, например, "Проект" на слое проектов
        /// </summary>
        public IEnumerable<TreeNode> ChildNodes { get; set; }
        /// <summary>
        /// Слой представляет из себя коллекцию деревьев, хранимую в рамках одном месте (директории, БД).
        /// По сути, является корнем дерева.
        /// Благодаря этому сохраняется возможность разворота отдельных слоев в зависимости от потребностей в данной конкретной ситуации.
        /// В качестве слоев могут выступать, например "Коллекция проектов", "Каталог", "Коллекция справочников".
        /// </summary>
        public TreeRoot()
        {
            Attributes = new List<Attribute>();
            ChildNodes = new List<TreeNode>();
        }
    }
}
