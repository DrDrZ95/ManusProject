$files = Get-ChildItem -Path . -Filter "*.cs" -Recurse | Where-Object { $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "\\bin\\" }

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    
    # 1. 移除末尾空行
    $content = $content.TrimEnd() + "`r`n"
    
    # 2. 将 namespace 移到第一行 (如果是 file-scoped namespace)
    if ($content -match "(?m)^namespace\s+[\w\.]+;") {
        $ns = $matches[0]
        # 移除原有的 namespace 行及其前后的空白
        $content = $content -replace "(?m)^\s*namespace\s+[\w\.]+;\s*", ""
        # 在最前面插入 namespace
        $content = $ns + "`r`n`r`n" + $content.TrimStart()
    }

    $content | Set-Content $file.FullName
}

# 3. 运行 dotnet format 来处理缩进
dotnet format
