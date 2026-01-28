using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace Tasky.Administration.Samples;

[Area(AdministrationRemoteServiceConsts.ModuleName)]
[RemoteService(Name = AdministrationRemoteServiceConsts.RemoteServiceName)]
[Route("api/Administration/sample")]
public class SampleController(ISampleAppService sampleAppService)
    : AdministrationController,
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
