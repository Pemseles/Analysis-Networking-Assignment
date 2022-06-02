import menuoptions as mo
import consolemenus as cm
import database as db
import dbclasses as dbc
import encryption as enc
import inputchecks as ic
import logfeatures as lg
from datetime import date

def ChangePassword(target, loggedInUser):
    if loggedInUser.role <= target.role and not loggedInUser.id == target.id:
        # logged in user has same authorization as target and target is not the logged in user themselves
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, f"Unauthorized attempt to change {target.username}'s password", "Password has remained unchanged"))
        return
    cm.LineInTerminal()
    print("New password must be at least 8 and at most 29 characters long & must contain at least 1 lowercase, uppercase, number and special character.")
    print(f"To change {target.first_name} {target.last_name}'s password, please enter the following:\n")

    # inputs
    newPass = input("New password: ")
    newPassRepeat = input("Repeat new password: ")

    # check if password is valid
    if newPass == newPassRepeat and ic.CheckPassword(newPass):
        # encrypt password & add to database; log out of account
        encryptedArr = enc.EncryptTupleOrArray((target.first_name, target.last_name, target.username, newPass, target.address, target.email_address, target.phone_number)) + [target.id]
        db.UpdateUserEntry(loggedInUser, tuple(encryptedArr))
        print("\nPassword changed successfully. Logging out...")
        lg.AppendToLog(lg.BuildLogText(loggedInUser, False, f"Successfully changed {target.username}'s password", "User's new password passed all checks and the database has been updated"))
        return "logout"
    elif newPass != newPassRepeat:
        # password repeat was not the same
        print("\nPassword change failed; did not repeat exact password.")
        lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Failed attempt at changing password", "User did not repeat exact password"))
    else:
        # password did not meet requirements
        print("\nPassword change failed; new password did not meet requirements.")
        lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Failed attempt at changing password", "User's new password did not meet requirements"))
    return

def AddMemberOrUser(loggedInUser, role):
    if loggedInUser.role < 0 or loggedInUser.role > 2:
        # insufficient authorization to perform adding to system; somehow logged in user has impossible role
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt at adding user/member", "User did not have a sufficient role to attempt adding to the system"))
        return 
    addingMember = True
    if ((role == 0 or role == 1) and loggedInUser.role > role):
        # adding user
        addingMember = False
    elif (role == -1 and (loggedInUser.role >= 0 or loggedInUser.role <= 2)):
        # adding member
        addingMember = True
    else:
        # either insufficient authorization to perform or role is somehow wrong
        # LOG: sus
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt at adding user/member", "User did not have a sufficient role to attempt adding to the system"))
        return
    
    # get print statement info
    roleName = "Advisor" if role == 0 else "Member" if role == -1 else "System Admin"
    printReferTo = "an" if role == 0 else "a"
    print(f"\nTo add {printReferTo} {roleName} to the system, please enter the following credentials.\n(If, at any point, you wish to return to the sub-menu, enter 'x' for any input)\n")
    
    # required input-loops
    # check first & last names
    firstName = lastName = ""
    while not ic.CheckFirstOrLastName(firstName) and not ic.CheckFirstOrLastName(lastName):
        firstName = input("First name: ")
        if mo.HandleXInput(firstName): return "sub-menu"
        lastName = input("Last name: ")
        if mo.HandleXInput(lastName): return "sub-menu"
    # 2 user only fields; username & password
    if not addingMember:
        # check username
        username = ""
        while not ic.CheckUsername(username):
            username = input("\nUsername: ")
            if mo.HandleXInput(username): return "sub-menu"
        # check password
        password = ""
        while not ic.CheckPassword(password):
            password = input("\nPassword: ")
            if mo.HandleXInput(password): return "sub-menu"
    # check address
    addressStreet = addressHouseNum = addressZip = addressCity = ""
    while not ic.CheckAddress(addressStreet, addressHouseNum, addressZip, addressCity):
        addressStreet = input("Address part 1/4 (Street name): ")
        if mo.HandleXInput(addressStreet): return "sub-menu"
        addressHouseNum = input("Address part 2/4 (House number): ")
        if mo.HandleXInput(addressHouseNum): return "sub-menu"
        addressZip = input("Address part 3/4 (Zip Code [DDDDXX]): ")
        if mo.HandleXInput(addressZip): return "sub-menu"
        addressCity = input("Address part 4/4 (City; Check list of valid cities in user manual): ")
        if mo.HandleXInput(addressCity): return "sub-menu"
    # check email
    email = ""
    while not ic.CheckEmail(email):
        email = input("Email Address: ")
        if mo.HandleXInput(email): return "sub-menu"
    # check phone
    phone = ""
    while not ic.CheckPhone(phone):
        phone = "+31-6-" + input("Mobile Phone (Must be 8 digits after pre-set value): +31-6-")
        if mo.HandleXInput(phone): return "sub-menu"
    
    # inputs were evaluated as valid
    print(f"\nInputs passed all evaluation, adding {roleName}...")

    # get registrationDate (if adding member), memberId and merge the 4 address-parts together
    registrationDate = date.today().strftime("%d-%m-%y")
    if addingMember:
        memberId = ic.GenerateMemberID()
    address = f"{addressStreet} {addressHouseNum} {addressZip} {addressCity.upper()}"

    # insert new member/user into database & return to same sub-menu (easier to add more than 1)
    if addingMember:
        db.InsertIntoMembersTable(memberId, registrationDate, firstName, lastName, address, email, phone, loggedInUser)
    else:
        db.InsertIntoUsersTable(registrationDate, firstName, lastName, username, password, address, email, phone, role, loggedInUser)

    return "sub-menu"

