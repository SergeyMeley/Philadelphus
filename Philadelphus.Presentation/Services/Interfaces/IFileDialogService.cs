namespace Philadelphus.Presentation.Wpf.UI.Services
{
    public interface IFileDialogService
    {
        string? OpenExcelFile();

        string? SavePhjsonFile(string defaultFileName);

        string? OpenImportSchemaFile();

        string? SaveImportSchemaFile(string defaultFileName);
    }
}
