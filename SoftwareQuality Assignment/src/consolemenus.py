from inspect import getgeneratorlocals
import os
import database as db
import dbclasses as dbc
import menuoptions as mo
import menufeatures as mf
import logfeatures as lg
import encryption as enc
import inputchecks as ic

def LineInTerminal():
    print("-------------------------------------------------------------------------------------------\n")

def GetGeneralOptions(loggedInUser):
    if loggedInUser.role >= 0 and loggedInUser.role <= 2:
        print("1 ) Change your password.")
        # for all role-dependent options: advisor+ can edit members, system admin+ can edit advisors, super admin can edit system admins
        if loggedInUser.role == 0:
            print("2 ) Add Members to system.")
        else:
            print("2 ) Add Member or User account to system.")
        if loggedInUser.role == 0:
            print("3 ) Change existing Members' information.")
        else:
            print("3 ) Change existing Members' or Users' information.")
        if loggedInUser.role == 0:
            print("4 ) Search through existing Members.")
        else:
            print("4 ) Search through existing Members and Users.")
    else:
        # unauthorized to have any menu option
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to get menu options (page 1)", "User attempted to get the menu options (is only for Advisors or higher)"))

def GetSysAdminOptions(loggedInUser):
    if loggedInUser.role == 1 or loggedInUser.role == 2:
        if loggedInUser.role == 1:
            print("1 ) Delete existing Member.")
        else:
            print("1 ) Delete existing Member or User.")
        # resetting password should set a temporary password, which gets changed back to what it was upon target's login
        print("2 ) Reset existing User's password.")
        print("3 ) Create database/log back-up")
        print("4 ) Restore database/log from back-up.")
        print("5 ) View system's log file.")
    else:
        # unauthorized to have page 2 menu option
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to get menu options (page 2)", "User attempted to get the menu options (is only for System Administrators or higher)"))

def SystemScreenLoop():
    # keeps opening SystemScreen() until user enters 1 or 2
    loginAttempts = 0
    while True:
        goToLogin = SystemScreen()
        if goToLogin:
            loginAttempts = LoginScreen(loginAttempts)
            if loginAttempts == None:
                loginAttempts = 0
        elif goToLogin == False:
            return

def SystemScreen():
    # 1st menu, forces user to either log in or exit
    LineInTerminal()
    print("Welcome to the Furnicor Administrative system.\nPlease select one of the following options:\n")
    print("1 ) Log into account.")
    print("2 ) Exit system.")

    choice = input("\nOption choice: ")
    return mo.HandleSystemScreenOption(choice)

def LoginScreen(loginAttempts):
    # block access if loginAttempts >= 5
    if loginAttempts >= 5:
        lg.AppendToLog(lg.BuildLogText("---", True, "Someone unsuccessfully tried to log in 5 times consecutively", "Person has been blocked access to the login screen"))
        print("Too many attempts at logging in; Blocked access.")
        return loginAttempts

    # get users from db
    try:
        registeredUsers = db.SelectAllFromTable("Users")
    except:
        print("Database was not recognised (possibly the wrong database.db was restored)")
        return
    LineInTerminal()
    print("Welcome to the Furnicor Administrative system.\nPlease provide your login credentials.\n")
    username = input("Username: ")
    password = input("Password: ")

    loginResult = dbc.AuthenticateCredentials(username, password)
    if loginResult == 0 or not ic.CheckUsername(username, True) or not ic.CheckPassword(password, True):
        # failed login result
        print("\nLogin failed; incorrect username or password")
        return loginAttempts + 1
    else:
        # successfull login result
        print(f"\nLogin successful, welcome {loginResult.first_name} {loginResult.last_name}")

        # reset temp password
        if loginResult.temp_password != "":
            lg.AppendToLog(lg.BuildLogText(loginResult, False, "Temporary password has been removed", "User with temporary password has logged into their account using the temporary password"))
            updateArr = enc.EncryptTupleOrArray([loginResult.first_name, loginResult.last_name, loginResult.username, loginResult.password, "", loginResult.address, loginResult.email_address, loginResult.phone_number]) + [loginResult.id]
            db.UpdateUserEntry(loginResult, updateArr)

        lg.AppendToLog(lg.BuildLogText(loginResult, False, "User successfully logged into their account", f"User has logged into their account after {loginAttempts + 1} attempt(s)"))
        return MainMenu(loginResult)

def MainMenu(loggedInUser):
    # page 1 of main menu
    LineInTerminal()
    if loggedInUser.role == 0:
        print(f"Welcome to the main menu of Furnicor Administrative system.\n(logged in as {loggedInUser.first_name} {loggedInUser.last_name} - {loggedInUser.role_name})")
    elif loggedInUser.role == 1 or loggedInUser.role == 2:
        print(f"Welcome to the main menu of Furnicor Administrative system (page 1/2).\n(logged in as {loggedInUser.first_name} {loggedInUser.last_name} - {loggedInUser.role_name})")
    else:
        # unauthorized attempt to access main menu
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to access main menu (page 1)", "User attempted to access main menu (is only for Advisors or higher)"))
        return
    print("Please select one of the following options:\n")

    GetGeneralOptions(loggedInUser)
    print("\nx ) Log out.")
    if (loggedInUser.role == 1 or loggedInUser.role == 2):
        print("n ) Navigate to page 2.")

    menuChoice = input("\nOption choice: ")
    return mo.HandleMenuOptionBase(menuChoice, 1, loggedInUser)

