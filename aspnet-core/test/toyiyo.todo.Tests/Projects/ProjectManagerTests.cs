using System;
using System.Threading.Tasks;
using toyiyo.todo.Projects;
using toyiyo.todo.Sessions;
using Xunit;
using Shouldly;
using System.Linq;
using System.Linq.Dynamic.Core.Exceptions;

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
            var result = await _projectManager.GetAll(new GetAllProjectsInput()
            {
                MaxResultCount = 10,
                SkipCount = 0,
                keyword = "test"
            });

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(1);
            result.First().Title.ShouldBe("test");
        }

        [Fact]
        public async Task GetAllProjects_FilterByKeyword_ReturnsProjects()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = Project.Create("test", currentUser, currentTenant.Id);
            var project2 = Project.Create("keyword", currentUser, currentTenant.Id);
            await _projectManager.Create(project);
            await _projectManager.Create(project2);

            // Act
            var result = await _projectManager.GetAll(new GetAllProjectsInput()
            {
                MaxResultCount = 10,
                SkipCount = 0,
                keyword = "test"
            });
            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(1);
            result.First().Title.ShouldBe("test");
        }

        [Fact]
        public async Task GetAllProjects_OrderByValidFieldAsc_ReturnsProjects()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = Project.Create("A", currentUser, currentTenant.Id);
            var project2 = Project.Create("Z", currentUser, currentTenant.Id);
            await _projectManager.Create(project);
            await _projectManager.Create(project2);

            // Act
            var result = await _projectManager.GetAll(new GetAllProjectsInput()
            {
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "Title asc"
            });
            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
            result.First().Title.ShouldBe("A");
        }

        [Fact]
        public async Task GetAllProjects_OrderByValidFieldDesc_ReturnsProjects()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = Project.Create("A", currentUser, currentTenant.Id);
            var project2 = Project.Create("Z", currentUser, currentTenant.Id);
            await _projectManager.Create(project);
            await _projectManager.Create(project2);

            // Act
            var result = await _projectManager.GetAll(new GetAllProjectsInput()
            {
                MaxResultCount = 10,
                SkipCount = 0,
                Sorting = "Title desc"
            });
            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
            result.First().Title.ShouldBe("Z");
        }

        [Fact]
        public async Task GetAllProjects_MaxCountLowerThanTotal_ReturnsProjects()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = Project.Create("A", currentUser, currentTenant.Id);
            var project2 = Project.Create("Z", currentUser, currentTenant.Id);
            await _projectManager.Create(project);
            await _projectManager.Create(project2);

            // Act
            var result = await _projectManager.GetAll(new GetAllProjectsInput()
            {
                MaxResultCount = 1,
                SkipCount = 0
            });
            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(1);
            result.First().Title.ShouldBe("Z");
        }

        [Fact]
        public async Task GetAllProjects_MaxCountLowerThanTotalSkip1_ReturnsProjects()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = Project.Create("A", currentUser, currentTenant.Id);
            var project2 = Project.Create("Z", currentUser, currentTenant.Id);
            await _projectManager.Create(project);
            await _projectManager.Create(project2);

            // Act
            var result = await _projectManager.GetAll(new GetAllProjectsInput()
            {
                MaxResultCount = 1,
                SkipCount = 1
            });
            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(1);
            result.First().Title.ShouldBe("A");
        }

        [Fact]
        public async Task GetAllProjects_BadKeyword_ReturnsEmptyProjects()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = Project.Create("A", currentUser, currentTenant.Id);
            var project2 = Project.Create("Z", currentUser, currentTenant.Id);
            await _projectManager.Create(project);
            await _projectManager.Create(project2);

            // Act
            var result = await _projectManager.GetAll(new GetAllProjectsInput()
            {
                keyword = "test"
            });
            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(0);
        }

        [Fact]
        public async Task GetAllProjects_Order_ReturnsProjects()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = Project.Create("A", currentUser, currentTenant.Id);
            var project2 = Project.Create("Z", currentUser, currentTenant.Id);
            await _projectManager.Create(project);
            await _projectManager.Create(project2);

            // Act
            var act = _projectManager.GetAll(new GetAllProjectsInput()
            {
                Sorting = "badfield desc"
            });
            // Assert
            ParseException ex = await Assert.ThrowsAsync<ParseException>(() => act);
            ex.Message.ShouldBe("No property or field 'badfield' exists in type 'Project'");
        }
    }
}
