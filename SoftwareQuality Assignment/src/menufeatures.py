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
        encryptedArr = enc.EncryptTupleOrArray((target.first_name, target.last_name, target.username, newPass, "", target.address, target.email_address, target.phone_number)) + [target.id]
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
            username = input("Username: ")
            if mo.HandleXInput(username): return "sub-menu"
        # check password
        password = ""
        while not ic.CheckPassword(password):
            password = input("Password: ")
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

    # insert new member/user into database & return to sub-menu
    if addingMember:
        db.InsertIntoMembersTable(memberId, registrationDate, firstName, lastName, address, email, phone, loggedInUser)
        lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Successfully added new member to system", f"New member is {firstName} {lastName}"))
    else:
        db.InsertIntoUsersTable(registrationDate, firstName, lastName, username, password, "", address, email, phone, role, loggedInUser)
        lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Successfully added new user to system", f"New user is {firstName} {lastName}"))

    return "sub-menu"

def PrintUserMemberList(loggedInUser, filter = "", includeMembers = True):
    # get list of members/users (depends on logged in user's role)
    listPrint = dbc.BuildUserAndMemberList(loggedInUser, filter, includeMembers)
    # listInstances contain only the members & users
    listInstances = []
    noMembers = False

    if listPrint[0] == "There were no results matching your request.":
        lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "User's search parameter returned no results", "No members/users in the system matched the parameter"))
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
    lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "User's search parameter returned at least 1 result", "At least 1 member/user in the system matched the parameter"))
    return listInstances

def DeleteUserMember(loggedInUser):
    print("\nPlease select which of the following you want to remove from the system.\n")
    if loggedInUser.role != 1 and loggedInUser.role != 2:
        # logged in user does not have authentication to do this
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to access delete user/member functionality", "User is not a System Administrator or higher"))
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
                lg.AppendToLog(lg.BuildLogText(loggedInUser, False, f"Successfully deleted {chosenEntry.first_name} {chosenEntry.last_name} from the system", "This entry has been removed from the system permanently"))
                db.DeleteFromTable(loggedInUser, chosenEntry)
                print("Successfully deleted entry from system.")
                return "sub-menu"
            elif choice == "2":
                # return to sub-menu
                print("Ending process of deletion...")
                return "sub-menu"
            # invalid menu-option (was a number, just not 1 or 2)
            print(f"{choice} was not recognised as a valid menu choice; ending process of deletion...")
            lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Invalid menu option inputted", "User inputted an invalid option when confirming delete member/user"))
            return mo.InvalidSubMenuChoice("sub-menu", choice, True)
        # invalid menu-option (was not a number)
        lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Invalid menu option inputted", "User inputted an invalid option when confirming delete member/user"))
        return mo.InvalidSubMenuChoice("sub-menu", choice, True)
    except:
        # invalid menu-option (was not a number)
        lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Invalid menu option inputted", "User inputted an invalid option when confirming delete member/user"))
        return mo.InvalidSubMenuChoice("sub-menu", choice, True)

def ModifyInfoList(loggedInUser):
    print("\nPlease select which of the following you would like to modify.\n")
    if loggedInUser.role < 0 or loggedInUser.role > 2:
        # logged in user does not have authentication to do this
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to access modify member/user info", "User is not an Advisor or higher"))
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
            # invalid menu option (less than 1 or higher than len of instances)
            lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Invalid menu option inputted", "User inputted an invalid option when selecting member/user to modify"))
            return mo.InvalidSubMenuChoice("sub-menu", choice, True)
    except Exception as e:
        # still invalid menu option (TODO: remove print statement before submission)
        lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Invalid menu option inputted", "User inputted an invalid option when selecting member/user to modify"))
        print(f"Error occured somewhere in ModifyInfoList(): {e}")
        return mo.InvalidSubMenuChoice("sub-menu", choice, True)

