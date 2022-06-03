# uses Vigenère cipher
# key can just be hardcoded & stored in database

# alphabet is in unicode from 32 - 126 (latin lowercase & uppercase, digits and most relevant special characters)
# ranges can be found at: (basic latin) https://www.ssec.wisc.edu/~tomw/java/unicode.html#x0000

def AlphabetExtended(length, offset):
    # fills array with characters within specified range
    newAlphabet = []
    for i in range(length):
        newAlphabet.append(chr(i + offset))
    return newAlphabet

def Encrypt(string, customRange = [], outliers = []):
    key = GenerateKey(string)
    if len(customRange) == 0:
        sample = AlphabetExtended(95, 32)
    else:
        sample = AlphabetExtended(customRange[0], customRange[1]) + outliers
    encrypt_text = []
    for i in range(len(string)):
        x = (sample.index(string[i]) + sample.index(key[i])) % len(sample)
        encrypt_text.append(sample[x])
    a = "" . join(encrypt_text)
    return("" . join(encrypt_text))

def Decrypt(encrypt_text, customRange = [], outliers = []):
    key = GenerateKey(encrypt_text)
    if len(customRange) == 0:
        sample = AlphabetExtended(95, 32)
    else:
        sample = AlphabetExtended(customRange[0], customRange[1]) + outliers
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

def EncryptTupleOrArray(toEnc, customRange = [], outliers = []):
    cipherArr = []
    for x in toEnc:
        cipherArr.append(Encrypt(x, customRange, outliers))
    return cipherArr

def DecryptTupleOrArray(toDecr, customRange = [], outliers = []):
    normalArr = []
    for x in toDecr:
        normalArr.append(Decrypt(str(x), customRange, outliers))
    return normalArr
