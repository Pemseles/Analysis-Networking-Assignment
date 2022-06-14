import dbclasses as dbc
import encryption as enc
import logfeatures as lg
from datetime import date
import sqlite3

def Create_Connection(db_file):
    conn = None
    try:
        conn = sqlite3.connect(db_file)
    except Exception as e:
        print(e)
    return conn

def CreateMemberTable():
    with Create_Connection("database.db") as db:
        c = db.cursor()
        c.execute(""" CREATE TABLE IF NOT EXISTS Members (
            membership_id integer PRIMARY KEY,
            registration_date DATE NOT NULL,
            first_name text NOT NULL,
            last_name text NOT NULL,
            address text NOT NULL,
            email_address text NOT NULL UNIQUE,
            phone_number text NOT NULL
            ); """)

def CreateUserTable():
    with Create_Connection("database.db") as db:
        c = db.cursor()
        c.execute(""" CREATE TABLE IF NOT EXISTS Users (
            id integer PRIMARY KEY AUTOINCREMENT,
            registration_date DATE NOT NULL,
            first_name text NOT NULL,
            last_name text NOT NULL,
            username text NOT NULL UNIQUE,
            password text NOT NULL UNIQUE,
            temp_password text NOT NULL,
            address text NOT NULL,
            email_address text NOT NULL UNIQUE,
            phone_number text NOT NULL,
            role integer NOT NULL,
            role_name text NOT NULL
            ); """) # role defines if user is Advisor (0), System Admin (1) or Super Admin (2)

def InsertIntoMembersTable(membership_id, registration_date, first_name, last_name, address, email_address, phone_number):
    with Create_Connection("database.db") as db:
        c = db.cursor()

        # encrypt sensitive info
        first_name = enc.Encrypt(first_name)
        last_name = enc.Encrypt(last_name)
        address = enc.Encrypt(address)
        email_address = enc.Encrypt(email_address.lower())
        phone_number = enc.Encrypt(phone_number)

        c.execute(""" INSERT INTO Members (membership_id, registration_date, first_name, last_name, address, email_address, phone_number) 
            VALUES(?,?,?,?,?,?,?)""",(membership_id, registration_date, first_name, last_name, address, email_address, phone_number))

def InsertIntoUsersTable(registration_date, first_name, last_name, username, password, temp_password, address, email_address, phone_number, role):
    with Create_Connection("database.db") as db:
        c = db.cursor()

        # determine role_name
        role_name = ""
        if (role == 0):
            role_name = "Advisor"
        elif (role == 1):
            role_name = "System Administrator"
        elif (role == 2):
            role_name = "Super Administrator"
        
        # encrypt sensitive info
        first_name = enc.Encrypt(first_name)
        last_name = enc.Encrypt(last_name)
        username = enc.Encrypt(username.lower())
        password = enc.Encrypt(password)
        temp_password = enc.Encrypt(temp_password)
        address = enc.Encrypt(address)
        email_address = enc.Encrypt(email_address.lower())
        phone_number = enc.Encrypt(phone_number)

        c.execute(""" INSERT INTO Users (registration_date, first_name, last_name, username, password, temp_password, address, email_address, phone_number, role, role_name)
            VALUES(?,?,?,?,?,?,?,?,?,?,?)""",(registration_date, first_name, last_name, username, password, temp_password, address, email_address, phone_number, role, role_name))

def DeleteFromTable(loggedInUser, target):
    # check auth
    if loggedInUser.role != 1 and loggedInUser.role != 2:
        # unauthorized attempt to delete from table
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized access to database method", "User attempted to delete an entry from Members/Users table"))
        return
    # delete from table
    table = ""
    filterDigit = 0
    filter = ""
    # check if target is member or user (determines table, filter & filter's value)
    if isinstance(target, dbc.Members):
        if not loggedInUser.role == 0 and not loggedInUser.role == 1 and not loggedInUser.role == 2:
            # logged in user is not authorized to delete member
            lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "User is not authorized to delete member", f"User attempted to delete {target.first_name} {target.last_name}'s entry"))
            return "Not authorized to delete member."
        table = "Members"
        filterDigit = target.membership_id
        filter = "membership_id"
    elif isinstance(target, dbc.Users):
        if loggedInUser.role <= target.role and loggedInUser.role > 0 and loggedInUser.role < 3:
            # logged in user is not authorized to delete user
            lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "User is not authorized to delete user", f"User attempted to delete {target.first_name} {target.last_name}'s entry"))
            return "Not authorized to delete user."
        table = "Users"
        filterDigit = target.id
        filter = "id"
    else:
        # unrecognized thing to delete
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "User attempted to delete an unknown entry", f"This unrecognized entry is as follows: {target}"))
        return "Nice try"
    # preceed with deleting target
    with Create_Connection("database.db") as db:
        c = db.cursor()
        print("deleting from table...")

        # fill sql statement with table, filter and filterDigit & execute
        c.execute(f"""DELETE FROM {table} WHERE {filter}={filterDigit}""")
        db.commit()

