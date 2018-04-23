
patches-own [  living? live-neighbors  ]

to setup-blank
  clear-all
  ask patches [ 
cell-death 
]
  reset-ticks
end


to cell-death
  set living? false
  set p-color "Grey"
end