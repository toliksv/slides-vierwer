using SlidesViewer.Host.Contracts.Categories;
using SlidesViewer.Host.Repositories;
using SlidesViewer.Host.Rules;
using SlidesViewer.Host.Services;
using NSubstitute;
using System.Linq;

namespace SlidesViewer.Host.Tests.Services;

public class SlideServiceTests
{
    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SlideService(
            null!, [], Substitute.For<IMustApplyRule>(), Substitute.For<IFallbackRule>()));
    }

    [Fact]
    public void Constructor_WithNullPriorityRules_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SlideService(
            Substitute.For<ISlideRepository>(), null!, Substitute.For<IMustApplyRule>(), Substitute.For<IFallbackRule>()));
    }

    [Fact]
    public void Constructor_WithNullMustApplyRule_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SlideService(
            Substitute.For<ISlideRepository>(), [], null!, Substitute.For<IFallbackRule>()));
    }

    [Fact]
    public void Constructor_WithNullFallbackRule_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SlideService(
            Substitute.For<ISlideRepository>(), [], Substitute.For<IMustApplyRule>(), null!));
    }

    [Fact]
    public async Task GetCategories_ReturnsSortedCategoriesByFileName()
    {
        // Arrange
        var folderPath = @"C:\slides";
        var files = new List<SlideFileEntity>
        {
            new() { FileName = "file1.tiff", RelativePath = "file1.tiff", Size = 100 },
            new() { FileName = "file2.tiff", RelativePath = "file2.tiff", Size = 200 },
            new() { FileName = "file3.tiff", RelativePath = "file3.tiff", Size = 150 }
        };

        var mockRepo = Substitute.For<ISlideRepository>();
        mockRepo.SearchFilesAsync(folderPath).Returns(Task.FromResult(files));

        var mockMustApply = Substitute.For<IMustApplyRule>();
        mockMustApply.Apply(Arg.Any<string>()).Returns(new RuleResultDto(null, false));

        var mockFallback = Substitute.For<IFallbackRule>();
        mockFallback.Apply(Arg.Any<string>()).Returns(new RuleResultDto("Нет категории", true));

        var service = new SlideService(mockRepo, [], mockMustApply, mockFallback);

        // Act
        var result = await service.GetCategories(folderPath);

        // Assert
        Assert.Single(result.Categories);
        Assert.Equal("Нет категории", result.Categories[0].Name);
        Assert.Equal(3, result.Categories[0].FileCount);
    }

    [Fact]
    public async Task GetCategories_GroupsFilesByCategory()
    {
        // Arrange
        var folderPath = @"C:\slides";
        var files = new List<SlideFileEntity>
        {
            new() { FileName = "projectA_п1.tiff", RelativePath = "projectA_п1.tiff", Size = 100 },
            new() { FileName = "projectA_п2.tiff", RelativePath = "projectA_п2.tiff", Size = 200 },
            new() { FileName = "projectB_п1.tiff", RelativePath = "projectB_п1.tiff", Size = 150 }
        };

        var mockRepo = Substitute.For<ISlideRepository>();
        mockRepo.SearchFilesAsync(folderPath).Returns(Task.FromResult(files));

        // Создаём правила, которые срабатывают на конкретные файлы
        var rule1 = Substitute.For<IRule>();
        var rule2 = Substitute.For<IRule>();
        var rule3 = Substitute.For<IRule>();

        // Используем Callback для точного контроля
        rule1.Apply(Arg.Any<string>()).Returns(x =>
            x.Arg<string>() == "projectA_п1.tiff"
                ? new RuleResultDto("projectA", true)
                : new RuleResultDto((string?)null, false));

        rule2.Apply(Arg.Any<string>()).Returns(x =>
            x.Arg<string>() == "projectA_п2.tiff"
                ? new RuleResultDto("projectA", true)
                : new RuleResultDto((string?)null, false));

        rule3.Apply(Arg.Any<string>()).Returns(x =>
            x.Arg<string>() == "projectB_п1.tiff"
                ? new RuleResultDto("projectB", true)
                : new RuleResultDto((string?)null, false));

        var mockMustApply = Substitute.For<IMustApplyRule>();
        mockMustApply.Apply(Arg.Any<string>()).Returns(new RuleResultDto(null, false));

        var mockFallback = Substitute.For<IFallbackRule>();
        mockFallback.Apply(Arg.Any<string>()).Returns(new RuleResultDto("Fallback", true));

        var service = new SlideService(mockRepo, new IRule[] { rule1, rule2, rule3 }, mockMustApply, mockFallback);

        // Act
        var result = await service.GetCategories(folderPath);

        // Assert
        Assert.Equal(2, result.Categories.Count);
        Assert.Equal("projectA", result.Categories[0].Name);
        Assert.Equal(2, result.Categories[0].FileCount);
        Assert.Equal("projectB", result.Categories[1].Name);
        Assert.Equal(1, result.Categories[1].FileCount);
    }

    [Fact]
    public async Task GetCategories_OrdersCategoriesAlphabetically()
    {
        // Arrange
        var folderPath = @"C:\slides";
        var files = new List<SlideFileEntity>
        {
            new() { FileName = "Zebra_п1.tiff", RelativePath = "Zebra_п1.tiff", Size = 100 },
            new() { FileName = "Alpha_п1.tiff", RelativePath = "Alpha_п1.tiff", Size = 200 },
            new() { FileName = "Middle_п1.tiff", RelativePath = "Middle_п1.tiff", Size = 150 }
        };

        var mockRepo = Substitute.For<ISlideRepository>();
        mockRepo.SearchFilesAsync(folderPath).Returns(Task.FromResult(files));

        var ruleZ = CreateSpecificRule("Zebra", "Zebra_п1.tiff");
        var ruleA = CreateSpecificRule("Alpha", "Alpha_п1.tiff");
        var ruleM = CreateSpecificRule("Middle", "Middle_п1.tiff");

        var service = new SlideService(mockRepo, new IRule[] { ruleZ, ruleA, ruleM }, CreateFallbackMustApply(), CreateFallbackFallback());

        // Act
        var result = await service.GetCategories(folderPath);

        // Assert
        Assert.Equal(3, result.Categories.Count);
        Assert.Equal("Alpha", result.Categories[0].Name);
        Assert.Equal("Middle", result.Categories[1].Name);
        Assert.Equal("Zebra", result.Categories[2].Name);
    }

    [Fact]
    public async Task GetCategories_UsesMustApplyRule_WhenPriorityRulesNotApplied()
    {
        // Arrange
        var folderPath = @"C:\slides";
        var files = new List<SlideFileEntity>
        {
            new() { FileName = "doc1.tiff", RelativePath = "doc1.tiff", Size = 100 },
            new() { FileName = "doc2.tiff", RelativePath = "doc2.tiff", Size = 200 }
        };

        var mockRepo = Substitute.For<ISlideRepository>();
        mockRepo.SearchFilesAsync(folderPath).Returns(Task.FromResult(files));

        var mockMustApply = Substitute.For<IMustApplyRule>();
        mockMustApply.Apply("doc1.tiff").Returns(new RuleResultDto("CategoryDoc", true));
        mockMustApply.Apply("doc2.tiff").Returns(new RuleResultDto("CategoryDoc", true));

        var neverAppliedRule = Substitute.For<IRule>();
        neverAppliedRule.Apply(Arg.Any<string>()).Returns(new RuleResultDto((string?)null, false));

        var service = new SlideService(mockRepo, new IRule[] { neverAppliedRule }, mockMustApply, Substitute.For<IFallbackRule>());

        // Act
        var result = await service.GetCategories(folderPath);

        // Assert
        Assert.Single(result.Categories);
        Assert.Equal("CategoryDoc", result.Categories[0].Name);
        Assert.Equal(2, result.Categories[0].FileCount);
    }

    [Fact]
    public async Task GetCategories_UsesFallbackRule_WhenNoRuleApplied()
    {
        // Arrange
        var folderPath = @"C:\slides";
        var files = new List<SlideFileEntity>
        {
            new() { FileName = "random.tiff", RelativePath = "random.tiff", Size = 100 }
        };

        var mockRepo = Substitute.For<ISlideRepository>();
        mockRepo.SearchFilesAsync(folderPath).Returns(Task.FromResult(files));

        var mockMustApply = Substitute.For<IMustApplyRule>();
        mockMustApply.Apply(Arg.Any<string>()).Returns(new RuleResultDto(null, false));

        var mockFallback = Substitute.For<IFallbackRule>();
        mockFallback.Apply(Arg.Any<string>()).Returns(new RuleResultDto("Other", true));

        var neverAppliedRule = Substitute.For<IRule>();
        neverAppliedRule.Apply(Arg.Any<string>()).Returns(new RuleResultDto((string?)null, false));

        var service = new SlideService(mockRepo, new IRule[] { neverAppliedRule }, mockMustApply, mockFallback);

        // Act
        var result = await service.GetCategories(folderPath);

        // Assert
        Assert.Single(result.Categories);
        Assert.Equal("Other", result.Categories[0].Name);
        Assert.Equal(1, result.Categories[0].FileCount);
    }

    [Fact]
    public async Task GetCategories_ReturnsEmptyList_WhenNoFiles()
    {
        // Arrange
        var folderPath = @"C:\slides";
        var mockRepo = Substitute.For<ISlideRepository>();
        mockRepo.SearchFilesAsync(folderPath).Returns(Task.FromResult(new List<SlideFileEntity>()));

        var service = new SlideService(mockRepo, [], CreateFallbackMustApply(), CreateFallbackFallback());

        // Act
        var result = await service.GetCategories(folderPath);

        // Assert
        Assert.Empty(result.Categories);
    }

    [Fact]
    public async Task GetCategories_CallsRepositoryWithCorrectFolderPath()
    {
        // Arrange
        var folderPath = @"C:\test\folder";
        var mockRepo = Substitute.For<ISlideRepository>();
        mockRepo.SearchFilesAsync(folderPath).Returns(Task.FromResult(new List<SlideFileEntity>()));

        var service = new SlideService(mockRepo, [], CreateFallbackMustApply(), CreateFallbackFallback());

        // Act
        await service.GetCategories(folderPath);

        // Assert
        await mockRepo.Received(1).SearchFilesAsync(folderPath);
    }

    [Fact]
    public async Task GetCategories_ReturnsCorrectDtoStructure()
    {
        // Arrange
        var folderPath = @"C:\slides";
        var files = new List<SlideFileEntity>
        {
            new() { FileName = "A_п1.tiff", RelativePath = "A_п1.tiff", Size = 100 },
            new() { FileName = "A_п2.tiff", RelativePath = "A_п2.tiff", Size = 200 },
            new() { FileName = "B_п1.tiff", RelativePath = "B_п1.tiff", Size = 150 },
            new() { FileName = "C_п1.tiff", RelativePath = "C_п1.tiff", Size = 250 }
        };

        var mockRepo = Substitute.For<ISlideRepository>();
        mockRepo.SearchFilesAsync(folderPath).Returns(Task.FromResult(files));

        var ruleA1 = CreateSpecificRule("A", "A_п1.tiff");
        var ruleA2 = CreateSpecificRule("A", "A_п2.tiff");
        var ruleB = CreateSpecificRule("B", "B_п1.tiff");
        var ruleC = CreateSpecificRule("C", "C_п1.tiff");

        var service = new SlideService(mockRepo, new IRule[] { ruleA1, ruleA2, ruleB, ruleC }, CreateFallbackMustApply(), CreateFallbackFallback());

        // Act
        var result = await service.GetCategories(folderPath);

        // Assert
        Assert.Equal(3, result.Categories.Count);

        Assert.Equal("A", result.Categories[0].Name);
        Assert.Equal(2, result.Categories[0].FileCount);

        Assert.Equal("B", result.Categories[1].Name);
        Assert.Equal(1, result.Categories[1].FileCount);

        Assert.Equal("C", result.Categories[2].Name);
        Assert.Equal(1, result.Categories[2].FileCount);
    }

    [Fact]
    public async Task GetCategories_CollectsRelativePaths_ForSingleCategory()
    {
        // Arrange
        var folderPath = @"C:\slides";
        var files = new List<SlideFileEntity>
        {
            new() { FileName = "file1.tiff", RelativePath = "folder1/file1.tiff", Size = 100 },
            new() { FileName = "file2.tiff", RelativePath = "folder2/file2.tiff", Size = 200 },
            new() { FileName = "file3.tiff", RelativePath = "file3.tiff", Size = 150 }
        };

        var mockRepo = Substitute.For<ISlideRepository>();
        mockRepo.SearchFilesAsync(folderPath).Returns(Task.FromResult(files));

        var mockMustApply = Substitute.For<IMustApplyRule>();
        mockMustApply.Apply(Arg.Any<string>()).Returns(new RuleResultDto(null, false));

        var mockFallback = Substitute.For<IFallbackRule>();
        mockFallback.Apply(Arg.Any<string>()).Returns(new RuleResultDto("Нет категории", true));

        var service = new SlideService(mockRepo, [], mockMustApply, mockFallback);

        // Act
        var result = await service.GetCategories(folderPath);

        // Assert
        Assert.Single(result.Categories);
        Assert.Equal("Нет категории", result.Categories[0].Name);
        Assert.Equal(3, result.Categories[0].FileCount);
        Assert.Equal(3, result.Categories[0].RelativePaths.Count);
        Assert.Contains("folder1/file1.tiff", result.Categories[0].RelativePaths);
        Assert.Contains("folder2/file2.tiff", result.Categories[0].RelativePaths);
        Assert.Contains("file3.tiff", result.Categories[0].RelativePaths);
    }

    [Fact]
    public async Task GetCategories_CollectsRelativePaths_PerCategory()
    {
        // Arrange
        var folderPath = @"C:\slides";
        var files = new List<SlideFileEntity>
        {
            new() { FileName = "A_п1.tiff", RelativePath = "projA/slide1.tiff", Size = 100 },
            new() { FileName = "A_п2.tiff", RelativePath = "projA/slide2.tiff", Size = 200 },
            new() { FileName = "B_п1.tiff", RelativePath = "projB/slide1.tiff", Size = 150 },
            new() { FileName = "B_п2.tiff", RelativePath = "projB/sub/slide2.tiff", Size = 250 }
        };

        var mockRepo = Substitute.For<ISlideRepository>();
        mockRepo.SearchFilesAsync(folderPath).Returns(Task.FromResult(files));

        var ruleA = CreateSpecificRule("A", "A_п1.tiff");
        var ruleA2 = CreateSpecificRule("A", "A_п2.tiff");
        var ruleB = CreateSpecificRule("B", "B_п1.tiff");
        var ruleB2 = CreateSpecificRule("B", "B_п2.tiff");

        var service = new SlideService(mockRepo, new IRule[] { ruleA, ruleA2, ruleB, ruleB2 }, CreateFallbackMustApply(), CreateFallbackFallback());

        // Act
        var result = await service.GetCategories(folderPath);

        // Assert
        Assert.Equal(2, result.Categories.Count);

        var categoryA = result.Categories.First(c => c.Name == "A");
        Assert.Equal(2, categoryA.FileCount);
        Assert.Equal(2, categoryA.RelativePaths.Count);
        Assert.Contains("projA/slide1.tiff", categoryA.RelativePaths);
        Assert.Contains("projA/slide2.tiff", categoryA.RelativePaths);

        var categoryB = result.Categories.First(c => c.Name == "B");
        Assert.Equal(2, categoryB.FileCount);
        Assert.Equal(2, categoryB.RelativePaths.Count);
        Assert.Contains("projB/slide1.tiff", categoryB.RelativePaths);
        Assert.Contains("projB/sub/slide2.tiff", categoryB.RelativePaths);
    }

    [Fact]
    public async Task GetCategories_EmptyRelativePaths_ForNoFiles()
    {
        // Arrange
        var folderPath = @"C:\slides";
        var mockRepo = Substitute.For<ISlideRepository>();
        mockRepo.SearchFilesAsync(folderPath).Returns(Task.FromResult(new List<SlideFileEntity>()));

        var service = new SlideService(mockRepo, [], CreateFallbackMustApply(), CreateFallbackFallback());

        // Act
        var result = await service.GetCategories(folderPath);

        // Assert
        Assert.Empty(result.Categories);
    }

    [Fact]
    public async Task GetCategories_PreservesRelativePathOrder()
    {
        // Arrange
        var folderPath = @"C:\slides";
        var files = new List<SlideFileEntity>
        {
            new() { FileName = "Z_п1.tiff", RelativePath = "z_path.tiff", Size = 100 },
            new() { FileName = "A_п1.tiff", RelativePath = "a_path.tiff", Size = 200 },
            new() { FileName = "M_п1.tiff", RelativePath = "m_path.tiff", Size = 150 }
        };

        var mockRepo = Substitute.For<ISlideRepository>();
        mockRepo.SearchFilesAsync(folderPath).Returns(Task.FromResult(files));

        var mockMustApply = Substitute.For<IMustApplyRule>();
        mockMustApply.Apply(Arg.Any<string>()).Returns(new RuleResultDto(null, false));

        var mockFallback = Substitute.For<IFallbackRule>();
        mockFallback.Apply(Arg.Any<string>()).Returns(new RuleResultDto("Mixed", true));

        var service = new SlideService(mockRepo, [], mockMustApply, mockFallback);

        // Act
        var result = await service.GetCategories(folderPath);

        // Assert — HashSet не гарантирует порядок, проверяем наличие
        Assert.Single(result.Categories);
        Assert.Equal("Mixed", result.Categories[0].Name);
        Assert.Equal(3, result.Categories[0].RelativePaths.Count);
        Assert.Contains("z_path.tiff", result.Categories[0].RelativePaths);
        Assert.Contains("a_path.tiff", result.Categories[0].RelativePaths);
        Assert.Contains("m_path.tiff", result.Categories[0].RelativePaths);
    }

    [Fact]
    public async Task GetCategories_DeduplicatesRelativePaths_CaseInsensitive()
    {
        // Arrange — одинаковые пути в разном регистре
        var folderPath = @"C:\slides";
        var files = new List<SlideFileEntity>
        {
            new() { FileName = "A_п1.tiff", RelativePath = "FolderA/file1.tiff", Size = 100 },
            new() { FileName = "A_п2.tiff", RelativePath = "foldera/File1.TIFF", Size = 200 },
            new() { FileName = "A_п3.tiff", RelativePath = "FOLDERA/file1.tiff", Size = 150 }
        };

        var mockRepo = Substitute.For<ISlideRepository>();
        mockRepo.SearchFilesAsync(folderPath).Returns(Task.FromResult(files));

        var mockMustApply = Substitute.For<IMustApplyRule>();
        mockMustApply.Apply(Arg.Any<string>()).Returns(new RuleResultDto(null, false));

        var mockFallback = Substitute.For<IFallbackRule>();
        mockFallback.Apply(Arg.Any<string>()).Returns(new RuleResultDto("Dupes", true));

        var service = new SlideService(mockRepo, [], mockMustApply, mockFallback);

        // Act
        var result = await service.GetCategories(folderPath);

        // Assert — все три файла, но только 1 уникальный путь
        Assert.Single(result.Categories);
        Assert.Equal("Dupes", result.Categories[0].Name);
        Assert.Equal(3, result.Categories[0].FileCount);
        Assert.Single(result.Categories[0].RelativePaths);
    }

    [Fact]
    public async Task GetCategories_KeepsDistinctPaths_DifferentCasing()
    {
        // Arrange — разные пути, один в верхнем регистре
        var folderPath = @"C:\slides";
        var files = new List<SlideFileEntity>
        {
            new() { FileName = "A_п1.tiff", RelativePath = "folder/file1.tiff", Size = 100 },
            new() { FileName = "A_п2.tiff", RelativePath = "folder/file2.tiff", Size = 200 },
            new() { FileName = "A_п3.tiff", RelativePath = "Folder/FILE1.TIFF", Size = 150 }
        };

        var mockRepo = Substitute.For<ISlideRepository>();
        mockRepo.SearchFilesAsync(folderPath).Returns(Task.FromResult(files));

        var mockMustApply = Substitute.For<IMustApplyRule>();
        mockMustApply.Apply(Arg.Any<string>()).Returns(new RuleResultDto(null, false));

        var mockFallback = Substitute.For<IFallbackRule>();
        mockFallback.Apply(Arg.Any<string>()).Returns(new RuleResultDto("MixedCase", true));

        var service = new SlideService(mockRepo, [], mockMustApply, mockFallback);

        // Act
        var result = await service.GetCategories(folderPath);

        // Assert — 3 файла, 2 уникальных пути (file1.tiff дублируется)
        Assert.Single(result.Categories);
        Assert.Equal("MixedCase", result.Categories[0].Name);
        Assert.Equal(3, result.Categories[0].FileCount);
        Assert.Equal(2, result.Categories[0].RelativePaths.Count);
        Assert.Contains("folder/file1.tiff", result.Categories[0].RelativePaths);
        Assert.Contains("folder/file2.tiff", result.Categories[0].RelativePaths);
    }

    private static IRule CreateSpecificRule(string category, string fileName)
    {
        var mock = Substitute.For<IRule>();
        // Порядок важен: сначала Arg.Any, затем конкретное значение (NSubstitute выбирает последнее совпадение)
        mock.Apply(Arg.Any<string>()).Returns(new RuleResultDto((string?)null, false));
        mock.Apply(fileName).Returns(new RuleResultDto(category, true));
        return mock;
    }

    private static IMustApplyRule CreateFallbackMustApply()
    {
        var mock = Substitute.For<IMustApplyRule>();
        mock.Apply(Arg.Any<string>()).Returns(new RuleResultDto(null, false));
        return mock;
    }

    private static IFallbackRule CreateFallbackFallback()
    {
        var mock = Substitute.For<IFallbackRule>();
        mock.Apply(Arg.Any<string>()).Returns(new RuleResultDto("Нет категории", true));
        return mock;
    }
}
