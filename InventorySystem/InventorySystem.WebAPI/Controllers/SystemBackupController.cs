using InventorySystem.WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/system/backup")]
    [Authorize(Roles = "SuperAdmin")]
    public class SystemBackupController : ControllerBase
    {
        private readonly ISystemBackupService _backup;

        public SystemBackupController(ISystemBackupService backup)
        {
            _backup = backup;
        }

        [HttpGet("list")]
        public async Task<IActionResult> List(CancellationToken ct) => Ok(await _backup.ListBackupsAsync(ct));

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] BackupCreateRequest request, CancellationToken ct)
        {
            try { return Ok(await _backup.CreateBackupAsync(request.Label, ct)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPost("restore")]
        public async Task<IActionResult> Restore([FromBody] BackupRestoreRequest request, CancellationToken ct)
        {
            try { return Ok(await _backup.RestoreBackupAsync(request.FileName, ct)); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }

    public class BackupCreateRequest
    {
        public string? Label { get; set; }
    }

    public class BackupRestoreRequest
    {
        public string FileName { get; set; } = string.Empty;
    }
}
