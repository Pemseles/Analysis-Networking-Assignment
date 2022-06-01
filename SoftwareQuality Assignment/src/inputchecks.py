import database as db
import encryption as enc

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

    # check if password is not ""; is for adding to system
    if password == "":
        return False

    # check length of password
    if len(password) >= 8 and len(password) < 30:
        # check if password contains lowercase, uppercase, digit & special character
        lowerPresent = any(x in password for x in enc.AlphabetExtended(26, 97))
        upperPresent = any(x in password for x in enc.AlphabetExtended(26, 65))
        digitPresent = any(x in password for x in enc.AlphabetExtended(10, 48))
        specialPresent = any(x in password for x in (enc.AlphabetExtended(15, 33) + enc.AlphabetExtended(7, 58) + enc.AlphabetExtended(6, 91) + enc.AlphabetExtended(4, 123)))
        if lowerPresent and upperPresent and digitPresent and specialPresent and not password in alreadyUsed:
            print("Password is valid.\n")
            return True
        else:
            print("Password does not contain minimum required characters (1 lowercase, 1 uppercase, 1 digit, 1 special character) or is already taken by another user.\n")
            # LOG: not sus
            return False
    print("Password is of insufficient length.\n")
    # LOG: not sus
    return False

def CheckUsername(username):
    alreadyUsedRaw = db.SelectColumnFromTable("Users", "username")
    alreadyUsed = enc.DecryptTupleOrArray(db.ConvertFetchToArray(alreadyUsedRaw))

    # check if password is not ""; is for adding to system
    if username == "":
        return False

    # checks username length
    if len(username) >= 6 and len(username) < 10 and not username in alreadyUsed:
        letterStart = username[0] in (enc.AlphabetExtended(26, 97) + enc.AlphabetExtended(26, 65))
        for letter in username:
            if not letter in (enc.AlphabetExtended(26, 97) + enc.AlphabetExtended(26, 65) + enc.AlphabetExtended(10, 48) + ["_", "'", ".", "-"]) or not letterStart:
                # username contains invalid character; invalid
                print("Username contains invalid character.\n")
                # LOG: not sus
                return False
        # username was evaluated as valid as it passed every check
        print("Username is valid.\n")
        return True
    print("Username is of insufficient length or is already taken by another user.\n")
    # LOG: not sus
    return False

def CheckFirstAndLastName(firstName, lastName):
    # check if firstName or lastName is not ""; is for adding to system
    if firstName == "" or lastName == "":
        return False

    # checks if first & last names are actually something real, like a real name
    if len(firstName) > 0 and len(lastName) > 0:
        for letter in firstName:
            if not letter in (enc.AlphabetExtended(26, 97) + enc.AlphabetExtended(26, 65) + ["'", "-", "."]):
                # firstname contains invalid character; invalid
                print("First name contains invalid character.\n")
                # LOG: not sus
                return False
        for letter in lastName:
            if not letter in (enc.AlphabetExtended(26, 97) + enc.AlphabetExtended(26, 65) + ["'", "-", "."]):
                # lastname contains invalid character; invalid
                print("Last name contains invalid character.\n")
                # LOG: not sus
                return False
        # first & last names were evaluated as valid as they passed every check
        print("First & Last names are valid.\n")
        return True
    print("First or Last name are of invalid length.\n")
    # LOG: not sus
    return False

def CheckAddress(street, houseNum, zipCode, city):
    # checks address
    validCities = ["amsterdam", "rotterdam", "den haag", "leiden", "groningen", "utrecht", "middelburg", "dordrecht", "assen", "arnhem"]

    # check if any of the 4 address components is not ""; is for adding to system
    if street == "" or houseNum == "" or zipCode == "" or city == "":
        return False

    # street name & street number can be anything really, just not nothing (also streetnum must contain a number)
    if street == "" or street == None or houseNum == "" or houseNum == None or not any(x in houseNum for x in enc.AlphabetExtended(10, 48)):
        print("Street name or House number is invalid.\n")
        # LOG: not sus
        return False

    # check if city = valid & evaluate zip code
    if city.lower() in validCities and len(zipCode) == 6:
        # check zip code
        zipFirst4 = zipCode[0:4]
        for digit in zipFirst4:
            if not digit in enc.AlphabetExtended(10, 48):
                # first 4 characters contained non-numbers; invalid
                print("First 4 characters of Zip code contain invalid characters.\n")
                # LOG: not sus
                return False
        zipLast2 = zipCode[4:]
        for letter in zipLast2:
            if not letter in enc.AlphabetExtended(26, 65):
                # last 2 characters contained non-uppercase letters; invalid
                print("Last 2 characters of Zip code contain invalid characters.\n")
                # LOG: not sus
                return False
        # zip code evaluated valid
        print("Address is valid\n")
        return True
    print("City or Zip code is invalid.\n")
    # LOG: not sus
    return False

