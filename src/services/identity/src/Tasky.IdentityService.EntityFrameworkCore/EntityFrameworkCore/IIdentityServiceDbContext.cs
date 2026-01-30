using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Tasky.IdentityService.EntityFrameworkCore;

[ConnectionStringName(TaskyNames.IdentityServiceDb)]
public interface IIdentityServiceDbContext : IEfCoreDbContext { }
