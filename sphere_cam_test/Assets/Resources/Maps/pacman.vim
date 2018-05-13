if exists("b:current_syntax")
  finish
endif

syn match wall 'w,*'
syn match pill 'p,*'
syn match powerpill '\*,*'

:let b:current_syntax = "pacman"

hi def wall ctermbg=blue ctermfg=grey guibg=blue guifg=grey
hi def pill ctermfg=yellow guifg=yellow 
hi def powerpill ctermfg=red guifg=red 