def PrintUserMemberList(loggedInUser, filter = ""):
    # get list of members/users (depends on logged in user's role)
    listPrint = dbc.BuildUserAndMemberList(loggedInUser, filter)
    # listInstances contain only the members & users
    listInstances = []
    noMembers = False

    if listPrint[0] == "There were no results matching your request.":
        print(listPrint[0])
        print("\nx ) Return to main menu.")
        return listPrint

    # prints entries & adds them to listInstances
    if not isinstance(listPrint[1], dbc.Members):
        noMembers = True
    for entry in listPrint:
        # print members
        if isinstance(entry, dbc.Members):
            print(f"{listPrint.index(entry)} ) {entry.GetInfo(loggedInUser)}")
            listInstances.append(entry)

        # print users (if no users, the option offset is -1 to make sure it doesn't start with 0)
        elif isinstance(entry, dbc.Users) and loggedInUser.role > entry.role:
            if not noMembers:
                print(f"{listPrint.index(entry) - 1} ) {entry.GetProfile(loggedInUser)}")
            elif noMembers:
                print(f"{listPrint.index(entry)} ) {entry.GetProfile(loggedInUser)}")
            listInstances.append(entry)
        else:
            print(entry)
    print("\nx ) Return to main menu.")
    return listInstances

def DeleteUserMember(loggedInUser):
    print("\nPlease select which of the following you want to remove from the system.\n")
    if loggedInUser.role != 1 and loggedInUser.role != 2:
        # logged in user does not have authentication to do this
        # LOG: sus
        return
    # print list of users & members
    delListInstances = PrintUserMemberList(loggedInUser)
    
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
                print("Successfully deleted entry from system.")
                return "sub-menu"
            elif choice == "2":
                # return to sub-menu
                print("Ending process of deletion...")
                return "sub-menu"
            # LOG: not sus
            print(f"{choice} was not recognised as a valid menu choice; ending process of deletion...")
            return mo.InvalidSubMenuChoice("sub-menu", choice, True)
        # LOG: not sus
        return mo.InvalidSubMenuChoice("sub-menu", choice, True)
    except:
        # LOG: not sus
        return mo.InvalidSubMenuChoice("sub-menu", choice, True)

def ModifyInfoList(loggedInUser):
    print("\nPlease select which of the following you would like to modify.\n")
    if loggedInUser.role < 0 or loggedInUser.role > 2:
        # logged in user does not have authentication to do this
        # LOG: sus
        return
    # print list of users & members
    userMemberInstances = PrintUserMemberList(loggedInUser)
    
    choice = input("\nOption choice: ")

    # validate if choice is actually possible
    if choice == "x":
        print("Returning to main menu.")
        return
    try:
        if int(choice) >= 1 and int(choice) <= len(userMemberInstances):
            # get entry and check if it's a member or not
            chosenEntry = userMemberInstances[int(choice) - 1]
            isMember = True if isinstance(chosenEntry, dbc.Members) else False
            return cm.ModifyInfoSubmenu(loggedInUser, chosenEntry, isMember)
        else:
            return mo.InvalidSubMenuChoice("sub-menu", choice, True)
    except Exception as e:
        # LOG: not sus
        print(f"Error occured somewhere in ModifyInfoList(): {e}")
        return mo.InvalidSubMenuChoice("sub-menu", choice, True)

