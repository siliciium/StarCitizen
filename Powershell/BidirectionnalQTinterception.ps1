<#
.SYNOPSIS
    BidirectionnalQTinterception.ps1 – Class to calculates quantum travel intersection.

.DESCRIPTION
    This tool calculates the quantum travel intersection point between the OM of 
    one celestial body and the center of another. Specify the source, destination, 
    and model of the Mantis quantum drive (size 1), and the tool will calculate the
    required distance (Gm) and time travel (sec) to the intersection point. 
    Using voiceattack, you can create some actions related to the travel time to stop 
    the ship.

.AUTHOR
    Silicium
    Silicium Corp

.COPYRIGHT
    © 2026 Silicium. All rights reserved.
    You must credit the author, not use for commercial purposes, and not modify this script without permission.

.LICENSE
    CC BY-NC-ND 4.0
    https://creativecommons.org/licenses/by-nc-nd/4.0/
.VERSION
    1.0.0

.LASTUPDATED
    11/20/2025

.NOTES
    Website project : https://bidirectional-qt-interception.github.io/
#>

class QTTravel{
    [object]$QDs = @{
        Atlas = @{ size = 1; vmax = 231000; a1 = 7000; a2 = 21000 };
        Beacon = @{ size = 1; vmax = 190000; a1 = 5750; a2 = 17300 };
        Burst = @{ size = 1; vmax = 198000; a1 = 6000; a2 = 18000 };
        Colossus = @{ size = 1; vmax = 257000; a1 = 4200; a2 = 18000 };
        Drift = @{ size = 1; vmax = 140000; a1 = 6500; a2 = 12800 };
        Eos = @{ size = 1; vmax = 165000; a1 = 5000; a2 = 15000 };
        Expedition = @{ size = 1; vmax = 165000; a1 = 5000; a2 = 15000 };
        Flood = @{ size = 1; vmax = 138000; a1 = 4170; a2 = 12500 };
        FoxFire = @{ size = 1; vmax = 168000; a1 = 6900; a2 = 23400 };
        Goliath = @{ size = 1; vmax = 215000; a1 = 3500; a2 = 15000 };
        Hyperion = @{ size = 1; vmax = 198000; a1 = 6000; a2 = 18000 };
        LightFire = @{ size = 1; vmax = 140000; a1 = 5750; a2 = 19500 };
        Rush = @{ size = 1; vmax = 165000; a1 = 5000; a2 = 15000 };
        Siren = @{ size = 1; vmax = 228000; a1 = 6900; a2 = 20700 };
        Spectre = @{ size = 1; vmax = 196000; a1 = 9100; a2 = 17900 };
        VK00 = @{ size = 1; vmax = 266000; a1 = 8050; a2 = 24200 };
        Voyage = @{ size = 1; vmax = 198000; a1 = 6000; a2 = 18000 };
        Vulcan = @{ size = 1; vmax = 179000; a1 = 2920; a2 = 12500 };
        Wayfare = @{ size = 1; vmax = 138000; a1 = 4170; a2 = 12500 };
        Zephyr = @{ size = 1; vmax = 168000; a1 = 7800; a2 = 15300 };
        Aither = @{ size = 2; vmax = 242000; a1 = 3600; a2 = 15800 };
        Bolon = @{ size = 2; vmax = 262000; a1 = 2100; a2 = 13200 };
        Bolt = @{ size = 2; vmax = 205000; a1 = 4680; a2 = 13500 };
        Cascade = @{ size = 2; vmax = 168000; a1 = 2500; a2 = 11000 };
        Crossfield = @{ size = 2; vmax = 231000; a1 = 3450; a2 = 15200 };
        Flash = @{ size = 2; vmax = 242000; a1 = 3600; a2 = 15800 };
        Hemera = @{ size = 2; vmax = 282000; a1 = 4200; a2 = 18500 };
        Huracan = @{ size = 2; vmax = 314000; a1 = 2520; a2 = 15800 };
        Khaos = @{ size = 2; vmax = 201000; a1 = 3000; a2 = 13200 };
        Nova = @{ size = 2; vmax = 171000; a1 = 3900; a2 = 11200 };
        Odyssey = @{ size = 2; vmax = 201000; a1 = 3000; a2 = 13200 };
        Quest = @{ size = 2; vmax = 168000; a1 = 2500; a2 = 11000 };
        Sojourn = @{ size = 2; vmax = 242000; a1 = 3600; a2 = 15800 };
        SparkFire = @{ size = 2; vmax = 171000; a1 = 3450; a2 = 17200 };
        Spicule = @{ size = 2; vmax = 240000; a1 = 5460; a2 = 15700 };
        SunFire = @{ size = 2; vmax = 205000; a1 = 4140; a2 = 20600 };
        Torrent = @{ size = 2; vmax = 201000; a1 = 3000; a2 = 13200 };
        XL1 = @{ size = 2; vmax = 324000; a1 = 4830; a2 = 21300 };
        Yaluk = @{ size = 2; vmax = 218000; a1 = 1750; a2 = 11000 };
        Yeager = @{ size = 2; vmax = 278000; a1 = 4140; a2 = 18200 };
        Agni = @{ size = 3; vmax = 383000; a1 = 1510; a2 = 13900 };
        Balandin = @{ size = 3; vmax = 339000; a1 = 2480; a2 = 16000 };
        Drifter = @{ size = 3; vmax = 205000; a1 = 1500; a2 = 9680 };
        Echo = @{ size = 3; vmax = 205000; a1 = 1500; a2 = 9680 };
        Erebos = @{ size = 3; vmax = 344000; a1 = 2520; a2 = 16300 };
        Fissure = @{ size = 3; vmax = 246000; a1 = 1800; a2 = 11600 };
        Impulse = @{ size = 3; vmax = 295000; a1 = 2160; a2 = 13900 };
        Kama = @{ size = 3; vmax = 319000; a1 = 1260; a2 = 11600 };
        Metis = @{ size = 3; vmax = 246000; a1 = 1800; a2 = 11600 };
        Pontes = @{ size = 3; vmax = 282000; a1 = 2070; a2 = 13400 };
        Ranger = @{ size = 3; vmax = 295000; a1 = 2160; a2 = 13900 };
        TS2 = @{ size = 3; vmax = 395000; a1 = 2900; a2 = 18700 };
        Tyche = @{ size = 3; vmax = 295000; a1 = 2160; a2 = 13900 };
        Vesta = @{ size = 3; vmax = 266000; a1 = 1050; a2 = 9680 };
        Wanderer = @{ size = 3; vmax = 246000; a1 = 1800; a2 = 11600 };
        Allegro = @{ size = 4; vmax = 512000; a1 = 907; a2 = 12600 };
        Frontline = @{ size = 4; vmax = 718000; a1 = 626; a2 = 12700 };
    }

