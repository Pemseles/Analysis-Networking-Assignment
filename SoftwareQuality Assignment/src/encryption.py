# uses Vigenère cipher

def encrypt(string, key): 
    encrypt_text = [] 
    for i in range(len(string)): 
        x = (ord(string[i]) +ord(key[i])) % 26
        x += ord('A') 
        encrypt_text.append(chr(x))
    print("" . join(encrypt_text)) 
    return("" . join(encrypt_text)) 

def decrypt(encrypt_text, key): 
    orig_text = [] 
    for i in range(len(encrypt_text)): 
        x = (ord(encrypt_text[i]) -ord(key[i]) + 26) % 26
        x += ord('A') 
        orig_text.append(chr(x))
    print("" . join(orig_text))
    return("" . join(orig_text))

def generateKey(string, key): 
    key = list(key) 
    if len(string) == len(key): 
        return(key) 
    else: 
        for i in range(len(string) -len(key)): 
            key.append(key[i % len(key)]) 
    print("" . join(key))
    return("" . join(key))