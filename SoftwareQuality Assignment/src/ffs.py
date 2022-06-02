import consolemenus as cm
import database as db
import menuoptions as mo
import logfeatures as lg

if __name__ == '__main__':
    lg.CreateLog()
    cm.SystemScreenLoop()

    # print statements for test purposes
    print("Printing members in db:")
    for i in db.SelectAllFromTable("Members"):
        print(i.GetInfo2())
    print ("Printing users in db:")
    for j in db.SelectAllFromTable("Users"):
        print(j.GetInfo2())