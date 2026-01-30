using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Tasky.Administration.EntityFrameworkCore;

[ConnectionStringName(TaskyNames.AdministrationDb)]
public interface IAdministrationDbContext : IEfCoreDbContext { }
