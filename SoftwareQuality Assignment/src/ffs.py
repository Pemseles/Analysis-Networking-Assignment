import console as cs
import database as db

def RunApplication():
    curUser = cs.LoginScreen()
    if curUser == 0:
        return
    else:
        cs.MainMenu(curUser)

#registeredUsers = selectAllFromTable("Users")
RunApplication()