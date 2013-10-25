'create the object
Set objShell = WScript.CreateObject("WScript.Shell")

'bring the Vacuum Plus window into focus	
success = False
Do Until Success = True
    Success = objShell.AppActivate("Vacuum Plus")
    Wscript.Sleep 10
loop

'turn filament off
objShell.SendKeys "{ESC}"
   Wscript.Sleep 500
objShell.SendKeys "%"
   Wscript.Sleep 500
objShell.SendKeys "C"
   Wscript.Sleep 500
objShell.SendKeys "F"
   Wscript.Sleep 500