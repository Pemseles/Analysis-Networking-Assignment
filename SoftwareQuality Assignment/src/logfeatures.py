import os
from datetime import date
import time

def BuildLogText(loggedInUser, isSus, description, additionalInfo):
    sus = "Suspicious" if isSus else "Not Suspicious"
    curDate = date.today().strftime("%d-%m-%y")
    curTime = time.strftime("%H:%M:%S", time.localtime())
    logText = f"Performed by: {loggedInUser.username} | {curDate} - {curTime} | {description} | {additionalInfo} | {sus}"
    return logText

def AppendToLog(text):
    with open("log.txt", 'r+') as logfile:
        for count in enumerate(logfile):
            pass
        print(f"Amount of lines: {count + 2}")
        
        logfile.write(f"[{count + 2}]: {text}\n")
    return

def CreateLog():
    if not os.path.exists("log.txt"):
        with open("log.txt", 'w') as logFile:
            logFile.write("")
        return
    return

def WipeLog(loggedInUser):
    if loggedInUser.role == 1 or loggedInUser.role == 2:
        open("log.txt", 'w').close()
        print("Log file contents have been erased.")
        AppendToLog(BuildLogText(loggedInUser, False, "Authorized erasure of log file contents", "Log file has been erased entirely, it can be restored if a back-up is present."))
        return
    # logged in user isn't authenticated to wipe log file contents
    AppendToLog(BuildLogText(loggedInUser, True, "Unauthorized attempt to erase log file contents", "Log file has not been erased"))
    return

def ReadLog(loggedInUser):
    if loggedInUser.role == 1 or loggedInUser.role == 2:
        # decrypt & print log.txt
        AppendToLog(BuildLogText(loggedInUser, False, "Authorized reading of log file", "Log file has been decrypted and it's contents printed to the console."))
        return
    # logged in user isn't authenticated to read log.txt
    AppendToLog(BuildLogText(loggedInUser, False, "Unauthorized attempt to read log file", "Log file has not been decrypted."))
    return