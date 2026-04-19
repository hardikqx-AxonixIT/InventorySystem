using System.Collections.Generic;

namespace InventorySystem.Application.Interfaces
{
    public interface ICommercialService
    {
        IReadOnlyList<object> GetPlans();

        object StartTrial(string tenantId, string email, int days);

        object ActivatePlan(string tenantId, string planCode, int months);

        object GenerateLicense(string tenantId, int months);

        object ValidateLicense(string key);

        object GetSubscription(string tenantId);
    }
}
