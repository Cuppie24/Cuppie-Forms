using System.ComponentModel.DataAnnotations;

namespace Cuppie.Domain.Entities;

public class PageEntity
{
    public int Id { get; set; }

    [Required] public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    [Required]
    public int ParentPageId { get; set; }

    public PageEntity ParentPageEntity { get; set; } = null!;
        
    [Required]
    public int WorkspaceId { get; set; }
    public WorkspaceEntity WorkspaceEntity { get; set; } = null!;
}