def CheckEmail(email):
    # check if email is not ""; is for adding to system
    if email == "":
        return False

    if not len(email) > 1:
        print("Email is of insufficient length.\n")
        # LOG: not sus
        return False
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
                print("Email contains more than 1 '@' character.\n")
                # LOG: not sus
                return False
            continue
        if appendPrefix:
            emailPrefix += char.lower()
        else:
            emailDomain += char.lower()
    # check duplicate as email must be unique
    if f"{emailPrefix}@{emailDomain}" in alreadyUsed:
        print("Email is already taken by another member/user.\n")
        # LOG: not sus
        return False

    # evaluating email prefix
    prefixSpecChars = ["_", ".", "-"]
    if not emailPrefix[len(emailPrefix) - 1:] in prefixSpecChars and not emailPrefix[0] in prefixSpecChars:
        for char in emailPrefix:
            if not char in (enc.AlphabetExtended(26, 97) + enc.AlphabetExtended(10, 48) + prefixSpecChars):
                # prefix contains invalid character; invalid
                print("Email prefix contains invalid character.\n")
                # LOG: not sus
                return False
        specCharMap = ([pos for pos, char in enumerate(emailPrefix) if char in prefixSpecChars])
        if len(specCharMap) > 0:
            for pos in specCharMap:
                if emailPrefix[pos + 1] in prefixSpecChars:
                    # 2+ special characters are next to eachother; invalid
                    print("Email prefix contains 2 adjacent special characters.\n")
                    # LOG: not sus
                    return False
    else:
        # started/ended with a special character; invalid
        print("Email prefix's first/last character is a special character.\n")
        # LOG: not sus
        return False
    
    # email prefix evaluated as valid
    # evaluating email domain
    domainSpecChars = [".", "-"]
    if not emailDomain[len(emailDomain) - 1:] in domainSpecChars and not emailDomain[0] in domainSpecChars:
        for char in emailDomain:
            if not char in (enc.AlphabetExtended(26, 97) + enc.AlphabetExtended(10, 48) + domainSpecChars):
                # domain contains invalid character; invalid
                print("Email domain contains invalid character.\n")
                # LOG: not sus
                return False
        specCharMap = ([pos for pos, char in enumerate(emailDomain) if char in domainSpecChars])
        if len(specCharMap) > 0:
            for pos in specCharMap:
                if emailDomain[pos + 1] in domainSpecChars:
                    # 2+ special characters are next to eachother; invalid
                    print("Email domain contains 2 adjacent special characters.\n")
                    # LOG: not sus
                    return False
            if not specCharMap[-1] + 2 < len(emailDomain):
                # not enough characters after the '.'; invalid
                print("Email domain length after the '.' is of insufficient length.\n")
                # LOG: not sus
                return False
        else:
            # did not contain a '.'; invalid
            print("Email domain does not contain a '.' character.\n")
            # LOG: not sus
            return False
        # domain + prefix is valid as it passed each check
        print("Email is valid.\n")
        return True
    else:
        # started/ended with a special character; invalid
        print("Email domain's first/last character is a special character.\n")
        # LOG: not sus
        return False

def CheckPhone(phone):
    # check if phone is not ""; is for adding to system
    if phone == "":
        return False

    # check phone number
    if len(phone) == 14 and phone[0:6] == "+31-6-":
        phoneDigits = phone[6:]
        for digit in phoneDigits:
            if not digit in enc.AlphabetExtended(10, 48):
                # phone number after '+31-6-' contains non-number character; invalid
                print("Phone number contains an invalid character.\n")
                # LOG: not sus
                return False
        # phone number is valid as it passed each check
        print("Phone number is valid.\n")
        return True
    elif phone[0:6] != "+31-6-":
        print("Phone number did not start with '+31-6-' sequence.\n")
        # LOG: sus
    else:
        print("Phone number is of insufficient length.\n")
        # LOG: not sus
    return False