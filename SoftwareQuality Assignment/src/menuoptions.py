import consolemenus as cm
import encryption as enc
import database as db
from datetime import date
import random

def GenerateMemberID():
    # get already used membership IDs
    alreadyUsedRaw = db.SelectColumnFromTable("Members", "membership_id")
    alreadyUsed = enc.DecryptTupleOrArray(db.ConvertFetchToArray(alreadyUsedRaw))
    
    # will prob never reach this, but better be safe than stuck in a while loop
    tries = 10000
    while tries > 0:
        # generates first digit (1-9)
        middleDigits = ""
        memberId = random.randrange(1, 10)
        while len(middleDigits) < 8:
            # appends until 9 digits long (0-9)
            middleDigits += str(random.randrange(0, 10))
        memberId = int(str(memberId) + middleDigits)
        # calculates checksum digit and appends it
        digitSum = 0
        for digit in str(memberId):
            digitSum += int(digit)
        memberId = int(str(memberId) + str(digitSum % 10))
        # checks if already used by other member, if true; invalid and retry from beginning
        if not memberId in alreadyUsed:
            return memberId
        tries -= 1
    return None

def CheckPassword(password):
    alreadyUsedRaw = db.SelectColumnFromTable("Users", "password")
    alreadyUsed = enc.DecryptTupleOrArray(db.ConvertFetchToArray(alreadyUsedRaw))

    # check length of password
    if len(password) >= 8 and len(password) < 30:
        # check if password contains lowercase, uppercase, digit & special character
        lowerPresent = any(x in password for x in enc.AlphabetExtended(26, 97))
        upperPresent = any(x in password for x in enc.AlphabetExtended(26, 65))
        digitPresent = any(x in password for x in enc.AlphabetExtended(10, 48))
        specialPresent = any(x in password for x in (enc.AlphabetExtended(15, 33) + enc.AlphabetExtended(7, 58) + enc.AlphabetExtended(6, 91) + enc.AlphabetExtended(4, 123)))
        if lowerPresent and upperPresent and digitPresent and specialPresent and not password in alreadyUsed:
            print("Password is valid")
            return True
    return False

def CheckUsername(username):
    alreadyUsedRaw = db.SelectColumnFromTable("Users", "username")
    alreadyUsed = enc.DecryptTupleOrArray(db.ConvertFetchToArray(alreadyUsedRaw))

    # checks username length
    if len(username) >= 6 and len(username) < 10 and not username in alreadyUsed:
        letterStart = username[0] in (enc.AlphabetExtended(26, 97) + enc.AlphabetExtended(26, 65))
        for letter in username:
            if not letter in (enc.AlphabetExtended(26, 97) + enc.AlphabetExtended(26, 65) + enc.AlphabetExtended(10, 48) + ["_", "'", ".", "-"]) or not letterStart:
                # username contains invalid character; invalid
                return False
        # username was evaluated as valid as it passed every check
        print("Username is valid")
        return True
    return False

def CheckFirstAndLastName(firstName, lastName):
    # checks if first & last names are actually something real, like a real name
    if len(firstName) > 0 and len(lastName) > 0:
        for letter in firstName:
            if not letter in (enc.AlphabetExtended(26, 97) + enc.AlphabetExtended(26, 65) + ["'", "-", "."]):
                # firstname contains invalid character; invalid
                return False
        for letter in lastName:
            if not letter in (enc.AlphabetExtended(26, 97) + enc.AlphabetExtended(26, 65) + ["'", "-", "."]):
                # lastname contains invalid character; invalid
                return False
        # first & last names were evaluated as valid as they passed every check
        print("First & Last names are valid")
        return True
    return False

def CheckAddress(street, houseNum, zipCode, city):
    # checks address
    validCities = ["amsterdam", "rotterdam", "den haag", "leiden", "groningen", "utrecht", "middelburg", "dordrecht", "assen", "arnhem"]

    # street name & street number can be anything really, just not nothing (also streetnum must contain a number)
    if street == "" or street == None or houseNum == "" or houseNum == None or not any(x in houseNum for x in enc.AlphabetExtended(10, 48)):
        return False

    # check if city = valid & evaluate zip code
    if city.lower() in validCities and len(zipCode) == 6:
        # check zip code
        zipFirst4 = zipCode[0:4]
        for digit in zipFirst4:
            if not digit in enc.AlphabetExtended(10, 48):
                # first 4 characters contained non-numbers; invalid
                return False
        zipLast2 = zipCode[4:]
        for letter in zipLast2:
            if not letter in enc.AlphabetExtended(26, 65):
                # last 2 characters contained non-uppercase letters; invalid
                return False
        # zip code evaluated valid
        print("Address is valid")
        return True
    return False

