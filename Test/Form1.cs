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
            var mainEntityRepository = new Philadelphus.MongoRepository.Repositories.MainEntitóRepository();
            var entities = new List<DbTreeRoot>();
            for (int i = 0; i < 5; i++)
            {
                entities.Add(new DbTreeRoot());
            }
            mainEntityRepository.InsertRoots((List<DbTreeRoot>)entities);
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            PathesListBox.Items.Clear();
            var mainEntityRepository = new Philadelphus.MongoRepository.Repositories.MainEntitóRepository();
            var result = mainEntityRepository.SelectRoots();

            foreach (var item in result)
            {
                PathesListBox.Items.Add(item.Guid);
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
