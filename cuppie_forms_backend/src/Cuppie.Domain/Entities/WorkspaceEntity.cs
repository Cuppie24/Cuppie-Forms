namespace Cuppie.Domain.Entities;

public class WorkspaceEntity
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int OrganisationId { get; set; }
    public OrganisationEntity OrganisationEntity { get; set; } = null!;
    public ICollection<PageEntity> Pages { get; set; } = new List<PageEntity>();
}