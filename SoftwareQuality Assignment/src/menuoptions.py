from socket import create_server
import consolemenus as cm
import encryption as enc
import database as db
import inputchecks as ic
import dbclasses as dbc
from datetime import date
import inspect

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
        return ChangePassword(loggedInUser, loggedInUser)
    # add members/users to system
    elif option == 2:
        return cm.AddToSystemSubmenu(loggedInUser)
    # change existing member's/user's info
    elif option == 3:
        print("implement change member/user info (3 options, depends on authorization lvl)")
    # search through/view list of members/users
    elif option == 4:
        print("implement search through/view list of members/users (2 options, depends on authorization lvl)")
    # delete existing member/user
    elif option == 5:
        return DeleteUserMember(loggedInUser)
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
        return AddMemberOrUser(loggedInUser, -1)
    elif (option == "2" and loggedInUser.role > 0) or (option == "3" and loggedInUser.role > 1):
        # proceed to add user
        return AddMemberOrUser(loggedInUser, int(option) - 2)
    else:
        # if anything else is inputted
        print(f"{option} was not recognised as a valid menu choice.")
        return cm.AddToSystemSubmenu(loggedInUser)
    return

def ChangePassword(target, loggedInUser):
    if loggedInUser.role <= target.role and not loggedInUser.id == target.id:
        # logged in user has same authorization as target and target is not the logged in user themselves
        return
    cm.LineInTerminal()
    print("New password must be at least 8 and at most 29 characters long & must contain at least 1 lowercase, uppercase, number and special character.")
    print(f"To change {target.first_name} {target.last_name}'s password, please enter the following:\n(If, at any point, you wish to return to the sub-menu, enter 'x' for any input)\n")

    # inputs
    newPass = input("New password: ")
    newPassRepeat = input("Repeat new password: ")

    # check if password is valid
    if newPass == newPassRepeat and ic.CheckPassword(newPass):
        # encrypt password & add to database; log out of account
        encryptedArr = enc.EncryptTupleOrArray((target.first_name, target.last_name, target.username, newPass, target.address, target.email_address, target.phone_number)) + [target.id]
        db.UpdateUserEntry(tuple(encryptedArr))
        print("\nPassword changed successfully. Logging out...")
        return "logout"
    elif newPass != newPassRepeat:
        # password repeat was not the same
        print("\nPassword change failed; did not repeat exact password.")
    else:
        # password did not meet requirements
        print("\nPassword change failed; new password did not meet requirements.")
    return

def AddMemberOrUser(loggedInUser, role):
    if loggedInUser.role < 0 or loggedInUser.role > 2:
        # insufficient authorization to perform adding to system; somehow logged in user has impossible role
        # LOG: sus
        return 
    addingMember = True
    if ((role == 0 or role == 1) and loggedInUser.role > role):
        # adding user
        print("adding user")
        addingMember = False
    elif (role == -1 and (loggedInUser.role >= 0 or loggedInUser.role <= 2)):
        # adding member
        print("adding member")
        addingMember = True
    else:
        # either insufficient authorization to perform or role is somehow wrong
        # LOG: sus
        return
    
    # get print statement info
    roleName = "Advisor" if role == 0 else "Member" if role == -1 else "System Admin"
    printReferTo = "an" if role == 0 else "a"
    print(f"\nTo add {printReferTo} {roleName} to the system, please enter the following credentials.\n(If, at any point, you wish to return to the sub-menu, enter 'x' for any input)\n")
    
    # required input-loops
    # check first & last names
    firstName = lastName = ""
    while not ic.CheckFirstAndLastName(firstName, lastName):
        firstName = input("First name: ")
        if HandleXInput(firstName): return "sub-menu"
        lastName = input("Last name: ")
        if HandleXInput(lastName): return "sub-menu"
    # 2 user only fields; username & password
    if not addingMember:
        # check username
        username = ""
        while not ic.CheckUsername(username):
            username = input("\nUsername: ")
            if HandleXInput(username): return "sub-menu"
        # check password
        password = ""
        while not ic.CheckPassword(password):
            password = input("\nPassword: ")
            if HandleXInput(password): return "sub-menu"
    # check address
    addressStreet = addressHouseNum = addressZip = addressCity = ""
    while not ic.CheckAddress(addressStreet, addressHouseNum, addressZip, addressCity):
        addressStreet = input("Address part 1/4 (Street name): ")
        if HandleXInput(addressStreet): return "sub-menu"
        addressHouseNum = input("Address part 2/4 (House number): ")
        if HandleXInput(addressHouseNum): return "sub-menu"
        addressZip = input("Address part 3/4 (Zip Code [DDDDXX]): ")
        if HandleXInput(addressZip): return "sub-menu"
        addressCity = input("Address part 4/4 (City; Check list of valid cities in user manual): ")
        if HandleXInput(addressCity): return "sub-menu"
    # check email
    email = ""
    while not ic.CheckEmail(email):
        email = input("Email Address: ")
        if HandleXInput(email): return "sub-menu"
    # check phone
    phone = ""
    while not ic.CheckPhone(phone):
        phone = "+31-6-" + input("Mobile Phone (Must be 8 digits after pre-set value): +31-6-")
        if HandleXInput(phone): return "sub-menu"
    
    # inputs were evaluated as valid
    print(f"\nInputs passed all evaluation, adding {roleName}...")

    # get registrationDate (if adding member), memberId and merge the 4 address-parts together
    registrationDate = date.today().strftime("%d-%m-%y")
    if addingMember:
        memberId = ic.GenerateMemberID()
    address = f"{addressStreet} {addressHouseNum} {addressZip} {addressCity.upper()}"

    # insert new member/user into database & return to same sub-menu (easier to add more than 1)
    if addingMember:
        db.InsertIntoMembersTable(memberId, registrationDate, firstName, lastName, address, email, phone)
    else:
        db.InsertIntoUsersTable(registrationDate, firstName, lastName, username, password, address, email, phone, role)

    return "sub-menu"

