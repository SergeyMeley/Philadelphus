using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.PostgreRepository.Repositories;
using System.IO;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void PathesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            DataTreeRepositoryService repositoryService = new DataTreeRepositoryService();
            var treeRepository = repositoryService.CreateRepository();
            treeRepository.DirectoryPath = NewPathTextBox.Text;
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            var mainEntityRepository = new Philadelphus.MongoRepository.Repositories.MainEntitóRepository();

            var rts = new List<DbTreeRoot>();
            rts.Add(new DbTreeRoot());

            mainEntityRepository.InsertRoots(rts);


            var result = mainEntityRepository.SelectRoots(new Philadelphus.InfrastructureEntities.MainEntities.DbTreeRepository());

            foreach (var root in result)
            { 
                PathesListBox.Items.Add(root.Name);
            }






            //DataTreeRepositoryService repositoryListService = new DataTreeRepositoryService();

            //PathesListBox.Items.Clear();
            //var list = repositoryListService.GetRepositoryList();
            //if (((List<TreeRepository>)list).Count > 0)
            //{
            //    foreach (var repo in list)
            //    {
            //        PathesListBox.Items.Add(repo.DirectoryPath);
            //    }
            //}
            //else
            //{
            //    PathesListBox.Items.Add("Ñïèñîê ðåïîçèòîðèåâ ïóñò");
            //}
        }

        private void NewPathTextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
