using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Domain.Entities
{
    public class RolePermission : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ModuleKey { get; set; } = string.Empty;

        public bool CanView { get; set; } = true;
        public bool CanCreate { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public bool CanApprove { get; set; }
        public bool CanExport { get; set; }
    }
}
