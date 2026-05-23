using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Policies.Attributes.Builders;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;
using Philadelphus.Core.Domain.Policies.Builders;
using Philadelphus.Core.Domain.Policies.Rules;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.Entities.MainEntities
{
    public class NamePropertiesRuleTests
    {
        [Fact]
        public void TreeNodeName_Should_Block_Class_Property_Name()
        {
            var rule = new ValidNamePropertiesRule<TreeNodeModel>(new FakeNotificationService(), NameUniquenessStrategy.TreeNode());
            var root = CreateRoot();
            var node = CreateNode(root);

            var result = rule.CanWrite(node, nameof(TreeNodeModel.Name), nameof(TreeNodeModel.ChildNodes));

            Assert.False(result);
        }

        [Fact]
        public void TreeNodeName_Should_Block_Class_Property_Display_Name()
        {
            var rule = new ValidNamePropertiesRule<TreeNodeModel>(new FakeNotificationService(), NameUniquenessStrategy.TreeNode());
            var root = CreateRoot();
            var node = CreateNode(root);

            var result = rule.CanWrite(node, nameof(TreeNodeModel.Name), "\u0421\u0438\u0441\u0442\u0435\u043c\u043d\u044b\u0439 \u0442\u0438\u043f");

            Assert.False(result);
        }

        [Fact]
        public void TreeNodeName_Should_Block_WorkingTreeMember_Property_Name()
        {
            var rule = new ValidNamePropertiesRule<TreeNodeModel>(new FakeNotificationService(), NameUniquenessStrategy.TreeNode());
            var root = CreateRoot();
            var node = CreateNode(root);

            var result = rule.CanWrite(node, nameof(TreeNodeModel.Name), nameof(WorkingTreeMemberBaseModel<TreeNodeModel>.CustomCode));

            Assert.False(result);
        }

        [Fact]
        public void TreeNodeName_Should_Block_Sibling_Leaf_Name()
        {
            var rule = new ValidNamePropertiesRule<TreeNodeModel>(new FakeNotificationService(), NameUniquenessStrategy.TreeNode());
            var root = CreateRoot();
            var node = CreateNode(root);
            var leaf = CreateLeaf(node);
            leaf.Name = "Same";

            var result = rule.CanWrite(CreateNode(node), nameof(TreeNodeModel.Name), "Same");

            Assert.False(result);
        }

        [Fact]
        public void ElementAttributeName_Should_Block_Owner_Child_Name()
        {
            var rule = new ValidNamePropertiesRule<ElementAttributeModel>(new FakeNotificationService(), NameUniquenessStrategy.ElementAttribute());
            var root = CreateRoot();
            var node = CreateNode(root);
            node.Name = "Same";
            var attribute = CreateOwnAttribute(root);

            var result = rule.CanWrite(attribute, nameof(ElementAttributeModel.Name), "Same");

            Assert.False(result);
        }

        [Fact]
        public void Name_Should_Remove_Special_Characters()
        {
            var root = CreateRoot();
            var notificationService = new FakeNotificationService();
            var node = new TreeNodeModel(
                Guid.NewGuid(),
                root,
                root.OwningWorkingTree,
                new FakeNotificationService(),
                PropertiesPolicyBuilder.CreateTreeNodeDefault(notificationService));

            node.Name = "  Bad{Name}[~]&%  ";

            Assert.Equal("BadName%", node.Name);
            Assert.Contains(notificationService.Messages, x => x.Contains("'{'")
                && x.Contains("'}'")
                && x.Contains("'['")
                && x.Contains("']'")
                && x.Contains("'~'")
                && x.Contains("'&'")
                && x.Contains("'%'") == false);
        }

        [Fact]
        public void Name_Should_Keep_Characters_Not_Listed_As_Invalid()
        {
            var root = CreateRoot();
            var node = new TreeNodeModel(
                Guid.NewGuid(),
                root,
                root.OwningWorkingTree,
                new FakeNotificationService(),
                PropertiesPolicyBuilder.CreateTreeNodeDefault(new FakeNotificationService()));

            node.Name = "Old";
            node.Name = "Bad@Name, мм";

            Assert.Equal("Bad@Name, мм", node.Name);
        }

        [Fact]
        public void Name_Should_Collapse_Duplicate_Spaces()
        {
            var root = CreateRoot();
            var node = new TreeNodeModel(
                Guid.NewGuid(),
                root,
                root.OwningWorkingTree,
                new FakeNotificationService(),
                PropertiesPolicyBuilder.CreateTreeNodeDefault(new FakeNotificationService()));

            node.Name = "  Bad   Name  Value  ";

            Assert.Equal("Bad Name Value", node.Name);
        }

        [Fact]
        public void Name_Should_Block_Empty_Value_After_Removing_Special_Characters()
        {
            var root = CreateRoot();
            var node = new TreeNodeModel(
                Guid.NewGuid(),
                root,
                root.OwningWorkingTree,
                new FakeNotificationService(),
                PropertiesPolicyBuilder.CreateTreeNodeDefault(new FakeNotificationService()));

            node.Name = "Old";
            node.Name = " {}[]~& ";

            Assert.Equal("Old", node.Name);
        }

        [Fact]
        public void WorkingTreeName_Should_Block_Other_Tree_Name_In_Shrub()
        {
            var rule = new ValidNamePropertiesRule<WorkingTreeModel>(new FakeNotificationService(), NameUniquenessStrategy.WorkingTree());
            var shrub = new FakeShrubModel();
            var tree = CreateWorkingTree(shrub);
            var otherTree = CreateWorkingTree(shrub);

            otherTree.Name = "Same";

            var result = rule.CanWrite(tree, nameof(WorkingTreeModel.Name), "Same");

            Assert.False(result);
        }

        [Fact]
        public void TreeRootName_Should_Block_Other_Root_Name_In_Shrub()
        {
            var rule = new ValidNamePropertiesRule<TreeRootModel>(new FakeNotificationService(), NameUniquenessStrategy.TreeRoot());
            var shrub = new FakeShrubModel();
            var firstTree = CreateWorkingTree(shrub);
            var secondTree = CreateWorkingTree(shrub);
            var firstRoot = CreateRoot(firstTree);
            var secondRoot = CreateRoot(secondTree);

            firstRoot.Name = "Same";

            var result = rule.CanWrite(secondRoot, nameof(TreeRootModel.Name), "Same");

            Assert.False(result);
        }

        [Fact]
        public void InheritedAttributeName_Should_Not_Be_Prepared_By_Name_Rule()
        {
            var rule = new ValidNamePropertiesRule<ElementAttributeModel>(new FakeNotificationService(), NameUniquenessStrategy.ElementAttribute());
            var root = CreateRoot();
            var node = CreateNode(root);
            var leaf = CreateLeaf(node);
            var attribute = CreateOwnAttribute(root);
            var inheritedAttribute = attribute.CloneForChild(leaf);

            var result = rule.PrepareWriteValue(inheritedAttribute, nameof(ElementAttributeModel.Name), "A&B");

            Assert.Equal("A&B", result);
        }

        [Fact]
        public void NonOwnRule_Should_Allow_Inherited_Attribute_Name_For_Leaf_Clone()
        {
            var rule = new NonOwnAttributePropertiesRule(new FakeNotificationService());
            var root = CreateRoot();
            var node = CreateNode(root);
            var leaf = CreateLeaf(node);
            var attribute = CreateOwnAttribute(root);
            attribute.Name = "Inherited";
            var inheritedAttribute = attribute.CloneForChild(leaf);

            var result = rule.CanWrite(inheritedAttribute, nameof(ElementAttributeModel.Name), "Inherited");

            Assert.True(result);
        }

        [Fact]
        public void Sequence_Should_Block_Duplicate_Value_In_Same_Collection()
        {
            var root = CreateRoot();
            var firstNode = CreateNodeWithDefaultPolicy(root);
            var secondNode = CreateNodeWithDefaultPolicy(root);

            firstNode.Sequence = 10;
            secondNode.Sequence = 10;

            Assert.Equal(0, secondNode.Sequence);
        }

        [Fact]
        public void Sequence_Should_Block_Not_Positive_Value()
        {
            var root = CreateRoot();
            var node = CreateNodeWithDefaultPolicy(root);

            node.Sequence = -1;

            Assert.Equal(0, node.Sequence);
        }

        [Fact]
        public void AttributeSequence_Should_Block_Duplicate_Inherited_Attribute_Value()
        {
            var root = CreateRoot();
            var node = CreateNode(root);
            var rootAttribute = CreateOwnAttribute(root);
            var inheritedAttribute = rootAttribute.CloneForChild(node);
            var ownAttribute = CreateOwnAttributeWithDefaultPolicy(node);

            inheritedAttribute.Sequence = 10;
            ownAttribute.Sequence = 10;

            Assert.Equal(0, ownAttribute.Sequence);
        }

        [Fact]
        public void AttributeName_Should_Block_Duplicate_Inherited_Attribute_Value()
        {
            var root = CreateRoot();
            var node = CreateNode(root);
            var rootAttribute = CreateOwnAttribute(root);
            rootAttribute.Name = "Same";
            _ = rootAttribute.CloneForChild(node);
            var ownAttribute = CreateOwnAttributeWithDefaultPolicy(node);

            ownAttribute.Name = "Same";

            Assert.Null(ownAttribute.Name);
        }

        [Fact]
        public void CustomCode_Should_Block_Duplicate_Value_In_WorkingTree()
        {
            var root = CreateRoot();
            var firstNode = CreateNodeWithDefaultPolicy(root);
            var secondNode = CreateNodeWithDefaultPolicy(root);

            firstNode.CustomCode = "CODE";
            secondNode.CustomCode = "CODE";

            Assert.Null(secondNode.CustomCode);
        }

        [Fact]
        public void AttributeCustomCode_Should_Block_Duplicate_Inherited_Attribute_Value()
        {
            var root = CreateRoot();
            var node = CreateNode(root);
            var rootAttribute = CreateOwnAttribute(root);
            var inheritedAttribute = rootAttribute.CloneForChild(node);
            inheritedAttribute.CustomCode = "CODE";
            var ownAttribute = CreateOwnAttributeWithDefaultPolicy(node);

            ownAttribute.CustomCode = "CODE";

            Assert.Null(ownAttribute.CustomCode);
        }

        [Fact]
        public void CustomCode_Should_Remove_Invalid_Characters_And_Block_Empty_Value()
        {
            var root = CreateRoot();
            var node = CreateNodeWithDefaultPolicy(root);

            node.CustomCode = "A{B}[C]~&";
            Assert.Equal("ABC", node.CustomCode);

            node.CustomCode = "AБ-12_%";
            Assert.Equal("A12", node.CustomCode);

            node.CustomCode = "{}[]~&Б_%";
            Assert.Equal("A12", node.CustomCode);
        }

        private static WorkingTreeModel CreateWorkingTree(ShrubModel shrub)
        {
            var tree = new TestWorkingTreeModel(shrub);
            shrub.AddContent(tree);
            return tree;
        }

        private static TreeRootModel CreateRoot()
        {
            var tree = new FakeWorkingTreeModel();
            tree.OwningShrub.AddContent(tree);
            return CreateRoot(tree);
        }

        private static TreeRootModel CreateRoot(WorkingTreeModel tree)
        {
            return new TreeRootModel(Guid.NewGuid(), tree, new FakeNotificationService(), new EmptyPropertiesPolicy<TreeRootModel>());
        }

        private static TreeNodeModel CreateNode(IParentModel parent)
        {
            var tree = parent is IWorkingTreeMemberModel workingTreeMember
                ? workingTreeMember.OwningWorkingTree
                : ((TreeRootModel)parent).OwningWorkingTree;

            return new TreeNodeModel(Guid.NewGuid(), parent, tree, new FakeNotificationService(), new EmptyPropertiesPolicy<TreeNodeModel>());
        }

        private static TreeNodeModel CreateNodeWithDefaultPolicy(IParentModel parent)
        {
            var tree = parent is IWorkingTreeMemberModel workingTreeMember
                ? workingTreeMember.OwningWorkingTree
                : ((TreeRootModel)parent).OwningWorkingTree;

            return new TreeNodeModel(
                Guid.NewGuid(),
                parent,
                tree,
                new FakeNotificationService(),
                PropertiesPolicyBuilder.CreateTreeNodeDefault(new FakeNotificationService()));
        }

        private static TreeLeaveModel CreateLeaf(TreeNodeModel parent)
        {
            return new TreeLeaveModel(Guid.NewGuid(), parent, parent.OwningWorkingTree, new FakeNotificationService(), new EmptyPropertiesPolicy<TreeLeaveModel>());
        }

        private static ElementAttributeModel CreateOwnAttribute(TreeRootModel owner)
        {
            var uuid = Guid.NewGuid();

            return new ElementAttributeModel(
                uuid,
                owner,
                uuid,
                owner,
                owner.OwningWorkingTree,
                new FakeNotificationService(),
                new EmptyPropertiesPolicy<ElementAttributeModel>());
        }

        private static ElementAttributeModel CreateOwnAttributeWithDefaultPolicy(IAttributeOwnerModel owner)
        {
            var uuid = Guid.NewGuid();
            var tree = owner is IWorkingTreeMemberModel workingTreeMember
                ? workingTreeMember.OwningWorkingTree
                : ((TreeRootModel)owner).OwningWorkingTree;
            var notificationService = new FakeNotificationService();

            return new ElementAttributeModel(
                uuid,
                owner,
                uuid,
                owner,
                tree,
                notificationService,
                AttributePolicyBuilder.CreateDefault(notificationService));
        }

        private class TestWorkingTreeModel : WorkingTreeModel
        {
            public TestWorkingTreeModel(ShrubModel owner)
                : base(Guid.NewGuid(), new FakeDataStorageModel(), owner, new FakeNotificationService(), new EmptyPropertiesPolicy<WorkingTreeModel>())
            {
            }
        }
    }
}
