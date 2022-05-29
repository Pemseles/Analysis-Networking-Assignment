import console as cs
import database as db

def RunApplication():
    cs.SystemScreenLoop()

if __name__ == '__main__':
    cs.SystemScreenLoop()

    # print statements for test purposes
    print("Printing members in db:")
    for i in db.SelectAllFromTable("Members"):
        print(i.GetInfo())
    print ("Printing users in db:")
    for j in db.SelectAllFromTable("Users"):
        print(j.GetInfo())