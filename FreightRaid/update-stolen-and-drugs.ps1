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
    "Sacren's Plot" = "Pyro > Pyro V";
    "Jackson's Swap" = "Pyro > Monox";
    "Canard View" = "Pyro > Terminus";
    "Shepherd's Rest" = "Pyro > Bloom";
    "Bueno Ravine" = "Pyro > Bloom";
    "Seer's Canyon" = "Pyro > Pyro V";
    "Last Landings" = "Pyro > Terminus";
    "Chawla's Beach" = "Pyro > Pyro V";
    "Sunset Mesa" = "Pyro > Monox";
    "Rustville" = "Pyro > Pyro I";
    #"Deakins Research" = "Stanton > Yela";
    #"Stanton Gateway (Nyx)" = "Nyx > Stanton Gateway";
    #"TDD Orison" = "Stanton > Crusader";
    #"CBD Lorville" = "Stanton > Hurston";
    #"TDD New Babbage" = "Stanton > Microtech";
    #"TDD Area 18" = "Stanton > ArcCorp";
    #"Levski" = "Nyx";
}


$rep = (invoke-webrequest -Uri "")

# stolens
<# $comnames = @(
"Bexalite";
"Fresh food";
"Thermalfoam";
"Tungsten";
"Organics";
"Laranite";
"Taranite";
"Atlasium";
"Quartz";
"Diamond";
"Compboard";
"Medical Supplies";
"Nitrogen";
"Bioplastic";
"Astatine";
"Cobalt";
"Recycled material composite"
) #>

# drugs
$comnames = @(
"Altruciatoxin";
"E'tam";
#"Gasping Weevil Eggs";
#"Maze";
"Neon";
"SLAM";
"WiDoW";
"Distilled spirits"
)


$data = ($rep | ConvertFrom-Json).data

$obj = @{}


foreach($comname in $comnames){

    Write-Host -ForegroundColor Blue "$comname"

    $objs_child = @()

    $data | Where-Object { $_.commodity_name -eq $comname -and $_.status_sell -gt 0 -and $_.price_buy -eq 0 -and $_.price_sell -gt 0 } | Sort-Object -Property price_sell_avg -Descending  | ForEach-Object {

        $loc = $links[$_.terminal_name]

        if(-not [string]::IsNullOrEmpty($loc)){
            $o = [ordered]@{}
            $o.location = $loc
            $o.shop = $_.terminal_name
            $o.price = $_.price_sell_avg

            $objs_child += $o;
        }else{
            Write-Host -ForegroundColor Red "Ignored : $($_.terminal_name)"
        }
    }

    $k = (CapitalizeFirstLetter -s $comname)
    $obj[$k] = $objs_child

    Write-Host -ForegroundColor Blue "($($objs_child.Count))"
}

$sortedResult = [ordered]@{}

foreach ($entry in $obj.GetEnumerator() | Sort-Object Name) {
    $sortedResult[$entry.Key] = $entry.Value
}


$sortedResult | ConvertTo-Json -Depth 5

