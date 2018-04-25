
patches-own [  living? live-neighbors  ]

to setup-blank
  clear-all
  ask patches [ 
cell-death 
]
  reset-ticks
end


to setup
  clear-all
let ran 0
  ask patches [ 
set ran random + 1

elseif ran > 41 [
cell-birth
]
[
cell-death
]
]
  reset-ticks
end

to go 

let liven 0

ask patches [
set liven 0
ask neighbours [

if living? =  true [
set liven liven + 1
]
]
if liven = 3 [
cell-birth
]

if liven > 3 [
cell-death
]

if liven < 2 [
cell-death
]

]
tick

end


to cell-death
  set living? false
  set p-color "Grey"
end

to cell-birth
  set living? true
  set p-color "Black"
end
