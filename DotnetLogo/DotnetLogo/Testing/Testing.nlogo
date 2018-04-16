
to test-assert
set x 10 
assertEql x 10
end

to testadd 
set x 15
set y 15
set z x + y
assertEql z 30
end

to test-param [ param ]
assertEql param 20
end

to test-patches
ask patches [
let r X % 2
if r = 0 [
set p-color "Green"
]
let s Y % 2
if s = 0 [
set p-color "Green"
]
]
end


to test-agents [ count ]

create-turtles count [
set color "Red"
setxy random-xcor random-ycor
]

end

to test-move
test-agents 10
tick
ask turtles [
fd 3
]
end




to test-stopwatch
let x "runtimetime"
start-stopwatch x
let dx 0
test-move
stop-stopwatch x dx
show "stop watch end:"
show dx
end
