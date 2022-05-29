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

    def GetInfo(self):
        return f"[{self.membership_id}] ({self.registration_date}) {self.first_name} {self.last_name} - {self.address} - {self.email_address} - {self.phone_number}"

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
    
    def GetInfo(self):
        return f"[{self.id} - {self.role_name}] ({self.registration_date}) {self.first_name} {self.last_name} (username = {self.username}, password = {self.password}) - {self.address} - {self.email_address} - {self.phone_number}"
    
    def GetProfile(self):
        return f"[{self.id} - {self.role_name}] {self.first_name} {self.last_name}, registered at {self.registration_date}"

def AuthenticateCredentials(username, password):
    for i in db.SelectAllFromTable("Users"):
        if i.password == password and i.username.upper() == username.upper():
            return i
    return 0