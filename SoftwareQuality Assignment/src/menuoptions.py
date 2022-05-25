import console as co

def HandleMenuOption(choice, pagenum, loggedInUser):
    if choice == "x":
        print("\nLogging out...")
    elif choice == "n" and pagenum == 1:
        print("\nNavigating to page 2.")
        return co.MainMenuPage2(loggedInUser)
    elif choice == "p" and pagenum == 2:
        print("\nNavigating to page 1.")
        return co.MainMenu(loggedInUser)
    else:
        try:
            if pagenum == 1 and (int(choice) >= 1 and int(choice) <= 4) or (pagenum == 2 and (int(choice) >= 1 and int(choice) <= 9)):
                print(f"succesfully starting action number {int(choice)}")
            else:
                print(f"{choice} was not recognised as a valid menu choice")
        except:
            print(f"{choice} was not recognised as a valid menu choice")
    return