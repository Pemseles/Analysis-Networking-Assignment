from inspect import getgeneratorlocals
import os
import database as db
import dbclasses as dbc
import menuoptions as mo

def ClearTerminal():
    os.system('cls' if os.name == 'nt' else 'clear')

def NewLinesTerminal():
    print("\n\n\n\n\n\n\n\n\n")

def LineInTerminal():
    print("-------------------------------------------------------------------------------------------\n")

def GetGeneralOptions(role):
    if role >= 0 and role <= 2:
        print("1 ) Change your password.")
        if role == 0:
            print("2 ) Add Members to system.")
        else:
            print("2 ) Add Member or User account to system.")
        if role == 0:
            print("3 ) Change existing Members' information.")
        else:
            print("3 ) Change existing Members' or Users' information.")
        if role == 0:
            print("4 ) Search through existing Members.")
        else:
            print("4 ) Search through existing Members or view list of registered Users.")

def GetSysAdminOptions(role):
    if role == 1 or role == 2:
        # 2-5) should be able to do this to sys admin as super admin & only to advisors as sys admin
        if role == 1:
            print("1 ) Delete existing Member.")
        else:
            print("1 ) Delete existing Member or User.")
        # resetting password should set a temporary password, which gets changed back to what it was upon target's login
        print("2 ) Reset existing User's password.")
        print("3 ) Create database back-up")
        print("4 ) Restore database from back-up.")
        print("5 ) View system's log file.")

def SystemScreenLoop():
    while True:
        goToLogin = SystemScreen()
        if goToLogin:
            LoginScreen()
        elif goToLogin == False:
            return

def SystemScreen():
    print()
    LineInTerminal()
    print("Welcome to the Furnicor Administrative system.\nPlease select one of the following options:\n")
    print("1 ) Log into account.")
    print("2 ) Exit system.")

    choice = input("\nOption choice: ")
    return mo.HandleSystemScreenOption(choice)

def LoginScreen():
    # get users from db
    registeredUsers = db.SelectAllFromTable("Users")
    LineInTerminal()
    print("Welcome to the Furnicor Administrative system.\nPlease provide your login credentials.\n")
    username = input("Username: ")
    password = input("Password: ")

    loginResult = dbc.AuthenticateCredentials(username, password)
    if loginResult == 0:
        print("\nLogin failed; incorrect username or password")
        return 0
    else:
        print(f"\nLogin successful, welcome {loginResult.first_name} {loginResult.last_name}")
        return MainMenu(loginResult)

def MainMenu(loggedInUser):
    LineInTerminal()
    if (loggedInUser.role == 0):
        print(f"Welcome to the main menu of Furnicor Administrative system.\n(logged in as {loggedInUser.first_name} {loggedInUser.last_name} - {loggedInUser.role_name})")
    elif (loggedInUser.role == 1 or loggedInUser.role == 2):
        print(f"Welcome to the main menu of Furnicor Administrative system (page 1/2).\n(logged in as {loggedInUser.first_name} {loggedInUser.last_name} - {loggedInUser.role_name})")
    print("Please select one of the following options:\n")

    GetGeneralOptions(loggedInUser.role)
    print("\nx ) Log out.")
    if (loggedInUser.role == 1 or loggedInUser.role == 2):
        print("n ) Navigate to page 2.")

    menuChoice = input("\nOption choice: ")
    return mo.HandleMenuOptionBase(menuChoice, 1, loggedInUser)

def MainMenuPage2(loggedInUser):
    LineInTerminal()
    print(f"Welcome to the main menu of Furnicor Administrative system (page 2/2)")
    print("Please select one of the following options:\n")

    GetSysAdminOptions(loggedInUser.role)
    print("\nx ) Log out.")
    print("p ) Navigate to page 1.")

    menuChoice = input("\nOption choice: ")
    return mo.HandleMenuOptionBase(menuChoice, 2, loggedInUser)