# only here for testing purposes & convenience
def InsertStaticUsers():
    # insert static super admin (change back username to superadmin & password to Admin321!)
    InsertIntoUsersTable(date.today().strftime("%d-%m-%y"), "Super", "Admin", "superadmin", "Admin321!", "", "Someplace", "super@admin.com", "+31-6-12345678", 2)
    
def ConvertFetchToArray(fetched):
    newArr = []
    for i in fetched:
        newArr.append(list(i)[0])
    return newArr

def SelectAllFromTable(table_name):
    if table_name != "Members" and table_name != "Users":
        # invalid table_name
        lg.AppendToLog(lg.BuildLogText("---", True, "Someone tried to select information using an invalid parameter", f"Parameter given was {table_name} instead of either 'Members' or 'Users'"))
        return []
    with Create_Connection("database.db") as db:
        c = db.cursor()
        c.execute(f"""SELECT * FROM {table_name}""")
        rows = c.fetchall()
        for i in range(len(rows)):
            if (table_name == "Members"):
                rows[i] = dbc.Members(*rows[i])
            elif (table_name == "Users"):
                rows[i] = dbc.Users(*rows[i])
        return rows

def SelectColumnFromTable(table_name, column_name):
    # possible columns
    possibleCols = ["first_name", "last_name", "username", "password", "temp_password", "address", "email_address", "phone_number", "role", "role_name", "registration_date", "id", "membership_id"]
    if (table_name != "Members" and table_name != "Users") or not column_name in possibleCols:
        # invalid table_name or column_name
        lg.AppendToLog(lg.BuildLogText("---", True, "Someone tried to select information using an invalid parameter", f"Parameters given were {table_name} and {column_name}, one of which is invalid"))
        return []
    with Create_Connection("database.db") as db:
        c = db.cursor()
        c.execute(f"""SELECT {column_name} FROM {table_name}""")
        rows = c.fetchall()
        return rows

def UpdateUserEntry(loggedInUser, newEntry):
    if loggedInUser.role < 0 and loggedInUser.role > 2:
        # unauthorized attempt to update user info
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized access to database method", "User attempted to update an entry in Users table"))
        return "Nice Try"
    print(f"in UpdateUserEntry: {newEntry}")
    with Create_Connection("database.db") as db:
        c = db.cursor()
        c.execute(""" UPDATE Users 
                    SET first_name = ? , 
                        last_name = ? , 
                        username = ? , 
                        password = ? , 
                        temp_password = ? , 
                        address = ? , 
                        email_address = ? , 
                        phone_number = ? 
                    WHERE id = ?""", newEntry)
        db.commit()

def UpdateMemberEntry(loggedInUser, newEntry):
    if loggedInUser.role < 0 and loggedInUser.role > 2:
        # unauthorized attempt to update member entry
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized access to database method", "User attempted to update an entry in Members table"))
        return "Nice Try"
    print(f"in UpdateMemberEntry: {newEntry}")
    with Create_Connection("database.db") as db:
        c = db.cursor()
        c.execute("""UPDATE Members 
                    SET first_name = ? , 
                        last_name = ? , 
                        address = ? , 
                        email_address = ? , 
                        phone_number = ? 
                    WHERE membership_id = ?""", newEntry)
        db.commit()

def UpdateRegistrationDateMember(loggedInUser, newEntry):
    if loggedInUser.role < 0 and loggedInUser.role > 2:
        # untauthorized attempt to update registration date 
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized access to database method", "User attempted to update an entry's registration date in Members table"))
        return "Nice Try"
    print(f"in UpdateRegistrationDateMember: {newEntry}")
    with Create_Connection("database.db") as db:
        c = db.cursor()
        c.execute("""UPDATE Members
                    SET registration_date = ?
                    WHERE membership_id = ?""", newEntry)
        db.commit()

def UpdateRegistrationDateUser(loggedInUser, newEntry):
    if loggedInUser.role < 1 and loggedInUser.role > 2:
        # untauthorized attempt to update registration date 
        lg.AppendToLog(lg.BuildLogText(loggedInUser, True, "Unauthorized access to database method", "User attempted to update an entry's registration date in Users table"))
        return "Nice Try"
    print(f"in UpdateRegistrationDateUser: {newEntry}")
    with Create_Connection("database.db") as db:
        c = db.cursor()
        c.execute("""UPDATE Users
                    SET registration_date = ?
                    WHERE id = ?""", newEntry)
        db.commit()

if __name__ == '__main__':
    with Create_Connection("database.db") as db:
        c = db.cursor()
        c.execute("""DROP TABLE Members""")
        c.execute("""DROP TABLE Users""")
    CreateMemberTable()
    CreateUserTable()
    InsertStaticUsers()