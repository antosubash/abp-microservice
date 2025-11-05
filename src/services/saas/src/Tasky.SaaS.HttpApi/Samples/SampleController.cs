using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace Tasky.SaaS.Samples;

[Area(SaaSRemoteServiceConsts.ModuleName)]
[RemoteService(Name = SaaSRemoteServiceConsts.RemoteServiceName)]
[Route("api/SaaS/sample")]
public class SampleController(ISampleAppService sampleAppService)
    : SaaSController,
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
