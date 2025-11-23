# USAGE EXAMPLE
```powershell
function Main(){

  $qttravel = [QTTravel]::new();
  
  $intersection = $qttravel.GetIntersection("ArcCorp", "Hurston");
  
  $intersection | Format-Table
  
  $travel = $qttravel.calc_traveltime_real("Spectre", $intersection.d_total, $intersection.d_om1_p1); 
  
  $travel | Format-Table 
  
  # here play with $travel.d_reach, $travel.t_reach ...

} Main
```
# OUTPUT
<p align="center" width="100%">
  <img width="933" height="526" alt="image" src="https://github.com/user-attachments/assets/199daea6-72e3-499b-a68d-614933d5779e" style="width:100%;"/>
</p>

# Related Tool WEB Interface

  [![Bidirectional QT Interception](https://raw.githubusercontent.com/bidirectional-qt-interception/bidirectional-qt-interception.github.io/refs/heads/main/Assets/preview.png)](https://bidirectional-qt-interception.github.io/)