def CheckEmail(email):
    alreadyUsedRaw = db.SelectColumnFromTable("Members", "email_address")
    alreadyUsed = enc.DecryptTupleOrArray(db.ConvertFetchToArray(alreadyUsedRaw))
    alreadyUsedRaw = db.SelectColumnFromTable("Users", "email_address")
    alreadyUsed += enc.DecryptTupleOrArray(db.ConvertFetchToArray(alreadyUsedRaw))

    # build email prefix & domain seperately
    emailPrefix = ""
    emailDomain = ""
    appendPrefix = True
    atCount = 0
    for char in email:
        if char == "@":
            appendPrefix = False
            atCount += 1
            if atCount > 1:
                # email cannot contain more than 1 '@'; invalid
                return False
            continue
        if appendPrefix:
            emailPrefix += char.lower()
        else:
            emailDomain += char.lower()
    # check duplicate as email must be unique
    if f"{emailPrefix}@{emailDomain}" in alreadyUsed:
        return False

    # evaluating email prefix
    prefixSpecChars = ["_", ".", "-"]
    if not emailPrefix[len(emailPrefix) - 1:] in prefixSpecChars and not emailPrefix[0] in prefixSpecChars:
        for char in emailPrefix:
            if not char in (enc.AlphabetExtended(26, 97) + enc.AlphabetExtended(10, 48) + prefixSpecChars):
                # prefix contains invalid character; invalid
                return False
        specCharMap = ([pos for pos, char in enumerate(emailPrefix) if char in prefixSpecChars])
        if len(specCharMap) > 0:
            for pos in specCharMap:
                if emailPrefix[pos + 1] in prefixSpecChars:
                    # 2+ special characters are next to eachother; invalid
                    return False
    else:
        # started/ended with a special character; invalid
        return False
    
    # email prefix evaluated as valid
    # evaluating email domain
    domainSpecChars = [".", "-"]
    if not emailDomain[len(emailDomain) - 1:] in domainSpecChars and not emailDomain[0] in domainSpecChars:
        for char in emailDomain:
            if not char in (enc.AlphabetExtended(26, 97) + enc.AlphabetExtended(10, 48) + domainSpecChars):
                # domain contains invalid character; invalid
                return False
        specCharMap = ([pos for pos, char in enumerate(emailDomain) if char in domainSpecChars])
        if len(specCharMap) > 0:
            for pos in specCharMap:
                if emailDomain[pos + 1] in domainSpecChars:
                    # 2+ special characters are next to eachother; invalid
                    return False
            if not specCharMap[-1] + 2 < len(emailDomain):
                # not enough characters after the '.'; invalid
                return False
        else:
            # did not contain a '.'; invalid
            return False
        # domain + prefix is valid as it passed each check
        print("Email is valid")
        return True
    else:
        # started/ended with a special character; invalid
        return False
    return False

def CheckPhone(phone):
    # check phone number
    if len(phone) == 14 and phone[0:6] == "+31-6-":
        phoneDigits = phone[6:]
        for digit in phoneDigits:
            if not digit in enc.AlphabetExtended(10, 48):
                # phone number after '+31-6-' contains non-number character; invalid
                return False
        # phone number is valid as it passed each check
        print("Phone number is valid")
        return True
    return False

def MainMenuPageShortcut(pagenum, loggedInUser):
    # literally just here so i don't have to write this code 2x
    if pagenum == 1:
        return cm.MainMenu(loggedInUser)
    else:
        return cm.MainMenuPage2(loggedInUser)

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
    elif choice == "n" and pagenum == 1:
        print("\nNavigating to page 2.")
        return cm.MainMenuPage2(loggedInUser)
    elif choice == "p" and pagenum == 2:
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
                if result == "logout":
                    return
            else:
                print(f"{choice} was not recognised as a valid menu choice")
        else:
            print(f"{choice} was not recognised as a valid menu choice")
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

