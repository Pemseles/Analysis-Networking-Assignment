import email
from msilib.schema import Error
import dbclasses as dbc
import encryption as enc
from datetime import date
import sqlite3

def Create_Connection(db_file):
    conn = None
    try:
        conn = sqlite3.connect(db_file)
    except Error as e:
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
            phone_number text NOT NULL UNIQUE
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
            address text NOT NULL,
            email_address text NOT NULL UNIQUE,
            phone_number text NOT NULL,
            role integer NOT NULL,
            role_name text NOT NULL
            ); """) # role defines if user is Advisor (0), System Admin (1) or Super Admin (2)

def InsertIntoMembersTable(membership_id, registration_date, first_name, last_name, address, email_address, phone_number, key):
    with Create_Connection("database.db") as db:
        c = db.cursor()

        # encrypt sensitive info
        first_name = enc.Encrypt(first_name, key)
        last_name = enc.Encrypt(last_name, key)
        address = enc.Encrypt(address, key)
        email_address = enc.Encrypt(email_address, key)
        phone_number = enc.Encrypt(phone_number, key)

        c.execute(""" INSERT INTO Members (membership_id, registration_date, first_name, last_name, address, email_address, phone_number) 
            VALUES(?,?,?,?,?,?,?)""",(membership_id, registration_date, first_name, last_name, address, email_address, phone_number))

def InsertIntoUsersTable(registration_date, first_name, last_name, username, password, address, email_address, phone_number, role, keyword):
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
        #first_name = enc.Encrypt(first_name, generateKey(first_name, keyword))
        #last_name = enc.Encrypt(last_name, generateKey(last_name, keyword))
        #username = enc.Encrypt(username, generateKey(username, keyword))
        #password = enc.Encrypt(password, generateKey(password, keyword))
        #address = enc.Encrypt(address, generateKey(address, keyword))
        #email_address = enc.Encrypt(email_address, generateKey(email_address, keyword))
        #phone_number = enc.Encrypt(phone_number, generateKey(phone_number, keyword))

        c.execute(""" INSERT INTO Users (registration_date, first_name, last_name, username, password, address, email_address, phone_number, role, role_name)
            VALUES(?,?,?,?,?,?,?,?,?,?)""",(registration_date, first_name, last_name, username, password, address, email_address, phone_number, role, role_name))

# only here for testing purposes & convenience
def InsertStaticUsers():
    # insert static super admin
    InsertIntoUsersTable(date.today().strftime("%m-%d-%y"), "Super", "Admin", "superadmin", "Admin321!", "Someplace", "super@admin.com", "+31-6-12345678", 2, "superkey")
    
    # insert static system admin
    InsertIntoUsersTable(date.today().strftime("%m-%d-%y"), "System", "Admin", "systemadmin", "System321!", "Someotherplace", "system@admin.com", "+31-6-87654321", 1, "systemkey")

    # insert static advisor
    InsertIntoUsersTable(date.today().strftime("%m-%d-%y"), "Ad", "Visor", "advisor", "Advisor321!", "Somerandomplace", "ad@visor.com", "+31-6-11111111", 0, "advisorkey")

def SelectAllFromTable(table_name):
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

if __name__ == '__main__':
    with Create_Connection("database.db") as db:
        c = db.cursor()
        c.execute("""DROP TABLE Members""")
        c.execute("""DROP TABLE Users""")
    CreateMemberTable()
    CreateUserTable()
    InsertStaticUsers()

    # print statements for test purposes
    print("Printing members in db:")
    for i in SelectAllFromTable("Members"):
        print(i.getInfo())
    print ("Printing users in db:")
    for j in SelectAllFromTable("Users"):
        print(j.getInfo())