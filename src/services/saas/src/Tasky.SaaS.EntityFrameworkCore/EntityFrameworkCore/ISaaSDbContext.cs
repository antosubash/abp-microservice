using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Tasky.SaaS.EntityFrameworkCore;

[ConnectionStringName(TaskyNames.SaaSDb)]
public interface ISaaSDbContext : IEfCoreDbContext { }
