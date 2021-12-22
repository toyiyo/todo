using System.Threading.Tasks;
using toyiyo.todo.Configuration.Dto;

namespace toyiyo.todo.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
