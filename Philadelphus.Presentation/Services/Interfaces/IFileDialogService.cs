namespace Philadelphus.Presentation.Services.Interfaces
{
    public interface IFileDialogService
    {
        string? BrowseLocalFile();

        string? OpenExcelFile();

        string? SavePhjsonFile(string defaultFileName);

        string? OpenImportSchemaFile();

        string? SaveImportSchemaFile(string defaultFileName);
    }
}