    [object]$Bodies = @{
        Crusader  = @{ coords = $this.vector3(-18962176,-2664959.99999999,0); radius = 7450.01; OMradius = 10673.1657 };
        Microtech = @{ coords = $this.vector3(22462085.2524847,37185744.9648301,0); radius = 1000; OMradius = 1439.5934 };
        Hurston   = @{ coords = $this.vector3(12850457.093,0,0); radius = 1000; OMradius = 1436.814 };
        ArcCorp   = @{ coords = $this.vector3(18587664.739856,-22151916.9203125,0); radius = 800; OMradius = 1151.4556 };
    }

    QTTravel(){}

    [double]distance3($v1, $v2){
        $dx = $v2.x - $v1.x;
        $dy = $v2.y - $v1.y;
        $dz = $v2.z - $v1.z;
        return [Math]::Sqrt($dx * $dx + $dy * $dy + $dz * $dz);
    }

    [object]vector3([double]$x, [double]$y, [double]$z){
        return @{x = $x; y = $y; z = $z;}
    }

    [object]add3($v1, $v2){
        return $this.vector3($v1.x + $v2.x, $v1.y + $v2.y, $v1.z + $v2.z);
    }

    [object]sub3($v1, $v2){
        return $this.vector3($v2.x - $v1.x, $v2.y - $v1.y, $v2.z - $v1.z);
    }

    [double]dot($v1, $v2) {
        return $v1.x * $v2.x + $v1.y * $v2.y + $v1.z * $v2.z;
    }

    [double]clip([double]$value, [double]$min, [double]$max){
        return [Math]::Max($min, [Math]::Min($max, $value))
    }

    [array]ScaleVector($v, [double]$scalar) {
        return $this.vector3($v.x * $scalar, $v.y * $scalar, $v.z * $scalar);
    }

    [double]VectorLength($v) {
        return [Math]::Sqrt($v.x * $v.x + $v.y * $v.y + $v.z * $v.z)
    }
   
