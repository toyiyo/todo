using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using toyiyo.todo.Projects;
using toyiyo.todo.Projects.Dto;
using toyiyo.todo.Sessions;
using Xunit;

namespace toyiyo.todo.Tests.Projects
{
    public class ProjectAppServiceTests : todoTestBase
    {
        private readonly IProjectAppService _projectAppService;
        private readonly ISessionAppService _sessionAppService;
        public ProjectAppServiceTests()
        {
            _projectAppService = Resolve<IProjectAppService>();
            _sessionAppService = Resolve<ISessionAppService>();
        }
        [Fact]
        public async Task CreateProject_ReturnsNewProject()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();

            // Act
            var result = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });

            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe("test");
        }

        [Fact]
        public async Task GetProject_ReturnsProject()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });

            // Act
            var result = await _projectAppService.Get(project.Id);

            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe("test");
        }

        [Fact]
        public async Task GetAllProjects_ReturnsProjects()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });

            // Act
            var result = await _projectAppService.GetAll(new GetAllProjectsInput(){});

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(1);
            result.Items.First().Title.ShouldBe("test");
        }

        [Fact]
        public async Task GetAllProjects_FilterByTitle_ReturnsProjects()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            await _projectAppService.Create(new CreateProjectInputDto() { Title = "Test" });

            // Act
            var result = await _projectAppService.GetAll(new GetAllProjectsInput(){
                keyword = "test"
            });

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(1);
            result.Items.First().Title.ShouldBe("Test");
        }

        [Fact]
        public async Task UpdateProject_ReturnsUpdatedProject()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });

            // Act
            var result = await _projectAppService.SetTitle(new ProjectDto() { Id = project.Id, Title = "test2" });

            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe("test2");
        }
    }
}