using System.Security.Cryptography;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.WebAPI.Services
{
    public sealed class EfCommercialService : ICommercialService
    {
        private readonly ApplicationDbContext _db;

        private static readonly SubscriptionPlan[] Plans =
        {
            new("STARTER", "Starter", 1499m, 5, true),
            new("GROWTH", "Growth", 3999m, 25, true),
            new("ENTERPRISE", "Enterprise", 9999m, 999, true)
        };

        public EfCommercialService(ApplicationDbContext db)
        {
            _db = db;
        }

        public IReadOnlyList<object> GetPlans() => Plans
            .Select(x => (object)new { x.Code, x.Name, x.MonthlyPrice, x.MaxUsers, x.IncludesSupport })
            .ToList();

        public object StartTrial(string tenantId, string email, int days)
        {
            var now = DateTime.UtcNow;
            var existing = _db.TenantSubscriptions.FirstOrDefault(x => x.TenantId == tenantId);
            if (existing == null)
            {
                _db.TenantSubscriptions.Add(new TenantSubscriptionRecord
                {
                    TenantId = tenantId,
                    Email = email,
                    PlanCode = "TRIAL",
                    StartedAtUtc = now,
                    ExpiresAtUtc = now.AddDays(days <= 0 ? 14 : days),
                    IsActive = true
                });
            }
            else
            {
                existing.Email = email;
                existing.PlanCode = "TRIAL";
                existing.StartedAtUtc = now;
                existing.ExpiresAtUtc = now.AddDays(days <= 0 ? 14 : days);
                existing.IsActive = true;
            }

            _db.SaveChanges();
            var sub = _db.TenantSubscriptions.First(x => x.TenantId == tenantId);
            return new { success = true, sub.TenantId, sub.PlanCode, sub.ExpiresAtUtc };
        }

        public object ActivatePlan(string tenantId, string planCode, int months)
        {
            var selected = Plans.FirstOrDefault(x => x.Code.Equals(planCode, StringComparison.OrdinalIgnoreCase));
            if (selected == null) throw new InvalidOperationException("Invalid plan code.");

            var now = DateTime.UtcNow;
            var m = months <= 0 ? 1 : months;
            var existing = _db.TenantSubscriptions.FirstOrDefault(x => x.TenantId == tenantId);
            if (existing == null)
            {
                _db.TenantSubscriptions.Add(new TenantSubscriptionRecord
                {
                    TenantId = tenantId,
                    Email = string.Empty,
                    PlanCode = selected.Code,
                    StartedAtUtc = now,
                    ExpiresAtUtc = now.AddMonths(m),
                    IsActive = true
                });
            }
            else
            {
                existing.PlanCode = selected.Code;
                existing.StartedAtUtc = now;
                existing.ExpiresAtUtc = now.AddMonths(m);
                existing.IsActive = true;
            }

            _db.SaveChanges();
            var sub = _db.TenantSubscriptions.First(x => x.TenantId == tenantId);
            return new { success = true, sub.TenantId, sub.PlanCode, sub.ExpiresAtUtc };
        }

        public object GenerateLicense(string tenantId, int months)
        {
            var key = $"AXN-{RandomNumberGenerator.GetInt32(100000, 999999)}-{RandomNumberGenerator.GetInt32(100000, 999999)}";
            var m = months <= 0 ? 1 : months;
            _db.CommercialLicenses.Add(new CommercialLicenseRecord
            {
                LicenseKey = key,
                TenantId = tenantId,
                IssuedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddMonths(m),
                IsActive = true
            });
            _db.SaveChanges();
            return new { success = true, key, tenantId, expiresAtUtc = DateTime.UtcNow.AddMonths(m) };
        }

        public object ValidateLicense(string key)
        {
            var record = _db.CommercialLicenses.AsNoTracking().FirstOrDefault(x => x.LicenseKey == key);
            if (record == null)
            {
                return new { valid = false, reason = "License not found" };
            }

            var valid = record.IsActive && record.ExpiresAtUtc > DateTime.UtcNow;
            return new
            {
                valid,
                record.TenantId,
                record.ExpiresAtUtc,
                reason = valid ? "OK" : "Expired or inactive"
            };
        }

        public object GetSubscription(string tenantId)
        {
            var sub = _db.TenantSubscriptions.AsNoTracking().FirstOrDefault(x => x.TenantId == tenantId);
            if (sub == null)
            {
                return new { found = false, tenantId };
            }

            return new
            {
                found = true,
                sub.TenantId,
                sub.Email,
                sub.PlanCode,
                sub.StartedAtUtc,
                sub.ExpiresAtUtc,
                sub.IsActive
            };
        }

        private sealed record SubscriptionPlan(string Code, string Name, decimal MonthlyPrice, int MaxUsers, bool IncludesSupport);
    }
}