    [object]GetTimeToReachDistance($dr, $a1, $a2, $vmax, $d_accel, $d_cruise, $d_decel, $t_accel, $t_cruise, $t_decel) {
        
        $o = $null;

        $d_total = $d_accel + $d_cruise + $d_decel

        if ($dr -le $d_accel) {
            return [math]::Sqrt(2 * $dr / $a1)
            $o = [ordered]@{
                time = [math]::Sqrt(2 * $dr / $a1);
                info = "in accel";
            }
        }
        elseif ($dr -le ($d_accel + $d_cruise)) {
            $o = [ordered]@{
                time = $t_accel + (($dr - $d_accel) / $vmax);
                info = "in accel + cruise"
            }
        }
        elseif ($dr -le $d_total) {
            $d_remaining = $d_total - $dr
            $t_remaining = $t_decel - [math]::Sqrt(2 * $d_remaining / $a2)
            $o = [ordered]@{
                time = $t_accel + $t_cruise + $t_remaining;
                info = "in accel + cruise + decel"
            }
        }
        
        return $o;
    }

    [double]GetDistanceAtTime($t, $a1, $a2, $vmax, $t_accel, $t_cruise, $t_decel, $d_accel, $d_cruise, $d_decel) {
        $t_total = $t_accel + $t_cruise + $t_decel

        if ($t -le $t_accel) {
            return 0.5 * $a1 * [math]::Pow($t, 2)
        }
        elseif ($t -le ($t_accel + $t_cruise)) {
            return $d_accel + $vmax * ($t - $t_accel) 
        }
        elseif ($t -le $t_total) {
            $t_phase = $t - $t_accel - $t_cruise
            return $d_accel + $d_cruise + ($vmax * $t_phase) - (0.5 * $a2 * [math]::Pow($t_phase, 2))
        }
        else {
            return $d_accel + $d_cruise + $d_decel
        }
    }

    [string]TimeHR([double]$Secondes) {

        $heures = [math]::Floor($Secondes / 3600)
        $minutes = [math]::Floor(($Secondes % 3600) / 60)
        $secondesRestantes = $Secondes % 60

        $heuresStr = $heures.ToString("00")
        $minutesStr = $minutes.ToString("00")
        $secondesStr = $secondesRestantes.ToString("00.00").Replace(",",".")

        $tempsFormate = "{0}:{1}:{2}" -f $heuresStr, $minutesStr, $secondesStr

        return $tempsFormate
    }

    [object]GetIntersection([string]$source, [string]$destination){

        $oSource = $this.Bodies.$source;
        $oDest = $this.Bodies.$destination;

        $oSource.OM1 = $this.add3($oSource.coords, $this.vector3(0, 0, $oSource.OMradius));
        $oDest.OM1   = $this.add3($oDest.coords, $this.vector3(0, 0, $oDest.OMradius));

        $d_total = $this.distance3($oSource.coords, $oDest.coords);

        $p1 = $oSource.OM1;
        $p2 = $oDest.coords;
        $q1 = $oDest.OM1;
        $q2 = $oSource.coords;

        $u = $this.sub3($p2, $p1);
        $v = $this.sub3($q2, $q1);
        $w0 = $this.sub3($p1, $q1);


        $a = $this.dot($u, $u);
        $b = $this.dot($u, $v);
        $c = $this.dot($v, $v);
        $d = $this.dot($u, $w0);
        $e = $this.dot($v, $w0);


        $denom = $a * $c - $b * $b;
        [double]$sc = 0;
        [double]$tc = 0;

        if ($denom -eq 0) {
            $sc = 0;
            if ($b -ne 0) { 
                $tc = $d / $b 
            } else { 
                $tc =0 
            };
        } else {
            $sc = ($b * $e - $c * $d) / $denom;
            $tc = ($a * $e - $b * $d) / $denom;
        }

        $sc = $this.clip($sc, [double]0, [double]1);
        $tc = $this.clip($tc, [double]0, [double]1);

        $point_on_seg1 = $this.add3( $p1, $this.ScaleVector($u, $sc));
        $point_on_seg2 = $this.add3( $q1, $this.ScaleVector($v, $tc));

        $distance = $this.VectorLength($this.sub3($point_on_seg1, $point_on_seg2));
        $d_om1_p1 = $this.VectorLength($this.sub3($point_on_seg1, $p1));
        $d_om1_q1 = $this.VectorLength($this.sub3($point_on_seg2, $q1));

        return [ordered]@{
            d_total  = $d_total;
            distance = $distance;
            point_on_seg1 = $point_on_seg1;
            point_on_seg2 = $point_on_seg2;
            d_om1_p1 = $d_om1_p1;
            d_om1_q1 = $d_om1_q1;
        };


    }

