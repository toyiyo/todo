using System;
using System.Threading.Tasks;
using toyiyo.todo.Projects;
using toyiyo.todo.Sessions;
using Xunit;
using Shouldly;
using System.Linq;

namespace toyiyo.todo.Tests.Projects
{
    public class ProjectManagerTests : todoTestBase
    {
        private readonly IProjectManager _projectManager;
        private readonly ISessionAppService _sessionAppService;
        public ProjectManagerTests()
        {
            _projectManager = Resolve<IProjectManager>();
            _sessionAppService = Resolve<ISessionAppService>();
        }
        [Fact]
        public async Task CreateProject_ReturnsNewProject()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();

            // Act
            var project = Project.Create("test", currentUser, currentTenant.Id);
            var result = await _projectManager.Create(project);
            
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
            var project = Project.Create("test", currentUser, currentTenant.Id);
            await _projectManager.Create(project);

            // Act
            var result = await _projectManager.Get(project.Id);

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
            var project = Project.Create("test", currentUser, currentTenant.Id);
            await _projectManager.Create(project);

            // Act
            var result = await _projectManager.GetAll(new GetAllProjectsInput(){
                MaxResultCount = 10,
                SkipCount = 0,
                keyword = "test"
            });

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(1);
            result.First().Title.ShouldBe("test");
        }
    }
}
