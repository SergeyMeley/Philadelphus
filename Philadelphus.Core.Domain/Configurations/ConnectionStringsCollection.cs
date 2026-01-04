namespace Philadelphus.Core.Domain.Configurations
{
    public class ConnectionStringsCollection
    {
        public IEnumerable<ConnectionStringContainer> ConnectionStringContainers { get; set; } = new List<ConnectionStringContainer>();
    }
}