def DeleteUserMember(loggedInUser):
    print("\nPlease select which of the following you want to remove from the system.\n")
    if loggedInUser.role != 1 and loggedInUser.role != 2:
        # logged in user does not have authentication to do this
        return
    # get list of deletable members/users (depends on logged in user's role)
    delListPrint = dbc.BuildDeleteList(loggedInUser)
    # delListInstances is here to easily get the chosen thing to delete
    delListInstances = []
    noMembers = False
    # print entries & add them to delListInstances
    if not isinstance(delListPrint[1], dbc.Members):
        noMembers = True
    for entry in delListPrint:
        # print members
        if isinstance(entry, dbc.Members):
            print(f"{delListPrint.index(entry)} ) {entry.GetInfo(loggedInUser)}")
            delListInstances.append(entry)
        # print users (if no users, the option offset is -1 to make sure it doesn't start with 0)
        elif isinstance(entry, dbc.Users):
            if not noMembers:
                print(f"{delListPrint.index(entry) - 1} ) {entry.GetProfile(loggedInUser)}")
            elif noMembers:
                print(f"{delListPrint.index(entry)} ) {entry.GetProfile(loggedInUser)}")
            delListInstances.append(entry)
        else:
            print(entry)
    print("\nx ) Return to main menu.")
    
    choice = input("\nOption choice: ")

    # validate if choice is actually possible
    if choice == "x":
        print("Returning to main menu.")
        return
    try:
        if int(choice) >= 1 and int(choice) <= len(delListInstances):
            # get entry and check if it's a member or not
            chosenEntry = delListInstances[int(choice) - 1]
            isMember = True if isinstance(chosenEntry, dbc.Members) else False
            
            # confirmation to delete
            if isMember:
                print(f"Are you sure you wish to delete\n{chosenEntry.GetInfo(loggedInUser)}\n")
            else:
                print(f"Are you sure you wish to delete\n{chosenEntry.GetProfile(loggedInUser)}\n")
            print("1 ) Yes, proceed with deletion.")
            print("2 ) No, end process of deletion.")

            choice = input("\nOption choice: ")
            if choice == "1":
                # delete chosen entry
                db.DeleteFromTable(loggedInUser, chosenEntry)
                return "sub-menu"
            elif choice == "2":
                # return to sub-menu
                return "sub-menu"
            # LOG: not sus
            return InvalidSubMenuChoice("sub-menu", choice, True)
        # LOG: not sus
        return InvalidSubMenuChoice("sub-menu", choice, True)
    except:
        # LOG: not sus
        return InvalidSubMenuChoice("sub-menu", choice, True)