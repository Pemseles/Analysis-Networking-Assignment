import consolemenus as cm
import menufeatures as mf
import encryption as enc

def MainMenuPageShortcut(pagenum, loggedInUser):
    # literally just here so i don't have to write this code 2x
    if pagenum == 1:
        return cm.MainMenu(loggedInUser)
    else:
        return cm.MainMenuPage2(loggedInUser)

def InvalidSubMenuChoice(returnStr, choice, newLine=False):
    if newLine:
        cm.LineInTerminal()
    print(f"{choice} was not recognized as a valid menu choice.")
    return returnStr

def HandleXInput(input):
    if input == "x":
        return True
    return False

def HandleSystemScreenOption(choice):
    # will keep user in the loop until they input 1 or 2, 1 brings them to loginscreen, 2 terminates program
    print(f"Inside HandleSystemScreenOption; choice = {choice}")
    if choice == "1":
        return True
    elif choice == "2":
        return False
    else:
        return None

def HandleMenuOptionBase(choice, pagenum, loggedInUser):
    # handles base option choices
    print(f"Inside HandleMenuOptionBase; choice = {choice}, page = {pagenum}, user = {loggedInUser.username}")
    if choice == "x":
        print("\nLogging out...")
        return
    elif choice == "n" and pagenum == 1 and loggedInUser.role >= 1 and loggedInUser.role <= 2:
        print("\nNavigating to page 2.")
        return cm.MainMenuPage2(loggedInUser)
    elif choice == "p" and pagenum == 2 and loggedInUser.role >= 1 and loggedInUser.role <= 2:
        print("\nNavigating to page 1.")
        return cm.MainMenu(loggedInUser)
    else:
        if choice in enc.AlphabetExtended(10, 48):
            if pagenum == 1 and (int(choice) >= 1 and int(choice) <= 4) or (pagenum == 2 and (int(choice) >= 1 and int(choice) <= 5)):
                result = ""
                if pagenum == 2:
                    choice = str(int(choice) + 4)
                while result == "sub-menu" or result == "":
                    result = HandleMenuOptions(int(choice), loggedInUser)
                    print("result:", result)
                if result == "logout":
                    return
            else:
                print(f"{choice} was not recognised as a valid menu choice.")
        else:
            print(f"{choice} was not recognised as a valid menu choice.")
    # return to the page logged in user was on previously
    return MainMenuPageShortcut(pagenum, loggedInUser)

def HandleMenuOptions(option, loggedInUser):
    # handles options of pages 1 & 2 of main menu
    print(f"Inside HandleMenuOptions; choice = {option}, user = {loggedInUser.username}")

    # change password of loggedInUser
    if option == 1:
        return mf.ChangePassword(loggedInUser, loggedInUser)
    # add members/users to system
    elif option == 2:
        return cm.AddToSystemSubmenu(loggedInUser)
    # change existing member's/user's info
    elif option == 3:
        print("implement change member/user info (3 options, depends on authorization lvl)")
        return mf.ModifyInfoList(loggedInUser)
    # search through/view list of members/users
    elif option == 4:
        print("implement search through/view list of members/users (2 options, depends on authorization lvl)")
    # delete existing member/user
    elif option == 5:
        return mf.DeleteUserMember(loggedInUser)
    # temp reset existing user's password
    elif option == 6:
        print("implement reset existing user's password temporarily")
    # database back-up
    elif option == 7:
        print("implement create database back-up")
    # restore database from back-up
    elif option == 8:
        print("implement restore database from back-up")
    # view system's log file
    elif option == 9:
        print("implement view system's log file")

def HandleMenuOptionsAdd(option, loggedInUser):
    # handles options of sub-menu of adding member/user to system
    print(f"Inside HandleMenuOptionsAdd; choice = {option}, user = {loggedInUser.username}")

    if option == "x":
        # return to page 1
        print("\nReturning to main page...")
    elif option == "1":
        # proceed to add member
        return mf.AddMemberOrUser(loggedInUser, -1)
    elif (option == "2" and loggedInUser.role > 0) or (option == "3" and loggedInUser.role > 1):
        # proceed to add user
        return mf.AddMemberOrUser(loggedInUser, int(option) - 2)
    else:
        # if anything else is inputted
        print(f"{option} was not recognised as a valid menu choice.")
        return cm.AddToSystemSubmenu(loggedInUser)
    return

def HandleMenuOptionsModify(loggedInUser, target, isMember, option):
    # handles options of sub-menu of modifying information
    print(f"Inside HandleMenuOptionsModify; choice = {option}, user = {loggedInUser.username}")

    if option == "x":
        # return to page 1
        print("\nReturning to main page...")
    elif option == "1":
        # change first name of target
        print()
    elif option == "2":
        # change last name of target
        print()
    elif option == "3":
        # change address of target
        print()
    elif option == "4":
        # change email of target
        print()
    elif option == "5":
        # change phone of target
        print()
    elif option == "6":
        # update registration date to today
        print()
    return