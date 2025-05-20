using System.ComponentModel.DataAnnotations;

namespace Cuppie.Domain.Entities;

public class PageEntity
{
    public int Id { get; set; }
    
    [Required]
    public string? Name { get; set; }
    public string? Content { get; set; }
    
    [Required]
    public int ParentPageId { get; set; }

    public PageEntity ParentPageEntity { get; set; } = null!;
        
    [Required]
    public int WorkspaceId { get; set; }
    public WorkspaceEntity WorkspaceEntity { get; set; } = null!;
}