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
    print("-------------------------------------------------------------------------------------------")

def GetGeneralOptions(role):
    if role >= 0 and role <= 2:
        print("1 ) Change your password.")
        print("2 ) Add Members to system.")
        print("3 ) Change existing Members' information.")
        print("4 ) Search through existing Members.")

def GetSysAdminOptions(role):
    if role == 1 or role == 2:
        print("1 ) View list of registered Users.")
        # 6-10) should be able to do this to sys admin as super admin & only to advisors as sys admin
        print("2 ) Delete existing User.")
        print("3 ) Add a User account to the system.")
        print("4 ) Change existing User's information.")
        print("5 ) Delete existing User's account.")
        print("6 ) Reset existing User's password.")
        print("7 ) Create database back-up")
        print("8 ) Restore database from back-up.")
        print("9 ) View system's log file.")

def LoginScreen():
    # get users from db
    registeredUsers = db.SelectAllFromTable("Users")
    NewLinesTerminal()
    
    LineInTerminal()
    print("Welcome to the Furnicor Administrative system.\nPlease provide your login credentials.\n")
    username = input("Username: ")
    password = input("Password: ")
    print("")

    loginResult = dbc.AuthenticateCredentials(username, password)
    if loginResult == 0:
        print("Login failed; incorrect username or password")
        return 0
    else:
        print(f"Login successfull, welcome {loginResult.first_name} {loginResult.last_name}")
        NewLinesTerminal()
        return loginResult

def MainMenu(loggedInUser):
    NewLinesTerminal()
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
    mo.HandleMenuOption(menuChoice, 1, loggedInUser)

def MainMenuPage2(loggedInUser):
    NewLinesTerminal()
    LineInTerminal()
    print(f"Welcome to the main menu of Furnicor Administrative system (page 2/2)")
    print("Please select one of the following options:\n")

    GetSysAdminOptions(loggedInUser.role)
    print("\nx ) Log out.")
    print("p ) Navigate to page 1.")

    menuChoice = input("\nOption choice: ")
    mo.HandleMenuOption(menuChoice, 2, loggedInUser)