# -*- coding: utf-8 -*-
def chuc(number, maxchuc = False):
    if maxchuc is True:
        if number == 0:
            return ''
        if number == 1:
            return 'Mười'
        if number == 2:
            return 'Hai mươi'
        if number == 3:
            return 'Ba mươi'
        if number == 4:
            return 'Bốn mươi'
        if number == 5:
            return 'Năm mươi'
        if number == 6:
            return 'Sáu mươi'
        if number == 7:
            return 'Bảy mươi'
        if number == 8:
            return 'Tám mươi'
        if number == 9:
            return 'Chín mươi'
    else:
        if number == 0:
            return ' linh'
        if number == 1:
            return ' mười'
        if number == 2:
            return ' hai mươi'
        if number == 3:
            return ' ba mươi'
        if number == 4:
            return ' bốn mươi'
        if number == 5:
            return ' năm mươi'
        if number == 6:
            return ' sáu mươi'
        if number == 7:
            return ' bảy mươi'
        if number == 8:
            return ' tám mươi'
        if number == 9:
            return ' chín mươi'
    return ''
def phanchuc(hdv, hc, maxchuc = False):
    pre = chuc(hc, maxchuc)
    if hdv == 1:
        if hc == 0:
            return pre +" một nghìn"
        else:
            return  pre +" mốt nghìn"
    if hdv == 2:
        return  pre +" hai nghìn"
    if hdv == 3:
        return  pre +" ba nghìn"
    if hdv == 4:
        return  pre +" tư nghìn"
    if hdv == 5:
        if hc == 0:
            return pre +" năm nghìn"
        else:
            return  pre +" lăm nghìn"
    if hdv == 6:
        return  pre +" sáu nghìn"
    if hdv == 7:
        return  pre +" bảy nghìn"
    if hdv == 8:
        return  pre +" tám nghìn"
    if hdv == 9:
        return  pre +" chín nghìn"
    if hdv == 0:
        if hc == 0:
            return pre +""
        else:
            return  pre +" nghìn"
    return ''

def phanchucle(hdv, hc):
    pre = chuc(hc, False)
    if hdv == 1:
        pre +" một"
    if hdv == 2:
        return  pre +" hai"
    if hdv == 3:
        return  pre +" ba"
    if hdv == 4:
        return  pre +" tư"
    if hdv == 5:
        if hc == 0:
            return pre +" năm"
        else:
            return  pre +" lăm"
    if hdv == 6:
        return  pre +" sáu"
    if hdv == 7:
        return  pre +" bảy"
    if hdv == 8:
        return  pre +" tám"
    if hdv == 9:
        return  pre +" chín"
    if hdv == 0:
        if hc == 0:
            return ""
        else:
            return pre

    return ''

def MaxDonvi(number):
    if number == 0:
        return ''
    if number == 1:
        return 'Một nghìn'
    if number == 2:
        return 'Hai nghìn'
    if number == 3:
        return 'Ba nghìn'
    if number == 4:
        return 'Bốn nghìn'
    if number == 5:
        return 'Năm nghìn'
    if number == 6:
        return 'Sáu nghìn'
    if number == 7:
        return 'Bảy nghìn'
    if number == 8:
        return 'Tám nghìn'
    if number == 9:
        return 'Chín nghìn'

def tram(number, maxtram = False):
    if maxtram is True:
        if number == 0:
            return ''
        if number == 1:
            return 'Một trăm'
        if number == 2:
            return 'Hai trăm'
        if number == 3:
            return 'Ba trăm'
        if number == 4:
            return 'Bốn trăm'
        if number == 5:
            return 'Năm trăm'
        if number == 6:
            return 'Sáu trăm'
        if number == 7:
            return 'Bảy trăm'
        if number == 8:
            return 'Tám trăm'
        if number == 9:
            return 'Chín trăm'
    else:
        if number == 0:
            return ''
        if number == 1:
            return ' một trăm'
        if number == 2:
            return ' hai trăm'
        if number == 3:
            return ' ba trăm'
        if number == 4:
            return ' bốn trăm'
        if number == 5:
            return ' năm trăm'
        if number == 6:
            return ' sáu trăm'
        if number == 7:
            return ' bảy trăm'
        if number == 8:
            return ' tám trăm'
        if number == 9:
            return ' chín trăm'
    return ''

