import console as co

def MainMenuPageShortcut(pagenum, loggedInUser):
    if pagenum == 1:
        return co.MainMenu(loggedInUser)
    else:
        return co.MainMenuPage2(loggedInUser)

def HandleSystemScreenOption(choice):
    if choice == "1":
        return True
    elif choice == "2":
        return False
    else:
        return None

def HandleMenuOptionBase(choice, pagenum, loggedInUser):
    if choice == "x":
        print("\nLogging out...")
        return
    elif choice == "n" and pagenum == 1:
        print("\nNavigating to page 2.")
        return co.MainMenuPage2(loggedInUser)
    elif choice == "p" and pagenum == 2:
        print("\nNavigating to page 1.")
        return co.MainMenu(loggedInUser)
    else:
        try:
            if pagenum == 1 and (int(choice) >= 1 and int(choice) <= 4) or (pagenum == 2 and (int(choice) >= 1 and int(choice) <= 5)):
                print(f"succesfully starting action number {int(choice)}")
                if pagenum == 2:
                    choice = int(choice) + 4
                HandleMenuOptions(choice, loggedInUser)
            else:
                print(f"{choice} was not recognised as a valid menu choice")
        except:
            print(f"{choice} was not recognised as a valid menu choice")
    return MainMenuPageShortcut(pagenum, loggedInUser)

def HandleMenuOptions(option, loggedInUser):
    # change password of loggedInUser
    if option == 1:
        print("implement self-password change")
    # add members/users to system
    elif option == 2:
        print("implement add members/users to system (3 options, depends on authorization lvl)")
    # change existing member's/user's info
    elif option == 3:
        print("implement change member/user info (3 options, depends on authorization lvl)")
    # search through/view list of members/users
    elif option == 4:
        print("implement search through/view list of members/users (2 options, depends on authorization lvl)")
    # delete existing member/user
    elif option == 5:
        print("implement delete existing member/user (depends on auth lvl)")
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