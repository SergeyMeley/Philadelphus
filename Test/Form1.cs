using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.Business.Entities.OtherEntities;
using Philadelphus.Business.Services;
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
            DataTreeRepositoryService repositoryListService = new DataTreeRepositoryService();
            TreeRepository treeRepository = new TreeRepository(NewPathTextBox.Text, Guid.Empty);
            treeRepository.DirectoryPath = NewPathTextBox.Text;
            repositoryListService.CreateRepository(treeRepository);
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            DataTreeRepositoryService repositoryListService = new DataTreeRepositoryService();

            PathesListBox.Items.Clear();
            var list = repositoryListService.GetRepositoryList();
            if (((List<TreeRepository>)list).Count > 0)
            {
                foreach (var repo in list)
                {
                    PathesListBox.Items.Add(repo.DirectoryPath);
                }
            }
            else
            {
                PathesListBox.Items.Add("Список репозиториев пуст");
            }
        }

        private void NewPathTextBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
