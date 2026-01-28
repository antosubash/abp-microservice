using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.Localization;
using Volo.Abp.OpenIddict.Applications;

namespace Tasky.Pages;

public class IndexModel(
    IOpenIddictApplicationRepository openIdApplicationRepository,
    ILanguageProvider languageProvider
) : AbpPageModel
{
    public List<OpenIddictApplication> Applications { get; protected set; }

    public IReadOnlyList<LanguageInfo> Languages { get; protected set; }

    public string CurrentLanguage { get; protected set; }

    protected IOpenIddictApplicationRepository OpenIdApplicationRepository { get; } = openIdApplicationRepository;

    protected ILanguageProvider LanguageProvider { get; } = languageProvider;

    public async Task OnGetAsync()
    {
        Applications = await OpenIdApplicationRepository.GetListAsync().ConfigureAwait(false);

        Languages = await LanguageProvider.GetLanguagesAsync().ConfigureAwait(false);
        CurrentLanguage = CultureInfo.CurrentCulture.DisplayName;
    }
}
