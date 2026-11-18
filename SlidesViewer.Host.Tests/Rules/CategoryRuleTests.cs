using SlidesViewer.Host.Rules.Categories;
using SlidesViewer.Host.Rules;
using NSubstitute;

namespace SlidesViewer.Host.Tests.Rules;

public class PCategoryRuleTests
{
    private readonly PCategoryRule _rule;

    public PCategoryRuleTests()
    {
        _rule = new PCategoryRule();
    }

    [Fact]
    public void Apply_WithNullInput_ReturnsNotApplied()
    {
        var result = _rule.Apply(null!);

        Assert.Null(result.Value);
        Assert.False(result.IsApplied);
    }

    [Fact]
    public void Apply_WithEmptyInput_ReturnsNotApplied()
    {
        var result = _rule.Apply(string.Empty);

        Assert.Null(result.Value);
        Assert.False(result.IsApplied);
    }

    [Fact]
    public void Apply_WithUnderscoreP_ReturnsCategoryBeforeP()
    {
        var result = _rule.Apply("ProjectName_п1.tiff");

        Assert.Equal("ProjectName", result.Value);
        Assert.True(result.IsApplied);
    }

    [Fact]
    public void Apply_WithMultipleUnderscoreP_ReturnsCategoryBeforeFirstP()
    {
        // IndexOf ищет подстроку "_п" и возвращает всё до неё (input[..firstUnderscoreIndex])
        // "Project_Name_п1.tiff" → firstUnderscoreIndex = позиция '_' перед "п" = 13
        // Result: "Project_Name" (включая внутренний _)
        var result = _rule.Apply("Project_Name_п1.tiff");

        Assert.Equal("Project_Name", result.Value);
        Assert.True(result.IsApplied);
    }

    [Fact]
    public void Apply_WithoutUnderscoreP_ReturnsNotApplied()
    {
        var result = _rule.Apply("ProjectName.tiff");

        Assert.Null(result.Value);
        Assert.False(result.IsApplied);
    }

    [Fact]
    public void Apply_CaseInsensitive_FindsUnderscoreP()
    {
        var resultUpper = _rule.Apply("Project_П1.tiff");
        var resultLower = _rule.Apply("Project_п1.tiff");

        Assert.Equal("Project", resultUpper.Value);
        Assert.True(resultUpper.IsApplied);
        Assert.Equal("Project", resultLower.Value);
        Assert.True(resultLower.IsApplied);
    }

    [Fact]
    public void Apply_WithUnderscorePAtStart_ReturnsEmptyString()
    {
        var result = _rule.Apply("_п1.tiff");

        Assert.Equal(string.Empty, result.Value);
        Assert.True(result.IsApplied);
    }
}

public class KCategoryRuleTests
{
    [Fact]
    public void Apply_WithUnderscoreK_ReturnsCategoryBeforeK()
    {
        var mockDefault = Substitute.For<IRule>();
        var rule = new KCategoryRule(mockDefault);

        var result = rule.Apply("ProjectName_к1.tiff");

        Assert.Equal("ProjectName", result.Value);
        Assert.True(result.IsApplied);
    }

    [Fact]
    public void Apply_WithoutUnderscoreK_ReturnsNotApplied()
    {
        var mockDefault = Substitute.For<IRule>();
        var rule = new KCategoryRule(mockDefault);

        var result = rule.Apply("ProjectName.tiff");

        Assert.Null(result.Value);
        Assert.False(result.IsApplied);
    }

    [Fact]
    public void Apply_WithEmptyInput_ReturnsNotApplied()
    {
        var mockDefault = Substitute.For<IRule>();
        var rule = new KCategoryRule(mockDefault);

        var result = rule.Apply(string.Empty);

        Assert.Null(result.Value);
        Assert.False(result.IsApplied);
    }

    [Fact]
    public void Apply_CaseInsensitive_FindsUnderscoreK()
    {
        var mockDefault = Substitute.For<IRule>();
        var rule = new KCategoryRule(mockDefault);

        var result = rule.Apply("Project_К1.tiff");

        Assert.Equal("Project", result.Value);
        Assert.True(result.IsApplied);
    }
}

public class ObereskiCategoryRuleTests
{
    private readonly ObereskiCategoryRule _rule;

    public ObereskiCategoryRuleTests()
    {
        _rule = new ObereskiCategoryRule();
    }

    [Fact]
    public void Apply_WithUnderscoreObrezki_ReturnsCategoryBeforeObrezki()
    {
        var result = _rule.Apply("Project_обрезки.tiff");

        Assert.Equal("Project", result.Value);
        Assert.True(result.IsApplied);
    }

    [Fact]
    public void Apply_WithoutUnderscoreObrezki_ReturnsNotApplied()
    {
        var result = _rule.Apply("Project.tiff");

        Assert.Null(result.Value);
        Assert.False(result.IsApplied);
    }

    [Fact]
    public void Apply_WithEmptyInput_ReturnsNotApplied()
    {
        var result = _rule.Apply(string.Empty);

        Assert.Null(result.Value);
        Assert.False(result.IsApplied);
    }

    [Fact]
    public void Apply_WithNullInput_ReturnsNotApplied()
    {
        var result = _rule.Apply(null!);

        Assert.Null(result.Value);
        Assert.False(result.IsApplied);
    }

