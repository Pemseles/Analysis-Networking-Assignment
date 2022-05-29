# uses Vigenère cipher
# key can just be hardcoded & stored in database

# alphabet is in unicode from 33 - 126 (latin lowercase & uppercase, digits and most relevant special characters)
# ranges can be found at: (basic latin) https://www.ssec.wisc.edu/~tomw/java/unicode.html#x0000

def AlphabetExtended(length, offset):
    # fills array with characters within specified range
    newAlphabet = []
    for i in range(length):
        newAlphabet.append(chr(i + offset))
    return newAlphabet

def Encrypt(string):
    key = GenerateKey(string)
    sample = AlphabetExtended(94, 33)
    encrypt_text = []
    for i in range(len(string)):
        x = (sample.index(string[i]) + sample.index(key[i])) % len(sample)
        encrypt_text.append(sample[x])
    a = "" . join(encrypt_text)
    return("" . join(encrypt_text))

def Decrypt(encrypt_text):
    key = GenerateKey(encrypt_text)
    sample = AlphabetExtended(94, 33)
    orig_text = []
    for i in range(len(encrypt_text)):
        x = ((sample.index(encrypt_text[i]) - sample.index(key[i])) + len(sample)) % len(sample)
        orig_text.append(sample[x])
    a = "" . join(orig_text)
    return("" . join(orig_text))

def GenerateKey(string):
    # hardcoded key = Secret
    key = list("Secret")
    if len(string) == len(key):
        return("" . join(key))
    else:
        for i in range(len(string) - len(key)):
            key.append(key[i % len(key)])
    return("" . join(key))

def EncryptTupleOrArray(toEnc):
    cipherArr = []
    for x in toEnc:
        cipherArr.append(Encrypt(x))
    return cipherArr

def DecryptTupleOrArray(toDecr):
    normalArr = []
    for x in toDecr:
        normalArr.append(Decrypt(x))
    return normalArr
