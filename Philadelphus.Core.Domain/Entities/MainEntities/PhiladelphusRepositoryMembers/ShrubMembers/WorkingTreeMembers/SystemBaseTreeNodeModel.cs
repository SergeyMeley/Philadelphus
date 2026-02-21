using Microsoft.EntityFrameworkCore;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.TreeRootMembers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    public class SystemBaseTreeNodeModel : TreeNodeModel
    {
        private Guid _uuid = Guid.NewGuid();

        private static Dictionary<Guid, SystemBaseType> _baseUuids = new Dictionary<Guid, SystemBaseType>()
        {
            { Guid.Parse("00000000-0000-0000-0000-00015b10ec20"), SystemBaseType.OBJECT },
            { Guid.Parse("00000000-0000-0000-0000-192018091407"), SystemBaseType.STRING },
            { Guid.Parse("00000000-0000-0000-0000-142113e1809c"), SystemBaseType.NUMERIC },
            { Guid.Parse("00000000-0000-0000-0000-914200507e18"), SystemBaseType.INTEGER },
            { Guid.Parse("00000000-0000-0000-0000-0000f1215a20"), SystemBaseType.FLOAT },
        };

        public override SystemBaseType SystemBaseType { get; }

        internal SystemBaseTreeNodeModel(
            IParentModel parent, 
            WorkingTreeModel owner, 
            SystemBaseType type) 
            : base(GetUuidByType(type), parent, owner, new TreeNode())
        {
            SystemBaseType = type;
            InitProperties(type);
        }

        internal SystemBaseTreeNodeModel(
            Guid uuid,
            IParentModel parent,
            WorkingTreeModel owner)
            : base(uuid, parent, owner, new TreeNode())
        {
            SystemBaseType = GetTypeByUuid(uuid);
            InitProperties(SystemBaseType);
        }

        private void InitProperties(SystemBaseType type)
        {
            switch (type)
            {
                case SystemBaseType.OBJECT:
                    Name = "Объект";
                    Description = "object, variant";
                    CustomCode = "OBJ0";
                    Alias = "obj";
                    break;
                case SystemBaseType.STRING:
                    Name = "Текст";
                    Description = "string, text";
                    CustomCode = "TEXT";
                    Alias = "txt";
                    break;
                case SystemBaseType.NUMERIC:
                    Name = "Число";
                    Description = "numeric";
                    CustomCode = "NUM0";
                    Alias = "num";
                    break;
                case SystemBaseType.INTEGER:
                    Name = "Целое число";
                    Description = "integer";
                    CustomCode = "INT0";
                    Alias = "int";
                    break;
                case SystemBaseType.FLOAT:
                    Name = "Дробное число";
                    Description = "float";
                    CustomCode = "FLT0";
                    Alias = "flt";
                    break;
                default:
                    throw new Exception();
            }
        }

        internal static Guid GetUuidByType(SystemBaseType type)
        {
            if (_baseUuids.ContainsValue(type))
            {
                return _baseUuids.SingleOrDefault(x => x.Value == type).Key;
            }
            throw new Exception();
        }

        internal static SystemBaseType GetTypeByUuid(Guid uuid)
        {
            if (_baseUuids.ContainsKey(uuid))
            {
                return _baseUuids[uuid];
            }
            throw new Exception();
        }

        internal static bool IsSystemBaseType(Guid uuid)
        {
            return _baseUuids.ContainsKey(uuid);
        }
    }
}
