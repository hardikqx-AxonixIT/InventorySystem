namespace InventorySystem.Domain.Entities
{
    public class BomTemplateItem : BaseEntity
    {
        public int BomTemplateId { get; set; }
        public BomTemplate? BomTemplate { get; set; }

        public int ComponentProductId { get; set; }
        public Product? ComponentProduct { get; set; }

        public decimal QuantityPerOutput { get; set; }
    }
}
