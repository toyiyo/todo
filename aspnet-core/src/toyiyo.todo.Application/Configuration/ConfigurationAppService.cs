using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using toyiyo.todo.Configuration.Dto;

namespace toyiyo.todo.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : todoAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
