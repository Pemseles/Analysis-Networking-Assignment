# uses Vigenère cipher
# key can just be hardcoded & stored in database

# using unicode characters  33 ('!') - 122 ('z')

# '+' 'S' turns into 'D' turns back into 'l'
# '/' 't' turns into 'i' turns back into 'p'
# '*' '#' turns into 'm' turns back into 'j'

def AlphabetExtended():
    newAlphabet = []
    for i in range(90):
        newAlphabet.append(chr(i + 33))
    return newAlphabet

def Encrypt(string):
    key = GenerateKey(string)
    sample = AlphabetExtended()
    encrypt_text = []
    for i in range(len(string)):
        x = (sample.index(string[i]) + sample.index(key[i])) % 90
        encrypt_text.append(sample[x])
    a = "" . join(encrypt_text)
    return("" . join(encrypt_text))

def Decrypt(encrypt_text):
    key = GenerateKey(encrypt_text)
    sample = AlphabetExtended()
    orig_text = []
    for i in range(len(encrypt_text)):
        x = ((sample.index(encrypt_text[i])) - sample.index(key[i]) + 90) % 90
        orig_text.append(sample[x])
    a = "" . join(orig_text)
    return("" . join(orig_text))

def GenerateKey(string):
    key = list("Secret")
    if len(string) == len(key):
        return("" . join(key))
    else:
        for i in range(len(string) - len(key)):
            key.append(key[i % len(key)])
    return("" . join(key))