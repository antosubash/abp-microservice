using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace Tasky.Projects.Samples;

[Area(ProjectsRemoteServiceConsts.ModuleName)]
[RemoteService(Name = ProjectsRemoteServiceConsts.RemoteServiceName)]
[Route("api/Projects/sample")]
public class SampleController(ISampleAppService sampleAppService)
    : ProjectsController,
        ISampleAppService
{
    private readonly ISampleAppService _sampleAppService = sampleAppService;

    [HttpGet]
    public Task<SampleDto> GetAsync()
    {
        return _sampleAppService.GetAsync();
    }

    [HttpGet]
    [Route("authorized")]
    [Authorize]
    public Task<SampleDto> GetAuthorizedAsync()
    {
        return _sampleAppService.GetAsync();
    }
}
