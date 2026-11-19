using System.Text;
using SlidesViewer.FilmAnalyzer.Dtos.Adjustments;
using SlidesViewer.FilmAnalyzer.Dtos.Analysis;
using SlidesViewer.FilmAnalyzer.Generators;

namespace SlidesViewer.FilmAnalyzer.Tests;

public class CaptureOneStyleGeneratorTests
{
    private readonly CaptureOneStyleGenerator _generator;

    public CaptureOneStyleGeneratorTests()
    {
        _generator = new CaptureOneStyleGenerator();
    }

    [Fact]
    public void GenerateStyle_ReturnsNonNullString()
    {
        // Arrange
        var analysis = CreateTestAnalysis();

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GenerateStyle_ContainsXmlDeclaration()
    {
        // Arrange
        var analysis = CreateTestAnalysis();

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.StartsWith("<?xml", result);
    }

    [Fact]
    public void GenerateStyle_ContainsRootElement()
    {
        // Arrange
        var analysis = CreateTestAnalysis();

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("<SL", result);
        Assert.Contains("Engine", result);
    }

    [Fact]
    public void GenerateStyle_ContainsNameElement()
    {
        // Arrange
        var analysis = CreateTestAnalysis();

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        var fileName = Path.GetFileNameWithoutExtension(analysis.SourceFilePath);
        Assert.Contains($"<E K=\"Name\" V=\"{fileName}\" ", result);
    }

    [Fact]
    public void GenerateStyle_ContainsUUIDElement()
    {
        // Arrange
        var analysis = CreateTestAnalysis();

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("<E K=\"UUID\" V=\"", result);
        // UUID format: 8-4-4-4-12 hex chars
        var uuidMatch = System.Text.RegularExpressions.Regex.Match(result, @"<E K=""UUID"" V=""([^""]+)""");
        Assert.True(uuidMatch.Success, "UUID element should be present");
        var uuid = uuidMatch.Groups[1].Value;
        Assert.Matches(@"^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$", uuid);
    }

    [Fact]
    public void GenerateStyle_ContainsStyleSourceElement()
    {
        // Arrange
        var analysis = CreateTestAnalysis();

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("<E K=\"StyleSource\" V=\"Styles\" ", result);
    }

    [Fact]
    public void GenerateStyle_ContainsExposureElement()
    {
        // Arrange
        var analysis = CreateTestAnalysis();
        analysis.Corrections.Exposure.Exposure = 0.5;

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("<E K=\"Exposure\" V=\"0.5\" ", result);
    }

    [Fact]
    public void GenerateStyle_ContainsWhiteBalanceTemperatureElement()
    {
        // Arrange
        var analysis = CreateTestAnalysis();
        analysis.Corrections.WhiteBalance.ColorTemperature = 5600;

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("<E K=\"WhiteBalanceTemperature\" V=\"5600\" ", result);
    }

    [Fact]
    public void GenerateStyle_ContainsWhiteBalanceTintElement()
    {
        // Arrange
        var analysis = CreateTestAnalysis();
        analysis.Corrections.WhiteBalance.Tint = 2.5;

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("<E K=\"WhiteBalanceTint\" V=\"2.5\" ", result);
    }

    [Fact]
    public void GenerateStyle_ContainsUsmAmountElement()
    {
        // Arrange
        var analysis = CreateTestAnalysis();
        analysis.Corrections.Detail.Sharpness = 75;

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("<E K=\"UsmAmount\" V=\"75\" ", result);
    }

    [Fact]
    public void GenerateStyle_ContainsUsmRadiusElement()
    {
        // Arrange
        var analysis = CreateTestAnalysis();
        analysis.Corrections.Detail.EdgeEnhancement = 1.5;

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("<E K=\"UsmRadius\" V=\"1.5\" ", result);
    }

    [Fact]
    public void GenerateStyle_ContainsUsmThresholdElement()
    {
        // Arrange
        var analysis = CreateTestAnalysis();
        analysis.Corrections.Detail.MicroContrast = 3.0;

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("<E K=\"UsmThreshold\" V=\"3\" ", result);
    }

    [Fact]
    public void GenerateStyle_ContainsToneCurveElements()
    {
        // Arrange
        var analysis = CreateTestAnalysis();
        analysis.Corrections.ToneCurve.LuminancePoints = new[] { 0, 0, 32768, 32768, 65535, 65535 };
        analysis.Corrections.ToneCurve.RedPoints = new[] { 0, 0, 65535, 65535 };
        analysis.Corrections.ToneCurve.GreenPoints = new[] { 0, 0, 65535, 65535 };
        analysis.Corrections.ToneCurve.BluePoints = new[] { 0, 0, 65535, 65535 };

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("<E K=\"GradationCurve\" V=\"", result);
        Assert.Contains("<E K=\"GradationCurveRed\" V=\"", result);
        Assert.Contains("<E K=\"GradationCurveGreen\" V=\"", result);
        Assert.Contains("<E K=\"GradationCurveBlue\" V=\"", result);
    }

    [Fact]
    public void GenerateStyle_ContainsNoiseReductionElements()
    {
        // Arrange
        var analysis = CreateTestAnalysis();
        analysis.Corrections.NoiseReduction.Luminance = 25;
        analysis.Corrections.NoiseReduction.Color = 15;
        analysis.Corrections.NoiseReduction.LuminanceDetail = 25;

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("<E K=\"NrAmount\" V=\"25\" ", result);
        Assert.Contains("<E K=\"CnrAmount\" V=\"15\" ", result);
        Assert.Contains("<E K=\"NoiseDetailsAmount\" V=\"25\" ", result);
    }

    [Fact]
    public void GenerateStyle_ContainsHighlightElement()
    {
        // Arrange
        var analysis = CreateTestAnalysis();
        analysis.Corrections.Exposure.HighlightR = 0.95;
        analysis.Corrections.Exposure.HighlightG = 0.90;
        analysis.Corrections.Exposure.HighlightB = 0.92;

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("<E K=\"Highlight\" V=\"", result);
    }

    [Fact]
    public void GenerateStyle_ContainsShadowElement()
    {
        // Arrange
        var analysis = CreateTestAnalysis();
        analysis.Corrections.Exposure.ShadowR = 0.05;
        analysis.Corrections.Exposure.ShadowG = 0.05;
        analysis.Corrections.Exposure.ShadowB = 0.05;

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("<E K=\"Shadow\" V=\"", result);
    }

    [Fact]
    public void GenerateStyle_ContainsHdrElements()
    {
        // Arrange
        var analysis = CreateTestAnalysis();
        analysis.Corrections.HdrToneMapping.Contrast = 1.2;
        analysis.Corrections.HdrToneMapping.Brightness = 0.1;
        analysis.Corrections.HdrToneMapping.Saturation = 1.1;

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("<E K=\"Contrast\" V=\"1.2\" ", result);
        Assert.Contains("<E K=\"Brightness\" V=\"0.1\" ", result);
        Assert.Contains("<E K=\"Saturation\" V=\"1.1\" ", result);
    }

    [Fact]
    public void GenerateStyle_GeneratesValidXml()
    {
        // Arrange
        var analysis = CreateTestAnalysis();

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Exception? ex = null;
        try { new System.Xml.XmlDocument().LoadXml(result); } catch (Exception e) { ex = e; }
        Assert.Null(ex);
    }

    [Fact]
    public void GenerateStyle_UsesFileNameWithoutExtension()
    {
        // Arrange
        var analysis = new FilmAnalysisResult
        {
            SourceFilePath = @"C:\test\my-slide.tif",
            Corrections = new CaptureOneAdjustments()
        };

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("my-slide", result);
    }

    [Fact]
    public void GenerateStyle_EmptyCorrections_GeneratesDefaultValues()
    {
        // Arrange
        var analysis = new FilmAnalysisResult
        {
            SourceFilePath = @"C:\test\test.png",
            Corrections = new CaptureOneAdjustments()
        };

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        var doc = new System.Xml.XmlDocument();
        doc.LoadXml(result);

        var exposureElem = doc.SelectSingleNode("//*[(@K='Exposure')]") as System.Xml.XmlElement;
        Assert.NotNull(exposureElem);
        Assert.Equal("0", exposureElem.GetAttribute("V"));

        var shadowElem = doc.SelectSingleNode("//*[(@K='Shadow')]") as System.Xml.XmlElement;
        Assert.NotNull(shadowElem);
        // Default shadow should be 0;0.05;0.05;0.05
        Assert.Contains("0.05", shadowElem.GetAttribute("V"));
    }

    [Fact]
    public void GenerateStyle_PerChannelShadow_UsesPerChannelValues()
    {
        // Arrange
        var analysis = new FilmAnalysisResult
        {
            SourceFilePath = @"C:\test\test.png",
            Corrections = new CaptureOneAdjustments
            {
                Exposure = new ExposureAdjustment
                {
                    ShadowR = 0.02,
                    ShadowG = 0.03,
                    ShadowB = 0.04
                }
            }
        };

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("0;0.02;0.03;0.04", result);
    }

    [Fact]
    public void GenerateStyle_PerChannelHighlight_UsesPerChannelValues()
    {
        // Arrange
        var analysis = new FilmAnalysisResult
        {
            SourceFilePath = @"C:\test\test.png",
            Corrections = new CaptureOneAdjustments
            {
                Exposure = new ExposureAdjustment
                {
                    HighlightR = 0.95,
                    HighlightG = 0.90,
                    HighlightB = 0.92
                }
            }
        };

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("1;0.95;0.9;0.92", result);
    }

    [Fact]
    public void GenerateStyle_UsesInvariantCultureForNumbers()
    {
        // Arrange - use values that would produce comma vs period differences
        var analysis = new FilmAnalysisResult
        {
            SourceFilePath = @"C:\test\test.png",
            Corrections = new CaptureOneAdjustments
            {
                Exposure = new ExposureAdjustment { Exposure = 0.5 },
                WhiteBalance = new WhiteBalanceAdjustment { ColorTemperature = 5500.5, Tint = -2.3 },
                HdrToneMapping = new HdrToneMappingAdjustment { Contrast = 1.25 }
            }
        };

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert - should use period as decimal separator, not comma
        Assert.DoesNotContain("0,5", result);
        Assert.Contains("0.5", result);
        Assert.DoesNotContain("-2,3", result);
        Assert.Contains("-2.3", result);
    }

    [Fact]
    public void GenerateStyle_EncodingIsUtf8WithoutBom()
    {
        // Arrange
        var analysis = CreateTestAnalysis();

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        var bytes = Encoding.UTF8.GetBytes(result);
        // UTF-8 BOM is EF BB BF
        Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
    }

    [Fact]
    public void GenerateStyle_IndentationIsTabs()
    {
        // Arrange
        var analysis = CreateTestAnalysis();

        // Act
        var result = _generator.GenerateStyle(analysis);

        // Assert
        Assert.Contains("\t", result);
    }

    private static FilmAnalysisResult CreateTestAnalysis()
    {
        return new FilmAnalysisResult
        {
            SourceFilePath = @"C:\test\test-image.png",
            Width = 3000,
            Height = 2000,
            Color = new Dtos.Analysis.ColorDegradationAnalysis
            {
                MediumType = Dtos.Common.FilmMediumType.Slide80s
            },
            Physical = new Dtos.Analysis.PhysicalDefectAnalysis(),
            Quality = new Dtos.Analysis.QualityAssessment(),
            Corrections = new CaptureOneAdjustments
            {
                Exposure = new ExposureAdjustment
                {
                    Exposure = 0.5,
                    Highlight = 0.95,
                    Shadow = 0.05,
                    HighlightR = 0.95,
                    HighlightG = 0.90,
                    HighlightB = 0.92,
                    ShadowR = 0.02,
                    ShadowG = 0.03,
                    ShadowB = 0.04
                },
                WhiteBalance = new WhiteBalanceAdjustment
                {
                    ColorTemperature = 5600,
                    Tint = 2.5
                },
                ToneCurve = new ToneCurveAdjustment
                {
                    LuminancePoints = new[] { 0, 0, 32768, 32768, 65535, 65535 },
                    RedPoints = new[] { 0, 0, 65535, 65535 },
                    GreenPoints = new[] { 0, 0, 65535, 65535 },
                    BluePoints = new[] { 0, 0, 65535, 65535 }
                },
                Detail = new DetailAdjustment
                {
                    Sharpness = 75,
                    EdgeEnhancement = 1.5,
                    MicroContrast = 3.0
                },
                NoiseReduction = new NoiseReductionAdjustment
                {
                    Luminance = 25,
                    Color = 15,
                    LuminanceDetail = 25,
                    ColorDetail = 35
                },
                HdrToneMapping = new HdrToneMappingAdjustment
                {
                    Contrast = 1.2,
                    Brightness = 0.1,
                    Saturation = 1.1
                }
            }
        };
    }
}
