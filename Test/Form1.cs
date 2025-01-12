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
            RepositoryListService repositoryListService = new RepositoryListService();
            TreeRepository treeRepository = new TreeRepository(NewPathTextBox.Text, NewPathTextBox.Text);
            repositoryListService.AddRepository(treeRepository);
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            RepositoryListService repositoryListService = new RepositoryListService();
            
            PathesListBox.Items.Clear();
            var list = repositoryListService.GetRepositoryList();
            if (((List<TreeRepository>)list).Count > 0)
            {
                foreach (var repo in list)
                {
                    PathesListBox.Items.Add(repo.Path);
                }
            }
            else 
            {
                PathesListBox.Items.Add("Список репозиториев пуст");
            }
        }
    }
}