def nghin(number, maxnghin = False):
    if maxnghin is True:
        if number == 0:
            return ''
        if number == 1:
            return 'Một triệu'
        if number == 2:
            return 'Hai triệu'
        if number == 3:
            return 'Ba triệu'
        if number == 4:
            return 'Bốn triệu'
        if number == 5:
            return 'Năm triệu'
        if number == 6:
            return 'Sáu triệu'
        if number == 7:
            return 'Bảy triệu'
        if number == 8:
            return 'Tám triệu'
        if number == 9:
            return 'Chín triệu'
    else:
        if number == 0:
            return ' triệu'
        if number == 1:
            return ' mốt triệu'
        if number == 2:
            return ' hai triệu'
        if number == 3:
            return ' ba triệu'
        if number == 4:
            return ' bốn triệu'
        if number == 5:
            return ' lăm triệu'
        if number == 6:
            return ' sáu triệu'
        if number == 7:
            return ' bảy triệu'
        if number == 8:
            return ' tám triệu'
        if number == 9:
            return ' chín triệu'
    return ''

def chucnghin(number, maxnghin = False):
    if maxnghin is True:
        if number == 0:
            return ''
        if number == 1:
            return 'Mười'
        if number == 2:
            return 'Hai mươi'
        if number == 3:
            return 'Ba mươi'
        if number == 4:
            return 'Bốn mươi'
        if number == 5:
            return 'Năm mươi'
        if number == 6:
            return 'Sáu mươi'
        if number == 7:
            return 'Bảy mươi'
        if number == 8:
            return 'Tám mươi'
        if number == 9:
            return 'Chín mươi'
    else:
        if number == 0:
            return ''
        if number == 1:
            return ' mười'
        if number == 2:
            return ' hai mươi'
        if number == 3:
            return ' ba mươi'
        if number == 4:
            return ' bốn mươi'
        if number == 5:
            return ' năm mươi'
        if number == 6:
            return ' sáu mươi'
        if number == 7:
            return ' bảy mươi'
        if number == 8:
            return ' tám mươi'
        if number == 9:
            return ' chín mươi'
    return ''

def tramnghin(number):
    if number == 0:
        return ''
    if number == 1:
        return 'Một trăm'
    if number == 2:
        return 'Hai trăm'
    if number == 3:
        return 'Ba trăm'
    if number == 4:
        return 'Bốn trăm'
    if number == 5:
        return 'Năm trăm'
    if number == 6:
        return 'Sáu trăm'
    if number == 7:
        return 'Bảy trăm'
    if number == 8:
        return 'Tám trăm'
    if number == 9:
        return 'Chín trăm'
    return ''
def docphanle(number):
    if number == 0:
        return " đồng"
    if number < 100:
        hc, hdv = divmod(number, 10)
        return phanchucle(hdv, hc) + " đồng"
    else:
        nb1, nb2 = divmod(number, 100)
        hc, hdv = divmod(nb2, 10)
        return tram(nb1, False) + phanchucle(hdv, hc) + " đồng"
def docsotien(tien):
    number, number1 = divmod(tien,1000)
    pl = docphanle(number1)
    if number < 10:
        return MaxDonvi(number) + pl
    elif number < 100:
        hc, hdv = divmod(number,10)
        return phanchuc(hdv, hc, True) + pl
    elif number < 1000:
        nb1, nb2 = divmod(number,100)
        hc, hdv = divmod(nb2, 10)
        return tram(nb1, True) + phanchuc(hdv, hc, False) + pl
    elif number < 10000:
        nb1, nb2 = divmod(number,1000)
        nb11, nb22 = divmod(nb2, 100)
        hc, hdv = divmod(nb22, 10)
        return nghin(nb1, True) + tram(nb11, False) + phanchuc(hdv, hc, False) + pl
    elif number < 100000:
        nb1, nb2 = divmod(number,10000)
        nb11, nb22 = divmod(nb2, 1000)
        nb111, nb222 = divmod(nb22, 100)
        hc, hdv = divmod(nb222, 10)
        return chucnghin(nb1, True) + nghin(nb11, False) + tram(nb111, False) + phanchuc(hdv, hc, False) + pl
    elif number < 1000000:
        nb1, nb2 = divmod(number, 100000)
        nb11, nb22 = divmod(nb2, 10000)
        nb111, nb222 = divmod(nb22, 1000)
        nb1111, nb2222 = divmod(nb222, 100)
        hc, hdv = divmod(nb2222, 10)
        return tramnghin(nb1) + chucnghin(nb11, False) + nghin(nb111, False) + tram(nb1111, False) + phanchuc(hdv, hc, False) + pl
    else: # Chỉ đọc số tiền nhỏ hơn một tỷ đồng
        return ''

import  random

def TestDoctien():
    i =0
    while i<100:
        tien = random.randint(1, 99999999)
        #tien= 302002000
        dst = docsotien(tien)
        print '%s: %s'%(tien, dst)
        i+=1
