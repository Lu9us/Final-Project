patches-own [chemical]

to setup
  clear-all
  create-turtles 5 [ 
      setxy random-xcor random-ycor
      set color "Blue"
	  ]  
	  ;; varying the color makes the bees easier to follow with your eyes
  ask patches  [
  set chemical 0
  set p-color "Red"
  ]
  reset-ticks
end

to go
  ask turtles [
    rt 4 * chemical
    fd 1 + chemical * 2 / 60
    set chemical chemical + 10     
  ]
  let tickr ticks % 10 
   if tickr = 0 [
   ; diffuse chemical 0.1
  
    ask patches [
         set chemical chemical * 0.90
	  elseif chemical > 3 [
      set p-color "Yellow"
	  ]
	  [
	  set p-color "Green"
	  ]
    ]
  ]

  ;; for speed, only run the patches every 10th tick
 
  tick
end

to flowTest
 if ticks = 0 [
   ; diffuse chemical 0.1
    ask patches [
      set chemical chemical * 0.90    
      set p-color "Yellow"
    ]
  ]
end


to testf 
let x 10 
let y 20
arrrg
let z x + y 
param-test z
end 

to arrrg
let str "aaa"
show str
end

to param-test [param]
let x param
show x
end



; Modified from a script developed by: Uri Wilensky.

