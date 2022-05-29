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
        middleDigits = ""
        memberId = int(str(random.randrange(1, 10)))
        while len(middleDigits) < 8:
            middleDigits += str(random.randrange(0, 10))
        memberId = int(str(memberId) + middleDigits)
        digitSum = 0
        for digit in str(memberId):
            digitSum += int(digit)
        memberId = int(str(memberId) + str(digitSum % 10))
        if not memberId in alreadyUsed:
            return memberId
        tries -= 1
    return None

def CheckPassword(password):
    # check length of password
    if len(password) >= 8 and len(password) < 30:
        # check if password contains lowercase, uppercase, digit & special character
        lowerPresent = any(x in password for x in enc.AlphabetExtended(26, 97))
        upperPresent = any(x in password for x in enc.AlphabetExtended(26, 65))
        digitPresent = any(x in password for x in enc.AlphabetExtended(10, 48))
        specialPresent = any(x in password for x in (enc.AlphabetExtended(15, 33) + enc.AlphabetExtended(7, 58) + enc.AlphabetExtended(6, 91) + enc.AlphabetExtended(4, 123)))
        if lowerPresent and upperPresent and digitPresent and specialPresent:
            return True
    return False

def CheckUsername(username):
    # check if unique, at least 6 and at most 9 characters
    # check if starts w a letter
    # should only contain [a-z0-9_'.] with no difference between upper & lowercase
    return False

def CheckAddress(address):
    
    return False

def CheckEmail(email):

    return False

def CheckPhone(phone):

    return False

def MainMenuPageShortcut(pagenum, loggedInUser):
    # literally just here so i don't have to write this code 2x
    if pagenum == 1:
        return cm.MainMenu(loggedInUser)
    else:
        return cm.MainMenuPage2(loggedInUser)

def HandleSystemScreenOption(choice):
    # will keep user in the loop until they input 1 or 2, 1 brings them to loginscreen, 2 terminates program
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
        return cm.MainMenuPage2(loggedInUser)
    elif choice == "p" and pagenum == 2:
        print("\nNavigating to page 1.")
        return cm.MainMenu(loggedInUser)
    else:
        try:
            if pagenum == 1 and (int(choice) >= 1 and int(choice) <= 4) or (pagenum == 2 and (int(choice) >= 1 and int(choice) <= 5)):
                if pagenum == 2:
                    choice = str(int(choice) + 4)
                result = HandleMenuOptions(int(choice), loggedInUser)
                if result == "logout":
                    return
            else:
                print(f"{choice} was not recognised as a valid menu choice")
        except:
            print(f"{choice} was not recognised as a valid menu choice")
    return MainMenuPageShortcut(pagenum, loggedInUser)

def HandleMenuOptions(option, loggedInUser):
    # change password of loggedInUser
    if option == 1:
        return ChangePassword(loggedInUser)
    # add members/users to system
    elif option == 2:
        print("implement add members/users to system (3 options, depends on authorization lvl)")
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
    if option == "x":
        # return to page 1
        print("\nReturning to main page...")
    elif option == "1":
        # proceed to add member
        return AddMember()
    elif (option == "2" and loggedInUser.role > 0) or (option == "3" and loggedInUser.role > 1):
        # proceed to add user
        AddUser(int(option) - 2)
        return cm.AddToSystemSubmenu(loggedInUser)
    else:
        # if anything else is inputted
        print(f"{choice} was not recognised as a valid menu choice")
        return cm.AddToSystemSubmenu(loggedInUser)
    return cm.MainMenu(loggedInUser)

def ChangePassword(target):
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

def AddMember():
    print("\nTo add a member to the system, please enter the following credentials.\n")

    firstName = input("First name: ")
    lastName = input("Last name: ")
    addressp1 = input("Address (Format = ([Street name] [House number] [Zip code])): ")
    # valid cities are: (Amsterdam, Rotterdam, Den Haag, Leiden, Groningen, Utrecht, Middelburg, Dordrecht, Assen, Arnhem)
    addressp2 = input("City (Check list of valid cities in user manual): ")
    email = input("Email Address: ")
    phone = "+31-6-" + input("Mobile Phone (Must be 8 digits after pre-set value): +31-6-")

    registrationDate = date.today().strftime("%d-%m-%y")
    memberId = GenerateMemberID()
    fullAddress = f"{addressp1} {addressp2}"

    # check if address, email & phone is valid before adding to db


def AddUser(role):
    print("")