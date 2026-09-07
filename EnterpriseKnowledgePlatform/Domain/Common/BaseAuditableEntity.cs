
namespace Domain.Common;

public class BaseAuditableEntity<TU> : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public TU? CreatedBy { get; set; }
    
    public DateTime ModifiedAt { get; set; }
    public TU? ModifiedBy { get; set; }
}