$filePath = 'E:\repo\ai\SildesViewer\SlidesViewer.FilmAnalyzer\Analyzers\PhysicalDefectAnalyzer.cs'
$content = [System.IO.File]::ReadAllText($filePath)
$clean = $content.TrimEnd([char]0)
[System.IO.File]::WriteAllText($filePath, $clean, [System.Text.Encoding]::UTF8)
Write-Host "Done. Original length: $($content.Length), Clean length: $($clean.Length)"
