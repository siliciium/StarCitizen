Clear-Host

function CapitalizeFirstLetter($s){
    if ($s.Length -lt 2) { return $s.ToUpper() }
    return $s.Substring(0,1).ToUpper() + $s.Substring(1)
}

$links = @{
    "Devlin Scrap" = "Stanton > MicroTech > Euterpe";
    "CRU-L4 Locker" = "Stanton > CRU L4 > CRU-L4 Shallow Fields Station";
    "Samson & Son's" = "Stanton > ArcCorp > Wala";
    "Brio's Breaker" = "Stanton > Crusader > Daymar";
    "Reclamation Orinth" = "Stanton > Hurston";
    "GrimHEX" = "Stanton > Crusader > Yela";
    "Gaslight" = "Pyro > Pyro V";
    "Rod's Fuel" = "Pyro > Pyro V";
    "Patch City" = "Pyro > Bloom";
    "Endgame" = "Pyro > Pyro VI";
    "Starlight Service" = "Pyro > Bloom";
    "Checkmate" = "Pyro > Monox";
    "Ashland" = "Pyro > Pyro V";
    "Ruin Station" = "Pyro > Terminus";
    "Orbituary" = "Pyro > Bloom";
    "Fallow Field" = "Pyro > Pyro V";
    "Megumi" = "Pyro > Pyro VI";
    "Dudley and Daughters" = "Pyro > Pyro VI";
    "CRU-L5 Maintenance" = "Stanton > CRU L5 > CRU-L5 Beautiful Glen Station";
    "The Golden Riviera" = "Pyro > Bloom";
    "Rat's Nest" = "Pyro > Pyro V";
}


$rep = (invoke-webrequest -Uri "")

$drugname = "Maze"

$data = ($rep | ConvertFrom-Json).data

Write-Host ("`t`"{0}`" : [" -f (CapitalizeFirstLetter -s $drugname))
$data | Where-Object { $_.commodity_name -eq $drugname -and $_.status_sell -gt 0 } | Sort-Object -Property price_sell_avg -Descending  | ForEach-Object {

    $loc = $links[$_.terminal_name]

    Write-Host "`t`t{"
    Write-Host ("`t`t`t`"location`": `"{0}`"," -f $loc)
    Write-Host ("`t`t`t`"shop`": `"{0}`"," -f $_.terminal_name)
    Write-Host ("`t`t`t`"price`": {0}," -f $_.price_sell_avg)
    Write-Host "`t`t},"
}
Write-Host "`t],"
