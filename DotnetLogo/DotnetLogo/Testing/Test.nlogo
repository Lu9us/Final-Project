patches-own [chemical]

to setup
  clear-all
  create-turtles 5 [ 
      setxy random-xcor random-ycor
      set color "yellow"
	  ]  
	  ;; varying the color makes the bees easier to follow with your eyes
  ask patches  [
  set chemical 0
  ]
  reset-ticks
end

to go
  ask turtles [
    rt 4 * chemical
    fd 1 + ((chemical ^ 2) / 60)
    set chemical chemical + 2     
  ]
  ;; for speed, only run the patches every 10th tick
  if ticks mod 10 = 0 [
    diffuse chemical 0.1
    ask patches [
      set chemical chemical * 0.90       ;; evaporate chemical
      set pcolor "grey"
    ]
  ]
  tick
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



; Copyright 2003 Uri Wilensky.
; See Info tab for full copyright and license.