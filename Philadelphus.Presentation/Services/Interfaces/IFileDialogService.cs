namespace Philadelphus.Presentation.Services.Interfaces
{
    public interface IFileDialogService
    {
        string? BrowseLocalFile();

        string? OpenExcelFile();

        string? SavePhjsonFile(string defaultFileName);

        string? OpenImportSchemaFile();

        string? SaveImportSchemaFile(string defaultFileName);

        string? OpenFile(string? filter = null, string? defaultExtension = null, string? title = null, string? initialDirectory = null);

        string? SaveFile(string? filter = null, string? defaultExtension = null, string? title = null, string? initialDirectory = null);

        string? BrowseFolder(string? title = null, string? initialDirectory = null);
    }
}