    [object]calc_traveltime_real([string]$qd_name, [double]$distance, [double]$distance_reach){

        $o = $null

        $a1   = $this.QDs[$qd_name].a1    # km/s²
        $a2   = $this.QDs[$qd_name].a2    # km/s²
        $vmax = $this.QDs[$qd_name].vmax  # km/s

        $dc = $distance - (4 * [math]::Pow($vmax,2) * ( 2*$a1+$a2)) / (3 * [math]::Pow($a1+$a2,2))
        
        if($dc -lt 0){
            Write-Host -ForegroundColor DarkGray "- la vitesse maximale ne peut pas être atteinte"
            $z = (3*[math]::Pow($a2-$a1,2) * [math]::Pow($a1+$a2,2) *$distance) / (8 * [math]::Pow($vmax,2) * [math]::Pow($a1,3)) -1
            if ($z -gt 1){
                $t = 4 * $a1 * $vmax / ( [math]::Pow($a2,2) - [math]::Pow($a1,2)) * (2 * [math]::Cosh((-[math]::Log($z - [math]::Sqrt(( [math]::Pow($a2,2) - 1 )))) /3) -1)
            }else{
                $t = 4 * $a1 * $vmax / ( [math]::Pow($a2,2) - [math]::Pow($a1,2)) * (2 *[math]::Cos(1/3*[math]::Acos($z))-1)
            }

        }else{

            $t_accel_decel = ((4 * $vmax) / ($a1+$a2));
            $t_vconst      = ($distance/$vmax);
            $numerator     = ((4*$vmax) * (2 * $a1 + $a2));
            $denominator   = (3 * [math]::Pow(($a1 + $a2), 2));

            $t = $t_accel_decel + $t_vconst - ( $numerator / $denominator);

            $t_accel = (2 * $vmax) / ($a1 + $a2);
            $t_decel = (2 * $vmax) / ($a1 + $a2);
            $t_cruise = ($distance / $vmax) - ((4 * $vmax) * (2 * $a1 + $a2) / (3 * [math]::Pow(($a1 + $a2), 2)));


            $d_accel = (2 * [math]::Pow($vmax, 2) * (2 * $a1 + $a2)) / (3 * [math]::Pow(($a1 + $a2), 2));
            $d_decel = (2 * [math]::Pow($vmax, 2) * (2 * $a1 + $a2)) / (3 * [math]::Pow(($a1 + $a2), 2));
            $d_cruise = $distance - ((4 * [math]::Pow($vmax, 2) * (2 * $a1 + $a2)) / (3 * [math]::Pow(($a1 + $a2), 2)));

            $t_reach = $this.GetTimeToReachDistance($distance_reach, $a1, $a2, $vmax, $d_accel, $d_cruise, $d_decel, $t_accel, $t_cruise, $t_decel); 

            if($null -ne $t_reach){
                $d_reach = $this.GetDistanceAtTime($t_reach.time, $a1, $a2, $vmax, $t_accel, $t_cruise, $t_decel, $d_accel, $d_cruise, $d_decel);

                $o = [ordered]@{  
                    qd_name  = $qd_name; 
                    info     = $t_reach.info;
                    t_total  = @{s = $t; str = $this.TimeHR($t) };
                    t_accel  = @{s = $t_accel; str = $this.TimeHR($t_accel) };
                    t_decel  = @{s = $t_decel; str = $this.TimeHR($t_decel) };
                    t_cruise = @{s = $t_cruise; str = $this.TimeHR($t_cruise) };
                    t_reach  = @{s = $t_reach.time; str = $this.TimeHR($t_reach.time) };
                    d_accel  = @{Km = $d_accel; Gm = ($d_accel/1e6) };
                    d_decel  = @{Km = $d_decel; Gm = ($d_decel/1e6) };
                    d_cruise = @{Km = $d_cruise; Gm = ($d_cruise/1e6) };         
                    d_reach  = @{Km = $d_reach; Gm = ($d_reach/1e6) };
                    d_total  = @{Km = $distance; Gm = ($distance/1e6) };
                }
            }
                
        }

        return $o;
    }
}




function Main(){

    $class = [QTTravel]::new();

    $intersection = $class.GetIntersection("ArcCorp", "Hurston");

    $intersections | Format-Table

    $travel = $class.calc_traveltime_real("Spectre", $intersection.d_total, $intersection.d_om1_p1); 

    $travel | Format-Table 

    # here play with $travel.d_reach, $travel.d_reach ...

}Main
