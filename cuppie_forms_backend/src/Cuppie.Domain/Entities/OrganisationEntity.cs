﻿using System.ComponentModel.DataAnnotations;

namespace Cuppie.Domain.Entities
{
    public class OrganisationEntity
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 3)]
        public string Name { get; set; } = null!;

        [Required]
        public DateOnly CreatedAt { get; set; }

        public ICollection<UserOrganisationEntity> UserOrganisations { get; set; } = new List<UserOrganisationEntity>();
        public ICollection<WorkspaceEntity> Workspaces { get; set; } = new List<WorkspaceEntity>();
    }
}