def HandleMenuOptionsAdd(option, loggedInUser):
    # handles options of sub-menu of adding member/user to system
    print(f"Inside HandleMenuOptionsAdd; choice = {option}, user = {loggedInUser.username}")

    if option == "x":
        # return to page 1
        print("\nReturning to main page...")
    elif option == "1":
        # proceed to add member
        return AddMember(loggedInUser)
    elif (option == "2" and loggedInUser.role > 0) or (option == "3" and loggedInUser.role > 1):
        # proceed to add user
        return AddUser(int(option) - 2)
    else:
        # if anything else is inputted
        print(f"{option} was not recognised as a valid menu choice")
        return cm.AddToSystemSubmenu(loggedInUser)
    return

def ChangePassword(target, loggedInUser):
    if loggedInUser.role <= target.role and not loggedInUser.id == target.id:
        return
    # change password menu
    cm.LineInTerminal()
    print("New password must be at least 8 and at most 29 characters long & must contain at least 1 lowercase, uppercase, number and special character.")
    print(f"To change {target.first_name} {target.last_name}'s password, please enter the following:\n")

    newPass = input("New password: ")
    newPassRepeat = input("Repeat new password: ")

    if newPass == newPassRepeat and CheckPassword(newPass):
        encryptedArr = enc.EncryptTupleOrArray((target.first_name, target.last_name, target.username, newPass, target.address, target.email_address, target.phone_number)) + [target.id]
        db.UpdateUserEntry(tuple(encryptedArr))
        print("\nPassword changed successfully. Logging out...")
        return "logout"
    elif newPass != newPassRepeat:
        print("\nPassword change failed; did not repeat exact password.")
    else:
        print("\nPassword change failed; new password did not meet requirements.")
    return

def AddMember(loggedInUser):
    if loggedInUser.role < 0 and loggedInUser.role > 3:
        return
    print("\nTo add a member to the system, please enter the following credentials.\n")

    firstName = input("First name: ")
    lastName = input("Last name: ")
    addressStreet = input("Address part 1/4 (Street name): ")
    addressHouseNum = input("Address part 2/4 (House number): ")
    addressZip = input("Address part 3/4 (Zip Code [DDDDXX]): ")
    addressCity = input("Address part 4/4 (City; Check list of valid cities in user manual): ")
    email = input("Email Address: ")
    phone = "+31-6-" + input("Mobile Phone (Must be 8 digits after pre-set value): +31-6-")

    if CheckFirstAndLastName(firstName, lastName) and CheckAddress(addressStreet, addressHouseNum, addressZip, addressCity) and CheckEmail(email) and CheckPhone(phone):
        print("Inputs passed all evaluation, adding Member...")
        registrationDate = date.today().strftime("%d-%m-%y")
        memberId = GenerateMemberID()
        address = f"{addressStreet} {addressHouseNum} {addressZip} {addressCity.upper()}"

        db.InsertIntoMembersTable(memberId, registrationDate, firstName, lastName, address, email, phone)
        return "sub-menu"
    else:
        print("At least one input was invalid, please try again.")
        return
    
def AddUser(role):
    if role != 0 and role != 1:
        return
    roleName = "Advisor" if role == 0 else "System Admin"
    printReferTo = "an" if role == 0 else "a"
    print(f"\nTo add {printReferTo} {roleName} to the system, please enter the following credentials.\n")
    
    firstName = input("First name: ")
    lastName = input("Last name: ")
    username = input("Username: ")
    password = input("Password: ")
    addressStreet = input("Address part 1/4 (Street name): ")
    addressHouseNum = input("Address part 2/4 (House number): ")
    addressZip = input("Address part 3/4 (Zip Code [DDDDXX]): ")
    addressCity = input("Address part 4/4 (City; Check list of valid cities in user manual): ")
    email = input("Email Address: ")
    phone = "+31-6-" + input("Mobile Phone (Must be 8 digits after pre-set value): +31-6-")

    if CheckFirstAndLastName(firstName, lastName) and CheckAddress(addressStreet, addressHouseNum, addressZip, addressCity) and CheckEmail(email) and CheckPhone(phone) and CheckPassword(password) and CheckUsername(username):
        print(f"inputs passed all evaluation, adding {roleName}...")
        registrationDate = date.today().strftime("%d-%m-%y")
        memberId = GenerateMemberID()
        address = f"{addressStreet} {addressHouseNum} {addressZip} {addressCity.upper()}"

        db.InsertIntoUsersTable(registrationDate, firstName, lastName, username, password, address, email, phone, role)
        return "sub-menu"
    else:
        print("At least one input was invalid, please try again.")
        return