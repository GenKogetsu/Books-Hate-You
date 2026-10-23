$root = Join-Path (Get-Location) 'Assets\Object'
$assetExt = @('.fbx','.obj','.blend','.dae','.3ds','.max','.mat','.png','.jpg','.jpeg','.tga','.tif','.tiff','.psd','.exr','.hdr','.bmp','.gif','.webp')
$animationExt = @('.anim','.controller')
$scriptExt = @('.cs','.js','.boo')
$specialAssetDirs = @('Model','Material','Material & Texture','textures','Texture','Model & Texture & Metrial','Model & Texture','Materials','Textures')

function Move-WithMeta($source, $destination) {
    if (-not (Test-Path -LiteralPath $source)) { return }
    $name = Split-Path -Leaf $source
    $target = Join-Path $destination $name
    if (Test-Path -LiteralPath $target) {
        throw "Target already exists: $target"
    }
    Move-Item -LiteralPath $source -Destination $destination
    $meta = "$source.meta"
    if (Test-Path -LiteralPath $meta) {
        Move-Item -LiteralPath $meta -Destination $destination
    }
}

Get-ChildItem -LiteralPath $root -Directory -Recurse | Sort-Object FullName -Descending | ForEach-Object {
    $objectDir = $_.FullName
    $children = @(Get-ChildItem -LiteralPath $objectDir -Force)
    if ($children.Count -eq 0) { return }

    $hasRecognizedContent = $false
    foreach ($child in $children) {
        if ($child.PSIsContainer -and ($specialAssetDirs -contains $child.Name)) { $hasRecognizedContent = $true; break }
        if (-not $child.PSIsContainer -and (($assetExt -contains $child.Extension.ToLower()) -or ($animationExt -contains $child.Extension.ToLower()) -or ($scriptExt -contains $child.Extension.ToLower()))) { $hasRecognizedContent = $true; break }
    }
    if (-not $hasRecognizedContent) { return }

    $assetDir = Join-Path $objectDir 'Model Texture & Material'
    $animationDir = Join-Path $objectDir 'Animation'
    $scriptsDir = Join-Path $objectDir 'Scripts'
    New-Item -ItemType Directory -Path $assetDir -Force | Out-Null

    foreach ($child in @($children)) {
        if ($child.Name -like '*.meta') { continue }
        if ($child.PSIsContainer -and ($specialAssetDirs -contains $child.Name)) {
            foreach ($item in @(Get-ChildItem -LiteralPath $child.FullName -Force)) {
                Move-WithMeta $item.FullName $assetDir
            }
            if (@(Get-ChildItem -LiteralPath $child.FullName -Force).Count -eq 0) {
                Remove-Item -LiteralPath $child.FullName
                $dirMeta = "$($child.FullName).meta"
                if (Test-Path -LiteralPath $dirMeta) { Remove-Item -LiteralPath $dirMeta }
            }
            continue
        }
        if (-not $child.PSIsContainer) {
            $ext = $child.Extension.ToLower()
            if ($animationExt -contains $ext) {
                New-Item -ItemType Directory -Path $animationDir -Force | Out-Null
                Move-WithMeta $child.FullName $animationDir
            } elseif ($scriptExt -contains $ext) {
                New-Item -ItemType Directory -Path $scriptsDir -Force | Out-Null
                Move-WithMeta $child.FullName $scriptsDir
            } elseif ($assetExt -contains $ext) {
                Move-WithMeta $child.FullName $assetDir
            }
        }
    }
}

Remove-Item -LiteralPath $PSCommandPath -Force
