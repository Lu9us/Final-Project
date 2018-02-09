patches-own [chemical]

to setup
  clear-all
  create-turtles num-bees
    [ setxy random-xcor random-ycor
      set color yellow - 3 + random 7 ]  
	  ;; varying the color makes the bees easier to follow with your eyes
  ask patches [ set chemical 0 ]
  reset-ticks
end

to go
  ask turtles [
    rt 4 * chemical
    fd 1 + ((chemical ^ 2) / 60)
    set chemical chemical + 2      ;; drop chemical onto patch
  ]
  ;; for speed, only run the patches every 10th tick
  if ticks mod 10 = 0 [
    diffuse chemical 0.1
    ask patches [
      set chemical chemical * 0.90       ;; evaporate chemical
      set pcolor scale-color gray chemical 0 20
    ]
  ]
  tick
end

to testf 
let x 10 
let y 20
let z x + y 
show z
end 


; Copyright 2003 Uri Wilensky.
; See Info tab for full copyright and license.