def MainMenuPage2(loggedInUser):
    # page 2 of main menu
    if loggedInUser.role != 1 and loggedInUser.role != 2:
        # unauthorized attempt to access page 2
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to access main menu (page 2)", "User attempted to access main menu page 2 (is only for System Administrators or higher)"))
        return
    LineInTerminal()
    print(f"Welcome to the main menu of Furnicor Administrative system (page 2/2)")
    print("Please select one of the following options:\n")

    GetSysAdminOptions(loggedInUser)
    print("\nx ) Log out.")
    print("p ) Navigate to page 1.")

    menuChoice = input("\nOption choice: ")
    return mo.HandleMenuOptionBase(menuChoice, 2, loggedInUser)

def AddToSystemSubmenu(loggedInUser):
    # check authorization
    if loggedInUser.role < 0 or loggedInUser.role > 2:
        # unauthorized
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to access add-to-system sub-menu", "User attempted to access add-to-system sub-menu (is only for Advisors or higher)"))
        return
    # sub-menu to decide to add member or some type of user
    LineInTerminal()
    print("Please select which of the following you would like to add to the system.\n")
    print("1 ) Add new Member.")
    if (loggedInUser.role == 1 or loggedInUser.role == 2):
        print("2 ) Add new Advisor.")
    if (loggedInUser.role == 2):
        print("3 ) Add new System Administrator.")
    print("\nx ) Return to main menu.")

    menuChoice = input("\nOption choice: ")
    return mo.HandleMenuOptionsAdd(menuChoice, loggedInUser)

def ModifyInfoSubmenu(loggedInUser, target, isMember):
    # check auth
    if loggedInUser.role < 0 or loggedInUser.role > 2:
        # unauthorized
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to access modify info sub-menu", "User attempted to access modify info sub-menu (is only for Advisors or higher)"))
        return
    # sub-menu to decide what specific piece of info to change
    LineInTerminal()
    print(f"Please select which you would like to modify (Current choice = {target.first_name} {target.last_name}).\n")
    print("1 ) Change first name.")
    print("2 ) Change last name.")
    print("3 ) Change address.")
    print("4 ) Change email address.")
    print("5 ) Change phone number.")
    print("6 ) Update registration date to today.")
    print("x ) Return to previous member & user overview.")

    menuChoice = input("\nOption choice: ")
    return mo.HandleMenuOptionsModify(loggedInUser, target, isMember, menuChoice)

def SearchDatabase(loggedInUser):
    # check auth
    if loggedInUser.role < 0 or loggedInUser.role > 2:
        # unauthorized
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to access search sub-menu", "User attempted to access search sub-menu (is only for Advisors or higher)"))
        return
    # sub-menu where user can enter any string and get a list of members & users that have any of their columns contain the string
    LineInTerminal()
    print(f"Please enter a search-parameter. Any entry that contains your input will be returned.\n(Note that first & last names are seperate and that entering nothing will return everything)\n")

    filter = input("Search-parameter: ")
    if filter == "": filter = " "
    print("The following results matched your input:\n")

    mf.PrintUserMemberList(loggedInUser, filter)

    print("r ) Search again using a new parameter.")
    
    menuChoice = input("\nOption choice: ")
    return mo.HandleMenuOptionsSearch(loggedInUser, menuChoice)

def ViewLogMenu(loggedInUser):
    # check auth
    if loggedInUser.role != 1 and loggedInUser.role != 2:
        # unauthorized
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to access view log sub-menu", "User attempted to access view log sub-menu (is only for System Administrators or higher)"))
        return
    # provide options
    LineInTerminal()
    print("Please choose one of the following actions regarding the log file.\n")
    print(f"v ) View the entire log file.\ne ) Erase the log file's contents.\nx ) Return back to the main page.")
    
    menuChoice = ""
    result = "v"
    while result == "v" or result == "e":
        menuChoice = input("\nOption choice: ")
        result = mo.HandleMenuOptionsLog(loggedInUser, menuChoice)
    return

def ResetPassMenu(loggedInUser):
    # check auth
    if loggedInUser.role != 1 and loggedInUser.role != 2:
        # unauthorized
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to access reset password sub-menu", "User attempted to access reset password sub-menu (is only for System Administrators or higher)"))
        return
    # provide options
    LineInTerminal()
    print(f"Please choose one of the following users to temporarily reset their password.")
    # get list of just users
    resetInstances = mf.PrintUserMemberList(loggedInUser, "", False)

    choice = input("\nOption choice: ")
    return mo.HandleMenuOptionsReset(loggedInUser, choice, resetInstances)

def BackupMenu(loggedInUser):
    # check auth
    if loggedInUser.role != 1 and loggedInUser.role != 2:
        # unauthorized
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to access back-up sub-menu", "User attempted to access back-up sub-menu (is only for System Administrators or higher)"))
        return
    # provide options
    LineInTerminal()
    print(f"Please choose which one of the following you would like to back up.\n")

    print("1 ) Back up database.")
    print("2 ) Back up log file.")
    print("\nx ) Return to main page.")

    choice = input("\nOption choice: ")
    return mo.HandleMenuOptionsBackup(loggedInUser, choice)

def RestoreMenu(loggedInUser):
    # check auth
    if loggedInUser.role != 1 and loggedInUser.role != 2:
        # unauthorized
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to access restore back-up sub-menu", "User attempted to access restore back-up sub-menu (is only for System Administrators or higher)"))
        return
    # provide options
    LineInTerminal()
    print(f"Please choose which one of the following you would like to restore.\n")

    print("1 ) Restore database.")
    print("2 ) Restore log file.")
    print("\nx ) Return to main page.")

    choice = input("\nOption choice: ")
    return mo.HandleMenuOptionsRestore(loggedInUser, choice)