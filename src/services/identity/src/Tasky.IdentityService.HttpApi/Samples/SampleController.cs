using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace Tasky.IdentityService.Samples;

[Area(IdentityServiceRemoteServiceConsts.ModuleName)]
[RemoteService(Name = IdentityServiceRemoteServiceConsts.RemoteServiceName)]
[Route("api/IdentityService/sample")]
public class SampleController(ISampleAppService sampleAppService)
    : IdentityServiceController,
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
