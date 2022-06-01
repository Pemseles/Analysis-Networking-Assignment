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

    def GetInfo2(self):
        #TODO: Remove this specific function before delivering, is only here for testing purposes
        return f"[ID: {self.membership_id}] {self.first_name} {self.last_name} - {self.address} - {self.email_address} - {self.phone_number} - registered at {self.registration_date}"

class Users:
    def __init__(self, id, registration_date, first_name, last_name, username, password, address, email_address, phone_number, role, role_name):
        self.id = id
        self.registration_date = registration_date
        self.first_name = enc.Decrypt(first_name)
        self.last_name = enc.Decrypt(last_name)
        self.username = enc.Decrypt(username)
        self.password = enc.Decrypt(password)
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

    def GetInfo2(self):
        #TODO: Remove this specific function before delivering, is only here for testing purposes
        return f"[{self.id}; {self.role_name}] ({self.registration_date}) {self.first_name} {self.last_name} (username = {self.username}, password = {self.password}) - {self.address} - {self.email_address} - {self.phone_number}"
    
    def GetProfile(self, loggedInUser):
        try:
            if loggedInUser.role >= 0 and loggedInUser.role <= 2:
                return f"[UserID: {self.id}] {self.first_name} {self.last_name}, registered at {self.registration_date} [{self.role_name}]"
            else:
                return "Nice try, but you don't have the required authorization to see this."
        except:
            return "Nice try, but you don't have the required authorization to see this."
        

def AuthenticateCredentials(username, password):
    for i in db.SelectAllFromTable("Users"):
        if i.password == password and i.username.upper() == username.upper():
            return i
    return 0

def BuildUserAndMemberList(loggedInUser):
    if loggedInUser.role != 1 and loggedInUser.role != 2:
        # logged in user cannot get list of members/users to delete
        return "Nice try, but you don't have the required authorization to see this."
    # builds a list of members & users that logged in user is able to delete
    membersAndUsers = db.SelectAllFromTable("Members") + db.SelectAllFromTable("Users")
    currentlyMembers = True
    result = []
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