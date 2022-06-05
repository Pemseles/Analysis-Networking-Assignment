import os
import encryption as enc
from datetime import date
import time

def BuildLogText(loggedInUser, isSus, description, additionalInfo):
    # builds log text to pass to AppendToLog depending on parameters
    sus = "Suspicious" if isSus else "Not Suspicious"
    curDate = date.today().strftime("%d-%m-%y")
    curTime = time.strftime("%H:%M:%S", time.localtime())
    if loggedInUser == "---":
        logText = f"Performed by: User (not logged in) | {curDate} - {curTime} | {description} | {additionalInfo} | {sus}"
    else:    
        logText = f"Performed by: {loggedInUser.username} | {curDate} - {curTime} | {description} | {additionalInfo} | {sus}"
    return logText

def AppendToLog(text):
    # appends text to log.txt (determines line count first as the line starts with [number])
    with open("log.txt", 'r+') as logfile:
        count = sum(1 for line in logfile)
        
        logfile.write(enc.Encrypt(f"[{count + 1}]: {text}") + "\n")
    return

def CreateLog():
    # creates log.txt (if it already exists, does nothing)
    if not os.path.exists("log.txt"):
        with open("log.txt", 'w') as logFile:
            logFile.write("")
        return
    return

def WipeLog(loggedInUser):
    if loggedInUser.role == 1 or loggedInUser.role == 2:
        # empty log contents (actually overwrites log.txt with a new file)
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

        # get each log line into array, then remove '\n' character at end of line
        with open("log.txt", 'r') as logFile:
            logArr = logFile.readlines()
            for i in logArr:
                logArr[logArr.index(i)] = i[:-1]
            return logArr
    # logged in user isn't authenticated to read log.txt
    AppendToLog(BuildLogText(loggedInUser, False, "Unauthorized attempt to read log file", "Log file has not been decrypted."))
    return