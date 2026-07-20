using FluentAssertions;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Presentation.Models.Tables;
using Philadelphus.Presentation.Services.Tables;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;
using System.Collections.ObjectModel;

namespace Philadelphus.Tests.Presentation.Services.Tables
{
    public class ChildCollectionTableBuilderTests
    {
        [Fact]
        public void buildChildCollectionTableColumns_Returns_Stable_System_Main_Attribute_Audit_Order()
        {
            var fixture = CreateFixture();

            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(
                fixture.Root,
                fixture.Root.Childs.Values);

            columns.Select(x => x.Key).Should().StartWith(new[]
            {
                nameof(IMainEntityModel.State),
                nameof(IChildrenModel.SequencePath),
                nameof(IMainEntityModel.Type),
                nameof(IMainEntityModel.Name),
                nameof(IMainEntityModel.Description),
                nameof(IWorkingTreeMemberModel.CustomCode),
                fixture.Root.Attributes.Single(x => x.Name == "Цена, руб").DeclaringUuid.ToString(),
            });

            columns.Select(x => x.Key).Should().ContainInOrder(
                $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.CreatedBy)}",
                $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.CreatedAt)}");
            columns.Select(x => x.Order).Should().BeInAscendingOrder();
        }

        [Fact]
        public void buildChildCollectionTableColumns_Identifies_Same_Name_Attributes_By_DeclaringUuid()
        {
            var fixture = CreateFixture();
            var secondUuid = Guid.CreateVersion7();
            _ = new ElementAttributeModel(
                secondUuid,
                fixture.Root,
                secondUuid,
                fixture.Root,
                fixture.Root.OwningWorkingTree,
                new FakeNotificationService(),
                new EmptyPropertiesPolicy<ElementAttributeModel>())
            {
                Name = "Цена, руб",
                Visibility = VisibilityScope.Public,
            };

            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(
                fixture.Root,
                fixture.Root.Childs.Values);

            var attributeColumns = columns.Where(x => x.Header == "Цена, руб").ToList();
            attributeColumns.Should().HaveCount(2);
            attributeColumns.Select(x => x.Key).Should().OnlyHaveUniqueItems();
            attributeColumns.Select(x => Guid.Parse(x.Key)).Should().Contain(secondUuid);
        }

        [Fact]
        public void buildChildCollectionTableColumns_Uses_Computed_Property_And_Attribute_Keys()
        {
            var fixture = CreateFixture();

            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(
                fixture.Root,
                fixture.Root.Childs.Values);

            columns.Single(x => x.Order == 0).Key.Should().Be(nameof(IMainEntityModel.State));
            columns.Single(x => x.Order == 1).Key.Should().Be(nameof(IChildrenModel.SequencePath));
            columns.Single(x => x.Order == 2).Key.Should().Be(nameof(IMainEntityModel.Type));
            columns.Single(x => x.Key == nameof(IMainEntityModel.Name)).Header.Should().NotBeNullOrWhiteSpace();
            columns.Single(x => x.Key == nameof(IMainEntityModel.Description)).Header.Should().NotBeNullOrWhiteSpace();
            columns.Single(x => x.Key == nameof(IWorkingTreeMemberModel.CustomCode)).Header.Should().NotBeNullOrWhiteSpace();
            columns.Single(x => x.Key == $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.CreatedBy)}").Header.Should().NotBeNullOrWhiteSpace();
            columns.Single(x => x.Key == $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.CreatedAt)}").Header.Should().NotBeNullOrWhiteSpace();
            var attributeColumn = columns.Single(x => x.Order == 6);
            Guid.Parse(attributeColumn.Key).Should().NotBeEmpty();
            attributeColumn.Header.Should().Be("Цена, руб");
            attributeColumn.BindingKey.Should().NotBe(attributeColumn.Key);
            attributeColumn.BindingKey.Should().StartWith("attribute_");
            attributeColumn.BindingKey.Should().NotContain(",");
            attributeColumn.BindingKey.Should().NotContain(" ");
            attributeColumn.IsAttribute.Should().BeTrue();
        }

        [Fact]
        public void buildChildCollectionTableColumns_Avoids_Binding_Key_Collision_With_Attribute_Name()
        {
            var fixture = CreateFixture();
            var attributeUuid = Guid.CreateVersion7();
            _ = new ElementAttributeModel(
                attributeUuid,
                fixture.Root,
                attributeUuid,
                fixture.Root,
                fixture.Root.OwningWorkingTree,
                new FakeNotificationService(),
                new EmptyPropertiesPolicy<ElementAttributeModel>())
            {
                Name = "attribute_6",
                Visibility = VisibilityScope.Public,
            };

            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(
                fixture.Root,
                fixture.Root.Childs.Values);

            var collisionColumn = columns.Single(x => x.Header == "attribute_6");
            collisionColumn.BindingKey.Should().NotBe(collisionColumn.Key);
            columns.Select(x => x.BindingKey).Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public void buildLeaveTableProjection_Returns_Readonly_System_Attribute_And_Audit_Values()
        {
            var fixture = CreateFixture();
            var attribute = fixture.Leave.Attributes.Single(x => x.Name == "Цена, руб");
            var secondUuid = Guid.CreateVersion7();
            _ = new ElementAttributeModel(
                secondUuid,
                fixture.Node,
                secondUuid,
                fixture.Node,
                fixture.Node.OwningWorkingTree,
                new FakeNotificationService(),
                new EmptyPropertiesPolicy<ElementAttributeModel>())
            {
                Name = "Цена, руб",
                ValueType = fixture.Node,
                Value = fixture.Leave,
            };

            var columns = LeaveTableProjectionBuilder.buildLeaveTableColumns([fixture.Leave]);
            var row = LeaveTableProjectionBuilder
                .buildLeaveTableRows([fixture.Leave], columns)
                .Single();

            columns.Select(x => x.Key).Should().StartWith(new[]
            {
                nameof(ISequencableModel.Sequence),
                nameof(ILinkableByUuidModel.Uuid),
                nameof(IMainEntityModel.Name),
                nameof(IMainEntityModel.Description),
                attribute.DeclaringUuid.ToString(),
                secondUuid.ToString(),
            });
            columns.Should().OnlyContain(x => x.IsReadOnly);
            columns.Where(x => x.Header == "Цена, руб").Should().HaveCount(2);
            row[nameof(ISequencableModel.Sequence)].Should().Be(10L);
            row[nameof(ILinkableByUuidModel.Uuid)].Should().Be(fixture.Leave.Uuid);
            row[attribute.DeclaringUuid.ToString()].Should().Be(fixture.PriceValue);
            row[secondUuid.ToString()].Should().Be(fixture.Leave);
            row[$"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.CreatedBy)}"]
                .Should().Be("user123");
        }

        [Fact]
        public void buildChildCollectionTableRows_Provides_CheckBox_Cell_State()
        {
            var fixture = CreateFixture();
            var isSelected = false;
            var column = new ChildCollectionTableColumn(
                "Selected",
                "Выбрано",
                0,
                _ => isSelected,
                isReadOnly: false,
                setterFactory: _ => value =>
                {
                    isSelected = value is true;
                    return isSelected;
                },
                columnType: ChildCollectionTableColumnType.CheckBox,
                cellEnabledGetter: _ => false,
                cellToolTipGetter: _ => "Выбор заблокирован");

            var row = ChildCollectionTableBuilder
                .buildChildCollectionTableRows([fixture.Leave], [column])
                .Single();

            column.ColumnType.Should().Be(ChildCollectionTableColumnType.CheckBox);
            row.CellEnabledStates[column.BindingKey].Should().BeFalse();
            row.CellToolTips[column.BindingKey].Should().Be("Выбор заблокирован");

            row[column.Key] = true;

            isSelected.Should().BeTrue();
            row[column.Key].Should().Be(true);
        }

        [Fact]
        public void buildChildCollectionTableColumns_Uses_Current_Element_Visible_Attributes_And_Excludes_Child_Own_Attributes()
        {
            var fixture = CreateFixture();
            var childAttributeUuid = Guid.CreateVersion7();
            _ = new ElementAttributeModel(
                childAttributeUuid,
                fixture.Node,
                childAttributeUuid,
                fixture.Node,
                fixture.Node.OwningWorkingTree,
                new FakeNotificationService(),
                new EmptyPropertiesPolicy<ElementAttributeModel>())
            {
                Name = "Child only",
                Visibility = VisibilityScope.Public,
            };

            var children = ChildCollectionTableBuilder.buildChildCollectionTableChildren(fixture.Root);

            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(
                fixture.Root,
                children);

            columns.Select(x => x.Header).Should().Contain("Цена, руб");
            columns.Select(x => x.Header).Should().NotContain("Child only");
        }

        [Fact]
        public void buildChildCollectionTableRows_Maps_Child_Values_And_Dynamic_Attributes()
        {
            var fixture = CreateFixture();
            var children = new IChildrenModel[] { fixture.Leave };
            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(
                fixture.Root,
                children);

            var rows = ChildCollectionTableBuilder.buildChildCollectionTableRows(
                children,
                columns);

            var row = rows.Single();
            var priceColumn = columns.Single(x => x.Header == "Цена, руб");

            row[nameof(IChildrenModel.SequencePath)].Should().Be("1.10");
            row[nameof(IMainEntityModel.State)].Should().Be(fixture.Leave.State);
            row[nameof(IMainEntityModel.Type)].Should().Be(nameof(TreeLeaveModel));
            row[nameof(IMainEntityModel.Name)].Should().Be("Шток");
            row[nameof(IMainEntityModel.Description)].Should().Be("Шток насоса");
            row[$"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.CreatedBy)}"].Should().Be("user123");
            row[priceColumn.Key].Should().Be(fixture.PriceValue);
            row[priceColumn.BindingKey].Should().Be(fixture.PriceValue);
        }

        [Fact]
        public void buildChildCollectionTableRows_Excludes_Sequenced_Root_From_SequencePath()
        {
            var fixture = CreateFixture();
            fixture.Root.Sequence = 30;
            var children = new IChildrenModel[] { fixture.Leave };
            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(
                fixture.Root,
                children);

            var row = ChildCollectionTableBuilder.buildChildCollectionTableRows(
                children,
                columns)
                .Single();

            row[nameof(IChildrenModel.SequencePath)].Should().Be("1.10");
        }

        [Fact]
        public void buildChildCollectionTableColumns_Marks_Writable_Property_Columns_As_Editable()
        {
            var fixture = CreateFixture();

            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(
                fixture.Root,
                fixture.Root.Childs.Values);

            columns.Select(x => x.Key).Should().NotContain(nameof(ISequencableModel.Sequence));
            columns.Single(x => x.Key == nameof(IChildrenModel.SequencePath)).IsReadOnly.Should().BeFalse();
            columns.Single(x => x.Key == nameof(IMainEntityModel.State)).IsReadOnly.Should().BeTrue();
            columns.Single(x => x.Key == nameof(IMainEntityModel.Name)).IsReadOnly.Should().BeFalse();
            columns.Single(x => x.Key == nameof(IMainEntityModel.Description)).IsReadOnly.Should().BeFalse();
            columns.Single(x => x.Key == nameof(IWorkingTreeMemberModel.CustomCode)).IsReadOnly.Should().BeFalse();
            columns.Single(x => x.Key == nameof(IMainEntityModel.Type)).IsReadOnly.Should().BeTrue();
            columns.Single(x => x.Key == $"{nameof(IMainEntityModel.AuditInfo)}.{nameof(AuditInfoModel.CreatedAt)}").IsReadOnly.Should().BeTrue();
        }

        [Fact]
        public void buildChildCollectionTableRows_Allows_Editing_Writable_Property_Cells()
        {
            var fixture = CreateFixture();
            var children = new IChildrenModel[] { fixture.Leave };
            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(
                fixture.Root,
                children);
            var row = ChildCollectionTableBuilder.buildChildCollectionTableRows(children, columns).Single();

            row[nameof(IChildrenModel.SequencePath)] = "999.42";
            row[nameof(IMainEntityModel.Name)] = "Edited name";
            row[nameof(IMainEntityModel.Description)] = "Edited description";
            row[nameof(IWorkingTreeMemberModel.CustomCode)] = "EDIT-1";

            fixture.Leave.Sequence.Should().Be(42);
            row[nameof(IChildrenModel.SequencePath)].Should().Be("1.42");
            fixture.Leave.Name.Should().Be("Edited name");
            fixture.Leave.Description.Should().Be("Edited description");
            fixture.Leave.CustomCode.Should().Be("EDIT-1");
            row[nameof(IMainEntityModel.State)].Should().Be(fixture.Leave.State);
        }

        [Fact]
        public void buildChildCollectionTableRows_Rolls_Back_Sequence_Cell_On_Invalid_Number()
        {
            var fixture = CreateFixture();
            var children = new IChildrenModel[] { fixture.Leave };
            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(
                fixture.Root,
                children);
            var row = ChildCollectionTableBuilder.buildChildCollectionTableRows(children, columns).Single();

            row[nameof(IChildrenModel.SequencePath)] = "not a number";

            fixture.Leave.Sequence.Should().Be(10);
            row[nameof(IChildrenModel.SequencePath)].Should().Be("1.10");
        }

        [Fact]
        public void buildChildCollectionTableRows_Notifies_Cell_Change_After_Successful_Edit()
        {
            var fixture = CreateFixture();
            var children = new IChildrenModel[] { fixture.Leave };
            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(
                fixture.Root,
                children);
            var changedCells = new List<(Guid SourceUuid, string ColumnKey)>();

            var row = ChildCollectionTableBuilder
                .buildChildCollectionTableRows(children, columns, (sourceUuid, columnKey) => changedCells.Add((sourceUuid, columnKey)))
                .Single();

            row[nameof(IMainEntityModel.Name)] = "Edited name";

            changedCells.Should().ContainSingle()
                .Which.Should().Be((fixture.Leave.Uuid, nameof(IMainEntityModel.Name)));
        }

        [Fact]
        public void buildChildCollectionTableRows_Allows_Editing_Single_Attribute_Value_Cells()
        {
            var fixture = CreateFixture();
            var newPriceValue = new TreeLeaveModel(
                Guid.CreateVersion7(),
                fixture.Node,
                fixture.Root.OwningWorkingTree,
                new FakeNotificationService(),
                new EmptyPropertiesPolicy<TreeLeaveModel>())
            {
                Name = "12000",
            };
            var children = new IChildrenModel[] { fixture.Leave };
            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(
                fixture.Root,
                children);
            var priceColumn = columns.Single(x => x.Header == "Цена, руб");
            var row = ChildCollectionTableBuilder.buildChildCollectionTableRows(children, columns).Single();

            row.ValueOptions[priceColumn.BindingKey].Should().Contain(newPriceValue);

            row[priceColumn.BindingKey] = newPriceValue;

            row[priceColumn.Key].Should().Be(newPriceValue);
            row[priceColumn.BindingKey].Should().Be(newPriceValue);
            fixture.Leave.Attributes.Single(x => x.Name == "Цена, руб").Value.Should().Be(newPriceValue);
        }

        [Fact]
        public void buildChildCollectionTableRows_Allows_Formula_Input_Via_EditText()
        {
            var fixture = CreateFixture();
            var children = new IChildrenModel[] { fixture.Leave };
            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(
                fixture.Root,
                children);
            var priceColumn = columns.Single(x => x.Header == "Цена, руб");
            var row = ChildCollectionTableBuilder.buildChildCollectionTableRows(children, columns).Single();

            row.EditText[priceColumn.BindingKey] = "=1+2";

            fixture.Leave.Attributes.Single(x => x.Name == "Цена, руб").ValueFormula.Should().Be("=1+2");
        }

        [Fact]
        public void buildChildCollectionTableRows_Resolves_Leaf_Reference_Via_EditText()
        {
            var fixture = CreateFixture();
            var newPriceValue = new TreeLeaveModel(
                Guid.CreateVersion7(),
                fixture.Node,
                fixture.Root.OwningWorkingTree,
                new FakeNotificationService(),
                new EmptyPropertiesPolicy<TreeLeaveModel>())
            {
                Name = "12000",
            };
            var children = new IChildrenModel[] { fixture.Leave };
            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(
                fixture.Root,
                children);
            var priceColumn = columns.Single(x => x.Header == "Цена, руб");
            var row = ChildCollectionTableBuilder.buildChildCollectionTableRows(children, columns).Single();

            row.ValueOptions[priceColumn.BindingKey].Should().Contain(newPriceValue);

            // Выбор из списка пишет в Text «[uuid]» (TextSearch.TextBinding) — сеттер EditText его разбирает.
            row.EditText[priceColumn.BindingKey] = $"[{newPriceValue.Uuid}]";

            fixture.Leave.Attributes.Single(x => x.Name == "Цена, руб").Value.Should().Be(newPriceValue);
        }

        [Fact]
        public void buildChildCollectionTableRows_Allows_Manual_SystemBase_Attribute_Value_Input()
        {
            var notificationService = new FakeNotificationService();
            var tree = new FakeWorkingTreeModel();
            var root = new TreeRootModel(
                Guid.CreateVersion7(),
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeRootModel>());

            var stringNode = new SystemBaseTreeNodeModel(
                root,
                tree,
                SystemBaseType.STRING,
                notificationService,
                new EmptyPropertiesPolicy<TreeNodeModel>());

            var existingValue = new SystemBaseTreeLeaveModel(
                Guid.CreateVersion7(),
                stringNode,
                tree,
                SystemBaseType.STRING,
                notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>());
            existingValue.StringValue = "Existing";

            var child = new TreeLeaveModel(
                Guid.CreateVersion7(),
                stringNode,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>())
            {
                Name = "Child",
            };

            var attributeUuid = Guid.CreateVersion7();
            var attribute = new ElementAttributeModel(
                attributeUuid,
                root,
                attributeUuid,
                root,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<ElementAttributeModel>())
            {
                Name = "Text",
                Visibility = VisibilityScope.Public,
                ValueType = stringNode,
                Value = existingValue,
            };

            _ = attribute;
            _ = child.Attributes;
            var children = new IChildrenModel[] { child };
            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(root, children);
            var textColumn = columns.Single(x => x.Header == "Text");
            var row = ChildCollectionTableBuilder.buildChildCollectionTableRows(children, columns).Single();

            row[textColumn.BindingKey] = "Manual";

            var assignedValue = child.Attributes.Single(x => x.Name == "Text").Value;
            assignedValue.Should().BeOfType<SystemBaseTreeLeaveModel>();
            assignedValue.ParentNode.Should().Be(stringNode);
            ((SystemBaseTreeLeaveModel)assignedValue).StringValue.Should().Be("Manual");
            stringNode.ChildLeaves.OfType<SystemBaseTreeLeaveModel>()
                .Count(x => x.StringValue == "Manual")
                .Should().Be(1);
        }

        [Fact]
        public void buildChildCollectionTableRows_Ignores_String_Input_For_Non_SystemBase_Attribute()
        {
            var fixture = CreateFixture();
            var children = new IChildrenModel[] { fixture.Leave };
            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(
                fixture.Root,
                children);
            var priceColumn = columns.Single(x => x.Header == "Цена, руб");
            var row = ChildCollectionTableBuilder.buildChildCollectionTableRows(children, columns).Single();

            row[priceColumn.BindingKey] = "not a TreeLeave";

            fixture.Leave.Attributes.Single(x => x.Name == "Цена, руб").Value.Should().Be(fixture.PriceValue);
            row[priceColumn.BindingKey].Should().Be(fixture.PriceValue);
        }

        [Fact]
        public void buildChildCollectionTableRows_Returns_Fallbacks_For_Unknown_Child_And_Missing_Values()
        {
            var child = new UnknownChildModel();
            var columns = ChildCollectionTableBuilder.buildChildCollectionTableColumns(null, new[] { child });

            var rows = ChildCollectionTableBuilder.buildChildCollectionTableRows(new[] { child }, columns);

            rows.Should().ContainSingle();
            rows[0][nameof(IMainEntityModel.Type)].Should().Be(nameof(UnknownChildModel));
            rows[0][nameof(IMainEntityModel.Name)].Should().BeNull();
        }

        [Fact]
        public void buildChildCollectionTableChildren_Returns_Depth_First_Descendants_Under_Their_Parents()
        {
            var fixture = CreateRecursiveFixture();

            var children = ChildCollectionTableBuilder.buildChildCollectionTableChildren(fixture.Root);

            children
                .OfType<IMainEntityModel>()
                .Select(x => x.Name)
                .Should()
                .Equal("Land", "Car", "Train", "Air", "Plane", "Helicopter");
        }

        private static TestFixture CreateFixture()
        {
            var notificationService = new FakeNotificationService();
            var tree = new FakeWorkingTreeModel();
            var root = new TreeRootModel(
                Guid.CreateVersion7(),
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeRootModel>())
            {
                Name = "Насос",
            };

            var node = new TreeNodeModel(
                Guid.CreateVersion7(),
                root,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeNodeModel>())
            {
                Name = "Узел",
                Sequence = 1,
                CustomCode = "NODE-1",
            };

            var leave = new TreeLeaveModel(
                Guid.CreateVersion7(),
                node,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>())
            {
                Name = "Шток",
                Description = "Шток насоса",
                Sequence = 10,
                CustomCode = "LEAVE-1",
                AuditInfo = new AuditInfoModel
                {
                    CreatedBy = "user123",
                    CreatedAt = new DateTime(2026, 1, 1, 16, 0, 0, DateTimeKind.Utc),
                },
            };

            var priceValue = new TreeLeaveModel(
                Guid.CreateVersion7(),
                node,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>())
            {
                Name = "10000",
                Sequence = 20,
            };

            var attributeUuid = Guid.CreateVersion7();
            var attribute = new ElementAttributeModel(
                attributeUuid,
                root,
                attributeUuid,
                root,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<ElementAttributeModel>())
            {
                Name = "Цена, руб",
                Visibility = VisibilityScope.Public,
                ValueType = node,
                Value = priceValue,
            };

            _ = attribute;
            _ = leave.Attributes;

            return new TestFixture(root, node, leave, priceValue);
        }

        private static RecursiveFixture CreateRecursiveFixture()
        {
            var notificationService = new FakeNotificationService();
            var tree = new FakeWorkingTreeModel();
            var root = new TreeRootModel(
                Guid.CreateVersion7(),
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeRootModel>());

            var land = new TreeNodeModel(
                Guid.CreateVersion7(),
                root,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeNodeModel>())
            {
                Name = "Land",
                Sequence = 1,
            };

            var air = new TreeNodeModel(
                Guid.CreateVersion7(),
                root,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeNodeModel>())
            {
                Name = "Air",
                Sequence = 2,
            };

            _ = new TreeNodeModel(
                Guid.CreateVersion7(),
                land,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeNodeModel>())
            {
                Name = "Car",
                Sequence = 1,
            };

            _ = new TreeLeaveModel(
                Guid.CreateVersion7(),
                land,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>())
            {
                Name = "Train",
                Sequence = 2,
            };

            _ = new TreeLeaveModel(
                Guid.CreateVersion7(),
                air,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>())
            {
                Name = "Plane",
                Sequence = 1,
            };

            _ = new TreeLeaveModel(
                Guid.CreateVersion7(),
                air,
                tree,
                notificationService,
                new EmptyPropertiesPolicy<TreeLeaveModel>())
            {
                Name = "Helicopter",
                Sequence = 2,
            };

            return new RecursiveFixture(root);
        }

        private sealed record TestFixture(TreeRootModel Root, TreeNodeModel Node, TreeLeaveModel Leave, TreeLeaveModel PriceValue);

        private sealed record RecursiveFixture(TreeRootModel Root);

        private sealed class UnknownChildModel : IChildrenModel
        {
            public Guid Uuid { get; } = Guid.CreateVersion7();

            public IParentModel Parent => throw new NotImplementedException();

            public ReadOnlyDictionary<Guid, IParentModel> AllParentsRecursive => new(
                new Dictionary<Guid, IParentModel>());

            public bool ChangeParent(IParentModel newParent)
            {
                return false;
            }
        }
    }
}
