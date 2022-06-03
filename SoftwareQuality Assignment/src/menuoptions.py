import consolemenus as cm
import menufeatures as mf
import inputchecks as ic
import encryption as enc
import logfeatures as lg

def MainMenuPageShortcut(pagenum, loggedInUser):
    # literally just here so i don't have to write this code 2x
    if pagenum == 1:
        return cm.MainMenu(loggedInUser)
    else:
        return cm.MainMenuPage2(loggedInUser)

def InvalidSubMenuChoice(returnStr, choice, newLine=False):
    if newLine:
        cm.LineInTerminal()
    print(f"{choice} was not recognised as a valid menu choice.")
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
        lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "User has logged out of their account", "No additional info required"))
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
                # where most of them go to sub-menus
                # depending on return, can re-enter the same menu indefinitely
                result = ""
                if pagenum == 2:
                    choice = str(int(choice) + 4)
                while result == "sub-menu" or result == "":
                    result = HandleMenuOptions(int(choice), loggedInUser)
                    print("result:", result)
                if result == "logout":
                    return
            else:
                lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Invalid menu option inputted", f"User inputted an invalid option when navigating main menu (page {pagenum})"))
                print(f"{choice} was not recognised as a valid menu choice. (base handler; inner)")
        else:
            lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Invalid menu option inputted", f"User inputted an invalid option when navigating main menu (page`{pagenum})"))
            print(f"{choice} was not recognised as a valid menu choice. (base handler; outer)")
    # return to the page logged in user was on previously
    return MainMenuPageShortcut(pagenum, loggedInUser)

def HandleMenuOptions(option, loggedInUser):
    # handles options of pages 1 & 2 of main menu
    print(f"Inside HandleMenuOptions; choice = {option}, user = {loggedInUser.username}")
    # loopResult is only here for a select few sub-menus
    loopResult = ""
    # change password of loggedInUser
    if option == 1:
        return mf.ChangePassword(loggedInUser, loggedInUser)
    # add members/users to system
    elif option == 2:
        return cm.AddToSystemSubmenu(loggedInUser)
    # change existing member's/user's info
    elif option == 3:
        return mf.ModifyInfoList(loggedInUser)
    # search through/view list of members/users
    elif option == 4:
        return cm.SearchDatabase(loggedInUser)
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
        return cm.ViewLogMenu(loggedInUser)

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
        lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Invalid menu option inputted", "User inputted an invalid option when choosing what to add to the system"))
        print(f"{option} was not recognised as a valid menu choice. (add sub-handler)")
        return cm.AddToSystemSubmenu(loggedInUser)
    return

def HandleMenuOptionsModify(loggedInUser, target, isMember, option):
    # handles options of sub-menu of modifying information
    print(f"Inside HandleMenuOptionsModify; choice = {option}, user = {loggedInUser.username}")
    # infoPiece will be passed to UpdateInfo()
    optionNums = ["First name", "Last name", "Address", "Email address", "Phone number", "Registration date"]
    infoPiece = ""

    if option == "x":
        # return to page 1
        print("\nReturning to main page...")
        return
    if option in enc.AlphabetExtended(10, 48):
        if int(option) >= 1 and int(option) <= 6:
            # get infoPiece
            infoPiece = optionNums[int(option) - 1]
            return mf.UpdateInfo(loggedInUser, target, infoPiece, isMember)
    else:
        # return to sub-menu; invalid option
        lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Invalid menu option inputted", "User inputted an invalid option when choosing who's information to modify"))
        print(f"{option} was not recognised as a valid menu choice. (modify sub-handler)")
        return cm.ModifyInfoSubmenu(loggedInUser)

def DecideCheckFunction(loggedInUser, infoPiece, newInput, houseNum = "", zipCode = "", city = ""):
    if infoPiece == "First name" or infoPiece == "Last name":
        return ic.CheckFirstOrLastName(newInput)
    elif infoPiece == "Address":
        return ic.CheckAddress(newInput, houseNum, zipCode, city)
    elif infoPiece == "Email address":
        return ic.CheckEmail(newInput)
    elif infoPiece == "Phone number":
        return ic.CheckPhone(newInput)
    return False

def HandleMenuOptionsSearch(loggedInUser, option):
    # handles options of sub-menu of search-results
    print(f"Inside HandleMenuOptionsSearch; choice = {option}, user = {loggedInUser.username}")

    if option == "x":
        # return to page 1
        print("\nReturning to main page...")
        return
    elif option == "r":
        # re-enter search sub-menu
        print("returning to sub-menu TEMP")
        return "sub-menu"
    else:
        # input was not recognized
        lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Invalid menu option inputted", "User inputted an invalid option when deciding what to do after searching through members/users"))
        print(f"{option} was not recognised as a valid menu choice. (search sub-handler)")
        return cm.SearchDatabase(loggedInUser)

def HandleMenuOptionsLog(loggedInUser, option):
    # handles options of sub-menu of viewing log file
    print(f"Inside HandleMenuOptionsLog; choice = {option}, user = {loggedInUser.username}")

    if option == "x":
        # return to main menu
        print("\nReturning to main page...")
        return "x"
    elif option == "v":
        # print entire log
        mf.ViewLog(loggedInUser)
        return "v"
    elif option == "e":
        # erase log file contents
        lg.WipeLog(loggedInUser)
        return "e"
    else:
        # invalid option
        lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Invalid menu option inputted", "User inputted an invalid option when attempting to view log file"))
        print(f"{option} was not recognised as a valid menu choice. (search sub-handler)")
        return