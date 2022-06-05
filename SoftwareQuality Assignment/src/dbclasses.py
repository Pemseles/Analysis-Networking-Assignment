from gettext import install
from msilib.schema import InstallUISequence
import database as db
import encryption as enc

class Members:
    def __init__(self, membership_id, registration_date, first_name, last_name, address, email_address, phone_number):
        self.membership_id = membership_id
        self.registration_date = registration_date
        self.first_name = enc.Decrypt(first_name)
        self.last_name = enc.Decrypt(last_name)
        self.address = enc.Decrypt(address)
        self.email_address = enc.Decrypt(email_address)
        self.phone_number = enc.Decrypt(phone_number)

    def GetInfo(self, loggedInUser):
        try:
            if loggedInUser.role >= 0 and loggedInUser.role <= 2:
                return f"[ID: {self.membership_id}] {self.first_name} {self.last_name} - {self.address} - {self.email_address} - {self.phone_number} - registered at {self.registration_date}"
            else:
                return "Nice try, but you don't have the required authorization to see this."
        except:
            return "Nice try, but you don't have the required authorization to see this."

class Users:
    def __init__(self, id, registration_date, first_name, last_name, username, password, temp_password, address, email_address, phone_number, role, role_name):
        self.id = id
        self.registration_date = registration_date
        self.first_name = enc.Decrypt(first_name)
        self.last_name = enc.Decrypt(last_name)
        self.username = enc.Decrypt(username)
        self.password = enc.Decrypt(password)
        self.temp_password = enc.Decrypt(temp_password)
        self.address = enc.Decrypt(address)
        self.email_address = enc.Decrypt(email_address)
        self.phone_number = enc.Decrypt(phone_number)
        self.role = role
        self.role_name = role_name
    
    def GetInfo(self, loggedInUser):
        try:
            if loggedInUser.role == 1 or loggedInUser.role == 2:
                return f"[{self.id}; {self.role_name}] ({self.registration_date}) {self.first_name} {self.last_name} (username = {self.username}, password = {self.password}) - {self.address} - {self.email_address} - {self.phone_number}"
            else:
                return "Nice try, but you don't have the required authorization to see this."
        except:
            return "Nice try, but you don't have the required authorization to see this."

    def GetProfile(self, loggedInUser):
        try:
            if loggedInUser.role >= 0 and loggedInUser.role <= 2:
                return f"[UserID: {self.id}] {self.first_name} {self.last_name}, registered at {self.registration_date} [{self.role_name}]"
            else:
                return "Nice try, but you don't have the required authorization to see this."
        except:
            return "Nice try, but you don't have the required authorization to see this."
        

def AuthenticateCredentials(username, password):
    # for each user, check if they match inputted username & password
    for i in db.SelectAllFromTable("Users"):
        # if user has a temporary password, compare it to the input instead
        checkTemp = True if i.temp_password != "" else False
        if not checkTemp and i.password == password and i.username.upper() == username.upper():
            return i
        elif checkTemp and (i.temp_password == password or i.password == password) and i.username.upper() == username.upper():
            return i
    return 0

def BuildUserAndMemberList(loggedInUser, filter, includeMembers = True):
    if loggedInUser.role < 0 or loggedInUser.role > 2:
        # logged in user cannot get list of members/users to delete
        return "Nice try, but you don't have the required authorization to see this."
    # builds a list of members & users that logged in user is able to delete
    if filter == "":
        if includeMembers:
            membersAndUsers = db.SelectAllFromTable("Members") + db.SelectAllFromTable("Users")
        else:
            membersAndUsers = db.SelectAllFromTable("Users")
    else:
        # check filter
        if filter == " ":
            filter = ""
        filter = filter.lower()
        # get members&users, then apply filter
        membersAndUsers = []
        if includeMembers:
            beforeFilterMembersAndUsers = db.SelectAllFromTable("Members") + db.SelectAllFromTable("Users")
        else:
            beforeFilterMembersAndUsers = db.SelectAllFromTable("Users")
        for entry in beforeFilterMembersAndUsers:
            # filters entries based on what the user gets to see (Members pretty much all info, Users only their profiles)
            if isinstance(entry, Members) and (loggedInUser.role >= 0 and loggedInUser.role <= 2):
                # checks if filter is substring of first/last name, address, email, phone num, membership_id or registration date of Member
                if filter in entry.first_name.lower() or filter in entry.last_name.lower() or filter in entry.address.lower() or filter in entry.email_address or filter in entry.phone_number or filter in str(entry.membership_id) or filter in str(entry.registration_date):
                    membersAndUsers.append(entry)
            elif isinstance(entry, Users) and (loggedInUser.role > 0 and loggedInUser.role <= 2 and loggedInUser.role > entry.role):
                # checks if filter is substring of first/last name, role_name, id or registration date
                if filter in entry.first_name.lower() or filter in entry.last_name.lower() or filter in entry.role_name.lower() or filter in str(entry.id) or filter in str(entry.registration_date):
                    membersAndUsers.append(entry)

    currentlyMembers = True
    result = []
    # make sure there is more than 1 result
    if len(membersAndUsers) < 1:
        result.append("There were no results matching your request.")
        return result

    if isinstance(membersAndUsers[0], Members):
        result.append("Members:")
    for entry in membersAndUsers:
        if entry != None and entry != "":
            try:
                # guaranteed to be a user
                # check if user is able to be deleted by logged in user
                if entry.role < loggedInUser.role:
                    if currentlyMembers:
                        result.append("\nUsers:")
                        currentlyMembers = False
                    result.append(entry)
            except:
                # guaranteed to be a member
                # add member to result
                result.append(entry)
    return result