# USAGE EXAMPLE
```powershell
function Main(){

  $class = [QTTravel]::new();
  
  $intersection = $class.GetIntersection("ArcCorp", "Hurston");
  
  $intersections | Format-Table
  
  $travel = $class.calc_traveltime_real("Spectre", $intersection.d_total, $intersection.d_om1_p1); 
  
  $travel | Format-Table 
  
  # here play with $travel.d_reach, $travel.t_reach ...

} Main
```
# OUTPUT
<p align="center">
<img width="915" height="532" alt="image" src="https://github.com/user-attachments/assets/26cfbd0f-71ff-4699-8752-f7fe32f1684c" />
</p>

# Related Tool WEB Interface

  [![Bidirectional QT Interception](https://raw.githubusercontent.com/bidirectional-qt-interception/bidirectional-qt-interception.github.io/refs/heads/main/Assets/preview.png)](https://bidirectional-qt-interception.github.io/)

