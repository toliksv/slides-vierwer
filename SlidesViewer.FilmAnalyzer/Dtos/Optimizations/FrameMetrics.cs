namespace SlidesViewer.FilmAnalyzer.Dtos.Optimizations;

public class FrameMetrics
{
    public double ClipHiR { get; set; }
    public double ClipHiG { get; set; }
    public double ClipHiB { get; set; }

    public double ClipLoR { get; set; }
    public double ClipLoG { get; set; }
    public double ClipLoB { get; set; }

    public double P50_R { get; set; }
    public double P50_G { get; set; }
    public double P50_B { get; set; }

    public double P95_R { get; set; }
    public double P95_G { get; set; }
    public double P95_B { get; set; }

    public double P02_R { get; set; }
    public double P02_G { get; set; }
    public double P02_B { get; set; }

    public double P05_R { get; set; }
    public double P05_G { get; set; }
    public double P05_B { get; set; }

    public double P98_R { get; set; }
    public double P98_G { get; set; }
    public double P98_B { get; set; }

    public bool IsNegative { get; set; }
    public double P01_R { get; set; }
    public double P01_G { get; set; }
    public double P01_B { get; set; }
    public double P99_R { get; set; }
    public double P99_G { get; set; }
    public double P99_B { get; set; }
}