    [Fact]
    public void Apply_CaseInsensitive_FindsUnderscoreObrezki()
    {
        var result = _rule.Apply("Project_ОБРЕЗКИ.tiff");

        Assert.Equal("Project", result.Value);
        Assert.True(result.IsApplied);
    }

    [Fact]
    public void Apply_WithMultipleUnderscoreObrezki_ReturnsCategoryBeforeFirst()
    {
        // IndexOf ищет подстроку "_обрезки" и возвращает всё до неё
        // "Project_Name_обрезки.tiff" → позиция '_' перед "обрезки" = 13
        // Result: "Project_Name"
        var result = _rule.Apply("Project_Name_обрезки.tiff");

        Assert.Equal("Project_Name", result.Value);
        Assert.True(result.IsApplied);
    }
}

public class KSubstringCategoryRuleTests
{
    private readonly KSubstringCategoryRule _rule;

    public KSubstringCategoryRuleTests()
    {
        _rule = new KSubstringCategoryRule();
    }

    [Fact]
    public void Apply_WithUnderscoreK_ReturnsCategoryBeforeK()
    {
        var result = _rule.Apply("Project_k.tiff");

        Assert.Equal("Project", result.Value);
        Assert.True(result.IsApplied);
    }

    [Fact]
    public void Apply_WithoutUnderscoreK_ReturnsNotApplied()
    {
        var result = _rule.Apply("Project.tiff");

        Assert.Null(result.Value);
        Assert.False(result.IsApplied);
    }

    [Fact]
    public void Apply_WithEmptyInput_ReturnsNotApplied()
    {
        var result = _rule.Apply(string.Empty);

        Assert.Null(result.Value);
        Assert.False(result.IsApplied);
    }

    [Fact]
    public void Apply_WithNullInput_ReturnsNotApplied()
    {
        var result = _rule.Apply(null!);

        Assert.Null(result.Value);
        Assert.False(result.IsApplied);
    }

    [Fact]
    public void Apply_CaseInsensitive_FindsUnderscoreK()
    {
        var result = _rule.Apply("Project_K.tiff");

        Assert.Equal("Project", result.Value);
        Assert.True(result.IsApplied);
    }
}

public class DefaultCategoryRuleTests
{
    private readonly DefaultCategoryRule _rule;

    public DefaultCategoryRuleTests()
    {
        _rule = new DefaultCategoryRule();
    }

    [Fact]
    public void Apply_WithUnderscore_ReturnsCategoryBeforeLastUnderscore()
    {
        // LastIndexOf('_') ищет ПОСЛЕДНИЙ '_'
        // "Project_Name_1.tiff" → lastUnderscoreIndex = 13 (перед "1.tiff")
        // input[..13] = "Project_Name"
        var result = _rule.Apply("Project_Name_1.tiff");

        Assert.Equal("Project_Name", result.Value);
        Assert.True(result.IsApplied);
    }

    [Fact]
    public void Apply_WithoutUnderscore_ReturnsNotApplied()
    {
        var result = _rule.Apply("Project.tiff");

        Assert.Null(result.Value);
        Assert.False(result.IsApplied);
    }

    [Fact]
    public void Apply_WithEmptyInput_ReturnsNotApplied()
    {
        var result = _rule.Apply(string.Empty);

        Assert.Null(result.Value);
        Assert.False(result.IsApplied);
    }

    [Fact]
    public void Apply_WithNullInput_ReturnsNotApplied()
    {
        var result = _rule.Apply(null!);

        Assert.Null(result.Value);
        Assert.False(result.IsApplied);
    }

    [Fact]
    public void Apply_WithSingleUnderscore_ReturnsEmptyString()
    {
        var result = _rule.Apply("_1.tiff");

        Assert.Equal(string.Empty, result.Value);
        Assert.True(result.IsApplied);
    }

    [Fact]
    public void Apply_ImplementsIMustApplyRule()
    {
        Assert.IsAssignableFrom<IMustApplyRule>(_rule);
    }

    [Fact]
    public void Apply_ImplementsIRule()
    {
        Assert.IsAssignableFrom<IRule>(_rule);
    }
}

public class NoCategoryRuleTests
{
    private readonly NoCategoryRule _rule;

    public NoCategoryRuleTests()
    {
        _rule = new NoCategoryRule();
    }

    [Fact]
    public void Apply_AlwaysReturnsNoCategory()
    {
        var result = _rule.Apply("any_file.tiff");

        Assert.Equal("Нет категории", result.Value);
        Assert.True(result.IsApplied);
    }

    [Fact]
    public void Apply_WithEmptyInput_ReturnsNoCategory()
    {
        var result = _rule.Apply(string.Empty);

        Assert.Equal("Нет категории", result.Value);
        Assert.True(result.IsApplied);
    }

    [Fact]
    public void Apply_WithNullInput_ReturnsNoCategory()
    {
        var result = _rule.Apply(null!);

        Assert.Equal("Нет категории", result.Value);
        Assert.True(result.IsApplied);
    }

    [Fact]
    public void Apply_ImplementsIFallbackRule()
    {
        Assert.IsAssignableFrom<IFallbackRule>(_rule);
    }

    [Fact]
    public void Apply_ImplementsIRule()
    {
        Assert.IsAssignableFrom<IRule>(_rule);
    }
}