def UpdateInfo(loggedInUser, target, infoPiece, isMember):
    if isMember:
        if loggedInUser.role < 0 or loggedInUser.role > 2:
            # logged in user does not have authentication to do this
            lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to update member info", "User is not an Advisor or higher"))
            return
    else:
        if (loggedInUser.role < 0 or loggedInUser.role > 2) and loggedInUser.role <= target.role:
            # logged in user does not have authentication to do this
            lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to update user info", "User is not a System Administrator or higher"))
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
            newDate = date.today().strftime("%d-%m-%y")
            if isMember:
                lg.AppendToLog(lg.BuildLogText(loggedInUser, False, f"{target.first_name} {target.last_name}'s registration date was successfully updated", f"Registration date was changed from {target.registration_date} to {newDate}"))
                db.UpdateRegistrationDateMember(loggedInUser, tuple([newDate, target.membership_id]))
                print("Member's Registration date modified successfully.")
            else:
                lg.AppendToLog(lg.BuildLogText(loggedInUser, False, f"{target.first_name} {target.last_name}'s registration date was successfully updated", f"Registration date was changed from {target.registration_date} to {newDate}"))
                db.UpdateRegistrationDateUser(loggedInUser, tuple([newDate, target.id]))
                print("User's Registration date modified successfully.")
        elif choice == "2":
            # return to sub-menu
            print("Ending process of modification...")
            return "sub-menu"
        else:
            # invalid menu choice
            print(f"{choice} was not recognised as a valid menu choice; ending process of modification...")
            lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Invalid menu option inputted", "User inputted an invalid option when confirming to modify member/user's registration date"))
            return mo.InvalidSubMenuChoice("sub-menu", choice, True)

    # handle the other infoPieces
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
            arrayToEncrypt += [target.username, target.password, ""]
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
            lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "User tried changing info using an unknown info piece", f"Denied action; user tried to change {target.first_name} {target.last_name}'s {infoPiece}"))
            return "Nice Try"

        # update entry (process is slightly different for members)
        if isMember:
            lg.AppendToLog(lg.BuildLogText(loggedInUser, False, f"Successfully updated member's {infoPiece}", f"{target.first_name} {target.last_name}'s {infoPiece} has been modified"))
            encryptedArr = enc.EncryptTupleOrArray(arrayToEncrypt) + [target.membership_id]
            db.UpdateMemberEntry(loggedInUser, tuple(encryptedArr))
            print(f"Member's {infoPiece} modified successfully.")
            return "sub-menu"
        else:
            lg.AppendToLog(lg.BuildLogText(loggedInUser, False, f"Successfully updated user's {infoPiece}", f"{target.username}'s {infoPiece} has been modified"))
            encryptedArr = enc.EncryptTupleOrArray(arrayToEncrypt) + [target.id]
            db.UpdateUserEntry(loggedInUser, tuple(encryptedArr))
            print(f"User's {infoPiece} modified successfully.")
            return "sub-menu"
    elif choice == "2":
        # return to sub-menu
        print("Ending process of modification...")
        return "sub-menu"
    else:
        # invalid menu option (was not 1 or 2)
        print(f"{choice} was not recognised as a valid menu choice; ending process of modification...")
        lg.AppendToLog(lg.BuildLogText(loggedInUser, False, "Invalid menu option inputted", f"User inputted an invalid option when confirming to modify member/user's {infoPiece}"))
        return mo.InvalidSubMenuChoice("sub-menu", choice, True)
    
def ViewLog(loggedInUser):
    # check auth
    if loggedInUser.role != 1 and loggedInUser.role != 2:
        # does not have authentication to view log file
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to view system's log file", "User is not a System Administrator or higher"))
        return

    # get log file contents & decrypt
    logArr = enc.DecryptTupleOrArray(lg.ReadLog(loggedInUser))

    # print logArr individually
    for i in logArr:
        print(i)
    return

def ResetPassword(loggedInUser, targetUser):
    # check auth
    if loggedInUser.role != 1 and loggedInUser.role != 2:
        # does not have authentication to view log file
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized attempt to reset another user's password", "User is not a System Administrator or higher"))
        return

    # ask for input loop
    tempPass = ""
    while not ic.CheckPassword(tempPass):
        tempPass = input("\nTemporary password: ")
        if mo.HandleXInput(tempPass): return "sub-menu"

    # input was evaluated as valid; get array of updated entry
    lg.AppendToLog(lg.BuildLogText(loggedInUser, False, f"Successfully reset user's password", f"{targetUser.username}'s password has been temporarily set to {tempPass}"))
    updateArr = enc.EncryptTupleOrArray([targetUser.first_name, targetUser.last_name, targetUser.username, targetUser.password, tempPass, targetUser.address, targetUser.email_address, targetUser.phone_number]) + [targetUser.id]
    db.UpdateUserEntry(loggedInUser, updateArr)
    print(f"User's password has been temporarily reset to {tempPass} (turns back to normal upon them logging in)")
    return