def UpdateInfo(loggedInUser, target, infoPiece, isMember):
    if isMember:
        if loggedInUser.role < 0 or loggedInUser.role > 2:
            # logged in user does not have authentication to do this
            # LOG: sus
            return
    else:
        if (loggedInUser.role < 0 or loggedInUser.role > 2) and loggedInUser.role <= target.role:
            # logged in user does not have authentication to do this
            # LOG: sus
            return

    # handle updating registration date differently; doesn't need to be encrypted or inputted
    if infoPiece == "Registration date":

        # confirmation to modify; continue here & add elsewhere for modification
        print(f"Are you sure you wish to update {target.first_name} {target.last_name}'s {infoPiece} to today's date?\n")
        print("1 ) Yes, proceed with modification.")
        print("2 ) No, end process of modification.")

        choice = input("\nOption choice: ")
        if choice == "1":
            # modify registration date (process is slightly different for members)
            if isMember:
                db.UpdateRegistrationDateMember(loggedInUser, tuple([date.today().strftime("%d-%m-%y"), target.membership_id]))
                print("Member's Registration date modified successfully.")
            else:
                db.UpdateRegistrationDateUser(loggedInUser, tuple([date.today().strftime("%d-%m-%y"), target.id]))
                print("User's Registration date modified successfully.")
        elif choice == "2":
            # return to sub-menu
            print("Ending process of modification...")
            return "sub-menu"
        else:
            print(f"{choice} was not recognised as a valid menu choice; ending process of modification...")
            # LOG: not sus
            return mo.InvalidSubMenuChoice("sub-menu", choice, True)

    print(f"\nTo modify {target.first_name} {target.last_name}'s {infoPiece}, please enter the following:\n(If, at any point, you wish to return to the sub-menu, enter 'x' for any input)\n")

    # input-loop
    print(f"Entering input-loop in UpdateInfo(); checking {infoPiece}")
    newInput = houseNum = zipCode = city = ""
    while not mo.DecideCheckFunction(loggedInUser, infoPiece, newInput, houseNum, zipCode, city):
        if infoPiece == "Phone number":
            newInput = "+31-6-" + input(f"New {infoPiece}: +31-6-")
            if mo.HandleXInput(newInput): return "sub-menu"
        elif infoPiece == "Address":
            newInput = input("Address part 1/4 (Street name): ")
            if mo.HandleXInput(newInput): return "sub-menu"
            houseNum = input("Address part 2/4 (House number): ")
            if mo.HandleXInput(houseNum): return "sub-menu"
            zipCode = input("Address part 3/4 (Zip Code [DDDDXX]): ")
            if mo.HandleXInput(zipCode): return "sub-menu"
            city = input("Address part 4/4 (City; Check list of valid cities in user manual): ")
            if mo.HandleXInput(city): return "sub-menu"
        else:
            newInput = input(f"New {infoPiece}: ")
            if mo.HandleXInput(newInput): return "sub-menu"
    
    # confirmation to modify; continue here & add elsewhere for modification
    print(f"Are you sure you wish to update {target.first_name} {target.last_name}'s {infoPiece}?\n")
    print("1 ) Yes, proceed with modification.")
    print("2 ) No, end process of modification.")

    choice = input("\nOption choice: ")
    if choice == "1":
        # make array; depending on infoPiece, specific index gets changed
        arrayToEncrypt = [target.first_name, target.last_name]
        if not isMember:
            arrayToEncrypt += [target.username, target.password]
        arrayToEncrypt += [target.address, target.email_address, target.phone_number]

        # change info in array
        if infoPiece == "First name":
            arrayToEncrypt[0] = newInput
        elif infoPiece == "Last name":
            arrayToEncrypt[1] = newInput
        elif infoPiece == "Address":
            arrayToEncrypt[arrayToEncrypt.index(target.address)] = f"{newInput} {houseNum} {zipCode} {city.upper()}"
        elif infoPiece == "Email address":
            arrayToEncrypt[arrayToEncrypt.index(target.email_address)] = newInput
        elif infoPiece == "Phone number":
            arrayToEncrypt[arrayToEncrypt.index(target.phone_number)] = newInput
        else:
            # shouldn't be able to reach here, but just in case
            # LOG: sus
            return "Nice Try"

        # update entry (process is slightly different for members)
        if isMember:
            encryptedArr = enc.EncryptTupleOrArray(arrayToEncrypt) + [target.membership_id]
            db.UpdateMemberEntry(loggedInUser, tuple(encryptedArr))
            print(f"Member's {infoPiece} modified successfully.")
            return "sub-menu"
        else:
            encryptedArr = enc.EncryptTupleOrArray(arrayToEncrypt) + [target.id]
            db.UpdateUserEntry(loggedInUser, tuple(encryptedArr))
            print(f"User's {infoPiece} modified successfully.")
            return "sub-menu"
    elif choice == "2":
        # return to sub-menu
        print("Ending process of modification...")
        return "sub-menu"
    else:
        print(f"{choice} was not recognised as a valid menu choice; ending process of modification...")
        # LOG: not sus
        return mo.InvalidSubMenuChoice("sub-menu", choice, True)
    
def ViewLog(loggedInUser):
    if loggedInUser.role != 1 or loggedInUser.role != 2:
        # does not have authentication to view log file
        # LOG: sus
        return

    
    return